using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Barracuda;
using UnityEngine;
using System.IO;

namespace Assets.Scripts
{
    public class Yolov4Detector : MonoBehaviour, Detector
    {
        public string INPUT_NAME;

        public virtual int RequiredWidth
        {
            get
            {
                return IMAGE_SIZE;
            }
        }

        public int IMAGE_SIZE = 416;
        public int CLASS_COUNT = 3;
        public int OUTPUT1_SIZE = 10647;
        public int OUTPUT2_SIZE = 10647;
        public int OUTPUT3_SIZE = 10647;
        public float MINIMUM_CONFIDENCE = 0.25f;
        public int OBJECTS_LIMIT = 20;


        public NNModel modelFile;
        public TextAsset labelsFile;

        private string[] labels;
        private IWorker worker;

        private const int IMAGE_MEAN = 0;
        private const float IMAGE_STD = 255.0F;
        private Tensor anchor;


        public void Start()
        {
            this.labels = Regex.Split(this.labelsFile.text, "\n|\r|\r\n")
                .Where(s => !String.IsNullOrEmpty(s)).ToArray();  
            var model = ModelLoader.Load(this.modelFile);
            this.worker = GraphicsWorker.GetV4Worker(model);
            int[] array1 = new int[] { 1, 3, 3, 2 };
            float[] array2 = new float[] { 12, 16, 19, 36, 40, 28, 36, 75, 76, 55, 72, 146, 142, 110, 192, 243, 459, 401 };
            anchor = new Tensor(array1, array2);
        }

        public IEnumerator Detect(Color32[] picture, int width, Rect rect, System.Action<IList<BoundingBox>> callback)
        {

            using (var tensor = TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width, rect))
            {
                var inputs = new Dictionary<string, Tensor>();
                var layer = new Dictionary<int, Tensor>();
                inputs.Add(INPUT_NAME, tensor);
                yield return StartCoroutine(worker.StartManualSchedule(inputs));

                layer.Add(OUTPUT1_SIZE, worker.PeekOutput("Identity"));
                layer.Add(OUTPUT2_SIZE, worker.PeekOutput("Identity_1"));
                layer.Add(OUTPUT3_SIZE, worker.PeekOutput("Identity_2"));
                var results = ParseYoloV5Output(layer, MINIMUM_CONFIDENCE);
                var boxes = FilterBoundingBoxes(results, OBJECTS_LIMIT, MINIMUM_CONFIDENCE);
                foreach (KeyValuePair<int, Tensor> l in layer) l.Value.Dispose();
                tensor.Dispose();
                callback(boxes);
            }
        }

        private static Tensor TransformInput(Color32[] pic, int width, int height, int requestedWidth, Rect rect)
        {
            float[] floatValues = new float[width * height * 3];

            float RationX = rect.width / width;
            float RationY = rect.height / height;

            float beginning = rect.yMin * requestedWidth;
            float leftOffset = rect.xMin / 2;
            //int beginning = (((pic.Length / requestedWidth) - height) * requestedWidth) / 2;
            //int leftOffset = (requestedWidth - width) / 2;

            float picX, picY;
            picY = beginning;

            Texture2D test = new Texture2D(width, height);

            for (int i = 0; i < height; i++)
            {
                picX = leftOffset;
                for (int j = 0; j < width; j++)
                {
                    int x = Mathf.RoundToInt(picX);
                    int y = Mathf.RoundToInt(picY);

                    var color = pic[x + y * requestedWidth];

                    test.SetPixel(j, i, color);

                    floatValues[(i * width + j) * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
                    floatValues[(i * width + j) * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
                    floatValues[(i * width + j) * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
                    picX += RationX;
                }
                picY += RationY;
            }


            var dirPath = Application.dataPath + "/../SaveImages/";

            byte[] bytes2 = test.EncodeToPNG();
            if (!System.IO.Directory.Exists(dirPath))
            {
                print(dirPath);
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "Image4.png", bytes2);



            return new Tensor(1, height, width, 3, floatValues);
        }


        private IList<BoundingBox> ParseYoloV5Output(Dictionary<int, Tensor> tensor, float thresholdMax)
        {
            var boxes = new List<BoundingBox>();
            int layer_num = 0;
            foreach (KeyValuePair<int, Tensor> layer in tensor)
            {
                for (int x = 0; x < layer.Key; x++)
                {
                    for (int y = 0; y < layer.Key; y++)
                    {
                        for (int a = 0; a < 3; a++)
                        {
                            float confidence = GetConfidence(layer.Value, x, y, a);
                            if (confidence < thresholdMax)
                                continue;
                            BoundingBoxDimensions dimensions = ExtractBoundingBoxDimensionsYolov5(layer.Value, x, y, a, layer_num);
                            (int classIdx, float maxClass) = GetClassIdx(layer.Value, x, y, a);

                            float maxScore = confidence * maxClass;

                            if (maxScore < thresholdMax)
                                continue;

                            boxes.Add(new BoundingBox
                            {
                                Dimensions = MapBoundingBoxToCell(dimensions),
                                Confidence = confidence,
                                Label = labels[classIdx]
                            });
                        }
                    }
                }
                layer_num++;
            }
            return boxes;
        }

        private BoundingBoxDimensions ExtractBoundingBoxDimensionsYolov5(Tensor tensor, int x, int y, int a, int layer)
        {
            Func<float, float> expit = (x) => 1 / (1 + (float)Math.Exp(-x));            
            int[] STRIDES = new int[] { 8, 16, 32 };
            float[] XYSCALE = new float[] { 1.2F, 1.1F, 1.05F };
            var conv_raw_dx = tensor[0, 0, 0, 0, x, y, a, 0];
            var conv_raw_dy = tensor[0, 0, 0, 0, x, y, a, 1];
            var conv_raw_dw = tensor[0, 0, 0, 0, x, y, a, 2];
            var conv_raw_dh = tensor[0, 0, 0, 0, x, y, a, 3];

            float pred_x = ((expit(conv_raw_dx) * XYSCALE[layer]) - 0.5F * (XYSCALE[layer] - 1) + x) * STRIDES[layer];
            float pred_y = ((expit(conv_raw_dy) * XYSCALE[layer]) - 0.5F * (XYSCALE[layer] - 1) + y) * STRIDES[layer];
            float pred_w = (float)Math.Exp(conv_raw_dw) * anchor[0, layer, a, 0];
            float pred_h = (float)Math.Exp(conv_raw_dh) * anchor[0, layer, a, 1];

            return new BoundingBoxDimensions
            {
                X = pred_x,
                Y = pred_y,
                Width = pred_w,
                Height = pred_h
            };            
        }

        private float GetConfidence(Tensor tensor, int x, int y, int a)
        {
            float tConf = tensor[0, 0, 0, 0, x, y, a, 4];
            return Sigmoid(tConf);
        }

        private ValueTuple<int, float> GetClassIdx(Tensor tensor, int x, int y, int a)
        {
            int classIdx = 0;

            float maxConf = tensor[0, 0, 0, 0, x, y, a, 5];

            for (int i = 0; i < CLASS_COUNT; i++)
            {
                if (tensor[0, 0, 0, 0, x, y, a, 5 + i] > maxConf)
                {
                    maxConf = tensor[0, 0, 0, 0, x, y, a, 5 + i];
                    classIdx = i;
                }
            }
            return (classIdx, maxConf);
        }

        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);

            return k / (1.0f + k);
        }

        private BoundingBoxDimensions MapBoundingBoxToCell(BoundingBoxDimensions boxDimensions)
        {
            return new BoundingBoxDimensions
            {
                X = (boxDimensions.X),
                Y = (boxDimensions.Y),
                Width = boxDimensions.Width,
                Height = boxDimensions.Height,
            };
        }

        private IList<BoundingBox> FilterBoundingBoxes(IList<BoundingBox> boxes, int limit, float threshold)
        {
            var activeCount = boxes.Count;
            var isActiveBoxes = new bool[boxes.Count];

            for (int i = 0; i < isActiveBoxes.Length; i++)
            {
                isActiveBoxes[i] = true;
            }

            var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
                    .OrderByDescending(b => b.Box.Confidence)
                    .ToList();

            var results = new List<BoundingBox>();

            for (int i = 0; i < boxes.Count; i++)
            {
                if (isActiveBoxes[i])
                {
                    var boxA = sortedBoxes[i].Box;
                    results.Add(boxA);

                    if (results.Count >= limit)
                        break;

                    for (var j = i + 1; j < boxes.Count; j++)
                    {
                        if (isActiveBoxes[j])
                        {
                            var boxB = sortedBoxes[j].Box;

                            if (IntersectionOverUnion(boxA.Rect, boxB.Rect) > threshold)
                            {
                                isActiveBoxes[j] = false;
                                activeCount--;

                                if (activeCount <= 0)
                                    break;
                            }
                        }
                    }

                    if (activeCount <= 0)
                        break;
                }
            }
            return results;
        }

        private float IntersectionOverUnion(Rect boundingBoxA, Rect boundingBoxB)
        {
            var areaA = boundingBoxA.width * boundingBoxA.height;

            if (areaA <= 0)
                return 0;

            var areaB = boundingBoxB.width * boundingBoxB.height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(boundingBoxA.xMin, boundingBoxB.xMin);
            var minY = Math.Max(boundingBoxA.yMin, boundingBoxB.yMin);
            var maxX = Math.Min(boundingBoxA.xMax, boundingBoxB.xMax);
            var maxY = Math.Min(boundingBoxA.yMax, boundingBoxB.yMax);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }

        
    }
}
