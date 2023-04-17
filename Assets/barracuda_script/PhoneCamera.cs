using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class PhoneCamera : MonoBehaviour
{

    private bool isCamera;
    private WebCamTexture cameraTexture;

    public Color colorTag1 = new Color(0.3843137f, 0, 0.9333333f, 1.0f);
    public Color colorTag2 = new Color(0.3843137f, 0, 0.9333333f, 1.0f);
    public Color colorTag3 = new Color(0.3843137f, 0, 0.9333333f, 1.0f);
    Color[] colorArray = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };

    public float DetectInterval = 1.0F;
    //public RawImage bckg;
    //public AspectRatioFitter fit;

    public Camera arCamera;
    public string dirPath;
    public Yolov4Detector yolov4Detector;
    public Yolov8Detector yolov8Detector;
    public GameObject boxContainer;
    public GameObject boxPrefab;
    public TextAsset testImage;

    public bool IsInference = false;
    public bool IsTestImage = false;
    public bool IsYoloV8 = true;
    public bool IsArCamera = true;
    public bool IsStorePhoto = false;
    public bool FixAspectRatio = true;
    public int width = 640;

    [SerializeField] public Text DebugText;

    [SerializeField] public List<Tuple<Ray, string, float>> SceneItemsLocations;
    [SerializeField] public List<GameObject> RecognitionBoxes;

    public int WINDOW_SIZE = 416;

    private int framesCount = 0;
    private float timeCount = 0.0f;
    private float refreshTime = 1.0f;
    private Detector detector;
    public float fps = 0.0f;
    struct Label
    {
        public string text;
        public Rect rect;
    }

    private RectTransform relative;
    private RectTransform rootCanvas;
    private float interval = 0.0F;

    List<Label> labels = new List<Label>();
    GUIStyle style;

    // Start is called before the first frame update
    void Start()
    {
        if(IsInference)StartInference();
    }

    public void StartInference() {
        Debug.Log("Start Inference called");
        dirPath = Application.dataPath + "/../SaveImages/";

        if (!IsArCamera)
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            if (devices.Length == 0)
            {
                isCamera = false;
                return;
            }

            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                    cameraTexture = new WebCamTexture(devices[i].name, 1080, 640);
            }

            if (cameraTexture == null)
            {
                if (devices.Length != 0)
                    cameraTexture = new WebCamTexture(devices[0].name, 1080, 640);
                else
                {
                    isCamera = false;
                    return;
                }

            }
            isCamera = true;
            cameraTexture.Play();
        }
        //Time.fixedDeltaTime = 0.02F;
        detector = IsYoloV8 ? yolov8Detector : yolov4Detector;
        width = detector.RequiredWidth;

        SceneItemsLocations = new List<Tuple<Ray, string, float>>();
        RecognitionBoxes = new List<GameObject>();

        
        //float ratio = ((RectTransform)background.transform).rect.width / 640;
        //boxContainer.transform.localScale = new Vector2(ratio, ratio);


        //var box = new BoundingBox();
        //box.Dimensions = new BoundingBoxDimensions
        //{
        //    X = 0,
        //    Y = 0,
        //    Width = width,
        //    Height = width,
        //};
        //box.Confidence = 0.9F;
        //box.Label = "cup";
        //
        //spawnBoundingBox(box);
        IsInference = true;
    }
    public void StopInference() {
        IsInference = false;
        foreach (var box in RecognitionBoxes)
        {
            Destroy(box);
        }
        RecognitionBoxes.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCamera&&!IsArCamera)
            return;
        interval += Time.deltaTime;
        if (IsInference)
        {
            if (interval > DetectInterval)
            {
                CaptureCurrentReality();
                DebugText.text = "Finish model inference, Blue box detection";
                interval -= DetectInterval;
            }
        }
    }

    public void CaptureCurrentReality()
    {
        
        DebugText.text = "start model inference, DO NOT move the camera!!";
        Rect rect;
        int requiredWidth;

        var cameraView = getCameraView();
        StartCoroutine(
            detector.Detect(IsArCamera ? SceenshotArCamera(out rect, out requiredWidth) : SceenshotWebCamera(out rect, out requiredWidth), requiredWidth, rect,  boxes =>
        {
            Resources.UnloadUnusedAssets();

            foreach (Transform child in boxContainer.transform)
            {
                Destroy(child.gameObject);
            }

            Debug.Log("Object reconition: " + boxes.Count);

            for (int i = 0; i < boxes.Count; i++)
            {
                //PhoneCamera.RecognitionBoxes.Add(spawnBoundingBox(boxes[i]));
                //PhoneCamera.SceneItemsLocations.Add(spawnRay(boxes[i], cameraView));
                RecognitionBoxes.Add(spawnBoundingBox(boxes[i]));
                SceneItemsLocations.Add(new Tuple<Ray, string, float>(spawnRay(boxes[i], cameraView), boxes[i].Label, boxes[i].Confidence));
            }
        }));
    }

    private Color32[] SceenshotArCamera(out Rect rect, out int requiredWidth) {

        RenderTexture rt = new RenderTexture(arCamera.pixelWidth, arCamera.pixelHeight, 24);
        arCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(arCamera.pixelWidth, FixAspectRatio ? arCamera.pixelWidth : arCamera.pixelHeight, TextureFormat.RGB24, false);
        arCamera.Render();

        RenderTexture.active = rt;
        int offset = (arCamera.pixelHeight - arCamera.pixelWidth) / 2;

        if (FixAspectRatio)
        {
            screenShot.ReadPixels(new Rect(0, offset, arCamera.pixelWidth, arCamera.pixelWidth + offset), 0, 0);
        }
        else
        {
            screenShot.ReadPixels(new Rect(0, 0, arCamera.pixelWidth, arCamera.pixelHeight), 0, 0); 
        }

        arCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);

        //screenShot.LoadImage(imageAsset.bytes);

        if(IsStorePhoto) storePNG(screenShot, "Image1.png");

        //Rect texR = new Rect(0, 0, width, width);
        //_gpu_scale(screenShot, width, width, FilterMode.Trilinear);
        //
        ////Get rendered data back to a new texture
        //Texture2D result = new Texture2D(width, width, TextureFormat.RGB24, true);
        //result.ReadPixels(texR, 0, 0, true);
        //
        //if (IsStorePhoto) storePNG(result, "Image2.png");

        rect = new Rect();

        if (!FixAspectRatio)
        {
            rect.xMin = 0;
            rect.yMin = 0;

            rect.xMax = arCamera.pixelWidth - 1;
            rect.yMax = arCamera.pixelHeight - 1;
        }
        else {
            rect.xMin = 0;
            rect.yMin = 0;

            rect.xMax = arCamera.pixelWidth - 1;
            rect.yMax = arCamera.pixelWidth - 1;
        }

        requiredWidth = arCamera.pixelWidth;

        var temp = screenShot.GetPixels32();

        return temp;
    }

    private Color32[] SceenshotWebCamera(out Rect rect, out int requiredWidth)
    {

        
        if (IsStorePhoto) {
            Texture2D result = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.RGB24, true);
            result.SetPixels(cameraTexture.GetPixels());
            result.Apply();
            storePNG(result, "Image3.png"); 
        }

        rect = new Rect();

        if (!FixAspectRatio)
        {
            rect.xMin = 0;
            rect.yMin = 0;

            rect.xMax = cameraTexture.width - 1;
            rect.yMax = cameraTexture.height - 1;
        }
        else
        {
            int offset = (cameraTexture.height - cameraTexture.width) / 2;
            rect.xMin = 0;
            rect.yMin = offset;

            rect.xMax = cameraTexture.width - 1;
            rect.yMax = cameraTexture.height + offset - 1;
        }

        requiredWidth = cameraTexture.requestedWidth;

        return cameraTexture.GetPixels32();
    }

    private void storePNG(Texture2D texture2D, string filename) {

        byte[] bytes2 = texture2D.EncodeToPNG();
        if (!Directory.Exists(dirPath))
        {
            print(dirPath);
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + filename, bytes2);
    }

    void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
    {
        //We need the source texture in VRAM because we render with it
        src.filterMode = fmode;
        src.Apply(true);

        //Using RTT for best quality and performance. Thanks, Unity 5
        RenderTexture rtt = new RenderTexture(width, height, 32);

        //Set the RTT in order to render to it
        Graphics.SetRenderTarget(rtt);

        //Setup 2D matrix in range 0..1, so nobody needs to care about sized
        GL.LoadPixelMatrix(0, 1, 1, 0);

        //Then clear & draw the texture to fill the entire RTT.
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
    }

    private GameObject spawnBoundingBox(BoundingBox box) {
        GameObject newBox = Instantiate(boxPrefab);
        newBox.name = box.Label + " " + box.Confidence;
        newBox.GetComponent<Image>().color = new Color(0.3843137f, 0, 0.9333333f, 1.0f);
        newBox.transform.parent = boxContainer.transform;

        var org_h = boxContainer.GetComponent<RectTransform>().rect.height;
        var org_w = boxContainer.GetComponent<RectTransform>().rect.width;

        Debug.Log("Object reconition: " + box.Label + " " + box.Confidence);

        if (!FixAspectRatio)
        {
            Debug.Log("Object reconition: " + box.Rect.x + " " + box.Rect.y);
            newBox.transform.localPosition = new Vector3(box.Rect.x * org_w / width, org_h - (box.Rect.y * org_h / width));
            newBox.transform.localScale = new Vector2(box.Rect.width * org_w / 100 / width, box.Rect.height * org_h / 100 / width);
            Debug.Log("Object reconition: " + (box.Rect.x * org_w / width) + " " + (org_h - (box.Rect.y * org_h / width)));
        }
        else
        {
            float offset = (org_h - org_w) / 2.0F;
            newBox.transform.localPosition = new Vector3(box.Rect.x * org_w / width, org_h - (box.Rect.y * org_w / width + offset));
            newBox.transform.localScale = new Vector2(box.Rect.width * org_w / 100 / width, box.Rect.height * org_w / 100 / width);
        }

        return newBox;
    }

    private Ray spawnRay(BoundingBox box, List<Vector3> cameraView) {
        float w, h;
        w = box.Rect.x / width;
        h = 1.0F - box.Rect.y / width;

        var ray = new Ray(cameraView[1] + w * cameraView[3] + h * cameraView[5], (cameraView[0] + w * cameraView[2] + h * cameraView[4]).normalized);
        return ray;
    }

    private List<Vector3> getCameraView() {
        Ray r1, r2, r3;
        if (!FixAspectRatio)
        {
            r1 = arCamera.ScreenPointToRay(new Vector2(0, 0));
            r2 = arCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height));
            r3 = arCamera.ScreenPointToRay(new Vector2(Screen.width, 0));
        }
        else {
            int offset = (Screen.height - Screen.width) / 2;
            r1 = arCamera.ScreenPointToRay(new Vector2(0, offset));
            r2 = arCamera.ScreenPointToRay(new Vector2(Screen.width, offset + Screen.width));
            r3 = arCamera.ScreenPointToRay(new Vector2(Screen.width, offset));
        }

        var cameraView = new List<Vector3>();

        cameraView.Add(r1.direction);                     //direction of r1
        cameraView.Add(r1.origin);                        //origin of r1
        cameraView.Add(r3.direction - r1.direction);      //width vector of direction
        cameraView.Add(r3.origin - r1.origin);            //width vector of origin
        cameraView.Add(r2.direction - r3.direction);      //height vector of direction
        cameraView.Add(r2.origin - r3.origin);            //height vector of origin

        return cameraView;
    }



}
