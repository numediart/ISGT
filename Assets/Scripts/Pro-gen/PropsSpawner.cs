using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
namespace Pro_gen
{
    public class PropsSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _SpawnablePrefabs;
        [SerializeField] private List<GameObject> _SpawnPoints;
        [SerializeField] private TypeOfProp _typeOfProp;
        private Random _random;
        
        public void Spawn(Random random)
        {
            _random = random;
            switch (_typeOfProp)
            {
                case TypeOfProp.TV:
                    SpawnTV();
                    break;
                case TypeOfProp.Chair:
                    break;
                case TypeOfProp.Armchair:
                    break;
            }
        }
        
        private void SpawnTV()
        {
            int randomIndex = _random.Next(_SpawnablePrefabs.Count + 1);
            
            Debug.Log("Random index: " + randomIndex);
            
            
            if (randomIndex == _SpawnablePrefabs.Count)
            {
                return;
            }
            
            GameObject TV = Instantiate(_SpawnablePrefabs[randomIndex], _SpawnPoints[0].transform.position, Quaternion.identity);
            
            //Make the TV go up by half of its height
            TV.transform.position += new Vector3(0, TV.transform.localScale.y / 2, 0);
            
            //Make TV rotation equal to the forward of this
            TV.transform.rotation = transform.rotation;
            
            //Make the TV a child of the spawn point
            TV.transform.parent = _SpawnPoints[0].transform;
        }
        
        
    }

    enum TypeOfProp
    {
        TV,
        Chair,
        Armchair,
    }
}