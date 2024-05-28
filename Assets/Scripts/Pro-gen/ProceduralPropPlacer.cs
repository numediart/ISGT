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

        [SerializeField] private int _gridSubdivision = 1;

        private void Awake()
        {
            _selectedProps = new List<Props>();
            _selectedPropsBounds = new List<Bounds>();
        }   

        public void Init(RoomsGenerationScriptableObject roomGenerationData)
        {
            _roomsGenerationData = roomGenerationData;
            _gridSubdivision = _roomsGenerationData._gridSubdivision;
            int gridWidth = Mathf.CeilToInt(_roomsGenerationData.width * _gridSubdivision);
            int gridHeight = Mathf.CeilToInt(_roomsGenerationData.height * _gridSubdivision);
            int gridWidthOffset = Mathf.CeilToInt(_roomsGenerationData.widthOffset * _gridSubdivision);
            int gridHeightOffset = Mathf.CeilToInt(_roomsGenerationData.heightOffset * _gridSubdivision);

            numberOfProps = _roomsGenerationData.ObjectNumberRatio;
        }

        public void PlaceProps(Random random)
        {
            float startTime = Time.time;
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

            for (int i = 0; i < numberOfProps; i++)
            {
                Props selectedProp =
                    _roomsGenerationData.PropsPrefabs[random.Next(_roomsGenerationData.PropsPrefabs.Count)];
                Props propInstance = Instantiate(selectedProp, Vector3.zero, Quaternion.identity, transform);
                _selectedProps.Add(propInstance);

                Physics.SyncTransforms(); // Force collider update

                Bounds propBounds = propInstance.CalculateBounds();
                _selectedPropsBounds.Add(propBounds);
                Vector3 positionInRoom = PropsPossiblePosition(random, roomBounds, propBounds);

                propInstance.transform.position = positionInRoom;
                _quadTree.Insert(propInstance);
                propBounds = propInstance.CalculateBounds(); // Recalculate bounds after moving
                _selectedPropsBounds[i] = propBounds;
            }

            Debug.Log("Props placed in " + (Time.time - startTime) + " seconds.");
        }


        private Vector3 PropsPossiblePosition(Random random, Bounds roomBounds, Bounds bounds)
        {
            Vector3 position = Vector3.zero;
            bool isPositionFound = false;
            Vector3 propExtents = bounds.extents;

            List<QuadTreeNode> biggestEmptyNodes = _quadTree.FindBiggestEmptyNodes();

            int nodeIndex = random.Next(biggestEmptyNodes.Count);

            QuadTreeNode selectedNode = biggestEmptyNodes[nodeIndex];

            while (!isPositionFound)
            {
                position = new Vector3(
                    NextFloat(random, selectedNode.bounds.min.x + propExtents.x,
                        selectedNode.bounds.max.x - propExtents.x),
                    GetGroundBounds().max.y - bounds.min.y, // Adjust the height to align with the ground,
                    NextFloat(random, selectedNode.bounds.min.z + propExtents.z,
                        selectedNode.bounds.max.z - propExtents.z)
                );

                isPositionFound = true;
                /*   if (CanPropsBePlaced(position, bounds))
                   {
                       isPositionFound = true;
                   }*/
            }

            return position;
        }

        private bool CanPropsBePlaced(Vector3 position, Bounds bounds)
        {
            // Calculate the new bounds based on the desired position
            bounds.center = position;

            // Check if the object intersects with walls (tagged "Wall")
            Collider[] intersectingWalls = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity,
                LayerMask.GetMask("Wall"));
            if (intersectingWalls.Length > 0)
            {
                return false;
            }

            // Check if the object intersects with other props (tagged "SimObjPhysics")
            Collider[] intersectingProps = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity,
                LayerMask.GetMask("SimObjPhysics"));
            if (intersectingProps.Length > 0)
            {
                return false;
            }

            // Check if the object intersects with doors (tagged "Door")

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