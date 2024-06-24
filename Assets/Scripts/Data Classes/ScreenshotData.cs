using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

public class ScreenshotData
{
    public List<OpeningData> OpeningsData = new List<OpeningData>();
    [JsonConverter(typeof(QuaternionConverter))]
    public UnityEngine.Quaternion CameraRotation;
}
