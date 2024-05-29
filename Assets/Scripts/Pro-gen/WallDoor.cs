using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDoor : MonoBehaviour
{
    [SerializeField] private ProGenParams _proGenParams;
    [SerializeField] private List<GameObject> _doorPrefabs;
    [SerializeField] private Transform _doorPosition;

    private void Awake()
    {
        int index = Random.Range(0, _doorPrefabs.Count);
        GameObject door = Instantiate(_doorPrefabs[index],
            new Vector3(_doorPosition.position.x, 0, _doorPosition.position.z), Quaternion.identity);
        door.transform.GetChild(0).tag = "Door";
        door.transform.parent = _doorPosition.parent;
        DestroyImmediate(_doorPosition.gameObject);
    }
}