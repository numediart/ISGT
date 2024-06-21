using System.IO;
using System.Numerics;
using UnityEngine;

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
        
        // Camera settings
        public Vector3Int MaxRotation;
        
        public int FieldOfView;
        public int ISO;
        public float Aperture;
        public float FocusDistance;
        
        // Precision settings
        public bool RaycastManualInput;
        public int RaycastAmount;
        
        public string ExportPath; // has to be a folder path
        
        public PresetData(bool sizeManualInput, bool propsManualInput, bool windowsManualInput, bool doorsManualInput, int maxWidth, int maxDepth, int propsRatio, int windowRatio, int doorRatio, int screenshotsCountPerRoom, int numberOfRoomsToGenerate, string exportPath, int fieldOfView, int iso, float aperture, float focusDistance, Vector3Int maxRotation, bool raycastManualInput,int raycastAmount)
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
            
            FieldOfView = fieldOfView;
            ISO = iso;
            Aperture = aperture;
            FocusDistance = focusDistance;
            
            MaxRotation = maxRotation;
            
            RaycastManualInput = raycastManualInput;
            RaycastAmount = raycastAmount;
        }
        
    }
}