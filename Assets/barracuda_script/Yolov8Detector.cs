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
    public class Yolov8Detector : MonoBehaviour, Detector
    {
        public string INPUT_NAME = "images";

        public virtual int RequiredWidth
        {
            get
            {
                return IMAGE_SIZE;
            }
        }

        public int IMAGE_SIZE = 640;
        public int CLASS_COUNT = 80;
        public int OUTPUT1_SIZE = 80;
        public int OUTPUT2_SIZE = 40;
        public int OUTPUT3_SIZE = 20;
        public float MINIMUM_CONFIDENCE = 0.5f;
        public int OBJECTS_LIMIT = 20;

        public NNModel modelFile;
        public TextAsset labelsFile;

        private string[] labels;
        private IWorker worker;

        private const int IMAGE_MEAN = 0;
        private const float IMAGE_STD = 255.0F;
        private Tensor anchor;
        private int outputSize; 

        public void Start()
        {
            outputSize = OUTPUT1_SIZE * OUTPUT1_SIZE + OUTPUT2_SIZE * OUTPUT2_SIZE + OUTPUT3_SIZE * OUTPUT3_SIZE;
            this.labels = Regex.Split(this.labelsFile.text, "\n|\r|\r\n")
                .Where(s => !String.IsNullOrEmpty(s)).ToArray();  
            Model model = ModelLoader.Load(this.modelFile);
            this.worker = GraphicsWorker.GetV8Worker(model);
            //int[] array1 = new int[] { 1, 3, 3, 2 };
            //float[] array2 = new float[] { 12, 16, 19, 36, 40, 28, 36, 75, 76, 55, 72, 146, 142, 110, 192, 243, 459, 401 };
            //anchor = new Tensor(array1, array2);
        }

        public bool checkOutputFormat(Tensor output) {
            bool result = true;
            double[] avg = new double[84];
            for (int i = 0; i < CLASS_COUNT; i++) avg[i] = 0.0;
            for (int i = 0; i < OUTPUT1_SIZE* OUTPUT1_SIZE + OUTPUT2_SIZE * OUTPUT2_SIZE + OUTPUT3_SIZE * OUTPUT3_SIZE; i++) {
                for (int j = 0; j < 4; j++) {
                    avg[j] += output[0, 0, i, j];
                    result &= inBetween(output[0, 0, i, j], 640, 0);
                }

                for (int j = 4; j < 84; j++)
                {

                    avg[j] += output[0, 0, i, j];
                    result &= inBetween(output[0, 0, i, j], 1, 0);
                }
            }
            for (int i = 0; i < 84; i++) Debug.Log("Object reconition Avg: " + avg[i]/ outputSize);
            return result;
        }

        public bool inBetween(float test, float upper, float lower) {
            return (upper > test && lower < test);        
        }

        public IEnumerator Detect(Color32[] picture, int width, Rect rect, System.Action<IList<BoundingBox>> callback)
        {

            using (var tensor = TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE, width, rect))
            {
                var inputs = new Dictionary<string, Tensor>();
                var layer = new Dictionary<int, Tensor>();
                inputs.Add(INPUT_NAME, tensor);
                yield return StartCoroutine(worker.StartManualSchedule(inputs));

                var output = worker.PeekOutput("output0");

                var results = ParseYoloV8Output(output, MINIMUM_CONFIDENCE);
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


        private IList<BoundingBox> ParseYoloV8Output(Tensor tensor, float thresholdMax)
        {
            var boxes = new List<BoundingBox>();
            for (int index = 0; index < outputSize; index++)
            {
                (int classIdx, float maxScore) = GetClassIdx(tensor, index);
        
                if (maxScore < thresholdMax)
                    continue;
        
                BoundingBoxDimensions dimensions = ExtractBoundingBoxDimensionsYolov5(tensor, 0, 0, index, 0);
        
        
                boxes.Add(new BoundingBox
                {
                    Dimensions = MapBoundingBoxToCell(dimensions),
                    Confidence = maxScore,
                    Label = labels[classIdx]
                });
            }
            print(boxes.Count);
            return boxes;
        }

        private BoundingBoxDimensions ExtractBoundingBoxDimensionsYolov5(Tensor tensor, int x, int y, int index, int layer)
        {
            //Func<float, float> expit = (x) => 1 / (1 + (float)Math.Exp(-x));            
            //int[] STRIDES = new int[] { 8, 16, 32 };
            //float[] XYSCALE = new float[] { 1.2F, 1.1F, 1.05F };
            var conv_raw_dx = tensor[0, 0, index, 0];
            var conv_raw_dy = tensor[0, 0, index, 1];
            var conv_raw_dx2 = tensor[0, 0, index, 2];
            var conv_raw_dy2 = tensor[0, 0, index, 3];
            //print(conv_raw_dx + " " + conv_raw_dy + " " + conv_raw_dx2 + " " + conv_raw_dy2);
            //float pred_x = ((expit(conv_raw_dx) * XYSCALE[layer]) - 0.5F * (XYSCALE[layer] - 1) + x) * STRIDES[layer];
            //float pred_y = ((expit(conv_raw_dy) * XYSCALE[layer]) - 0.5F * (XYSCALE[layer] - 1) + y) * STRIDES[layer];
            //
            //float pred_x2 = ((expit(conv_raw_dx2) * XYSCALE[layer]) - 0.5F * (XYSCALE[layer] - 1) + x) * STRIDES[layer];
            //float pred_y2 = ((expit(conv_raw_dy2) * XYSCALE[layer]) - 0.5F * (XYSCALE[layer] - 1) + y) * STRIDES[layer];
            //print("after: "+pred_x + " " + pred_y + " " + pred_x2 + " " + pred_y2);
            //return new BoundingBoxDimensions
            //{
            //    X = (pred_x + pred_x2)/2,
            //    Y = (pred_y + pred_y2) / 2,
            //    Width = pred_x2 - pred_x,
            //    Height = pred_y2 - pred_y
            //};            
            return new BoundingBoxDimensions
            {
                X = (conv_raw_dx),
                Y = (conv_raw_dy) ,
                Width = conv_raw_dx2,
                Height = conv_raw_dy2
            };
        }

        private float GetConfidence(Tensor tensor, int index)
        {
            float tConf = tensor[0, 0, index, 4];
            return Sigmoid(tConf);
        }

        private ValueTuple<int, float> GetClassIdx(Tensor tensor, int index)
        {
            int classIdx = 0;

            float maxConf = 0;

            for (int i = 0; i < CLASS_COUNT; i++)
            {
                if (tensor[0, 0, index, 4 + i] > maxConf)
                {
                    maxConf = tensor[0, 0, index, 4 + i];
                    classIdx = i;
                }
            }
            return (classIdx, maxConf);
        }

        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);

            //return k / (1.0f + k);
            return value;
        }

        private BoundingBoxDimensions MapBoundingBoxToCell(BoundingBoxDimensions boxDimensions)
        {
            return boxDimensions;
            //return new BoundingBoxDimensions
            //{
            //    X = (boxDimensions.X),
            //    Y = (boxDimensions.Y),
            //    Width = boxDimensions.Width,
            //    Height = boxDimensions.Height,
            //};
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
