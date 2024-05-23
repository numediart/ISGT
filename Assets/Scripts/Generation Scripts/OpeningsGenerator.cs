using System;
using InternalRealtimeCSG;
using RealtimeCSG.Components;
using RealtimeCSG.Foundation;
using System.Collections.Generic;
using UnityEngine;
using Utils;

using Random = System.Random;

public class OpeningsGenerator : MonoBehaviour
{
    #region Public Fields

    public RoomsGenerationScriptableObject RoomsGenerationData;
    public GeneratorsContainer GeneratorsContainer;
    public int NumberOfOpenings { get; private set; }
    #endregion

    #region Private Fields

    private GameObject _wallModel;
    private Random _random;
    int _doorsRandomNumber;
    int _windowsRandomNumber;
    #endregion

    private void Awake()
    {
        GeneratorsContainer = GameObject.Find("GeneratorsContainer").GetComponent<GeneratorsContainer>();
        RoomsGenerationData = GeneratorsContainer.RoomsGenerator.RoomsGenerationData;
        
    }

    #region Openings Generation Methods
    
    /// <summary>
    /// Generates the doors and the the windows on the walls of a room for every room.
    /// </summary>
    /// <param name="rooms"></param>
    ///
    public void OpeningsGeneration(GameObject room, Random openingRandomn)
    {
            _random = openingRandomn;
            List<GameObject> walls = RoomsGenerator.GetRoomCategoryObjects(room, RoomCategory.Walls);
            _doorsRandomNumber = _random.Next(RoomsGenerationData.DoorPerRoomMinimumNumber, RoomsGenerationData.DoorPerRoomMaximumNumber + 1);
            _windowsRandomNumber = _random.Next(RoomsGenerationData.WindowPerRoomMinimumNumber, RoomsGenerationData.WindowPerRoomMaximumNumber + 1);
            NumberOfOpenings = _doorsRandomNumber + _windowsRandomNumber;
            List<GameObject> wallsWithDoors = DoorsGeneration(walls);

            WindowsGeneration(wallsWithDoors);
        
    }

    /// <summary>
    /// Creates a hole in a given wall of a room at a random position previously choosen and with the dimensions of a given opening.
    /// </summary>
    /// <param name="wall"></param>
    /// <param name="randomOffset"></param>
    /// <param name="holeSize"></param>
    /// <returns></returns>
    private GameObject CuttingHole(GameObject wall, Vector3 randomOffset, Vector3 holeSize)
    {
        wall.TryGetComponent(out CSGModel wallModel);
        Transform currentWallTransform = wall.transform;
        Vector3 currentWallPosition = currentWallTransform.position;
        Vector3 currentWallScale = currentWallTransform.localScale;
        Quaternion currentWallRotation = currentWallTransform.rotation;
        
        if (wallModel)
        {
            _wallModel = wall;
            Transform wallBrushTransform = _wallModel.transform.GetChild(0);

            GameObject holeBrush = Instantiate(RoomsGenerationData.HoleBrushPrefab, wallBrushTransform.position + randomOffset, wallBrushTransform.rotation, _wallModel.transform);
            holeBrush.transform.localScale = holeSize;
        }
        else
        {
            
            float wallHeight = wall.transform.localScale.y;

            _wallModel = Instantiate(RoomsGenerationData.WallModelPrefab, wall.transform.parent);// instantiate a new wall model with the hole
            GameObject brushesMeshRenderer = GeneratorsContainer.RoomsGenerator.GetBrushesMeshRenderer(_wallModel);
            brushesMeshRenderer.TryGetComponent(out GeneratedMeshInstance generatedMeshInstance);
            wall.TryGetComponent(out MeshRenderer wallGeneratedMeshInstance);
            generatedMeshInstance.RenderMaterial = wallGeneratedMeshInstance.sharedMaterial;

            GameObject wallBrush = _wallModel.transform.GetChild(0).gameObject;
            wallBrush.transform.position = currentWallPosition - new Vector3(0, wallHeight / 2f, 0);
            wallBrush.transform.rotation = currentWallRotation;
            wallBrush.transform.localScale = currentWallScale;
            
            GameObject holeBrush = Instantiate(RoomsGenerationData.HoleBrushPrefab, wallBrush.transform.position + randomOffset, currentWallTransform.rotation, _wallModel.transform);
            holeBrush.transform.localScale = holeSize;

            wall.SetActive(false);
        }
        return _wallModel;
    }

    /// <summary>
    /// Chooses a random number of doors to place.
    /// Chooses a random door (among a list of door objets), a random and valid position for the door on a wall, calls the CuttingHole method 
    /// to cut a hole at this position and then instantiates the door.
    /// The process is repeated for every door to place.
    /// </summary>
    /// <param name="walls"></param>
    /// <returns></returns>
    private List<GameObject> DoorsGeneration(List<GameObject> walls)
    {
        List<GameObject> localWallsList = walls;
        int remainingDoors = _doorsRandomNumber;
        Dictionary<int, int> doorPerWall = new Dictionary<int, int>();
        for (int i = 0; i < localWallsList.Count; i++)
        {
            doorPerWall.Add(i, 0);
        }

        while (remainingDoors > 0)
        {
            int wallRandomIndex = _random.Next(0, localWallsList.Count);

            float wallWidth = (localWallsList[wallRandomIndex].transform.childCount == 0) ? GeneratorsContainer.RoomsGenerator.GetWallWidth(localWallsList[wallRandomIndex]) :
                GeneratorsContainer.RoomsGenerator.GetWallWidth(localWallsList[wallRandomIndex].transform.GetChild(0).gameObject);

            if (doorPerWall[wallRandomIndex] <= RoomsGenerationData.DoorPerWallMaximumNumber)
            {
                int doorIndex = _random.Next(0, RoomsGenerationData.Doors.Count);
                RoomsGenerationData.Doors[doorIndex].TryGetComponent(out BoxCollider doorCollider);
                Vector3 doorColliderSize = doorCollider.size;
                Vector3 doorHole = new Vector3(RoomsGenerator.GetOpeningWidth(doorColliderSize), doorColliderSize.y, 1);

                GameObject door;
                
                localWallsList[wallRandomIndex].TryGetComponent(out CSGModel csgModel);
                if (csgModel)
                {
                    List<Vector3> doorPossiblePositionOffsets = DoorPossiblePositionOffsets(localWallsList[wallRandomIndex], doorHole);
                    if (doorPossiblePositionOffsets.Count > 0)
                    {
                        Vector3 randomHorizontalOffset = doorPossiblePositionOffsets[_random.Next(0, doorPossiblePositionOffsets.Count)];
                        localWallsList[wallRandomIndex] = CuttingHole(localWallsList[wallRandomIndex], randomHorizontalOffset, doorHole);

                        Transform wallBrushTransform = localWallsList[wallRandomIndex].transform.GetChild(0);
                        door = Instantiate(RoomsGenerationData.Doors[doorIndex], wallBrushTransform.position + randomHorizontalOffset + new Vector3(0, doorHole.y / 2f, 0),
                            wallBrushTransform.rotation, wallBrushTransform.parent);
                    }
                    else
                        continue;
                }
                else
                {
                    Vector3 horizontalOffset = localWallsList[wallRandomIndex].transform.right.normalized *
                                               _random.Next((int)(-wallWidth / 2f + RoomsGenerator.GetOpeningWidth(doorColliderSize) / 2f + RoomsGenerationData.MinimumDistanceBetweenBorders),
                        (int)(wallWidth / 2f - RoomsGenerator.GetOpeningWidth(doorColliderSize) / 2f - RoomsGenerationData.MinimumDistanceBetweenBorders));
                    localWallsList[wallRandomIndex] = CuttingHole(localWallsList[wallRandomIndex], horizontalOffset, doorHole);

                    Transform wallBrushTransform = localWallsList[wallRandomIndex].transform.GetChild(0);
                    door = Instantiate(RoomsGenerationData.Doors[doorIndex], wallBrushTransform.position + horizontalOffset + new Vector3(0, doorHole.y / 2f, 0),
                        wallBrushTransform.rotation, wallBrushTransform.parent);
                }
    
                for (int i = 0; i < door.transform.childCount; i++)
                {
                    GameObject doorPart = door.transform.GetChild(i).gameObject;

                    doorPart.TryGetComponent(out MeshRenderer meshRenderer);
                    if (meshRenderer && meshRenderer.sharedMaterial.name.Contains("Default-Material"))
                    {
                        if (doorPart.name.Contains("Frame"))
                            meshRenderer.material = RoomsGenerationData.DoorFrameMaterials[_random.Next(0, RoomsGenerationData.DoorFrameMaterials.Count)];
                        else
                            meshRenderer.material = RoomsGenerationData.DoorMaterials[_random.Next(0, RoomsGenerationData.DoorMaterials.Count)];
                    }
                }

                door.TryGetComponent(out Opening doorOpeningComponent);
               doorOpeningComponent.Type = OpeningType.Door;
                SetOpeningRandomOpeness(door);

                doorPerWall[wallRandomIndex]++;
                remainingDoors--;
            }
        }

        return localWallsList;
    }

    /// <summary>
    /// Chooses, for every wall of a room, a random number of windows to place.
    /// Chooses a random window (among a list of window objets), a random and valid position for the window on a wall, calls the CuttingHole method 
    /// to cut a hole at this position and then instantiates the window (and eventually a shutter to simulate a closed window).
    /// The process is repeated for every window to place on a wall and for every wall of the room.
    /// </summary>
    /// <param name="walls"></param>
    private void WindowsGeneration(List<GameObject> walls)
    {
        int remainingWindows = _windowsRandomNumber;
        foreach (GameObject wall in walls)
        {
            float wallWidth = (wall.transform.childCount == 0) ? GeneratorsContainer.RoomsGenerator.GetWallWidth(wall) :
                GeneratorsContainer.RoomsGenerator.GetWallWidth(wall.transform.GetChild(0).gameObject);// get the width of the wall 
            float wallHeight = (wall.transform.childCount == 0) ? wall.transform.localScale.y : wall.transform.GetChild(0).gameObject.transform.localScale.y;
            int holesNumber;
            if (remainingWindows >0)
            {
                 holesNumber = _random.Next(RoomsGenerationData.WindowPerWallMinimumNumber,
                    RoomsGenerationData.WindowPerWallMaximumNumber + 1);
            }
            else
            {
                break;
            }

            GameObject wallSelected = wall;

            for (int i = 0; i < holesNumber; i++)
            {
                int windowIndex = _random.Next(0, RoomsGenerationData.Windows.Count);
                RoomsGenerationData.Windows[windowIndex].TryGetComponent(out BoxCollider windowCollider);
                Vector3 windowColliderSize = windowCollider.size;
                Vector3 holeSize = new Vector3(RoomsGenerator.GetOpeningWidth(windowColliderSize), windowColliderSize.y, 1);
                GameObject window = null;
                wallSelected.TryGetComponent(out CSGModel csgModel);
                if (csgModel)
                {
                    GameObject wallBrush = wallSelected.transform.GetChild(0).gameObject;
                    List<Vector3> windowPossiblePositionOffsets = WindowPossiblePositionOffsets(wallSelected, holeSize);

                    if (windowPossiblePositionOffsets.Count > 0)
                    {
                        Vector3 randomOffset = windowPossiblePositionOffsets[_random.Next(0, windowPossiblePositionOffsets.Count)];
                        wallSelected = CuttingHole(wallSelected, randomOffset, holeSize);

                        window = Instantiate(RoomsGenerationData.Windows[windowIndex], wallBrush.transform.position + randomOffset + new Vector3(0, holeSize.y / 2f, 0),
                            wallBrush.transform.rotation, wallSelected.transform);
                    }
                    else
                    {
                        holesNumber = 0;
                    }
                }
                else
                {
                    Vector3 horizontalOffset = wall.transform.right.normalized * _random.Next((int)(-wallWidth / 2f + RoomsGenerator.GetOpeningWidth(windowColliderSize) / 2f +
                        RoomsGenerationData.MinimumDistanceBetweenBorders), (int)(wallWidth / 2f - RoomsGenerator.GetOpeningWidth(windowColliderSize) / 2f - RoomsGenerationData.MinimumDistanceBetweenBorders));
                    Vector3 verticalOffset = wall.transform.up.normalized * _random.Next((int)(RoomsGenerationData.MinimumDistanceBetweenBorders), (int)(wallHeight - 2 *
                        windowColliderSize.y / 2f - RoomsGenerationData.MinimumDistanceBetweenBorders));
                    Vector3 randomOffset = horizontalOffset + verticalOffset;

                    wallSelected = CuttingHole(wallSelected, randomOffset, holeSize);

                    window = Instantiate(RoomsGenerationData.Windows[windowIndex], wallSelected.transform.GetChild(0).position + randomOffset + new Vector3(0, holeSize.y / 2f, 0),
                        wallSelected.transform.GetChild(0).rotation, wallSelected.transform);
                }

                if (window)
                {
                    for (int k = 0; k < window.transform.childCount; k++)
                    {
                        GameObject windowPart = window.transform.GetChild(k).gameObject;
                        windowPart.TryGetComponent(out MeshRenderer meshRenderer);
                        if (meshRenderer && meshRenderer.sharedMaterial.name.Contains("Default-Material"))
                            meshRenderer.material = RoomsGenerationData.WindowStructureMaterials[_random.Next(0, RoomsGenerationData.WindowStructureMaterials.Count)];
                    }
                    window.TryGetComponent(out Opening windowComponent);
                   windowComponent.Type = OpeningType.Window;
                    SetOpeningRandomOpeness(window);
                }

                remainingWindows--;
            }
        }
    }

    /// <summary>
    /// Convert the opening direction of a specific opening from an enum type to 3D vector.
    /// </summary>
    /// <param name="openingComponent"></param>
    /// <returns></returns>
    // private Vector3 GetOpeningDirectionVector(Opening openingComponent)
    // {
    //     switch (openingComponent.OpeningDirection)
    //     {
    //         case OpeningDirection.Up:
    //             return openingComponent.MovingPart.transform.up;
    //         case OpeningDirection.Down:
    //             return -openingComponent.MovingPart.transform.up;
    //         case OpeningDirection.Right:
    //             return openingComponent.MovingPart.transform.right;
    //         case OpeningDirection.Left:
    //             return -openingComponent.MovingPart.transform.right;
    //         default:
    //             return Vector3.negativeInfinity;
    //     }
    // }

    /// <summary>
    /// Set a random openess degree to a specific opening adapted to its means of opening.
    /// </summary>
    /// <param name="opening"></param>
    private void SetOpeningRandomOpeness(GameObject opening)
    {
        opening.TryGetComponent(out Opening openingComponent);
        float openessDegree = (float) _random.NextDouble();
        // openingComponent.OpennessDegree = openessDegree;

        // Vector3 openingDirection = GetOpeningDirectionVector(openingComponent);

        // switch (openingComponent.MeansOfOpening)
        // {
        //     case MeansOfOpening.Translation:
        //         Vector3 openingDimensions = openingComponent.MovingPart.transform.GetChild(0).localScale;
        //         float sideLength = (openingComponent.OpeningDirection == OpeningDirection.Up || openingComponent.OpeningDirection == OpeningDirection.Down) ?
        //             openingDimensions.y : RoomsGenerator.GetOpeningWidth(openingDimensions);
        //         openingComponent.MovingPart.transform.position += openessDegree * (sideLength / 2f) * openingDirection.normalized;
        //         if (openingComponent.OpeningDirection == OpeningDirection.Up || openingComponent.OpeningDirection == OpeningDirection.Down)
        //             openingComponent.MovingPart.transform.GetChild(0).localScale = new Vector3(openingDimensions.x, (1 - openessDegree) * sideLength, openingDimensions.z);
        //         else if (openingComponent.OpeningDirection == OpeningDirection.Right || openingComponent.OpeningDirection == OpeningDirection.Left)
        //             openingComponent.MovingPart.transform.GetChild(0).localScale = new Vector3((1 - openessDegree) * sideLength, openingDimensions.y, openingDimensions.z);
        //         break;
        //     case MeansOfOpening.Rotation:
        //         openingComponent.MovingPart.transform.Rotate(openingDirection, -openessDegree * 90f);
        //         break;
        // }
    }

    #endregion

    #region Position Verification Methods

    /// <summary>
    /// may says overlap verification???
    /// Verifies if there is an overlay between the openings already set on the wall and a new opening that would be placed at a specific position.
    /// </summary>
    /// <param name="wallModel"></param>
    /// <param name="newHolePositionOffset"></param>
    /// <param name="newHoleSize"></param>
    /// <returns></returns>
    private bool HolesOverlayVerification(GameObject wallModel, Vector3 newHolePositionOffset, Vector3 newHoleSize)
    {
        wallModel.TryGetComponent(out CSGModel wallModelComponent);
        if (wallModelComponent)
        {
            GameObject wallBrush = wallModel.transform.GetChild(0).gameObject;

            for (int i = 0; i < wallModel.transform.childCount; i++)
            {
                GameObject holeBrush = wallModel.transform.GetChild(i).gameObject;
                holeBrush.TryGetComponent(out CSGBrush holeBrushComponent);
                if (holeBrushComponent&& holeBrushComponent.OperationType == CSGOperationType.Subtractive)
                {
                    Vector3 formerHolePosition = holeBrush.transform.position;
                    Vector3 holeBrushLocalScale = holeBrush.transform.localScale;
                    Vector3 realFormerHolePosition = formerHolePosition + new Vector3(0, holeBrushLocalScale.y / 2f, 0);
                    Vector3 realNewHolePosition = wallBrush.transform.position + newHolePositionOffset + new Vector3(0, newHoleSize.y / 2f, 0);
                    Vector3 holesDistanceVector = realNewHolePosition - realFormerHolePosition;
               
                    float horizontalMinimumDistance = RoomsGenerator.GetOpeningWidth(holeBrushLocalScale) / 2f + RoomsGenerator.GetOpeningWidth(newHoleSize) / 2f + RoomsGenerationData.MinimumDistanceBetweenBorders;
                    float verticalMinimumDistance = holeBrushLocalScale.y / 2f + newHoleSize.y / 2f + RoomsGenerationData.MinimumDistanceBetweenBorders;

                    if (Mathf.Abs(Vector3.Project(holesDistanceVector, wallBrush.transform.right).magnitude) < horizontalMinimumDistance)
                    {
                        if (Mathf.Abs(Vector3.Project(holesDistanceVector, wallBrush.transform.up).magnitude) < verticalMinimumDistance)
                            return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a list of all the possible position offsets (2D offsets from the center of the wall) of a specific window on a specific wall.
    /// </summary>
    /// <param name="wallModel"></param>
    /// <param name="newHoleSize"></param>
    /// <returns></returns>
    private List<Vector3> WindowPossiblePositionOffsets(GameObject wallModel, Vector3 newHoleSize)
    {
        List<Vector3> possiblePositionOffsets = new List<Vector3>();

        GameObject wallBrush = wallModel.transform.GetChild(0).gameObject;
        float wallWidth = GeneratorsContainer.RoomsGenerator.GetWallWidth(wallBrush);
        float wallHeight = wallBrush.transform.localScale.y;

        for (float i = -wallWidth / 2f + RoomsGenerationData.MinimumDistanceBetweenBorders + newHoleSize.x / 2f; i < wallWidth / 2f - RoomsGenerationData.MinimumDistanceBetweenBorders - newHoleSize.x / 2f; i += 0.05f)
        {
            for (float j = RoomsGenerationData.MinimumDistanceBetweenBorders; j < wallHeight - 2 * newHoleSize.y / 2f - RoomsGenerationData.MinimumDistanceBetweenBorders; j += 0.05f)
            {
                Vector3 horizontalOffset = wallBrush.transform.right.normalized * i;
                Vector3 verticalOffset = wallBrush.transform.up.normalized * j;
                Vector3 randomOffset = horizontalOffset + verticalOffset;

                if (!HolesOverlayVerification(wallModel, randomOffset, newHoleSize))
                    possiblePositionOffsets.Add(randomOffset);
            }
        }

        return possiblePositionOffsets;
    }

    /// <summary>
    /// Returns a list of all the possible position offsets (2D offsets from the center of the wall) of a specific window on a specific wall.
    /// (Two separated methods for doors and windows because the bottom of a door has to be at the ground height level contrarily to a window).
    /// </summary>
    /// <param name="wallModel"></param>
    /// <param name="newHoleSize"></param>
    /// <returns></returns>
    private List<Vector3> DoorPossiblePositionOffsets(GameObject wallModel, Vector3 newHoleSize)
    {
        List<Vector3> possiblePositionOffsets = new List<Vector3>();

        GameObject wallBrush = wallModel.transform.GetChild(0).gameObject;
        float wallWidth = GeneratorsContainer.RoomsGenerator.GetWallWidth(wallBrush);

        for (float i = -wallWidth / 2f + RoomsGenerationData.MinimumDistanceBetweenBorders + newHoleSize.x / 2f; i < wallWidth / 2f - RoomsGenerationData.MinimumDistanceBetweenBorders - newHoleSize.x / 2f; i += 0.05f)
        {
            Vector3 horizontalOffset = wallBrush.transform.right.normalized * i;

            if (!HolesOverlayVerification(wallModel, horizontalOffset, newHoleSize))
                possiblePositionOffsets.Add(horizontalOffset);
        }

        return possiblePositionOffsets;
    }

    #endregion
}
