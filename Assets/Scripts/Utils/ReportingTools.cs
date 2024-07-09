using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public class ReportingTools
    {
        private int _startTime=0;
        private int _endTime=0;
        private static string _path;
        public static int reportingIndex = 0;
        public void StartTimer()
        {
            _startTime = System.Environment.TickCount;
        }

        public void EndTimer()
        {
            _endTime = Environment.TickCount;
            Debug.Log("Elapsed time: " + (_endTime - _startTime) + " ms");
        }

        public int GetElapsedTime()
        {
            Debug.Log("Elapsed time: " + (_endTime - _startTime) + " ms");
            return  (_endTime - _startTime);
        }

        public static void AppendInJson(int time, string label)
        {
            _path = Application.dataPath + "/reporting"+reportingIndex+".json";
            string data = "";
            if (File.Exists(_path))
            {
                data = System.IO.File.ReadAllText(_path);
                if (data == "")
                {
                    data = JsonConvert.SerializeObject(Array.Empty<object>());
                }
            }
            else
            {
                System.IO.File.WriteAllText(_path, JsonConvert.SerializeObject(Array.Empty<object>()));
            }

            object[] reportingData = JsonConvert.DeserializeObject<object[]>(data);
            if (reportingData == null)
            {
                reportingData = Array.Empty<object>();
            }
            System.Collections.Generic.List<object> reportingDataList =
                new System.Collections.Generic.List<object>(reportingData) { new { time = time, label = label } };
            string json = JsonConvert.SerializeObject(reportingDataList.ToArray());
            System.IO.File.WriteAllText(_path, json);
        }
    }
}