using UnityEngine;

namespace Utils
{
    public class TimeTools
    {
        private int _startTime;
        private int _endTime;

        public TimeTools()
        {
            _startTime = 0;
            _endTime = 0;
        }

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
        public static string ETA(int elapsedTime, int total, int current)
        {
            int remainingTime = (int)((float)elapsedTime / current * (total - current));
            int hours = remainingTime / 3600000;
            int minutes = (remainingTime % 3600000) / 60000;
            int seconds = ((remainingTime % 3600000) % 60000) / 1000;
            return hours + "h " + minutes + "m " + seconds + "s";
        }
    }
}