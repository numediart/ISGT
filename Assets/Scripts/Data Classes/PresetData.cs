using System.IO;
using System.Numerics;
using UnityEngine;

namespace Data_Classes
{
    public class PresetData
    {
        // Room settings
        public int MaxWidth;
        public int MaxDepth;

        public int PropsRatio;
        public int WindowRatio;
        public int DoorRatio;
        
        // Generation settings
        public int ScreenshotsCountPerRoom;
        public int NumberOfRoomsToGenerate;
        
        // Camera settings
        public Vector3Int MaxRotation;
        
        public int FieldOfView;
        public int ISO;
        public float Aperture;
        public float FocusDistance;
        
        // Precision settings
        public int RaycastAmount;
        public int Resolution;
        
        public string ExportPath; // has to be a folder path
        
        public PresetData(int maxWidth, int maxDepth, int propsRatio, int windowRatio, int doorRatio, int screenshotsCountPerRoom, int numberOfRoomsToGenerate, string exportPath, int fieldOfView, int iso, float aperture, float focusDistance, Vector3Int maxRotation,int raycastAmount, int resolution)
        {
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
            RaycastAmount = raycastAmount;
            Resolution = resolution;
        }
    }
}