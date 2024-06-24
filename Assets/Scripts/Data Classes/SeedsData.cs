namespace Data_Classes
{
    public class SeedsData
    {
        public int DatabaseSeed;
        public int RoomsSeed;
        public int OpeningsSeed;
        public int ObjectsSeed;
        
        public SeedsData(int roomsSeed, int openingsSeed, int objectsSeed, int databaseSeed)
        {
            RoomsSeed = roomsSeed;
            OpeningsSeed = openingsSeed;
            ObjectsSeed = objectsSeed;
            DatabaseSeed = databaseSeed;
        }
    }
}