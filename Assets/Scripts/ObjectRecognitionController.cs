using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;
using UnityEngine.UI;

public class ObjectRecognitionController : MonoBehaviour
{
    [Header("Controller Reference")] 
    public ARController ARCtrl;
    public GameFlowController GameFlowCtrl;
    public Camera _arCamera;
    public IGameboard _gameboard;
    
    [Header("Object Recognition")]
    public GameObject PlacementObjectPf;
    public GameObject boxContainer;
    public GameObject boxPrefab;
    public Yolov5Detector yolov5Detector;
    /// A list of placed game objects to be destroyed in the OnDestroy method.
    private List<GameObject> _placedObjects = new List<GameObject>();
    
    [Header("Debug and check status")]
    [SerializeField] public Text DebugText;
    [SerializeField] private bool IsCapturingEnv = false;

    public void CaptureCurrentReality()
    {
        if (IsCapturingEnv) return;
        IsCapturingEnv = true;
        RenderTexture rt = new RenderTexture(_arCamera.pixelWidth, _arCamera.pixelHeight, 24);
        _arCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(_arCamera.pixelWidth, _arCamera.pixelHeight, TextureFormat.RGB24, false);
        _arCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, _arCamera.pixelWidth, _arCamera.pixelHeight), 0, 0);
        _arCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        
        //byte[] bytes = screenShot.EncodeToPNG();
        //var dirPath = Application.dataPath + "/../SaveImages/";
        //if (!Directory.Exists(dirPath))
        //{
        //    Directory.CreateDirectory(dirPath);
        //}
        //File.WriteAllBytes(dirPath + "Image1" + ".png", bytes);

        Rect texR = new Rect(0, 0, 416, 416);
        _gpu_scale(screenShot, 416, 416, FilterMode.Trilinear);

        //Get rendered data back to a new texture
        Texture2D result = new Texture2D(416, 416, TextureFormat.RGB24, true);
        result.Reinitialize(416, 416);
        result.ReadPixels(texR, 0, 0, true);

        var dirPath = Application.dataPath + "/../SaveImages/";
        byte[] bytes = result.EncodeToPNG();
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image2" + ".png", bytes);

        DebugText.text = "start model inference";
        StartCoroutine(yolov5Detector.Detect(result.GetPixels32(), 416, boxes =>
        {
            Resources.UnloadUnusedAssets();

            foreach (Transform child in boxContainer.transform)
            {
                Destroy(child.gameObject);
            }
            Debug.Log("3");
            DebugText.text = "finish model inference: " + boxes.Count;
            for (int i = 0; i < boxes.Count; i++)
            {
                GameObject newBox = Instantiate(boxPrefab);
                newBox.name = boxes[i].Label + " " + boxes[i].Confidence;
                newBox.GetComponent<Image>().color = new Color(0.3843137f, 0, 0.9333333f, 1.0f);
                newBox.transform.parent = boxContainer.transform;

                var pred_x = boxes[i].Rect.x;
                var pred_y = boxes[i].Rect.y;



                var org_h = boxContainer.GetComponent<RectTransform>().rect.height;
                var org_w = boxContainer.GetComponent<RectTransform>().rect.width;

                Debug.Log(boxes[i].Label + "  x: " + boxes[i].Rect.x + "y: " + boxes[i].Rect.y + "w: " + boxes[i].Rect.width + "h: " + boxes[i].Rect.height
                    + new Vector3(boxes[i].Rect.x * org_w / 416, org_h - (boxes[i].Rect.y * org_h / 416) - 0.5f * org_h)
                    + new Vector2(boxes[i].Rect.width * org_w / 100 / 416, boxes[i].Rect.height * org_h / 100 / 416));

                newBox.transform.localPosition = new Vector3(boxes[i].Rect.x * org_w / 416, org_h - (boxes[i].Rect.y * org_h / 416) - 0.5f * org_h);
                newBox.transform.localScale = new Vector2(boxes[i].Rect.width * org_w / 100 / 416, boxes[i].Rect.height * org_h / 100 / 416);

                SpawnSceneItem(new Vector2(boxes[i].Rect.x * Screen.width / 416, Screen.height - boxes[i].Rect.y * Screen.height / 416));
            }

        }));

        IsCapturingEnv = false;
    }
    
    public void SpawnSceneItem(Vector2 position) {
        var ray = _arCamera.ScreenPointToRay(position);
        var hitPoint = new Vector3();

        var b = _gameboard.RayCast(ray, out hitPoint);      // Null Object reference 
        // Intersect the Gameboard with the ray
        if (b)
        {
            // Check whether the object can be fit in the resulting position
            if (_gameboard.CheckFit(center: hitPoint, 0.1f))
            {
                var landscape = Instantiate(PlacementObjectPf, hitPoint, Quaternion.identity);
                landscape.transform.localScale = new Vector3(3, 3, 3);
                _placedObjects.Add(landscape);
            }
        }
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
}
