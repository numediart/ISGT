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
                    SpawnChairs();
                    break;
                case TypeOfProp.Armchair:
                    SpawnArmchairs();
                    break;
            }
        }
        
        // Spawn a TV on the spawn point which is on top of TV stand
        private void SpawnTV()
        {
            int randomIndex = _random.Next(_SpawnablePrefabs.Count + 1);
            
            if (randomIndex == _SpawnablePrefabs.Count)
            {
                return;
            }
            
            GameObject TV = Instantiate(_SpawnablePrefabs[randomIndex], _SpawnPoints[0].transform.position, Quaternion.identity);
            
            //Make the TV go up by half of its height, so that it is on top of the TV stand
            TV.transform.position += new Vector3(0, TV.transform.localScale.y / 2, 0);
            
            //Make TV rotation equal to the forward of this, and add a random rotation between -6 and 6 degrees
            TV.transform.rotation = transform.rotation;
            
            float randomRotation = _random.Next(-60, 60);
            TV.transform.Rotate(0, randomRotation / 10, 0);
            
            //Make the TV a child of the spawn point
            TV.transform.parent = _SpawnPoints[0].transform;
        }
        
        // Spawn chairs on the spawn points which are around the table, and make them look at the table
        private void SpawnChairs()
        {
            foreach (var spawnPoint in _SpawnPoints)
            {
                int randomIndex = _random.Next(_SpawnablePrefabs.Count + 1);
                
                if (randomIndex != _SpawnablePrefabs.Count)
                {
                    GameObject chair = Instantiate(_SpawnablePrefabs[randomIndex], spawnPoint.transform.position,
                        Quaternion.identity);
        
                    chair.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

                    if (_SpawnPoints.Count > 4) //the table is a rectangle
                    {
                        //make the table rotation match the forward of the spawn point
                        chair.transform.rotation = spawnPoint.transform.rotation;
                    }
                    else //the table is round
                    {
                        // Make the chair look at the table horizontally
                        Vector3 lookAtPosition = new Vector3(transform.position.x, chair.transform.position.y, transform.position.z);
                        chair.transform.LookAt(lookAtPosition);
                    }
                

                    // Make the chair a child of the spawn point
                    chair.transform.parent = spawnPoint.transform;
                }
                
            }
        }
        
        // Spawn an armchair on the spawn point which is in front of the desk and make it look at it
        private void SpawnArmchairs()
        {
            int randomIndex = _random.Next(_SpawnablePrefabs.Count + 1);
            
            if (randomIndex == _SpawnablePrefabs.Count)
            {
                return;
            }
            
            GameObject chair = Instantiate(_SpawnablePrefabs[randomIndex], _SpawnPoints[0].transform.position, transform.rotation);
            
            chair.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            //Make the chair rotation equal to the opposite of the forward of this, and add a random rotation between -6 and 6 degrees
            chair.transform.Rotate(0,180, 0);
            
            
            float randomRotation = _random.Next(-60, 60);
            chair.transform.Rotate(0, randomRotation / 10, 0);
            
            //Make the chair a child of the spawn point
            chair.transform.parent = _SpawnPoints[0].transform;
        }
    }

    enum TypeOfProp
    {
        TV,
        Chair,
        Armchair,
    }
}