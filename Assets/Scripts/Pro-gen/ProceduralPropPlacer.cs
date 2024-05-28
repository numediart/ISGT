using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using QuadTreeNode = Pro_gen.QuadTreeNode;
using Random = System.Random;

namespace Pro_gen
{
    public class ProceduralPropPlacer : MonoBehaviour
    {
        private RoomsGenerationScriptableObject _roomsGenerationData;
        [SerializeField] private int numberOfProps = 4;
        [SerializeField] private List<Props> _selectedProps;
        [SerializeField] private List<Bounds> _selectedPropsBounds;
        private QuadTreeNode _quadTree;
        private bool[,][,] _propsGrid;
        private int _maxAttempts = 100;
        private List<Vector3> _propsPositions;
        [SerializeField] private int _gridSubdivision = 1;

        private void Awake()
        {
            _selectedProps = new List<Props>();
            _selectedPropsBounds = new List<Bounds>();
            _propsPositions = new List<Vector3>();
        }   

        public void Init(RoomsGenerationScriptableObject roomGenerationData)
        {
            _roomsGenerationData = roomGenerationData;
            _gridSubdivision = _roomsGenerationData._gridSubdivision;
            numberOfProps = _roomsGenerationData.ObjectNumberRatio;
        }

        public void PlaceProps(Random random, int area)
        {
            if (_roomsGenerationData == null)
            {
                Debug.LogError("ProGenParams not assigned.");
                return;
            }

            Vector3 roomCenter = new Vector3(
                _roomsGenerationData.widthOffset * _roomsGenerationData.width / 2,
                _roomsGenerationData.heightOffset / 2,
                _roomsGenerationData.heightOffset * _roomsGenerationData.height / 2
            );
            Bounds roomBounds = new Bounds(
                roomCenter,
                new Vector3(
                    _roomsGenerationData.widthOffset * _roomsGenerationData.width,
                    _roomsGenerationData.heightOffset,
                    _roomsGenerationData.heightOffset * _roomsGenerationData.height
                )
            );

            _quadTree = new QuadTreeNode(roomBounds, 0);
            _quadTree.determineMaxDepth(area);
            TimeTools timeTools = new TimeTools();
            timeTools.Start();
            for (int i = 0; i < numberOfProps; i++)
            {
                Props selectedProp =
                    _roomsGenerationData.PropsPrefabs[random.Next(_roomsGenerationData.PropsPrefabs.Count)];
                Props propInstance = Instantiate(selectedProp, Vector3.zero, Quaternion.Euler(0, NextFloat(random, 0f,360f), 0), transform);
                

                Bounds propBounds = propInstance.CalculateBounds();
                
                Vector3 positionInRoom = PropsPossiblePosition(random, roomBounds, propBounds, propInstance.transform);
                
                if (positionInRoom == Vector3.zero)
                {
                    Destroy(propInstance.gameObject);
                }
                else
                {
                  
                    propInstance.transform.position = positionInRoom;
                    _quadTree.Insert(propInstance);
                    propBounds = propInstance.CalculateBounds(); // Recalculate bounds after moving
                    _selectedProps.Add(propInstance);
                    _selectedPropsBounds.Add(propBounds);
                    _propsPositions.Add(positionInRoom);
                    
                }
                
            }
            timeTools.Stop();
            Debug.Log($"{_propsPositions.Count} Props placed in " + timeTools.GetElapsedTime() + " milliseconds.");
        }


        private Vector3 PropsPossiblePosition(Random random, Bounds roomBounds, Bounds bounds, Transform propTransform)
        {
            Vector3 position = Vector3.zero;
            bool isPositionFound = false;
            Vector3 propExtents = bounds.extents;
            int attempts = 0;
            List<QuadTreeNode> biggestEmptyNodes = _quadTree.FindBiggestEmptyNodes();

            int nodeIndex = random.Next(biggestEmptyNodes.Count);

            QuadTreeNode selectedNode = biggestEmptyNodes[nodeIndex];
            
            while (!isPositionFound && attempts<_maxAttempts)
            {
                position = new Vector3(
                    NextFloat(random, selectedNode.bounds.min.x + propExtents.x,
                        selectedNode.bounds.max.x - propExtents.x),
                    GetGroundBounds().max.y - bounds.min.y, // Adjust the height to align with the ground,
                    NextFloat(random, selectedNode.bounds.min.z + propExtents.z,
                        selectedNode.bounds.max.z - propExtents.z)
                );
                propTransform.position = position;
                if (CanPropsBePlaced(propTransform, bounds))
                {
                       isPositionFound = true;
                }
                else
                {
                    attempts++;
                }
            }
            
            if (attempts == _maxAttempts)
            {
                return Vector3.zero;
            }
            return position;
        }

        private bool CanPropsBePlaced(Transform propTransform, Bounds bounds)
        {
            // Calculate the new bounds based on the desired position
            bounds.center = propTransform.position;

            // Check if the object intersects with walls (tagged "Wall")
            Collider[] intersectingWalls = Physics.OverlapBox(bounds.center, bounds.extents,propTransform.rotation);
            
            foreach (Collider wall in intersectingWalls)
            {
                if (wall.CompareTag("Wall") || wall.CompareTag("SimObjPhysics") || wall.CompareTag("Door")) 
                {
                    Debug.Log("Collided with" + wall.tag);
                    return false;
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

        private float NextFloat(Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
        
        public List<Vector3> GetPropsPositions()
        {
            return _propsPositions;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 roomCenter = new Vector3(_roomsGenerationData.widthOffset * _roomsGenerationData.width / 2,
                _roomsGenerationData.heightOffset / 2,
                _roomsGenerationData.heightOffset * _roomsGenerationData.height / 2);
            Gizmos.DrawWireCube(roomCenter,
                new Vector3(_roomsGenerationData.widthOffset * _roomsGenerationData.width,
                    _roomsGenerationData.heightOffset,
                    _roomsGenerationData.heightOffset * _roomsGenerationData.height));
        
            Bounds groundBounds = GetGroundBounds();
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundBounds.center, groundBounds.size);
        
            foreach (Props props in _selectedProps)
            {
                Bounds bounds = props.CalculateBounds();
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        
        
            if (_quadTree != null)
            {
                _quadTree.DrawGizmo();
            }
        }
    }
}