using UnityEngine;

[ExecuteInEditMode]
public class CamrotationTest : MonoBehaviour
{
    public GameObject target;

    public void RotateCamera()
    {
        // Calculate the rotation quaternion from the camera to the target.
        Quaternion quaternion = Quaternion.LookRotation(target.transform.position - Camera.main.transform.position);
        Camera.main.transform.rotation = quaternion;
    }
}