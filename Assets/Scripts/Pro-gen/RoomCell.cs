using System;
using System.Collections.Generic;
using Pro_gen;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomCell : MonoBehaviour
{
    [SerializeField] private RoomsGenerationScriptableObject _proGenParams;
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _frontWallPrefabs;
    [SerializeField] private GameObject _backWallPrefabs;
    [SerializeField] private GameObject _leftWallPrefabs;
    [SerializeField] private GameObject _rightWallPrefabs;
    [SerializeField] private GameObject _ceilingPrefabs;
    [SerializeField] private GameObject _unvisitedBlock;

    public Dictionary<RoomCellDirections, GameObject>
        ActiveWalls { get; private set; } // store the active walls in the room cell

    public Vector2 Position { get; set; } // store the position of the room cell

    /// <summary>
    ///  Set the initial state of the room cell Method is called when the object is created
    /// </summary>
    private void Awake()
    {
        _unvisitedBlock.SetActive(true); // set the unvisited block to be active
        ActiveWalls = new Dictionary<RoomCellDirections, GameObject> // create a dictionary to store the active walls
        {
            { RoomCellDirections.Front, _frontWallPrefabs },
            { RoomCellDirections.Back, _backWallPrefabs },
            { RoomCellDirections.Left, _leftWallPrefabs },
            { RoomCellDirections.Right, _rightWallPrefabs }
        };
    }

    public bool IsVisited { get; set; } // store the state of the room cell

    /// <summary>
    /// Set If the room cell has been visited
    /// </summary>
    public void Visit()
    {
        IsVisited = true;
        DestroyImmediate(_unvisitedBlock); // destroy the unvisited block when the room cell is visited (ressource management)
    }

    /// <summary>
    ///  Clear the walls of the room cell here it's the left wall
    /// </summary>
    public void ClearLeftWall()
    {
        DestroyImmediate(_leftWallPrefabs); // destroy the left wall
        ActiveWalls.Remove(RoomCellDirections.Left); // remove the left wall from the active walls
    }

    /// <summary>
    ///  Clear the walls of the room cell here it's the right wall
    /// </summary>
    public void ClearRightWall()
    {
        DestroyImmediate(_rightWallPrefabs);
        ActiveWalls.Remove(RoomCellDirections.Right); // remove the right wall from the active walls
    }

    /// <summary>
    ///  Clear the walls of the room cell here it's the front wall
    /// </summary>
    public void ClearFrontWall()
    {
        DestroyImmediate(_frontWallPrefabs);
        ActiveWalls.Remove(RoomCellDirections.Front); // remove the front wall from the active walls
    }

    /// <summary>
    ///  Clear the walls of the room cell here it's the back wall
    /// </summary>
    public void ClearBackWall()
    {
        DestroyImmediate(_backWallPrefabs);
        ActiveWalls.Remove(RoomCellDirections.Back); // remove the back wall from the active walls
    }

    /// <summary>
    ///  Return the position of the room cell in the grid (x,y) not the world position
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCellPosition()
    {
        return Position; // return the position of the room cell
    }

    /// <summary>
    /// Replace the wall of the room cell with a new wall prefab
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="newWall"></param>
    public void ReplaceWall(RoomCellDirections direction, GameObject newWall)
    {
        ReplaceWallInternal(direction, newWall,
            GetWallPrefab(direction)); // call the internal method to replace the wall
    }

    /// <summary>
    /// Replace the wall of the room cell with a new wall prefab and destroy the current wall prefab and update the active walls and check if the wall is already a door or a window
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="newWall"></param>
    /// <param name="currentWall"></param>
    private void ReplaceWallInternal(RoomCellDirections direction, GameObject newWall, GameObject currentWall)
    {
        if (currentWall == null || newWall == null) // make sure the current wall and the new wall are not null
            return;


        if (currentWall.name.Contains("Window") ||
            currentWall.name.Contains("Door")) // ensure the current wall is not a door or a window
        {
            return;
        }

        ActiveWalls.Remove(direction); // remove the wall from the active walls
        GameObject instantiatedWall =
            Instantiate(newWall, currentWall.transform.parent); // instantiate the new wall prefab
        Vector3 newPosition = currentWall.transform.position; // get the position of the current wall
        Quaternion newRotation = Quaternion.identity; // set the rotation of the new wall to be the identity quaternion

        switch (direction) // switch the direction of the wall
        {
            case RoomCellDirections.Front:
                newPosition =
                    new Vector3(-0.055f + newPosition.x, 1.25f,
                        1.25f + newPosition.z); // set the new position of the wall
                newRotation = Quaternion.Euler(0, 270, 0); // set the new rotation of the wall to be 90 degrees on the y axis it can be 270 degrees if the wall is a window
                _frontWallPrefabs = instantiatedWall;
                break;
            case RoomCellDirections.Back:
                newPosition = new Vector3(-0.055f + newPosition.x, 1.25f, 1.25f + newPosition.z);
                newRotation =
                    Quaternion.Euler(0, 90,
                        0); // set the new rotation of the wall to be 90 degrees on the y axis no special case here
                _backWallPrefabs = instantiatedWall;
                break;
            case RoomCellDirections.Left:
                newPosition = new Vector3(1.25f + newPosition.x, 1.25f, 0.05f + newPosition.z + 0.005f);
                newRotation = Quaternion.Euler(0, 180, 0);// set the new rotation of the wall to be 0 degrees on the y axis it can be 180 degrees if the wall is a window (need to flip it to have well oriented windows)
                _leftWallPrefabs = instantiatedWall;
                break;
            case RoomCellDirections.Right:
                newPosition = new Vector3(1.25f + newPosition.x, 1.25f, 0.05f + newPosition.z + 0.005f);
                newRotation = Quaternion.Euler(0, 0, 0);
                _rightWallPrefabs = instantiatedWall;
                break;
        }

        instantiatedWall.transform.position = newPosition;
        instantiatedWall.transform.rotation = newRotation;

        // Add the new wall back to ActiveWalls
        ActiveWalls[direction] = instantiatedWall;
        DestroyImmediate(currentWall); // destroy the current wall
    }

    public void ApplyWallTexture(int materialIndex, RoomCellDirections directions)
    {
        if (ActiveWalls.ContainsKey(directions))
        {
            if (ActiveWalls[directions].TryGetComponent(out Renderer renderer))
            {
                renderer.material = _proGenParams.WallMaterials[materialIndex];
            }
        }
    }

    public void ApplyFloorTexture(int materialIndex)
    {
        if(_floorPrefab.TryGetComponent(out Renderer renderer))
            renderer.material = _proGenParams.FloorMaterials[materialIndex];
    }

    public void ApplyCeilingTexture(int materialIndex)
    {
        if(_ceilingPrefabs.TryGetComponent(out Renderer renderer))
            renderer.material = _proGenParams.CeilingMaterials[materialIndex];
    }
    
    public void ApplyWindowTexture(int materialIndex, RoomCellDirections directions)
    {
        if (ActiveWalls.ContainsKey(directions))
        {
            // Find all the windows in the wall
            Opening[] windows = ActiveWalls[directions].GetComponentsInChildren<Opening>(); //Get all the openings in the wall
            foreach (var window in windows)
            {
                if (window.MeansOfOpening == MeansOfOpening.Translation) // Make sure the opening is a window
                {
                    //Name of the window
                    GameObject windowFrame = window.Structure; // Get the window frame
                    
                    // Check if window frame is not null

                    if (!windowFrame)
                    {
                        if (windowFrame.TryGetComponent(out Renderer renderer))
                        {
                            renderer.material = _proGenParams.WindowMaterials[materialIndex];
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    ///  Return the wall prefab of the room cell based on the direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private GameObject GetWallPrefab(RoomCellDirections direction)
    {
        switch (direction)
        {
            case RoomCellDirections.Front:
                return _frontWallPrefabs;
            case RoomCellDirections.Back:
                return _backWallPrefabs;
            case RoomCellDirections.Left:
                return _leftWallPrefabs;
            case RoomCellDirections.Right:
                return _rightWallPrefabs;
            default:
                return null;
        }
    }
    public GameObject GetActiveWall(RoomCellDirections direction)
    {
        return ActiveWalls[direction];
    }
}