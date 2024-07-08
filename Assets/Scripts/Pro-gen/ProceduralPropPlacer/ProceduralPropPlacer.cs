using System.Collections;
using System.Collections.Generic;
using Pro_gen.QuadTree;
using UnityEngine;
using Utils;
using Random = System.Random;

namespace Pro_gen
{
    public class ProceduralPropPlacer : AbstractProceduralPropPlacer
    {
  
        
        private new void Awake()
        {
            _selectedProps = new List<Props>();
            _selectedPropsBounds = new List<Bounds>();
            _propsPositions = new List<Vector3>();
            Time.fixedDeltaTime = 0.001f;
        }
        

        /// <summary>
        ///  Initialize the Procedural Prop Placer with the  room generation data and calculate the number of props to place
        /// </summary>
        /// <param name="roomGenerationData"></param>
        public override void Init(RoomsGenerationScriptableObject roomGenerationData)
        {
            _roomsGenerationData = roomGenerationData;
            numberOfProps = Mathf.RoundToInt((((float)_roomsGenerationData.ObjectNumberRatio / 100) *
                                             (_roomsGenerationData.width * _roomsGenerationData.height* _roomsGenerationData.heightOffset))/2);
            _groundBounds = GetGroundBounds();
        }

        /// <summary>
        ///  Place the props in the room using a quad tree structure to optimize the placement
        /// </summary>
        /// <param name="random"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public override IEnumerator PlaceProps(Random random, int area)
        {
            if (!TryGetComponent(out ClassicRoom room))
            {
                Debug.LogError("Room component not found.");
            }

            if (_roomsGenerationData == null)
            {
                Debug.LogError("ProGenParams not assigned.");
                yield break;
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
            _quadTree.DetermineMaxDepth(area);
            TimeTools timeTools = new TimeTools();
            timeTools.Start();
            for (int i = 0; i < numberOfProps; i++)
            {
                Props selectedProp =
                    _roomsGenerationData.PropsPrefabs[random.Next(_roomsGenerationData.PropsPrefabs.Count)];
                Props propInstance = Instantiate(selectedProp, Vector3.zero,
                    Quaternion.Euler(0, NextFloat(random, 0f, 360f), 0), transform);

                // If object has a spawner script, use it
                if (propInstance.TryGetComponent(out PropsSpawner propSpawner))
                {
                    propSpawner.Spawn(random);
                }


                Bounds propBounds = propInstance.CalculateBounds();

                // wait for next fixed frame
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();
                Vector3? positionInRoom = PropsPossiblePosition(random, roomBounds, propBounds, propInstance.transform, propInstance.PropsCategory);

                if (positionInRoom == null) // if there is no empty node anymore, stop placing props
                {
                    Destroy(propInstance.gameObject);
                    break;
                }
                else if (positionInRoom == Vector3.zero) // if the prop couldn't be placed, in the max attempts, destroy it
                {
                    Destroy(propInstance.gameObject);
                }
                else
                {
                    propInstance.transform.position = (Vector3)positionInRoom;
                    Physics.SyncTransforms();
                    _quadTree.Insert(propInstance);
                    propBounds = propInstance.CalculateBounds(); // Recalculate bounds after moving
                    _selectedProps.Add(propInstance);
                    _selectedPropsBounds.Add(propBounds);
                    _propsPositions.Add((Vector3)positionInRoom);
                }
            }

            timeTools.Stop();
            Debug.Log($"{_propsPositions.Count} Props placed in " + timeTools.GetElapsedTime() + " milliseconds.");
            room.RoomState = RoomState.Filled;
        }
        
        /// <summary>
        /// Check if the prop can be placed in the room and return its position if it can
        /// </summary>
        /// <param name="random"></param>
        /// <param name="roomBounds"></param>
        /// <param name="bounds"></param>
        /// <param name="propTransform"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        protected override Vector3? PropsPossiblePosition(Random random, Bounds roomBounds, Bounds bounds, Transform propTransform, PropsCategory category)
        {
            Vector3 position = Vector3.zero;
            bool isPositionFound = false;
            Vector3 propExtents = bounds.extents;
            int attempts = 0;
            List<QuadTreeNodeBase> biggestEmptyNodes = _quadTree.FindBiggestEmptyNodes(category);
            
            if (biggestEmptyNodes.Count == 0) // if there are no empty nodes
            {
                return null;
            }

            int nodeIndex = random.Next(biggestEmptyNodes.Count);

            QuadTreeNodeBase selectedNode = biggestEmptyNodes[nodeIndex];

            while (!isPositionFound && attempts < _maxAttempts)
            {
                position = new Vector3(
                    NextFloat(random, selectedNode.Bounds.min.x + propExtents.x,
                        selectedNode.Bounds.max.x - propExtents.x),
                    _groundBounds.max.y - bounds.min.y, // Adjust the height to align with the ground,
                    NextFloat(random, selectedNode.Bounds.min.z + propExtents.z,
                        selectedNode.Bounds.max.z - propExtents.z)
                );
                propTransform.position = position;
                RotateProp(propTransform, category);
                if (CanPropsBePlaced(propTransform, bounds))
                {
                    isPositionFound = true;
                }
                else
                {
                    attempts++;
                }
            }

            if (attempts == _maxAttempts && !isPositionFound) // if the prop couldn't be placed in time, return Vector3.zero
            {
                return Vector3.zero;
            }

            return position;
        }

        /// <summary>
        /// Check if the prop can be placed in the room without intersecting with walls or other props
        /// </summary>
        /// <param name="propTransform"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        protected override bool CanPropsBePlaced(Transform propTransform, Bounds bounds)
        {
            // Calculate the new bounds based on the desired position
            bounds.center = propTransform.position;

            // Check if the object intersects with walls (tagged "Wall")

            Collider[] intersectingWalls = Physics.OverlapBox(bounds.center, bounds.extents, propTransform.rotation);


            foreach (Collider wall in intersectingWalls)
            {
                if (wall.CompareTag("Walls") || wall.CompareTag("SimObjPhysics") || wall.CompareTag("Door"))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Rotate the prop based on its category and the closest wall
        /// </summary>
        /// <param name="propTransform"></param>
        /// <param name="category"></param>
        protected override void RotateProp(Transform propTransform, PropsCategory category)
        {
            // If the object is a fridge, a shelf or a sofa, we want to align it perpendicular with the closest wall
            // If it's a bed, we want to align it with the closest wall but parallel to it
            // Do it only if the object is close enough to a wall

            if (category == PropsCategory.Bed || category == PropsCategory.Sofa || category == PropsCategory.Fridge)
            {
                GameObject closestWall = findClosestWall(propTransform);
                if (closestWall != null)
                {
                    propTransform.LookAt(closestWall.transform.position);

                    // Adjust rotation for Bed, Sofa, and Fridge
                    float rotationOffset = category == PropsCategory.Bed ? 90 : 180;

                    // Get the current Y rotation and add the offset
                    float currentYRotation = propTransform.eulerAngles.y + rotationOffset;

                    // Round the Y rotation to the nearest multiple of 90 degrees
                    float roundedYRotation = Mathf.Round(currentYRotation / 90) * 90;

                    // Apply the rounded rotation back to the propTransform
                    propTransform.rotation = Quaternion.Euler(0, roundedYRotation, 0);
                }
            }
        }

        /// <summary>
        /// Find the closest wall to the prop and return it
        /// </summary>
        /// <param name="propTransform"></param>
        /// <returns></returns>
        protected override GameObject findClosestWall(Transform propTransform)
        {
            // Iterate through all the walls and find the closest one
            GameObject closestWall = null;
            float minDistance = float.MaxValue;
            foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Walls"))
            {
                float distance = Vector3.Distance(propTransform.position, wall.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestWall = wall;
                }
            }
            
            
            return minDistance < 3f ? closestWall : null;
            
        }
        
        /// <summary>
        ///  Get the bounds of the ground in the room
        /// </summary>
        /// <returns></returns>
        protected override Bounds GetGroundBounds()
        {
            Bounds groundBounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
            {
                if (childRenderer.CompareTag("Ground"))
                {
                    groundBounds.Encapsulate(childRenderer.bounds);
                }
            }

            return groundBounds;
        }

        protected override float NextFloat(Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        ///  Get all empty quad nodes in the quad tree
        /// </summary>
        /// <returns></returns>
        public override List<Bounds> GetAllEmptyQuadNodes()
        {
            Debug.Log("Getting all empty quad nodes");
            return _quadTree.GetAllEmptyNodes();
        }

        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 roomCenter = new Vector3(_roomsGenerationData.widthOffset * _roomsGenerationData.width / 2,
                _roomsGenerationData.heightOffset / 2,
                _roomsGenerationData.heightOffset * _roomsGenerationData.height / 2);
            Gizmos.DrawWireCube(roomCenter,
                new Vector3(_roomsGenerationData.widthOffset * _roomsGenerationData.width,
                    _roomsGenerationData.heightOffset,
                    _roomsGenerationData.heightOffset * _roomsGenerationData.height));

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundBounds.center, _groundBounds.size);

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