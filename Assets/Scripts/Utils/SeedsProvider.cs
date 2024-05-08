using System;

namespace Utils
{
    public class SeedsProvider
    {
        private int _mainSeed;
        private Random _random;


        public SeedsProvider()
        {
            _mainSeed = (int)DateTime.Now.Ticks;
            _random = new Random(_mainSeed);
        }

        public int GetSeed()
        {
            return _mainSeed;
        }

        public int CreateSubSeed()
        {
            return _random.Next();
        }
    }
}