namespace Data_Classes
{
    public class CameraData
    {
        public float FieldOfView;
        public float NearClipPlane;
        public float FarClipPlane;
        public float ViewportRectX;
        public float ViewportRectY;
        public int ViewportRectWidth;
        public int ViewportRectHeight;
        public float Depth;
        public bool IsOrthographic;


        public CameraData(float fieldOfView, float nearClipPlane, float farClipPlane, float viewportRectX,
            float viewportRectY, int viewportRectWidth, int viewportRectHeight, float depth, bool isOrthographic)
        {
            FieldOfView = fieldOfView;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
            ViewportRectX = viewportRectX;
            ViewportRectY = viewportRectY;
            ViewportRectWidth = viewportRectWidth;
            ViewportRectHeight = viewportRectHeight;
            Depth = depth;
            IsOrthographic = isOrthographic;
        }
    }
}