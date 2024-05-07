using System;
using UnityEngine;

namespace Utils
{
    public class SeedsProvider
    {
     
        private int _mainSeed;
        public int MainSeed => _mainSeed;
        public SeedsProvider()
        {// This class is used to manage the seeds for visg
            _mainSeed = (int) DateTime.Now.Ticks;
        }
        public int GetSeed()
        {
            return _mainSeed;
        }
        
        public int CreateSubSeed()
        {
            int subSeed = DateTime.Now.Ticks.GetHashCode();
            return subSeed;
        }
        
        
    }
}