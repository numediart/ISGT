using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Pro_gen
{
    public class ProceduralPropPlacer : MonoBehaviour
    {
        [SerializeField] private ProGenParams _proGenParams;
        [SerializeField] private int numberOfProps = 4;
        [SerializeField] private List<Props> _selectedProps;
        [SerializeField] private List<Bounds> _selectedPropsBounds;
         private bool[,] _propsGrid;

         private void Awake()
         {
             _propsGrid = new bool[ Mathf.CeilToInt(_proGenParams.width*_proGenParams.widthOffset), Mathf.CeilToInt(_proGenParams.height*_proGenParams.heightOffset)];
         }

         public void PlaceProps(System.Random random)
    {
        if (_proGenParams == null)
        {
            Debug.LogError("ProGenParams not assigned.");
            return;
        }

        // Define the bounds of the room
        Vector3 roomCenter = new Vector3(
            _proGenParams.widthOffset * _proGenParams.width / 2,
            _proGenParams.heightOffset / 2,
            _proGenParams.heightOffset * _proGenParams.height / 2
        );
        Bounds roomBounds = new Bounds(
            roomCenter,
            new Vector3(
                _proGenParams.widthOffset * _proGenParams.width,
                _proGenParams.heightOffset,
                _proGenParams.heightOffset * _proGenParams.height
            )
        );

        for (int i = 0; i < numberOfProps; i++)
        {
            // Select a random prop
            Props selectedProp = _proGenParams.PropsPrefabs[random.Next(_proGenParams.PropsPrefabs.Count)];
            Props propInstance = Instantiate(selectedProp, Vector3.zero, Quaternion.identity, transform);
            _selectedProps.Add(propInstance);
            
            // Get the bounds of the prop
            Bounds propBounds = CalculateBounds(propInstance);
            _selectedPropsBounds.Add(propBounds);
              Vector3 positionInRoom=  PropsPossiblePosition(random, roomBounds, propBounds);
            
            
            // Generate a random position within the room bounds, adjusted for prop size
            propInstance.transform.position = positionInRoom;
            propBounds = CalculateBounds(propInstance); // Recalculate bounds after moving
            PopulatePropsGridWithProps();
            _selectedPropsBounds[i] = propBounds;
           /* if (!CanPropsBePlaced(positionInRoom, propBounds))
            {
                  Destroy(propInstance.gameObject);
                    _selectedProps.RemoveAt(i);
                    _selectedPropsBounds.RemoveAt(i);
                    i--;       
            }*/
           // Check for overlaps with already placed props
        }
    }
         
         private Vector3 PropsPossiblePosition(Random random, Bounds roomBounds, Bounds bounds)
         {
             Vector3 position = Vector3.zero;
             bool isPositionFound = false;
                Vector3 propExtents = bounds.extents;
             while (!isPositionFound)
             {
                 position = new Vector3(
                     NextFloat(random, roomBounds.min.x + propExtents.x, roomBounds.max.x - propExtents.x),
                     GetGroundBounds().max.y - bounds.min.y, // Adjust the height to align with the ground
                     NextFloat(random, roomBounds.min.z + propExtents.z, roomBounds.max.z - propExtents.z)
                 );

                 if (CanPropsBePlaced(position, bounds))
                 {
                     isPositionFound = true;
                 }
             }

             return position;
         }
        private bool CanPropsBePlaced(Vector3 position, Bounds bounds)
        {
            int x = Mathf.FloorToInt(position.x / _proGenParams.widthOffset);
            int z = Mathf.FloorToInt(position.z / _proGenParams.heightOffset);
            int width = Mathf.CeilToInt(bounds.extents.x / _proGenParams.widthOffset);
            int height = Mathf.CeilToInt(bounds.extents.z / _proGenParams.heightOffset);
            for (int k = x; k < x + width; k++)
            {
                for (int l = z; l < z + height; l++)
                {
                    if (_propsGrid[k, l])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
     
        private Bounds GetGroundBounds()
        {
            Bounds groundBounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer.CompareTag("Ground"))
                {
                    groundBounds.Encapsulate(renderer.bounds);
                }
            }

            return groundBounds;
        }

        Bounds CalculateBounds(Props props)
        {
            Bounds combinedBounds = new Bounds(props.transform.position, Vector3.zero);

            
            // Iterate through all renderers of the object and its children
            foreach (Collider collider in props.GetComponentsInChildren<Collider>())
            {
                combinedBounds.Encapsulate(collider.bounds);
            }

            return combinedBounds;
        }

        private float NextFloat(System.Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
        
        private void PopulatePropsGridWithProps()
        {
            foreach (Props props in _selectedProps)
            {
                Bounds bounds = CalculateBounds(props);
                // Get Cell position of the prop
                Vector3 position = props.transform.position;
                Vector3 extents = bounds.extents;
                int x = Mathf.FloorToInt(position.x / _proGenParams.widthOffset);
                int z = Mathf.FloorToInt(position.z / _proGenParams.heightOffset);
                int width = Mathf.CeilToInt(extents.x / _proGenParams.widthOffset);
                int height = Mathf.CeilToInt(extents.z / _proGenParams.heightOffset);
                for (int i = x; i < x + width; i++)
                {
                    for (int j = z; j < z + height; j++)
                    {
                        _propsGrid[i, j] = true;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 roomCenter = new Vector3(_proGenParams.widthOffset * _proGenParams.width / 2,
                _proGenParams.heightOffset / 2,
                _proGenParams.heightOffset * _proGenParams.height / 2);
            Gizmos.DrawWireCube(roomCenter,
                new Vector3(_proGenParams.widthOffset * _proGenParams.width, _proGenParams.heightOffset,
                    _proGenParams.heightOffset * _proGenParams.height));

            // draw the ground bounds
            Bounds groundBounds = GetGroundBounds();
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundBounds.center, groundBounds.size);


            // draw the props bounds
            foreach (Props props in _selectedProps)
            {
                Bounds bounds = CalculateBounds(props);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}