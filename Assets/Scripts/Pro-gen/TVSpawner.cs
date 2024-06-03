using System.Collections.Generic;
using UnityEngine;

namespace Pro_gen
{
    public class TVSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _TVPrefabs;
        [SerializeField] private GameObject _SpawnPoint;
        
        private void Start()
        {
            SpawnTV();
        }
        
        private void SpawnTV()
        {
            int randomIndex = Random.Range(0, _TVPrefabs.Count + 1);
            
            if (randomIndex == _TVPrefabs.Count)
            {
                return;
            }
            
            GameObject TV = Instantiate(_TVPrefabs[randomIndex], _SpawnPoint.transform.position, Quaternion.identity);
            
            //Make the TV go up by half of its height
            TV.transform.position += new Vector3(0, TV.transform.localScale.y / 2, 0);
            
            //Make TV rotation equal to the forward of this
            TV.transform.rotation = transform.rotation;
            
            //Make the TV a child of the spawn point
            TV.transform.parent = _SpawnPoint.transform;
        }
        
        
    }
}