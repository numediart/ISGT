using System.Collections;
using UnityEngine;
using System.IO;

public class CameraScreenshot : MonoBehaviour
{
    public Camera cameraToCapture;
    public  int imageWidth;
    public  int imageHeight;
    public string savePath = "";

    void Start()
    {
        if (!Directory.Exists(savePath) && savePath.Contains(MainMenuController.PresetData.ExportPath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        imageWidth = 640 * MainMenuController.PresetData.Resolution;
        imageHeight = 360 * MainMenuController.PresetData.Resolution;
    }

    public void CaptureScreenshot()
    {
        StartCoroutine(Capture());
    }

    private IEnumerator Capture()
    {
        // Créez un RenderTexture et l'associez à la caméra
        RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
        cameraToCapture.targetTexture = rt;

        // Créez une texture pour sauvegarder l'image
        Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        cameraToCapture.Render();

        // Activez le RenderTexture
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        screenShot.Apply();

        // Réinitialisez la caméra et RenderTexture
        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Sauvegardez l'image dans un fichier
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = $"{savePath}";
        File.WriteAllBytes(filename, bytes);
        yield return null;
    }
}