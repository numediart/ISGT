
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
            return _endTime - _startTime;
        }
        public void PrintElapsedTime()
        {
            Debug.Log("Elapsed time: " + GetElapsedTime() + " ms");
        }
        
    }
}