using System.IO;

namespace Data_Classes
{
    public class PresetData
    {
        public bool SizeManualInput;
        public bool PropsManualInput;
        public bool WindowsManualInput;
        public bool DoorsManualInput;
        
        public int MaxWidth;
        public int MaxDepth;

        public int PropsRatio;
        public int WindowRatio;
        public int DoorRatio;
        
        public int ScreenshotsCountPerRoom;
        public int NumberOfRoomsToGenerate;
        
        public string ExportPath; // has to be a folder path
        
        public PresetData(bool sizeManualInput, bool propsManualInput, bool windowsManualInput, bool doorsManualInput, int maxWidth, int maxDepth, int propsRatio, int windowRatio, int doorRatio, int screenshotsCountPerRoom, int numberOfRoomsToGenerate, string exportPath)
        {
            SizeManualInput = sizeManualInput;
            PropsManualInput = propsManualInput;
            WindowsManualInput = windowsManualInput;
            DoorsManualInput = doorsManualInput;
            MaxWidth = maxWidth;
            MaxDepth = maxDepth;
            PropsRatio = propsRatio;
            WindowRatio = windowRatio;
            DoorRatio = doorRatio;
            ScreenshotsCountPerRoom = screenshotsCountPerRoom;
            NumberOfRoomsToGenerate = numberOfRoomsToGenerate;
            ExportPath = exportPath;
        }
        
    }
}