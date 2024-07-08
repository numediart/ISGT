using UnityEngine;

namespace Utils
{
    public class TimeTools
    {
        private int _startTime;
        private int _endTime;

        public void Start()
        {
            _startTime = System.Environment.TickCount;
        }

        public void Stop()
        {
            _endTime = System.Environment.TickCount;
        }

        public int GetElapsedTime()
        {
            Stop();
            return _endTime - _startTime;
        }

        public float GetElapsedTimeInSeconds()
        {
            return (float)GetElapsedTime() / 1000;
        }

        public void PrintElapsedTime()
        {
            Debug.Log("Elapsed time: " + GetElapsedTime() + " ms");
        }

        public void GetFormattedElapsedTime()
        {
            int elapsedTime = GetElapsedTime();
            int hours = elapsedTime / 3600000;
            int minutes = (elapsedTime % 3600000) / 60000;
            int seconds = ((elapsedTime % 3600000) % 60000) / 1000;
            Debug.Log("Elapsed time: " + hours + "h " + minutes + "m " + seconds + "s");
        }

        public string GetStringFormattedElapsedTime()
        {
            int elapsedTime = GetElapsedTime();
            InGameMenuController.ElapsedTime = elapsedTime;
            int hours = elapsedTime / 3600000;
            int minutes = (elapsedTime % 3600000) / 60000;
            int seconds = ((elapsedTime % 3600000) % 60000) / 1000;
            return hours + "h " + minutes + "m " + seconds + "s";
        }
    }
}