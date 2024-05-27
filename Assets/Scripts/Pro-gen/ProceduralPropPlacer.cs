using System;
using System.Collections.Generic;
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
         
        [SerializeField] private int _gridSubdivision = 1;

        private void Awake()
        {
            int gridWidth = Mathf.CeilToInt(_proGenParams.width * _gridSubdivision);
            int gridHeight = Mathf.CeilToInt(_proGenParams.height * _gridSubdivision);
            _propsGrid = new bool[gridWidth, gridHeight];
            
        }

        public void PlaceProps(System.Random random)
        {
            if (_proGenParams == null)
            {
                Debug.LogError("ProGenParams not assigned.");
                return;
            }

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
                Props selectedProp = _proGenParams.PropsPrefabs[random.Next(_proGenParams.PropsPrefabs.Count)];
                Props propInstance = Instantiate(selectedProp, Vector3.zero, Quaternion.identity, transform);
                _selectedProps.Add(propInstance);

                Bounds propBounds = CalculateBounds(propInstance);
                _selectedPropsBounds.Add(propBounds);
                Vector3 positionInRoom = PropsPossiblePosition(random, roomBounds, propBounds);

                propInstance.transform.position = positionInRoom;
                propBounds = CalculateBounds(propInstance); // Recalculate bounds after moving
                PopulatePropsGridWithProps();
                _selectedPropsBounds[i] = propBounds;
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
            int gridX = Mathf.FloorToInt(position.x * _gridSubdivision / _proGenParams.width);
            int gridZ = Mathf.FloorToInt(position.z * _gridSubdivision / _proGenParams.height);
            int propWidth = Mathf.CeilToInt(bounds.extents.x * _gridSubdivision / _proGenParams.width);
            int propHeight = Mathf.CeilToInt(bounds.extents.z * _gridSubdivision / _proGenParams.height);

            for (int x = gridX; x < gridX + propWidth; x++)
            {
                for (int z = gridZ; z < gridZ + propHeight; z++)
                {
                    if (x < 0 || x >= _propsGrid.GetLength(0) || z < 0 || z >= _propsGrid.GetLength(1) || _propsGrid[x, z])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void PopulatePropsGridWithProps()
        {
            foreach (Props props in _selectedProps)
            {
                Bounds bounds = CalculateBounds(props);
                Vector3 position = props.transform.position;
                Vector3 extents = bounds.extents;

                int gridX = Mathf.FloorToInt(position.x * _gridSubdivision / _proGenParams.width);
                int gridZ = Mathf.FloorToInt(position.z * _gridSubdivision / _proGenParams.height);
                int propWidth = Mathf.CeilToInt(extents.x * _gridSubdivision / _proGenParams.width);
                int propHeight = Mathf.CeilToInt(extents.z * _gridSubdivision / _proGenParams.height);

                for (int x = gridX; x < gridX + propWidth; x++)
                {
                    for (int z = gridZ; z < gridZ + propHeight; z++)
                    {
                        if (x >= 0 && x < _propsGrid.GetLength(0) && z >= 0 && z < _propsGrid.GetLength(1))
                        {
                            _propsGrid[x, z] = true;
                        }
                    }
                }
            }
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 roomCenter = new Vector3(_proGenParams.widthOffset * _proGenParams.width / 2,
                _proGenParams.heightOffset / 2,
                _proGenParams.heightOffset * _proGenParams.height / 2);
            Gizmos.DrawWireCube(roomCenter,
                new Vector3(_proGenParams.widthOffset * _proGenParams.width, _proGenParams.heightOffset,
                    _proGenParams.heightOffset * _proGenParams.height));

            Bounds groundBounds = GetGroundBounds();
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundBounds.center, groundBounds.size);

            foreach (Props props in _selectedProps)
            {
                Bounds bounds = CalculateBounds(props);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            
            //Preview grid 
            float cellWidth = _proGenParams.width / _gridSubdivision;
            
            
            
            
            // if (_propsGrid != null)
            // {
            //     
            //     float cellWidth = _proGenParams.width / _gridSubdivision;
            //     float cellHeight = _proGenParams.height / _gridSubdivision;
            //     for (int x = 0; x < _propsGrid.GetLength(0); x++)
            //     {
            //         for (int z = 0; z < _propsGrid.GetLength(1); z++)
            //         {
            //             Vector3 cellCenter = new Vector3(
            //                 x * cellWidth + cellWidth / 2,
            //                 groundBounds.max.y + 0.1f,
            //                 z * cellHeight + cellHeight / 2
            //             );
            //
            //             Gizmos.color = _propsGrid[x, z] ? Color.magenta : Color.blue;
            //             Gizmos.DrawCube(cellCenter, new Vector3(cellWidth, 0.1f, cellHeight));
            //         }
            //     }
            // }
        }
    }
}
