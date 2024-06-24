using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class QuaternionConverter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(Quaternion);
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        float x = (float)jo["x"];
        float y = (float)jo["y"];
        float z = (float)jo["z"];
        float w = (float)jo["w"];
        return new Quaternion(x, y, z, w);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Quaternion q = (Quaternion)value;
        JObject jo = new JObject();
        jo.Add("w", q.w);
        jo.Add("x", q.x);
        jo.Add("y", q.y);
        jo.Add("z", q.z);
        jo.WriteTo(writer);
    }
}