using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pro_gen;
using UnityEngine;
using Random = System.Random;

public class RoomCell : MonoBehaviour
{
    [SerializeField] private ProGenParams _proGenParams;
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _frontWallPrefabs;
    [SerializeField] private GameObject _backWallPrefabs;
    [SerializeField] private GameObject _leftWallPrefabs;
    [SerializeField] private GameObject _rightWallPrefabs;
    [SerializeField] private GameObject _ceilingPrefabs;

    [SerializeField] private GameObject _unvisitedBlock;

    public Dictionary<RoomCellDirections, GameObject> ActiveWalls { get; private set; }

    Random _random;

    private void Awake()
    {
        _unvisitedBlock.SetActive(true);
        _random = new Random(_proGenParams.roomSeed);
        ActiveWalls = new Dictionary<RoomCellDirections, GameObject>
        {
            { RoomCellDirections.Front, _frontWallPrefabs },
            { RoomCellDirections.Back, _backWallPrefabs },
            { RoomCellDirections.Left, _leftWallPrefabs },
            { RoomCellDirections.Right, _rightWallPrefabs }
        };
    }

    public bool IsVisited { get; set; }

    public void Visit()
    {
        IsVisited = true;
        _unvisitedBlock.SetActive(false);
    }

    public void ClearLeftWall()
    {
        _leftWallPrefabs.SetActive(false);
        ActiveWalls.Remove(RoomCellDirections.Left);
    }

    public void ClearRightWall()
    {
        _rightWallPrefabs.SetActive(false);
        ActiveWalls.Remove(RoomCellDirections.Right);
    }

    public void ClearFrontWall()
    {
        _frontWallPrefabs.SetActive(false);
        ActiveWalls.Remove(RoomCellDirections.Front);
    }

    public void ClearBackWall()
    {
        _backWallPrefabs.SetActive(false);
        ActiveWalls.Remove(RoomCellDirections.Back);
    }

    public Vector2 GetCellPosition()
    {
        // find number in cell name with regex
        Regex regex = new Regex(@"\d+");

        Match regexResult = regex.Match(name);
        return new Vector2(long.Parse(regexResult.Value), long.Parse(regexResult.NextMatch().Value));
    }


    public void ReplaceWall(RoomCellDirections direction, GameObject newWall)
    {
        
        switch (direction)
        {
            case RoomCellDirections.Front:
                Debug.Log(_frontWallPrefabs.name);
                Transform frontWallTransform = _frontWallPrefabs.transform;
                if (_frontWallPrefabs.gameObject.name.Contains("Window") ||
                    _frontWallPrefabs.gameObject.name.Contains("Door"))
                {
                    return;
                }

                
                ActiveWalls.Remove(RoomCellDirections.Front);
                Destroy(_frontWallPrefabs);
                newWall = Instantiate(newWall, _frontWallPrefabs.transform.parent);
                newWall.transform.position = new Vector3(-0.05f + _frontWallPrefabs.transform.position.x, 1.25f,
                    1.25f + _frontWallPrefabs.transform.position.z);
                if (newWall.GetComponent<WallDoor>())
                {
                    newWall.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    newWall.transform.rotation = Quaternion.Euler(0, 270, 0);
                }
                _frontWallPrefabs = newWall;

                break;
            case RoomCellDirections.Back:
                Debug.Log(_backWallPrefabs.name);
                Transform backWallTransform = _backWallPrefabs.transform;
                if (_backWallPrefabs.name.Contains("Window") ||
                    _backWallPrefabs.name.Contains("Door"))
                {
                    Debug.Log("Back wall is a door or window");
                    return;
                }
                
                ActiveWalls.Remove(RoomCellDirections.Back);
                Destroy(_backWallPrefabs);
                newWall = Instantiate(newWall, _backWallPrefabs.transform.parent);
                newWall.transform.position = new Vector3(-0.05f + _backWallPrefabs.transform.position.x, 1.25f,
                    1.25f + _backWallPrefabs.transform.position.z);
                newWall.transform.rotation = Quaternion.Euler(0, 90, 0);
                _backWallPrefabs = newWall;
                break;
            case RoomCellDirections.Left:
          
                Transform leftWallTransform = _leftWallPrefabs.transform;
                if (_leftWallPrefabs.gameObject.name.Contains("Window") ||
                    _leftWallPrefabs.gameObject.name.Contains("Door"))
                {
                    return;
                }
                
                ActiveWalls.Remove(RoomCellDirections.Left);
                Destroy(_leftWallPrefabs);
                newWall = Instantiate(newWall, _leftWallPrefabs.transform.parent);
                newWall.transform.position = new Vector3(1.25f + _leftWallPrefabs.transform.position.x, 1.25f,
                    0.05f + _leftWallPrefabs.transform.position.z);
                if (newWall.GetComponent<WallDoor>())
                {
                    newWall.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    newWall.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                _leftWallPrefabs = newWall;
                break;
            case RoomCellDirections.Right:
                Transform rightWallTransform = _rightWallPrefabs.transform;
                if (_rightWallPrefabs.gameObject.name.Contains("Window") ||
                    _rightWallPrefabs.gameObject.name.Contains("Door"))
                {
                    return;
                }

               
                ActiveWalls.Remove(RoomCellDirections.Right);
                Destroy(_rightWallPrefabs);
                newWall = Instantiate(newWall, _rightWallPrefabs.transform.parent);
                newWall.transform.position = new Vector3(1.25f + rightWallTransform.transform.position.x, 1.25f,
                    0.05f + rightWallTransform.transform.position.z);
                newWall.transform.rotation = Quaternion.Euler(0, 0, 0);
                _rightWallPrefabs = newWall;
                break;
        }
    }

    public bool IsWallADoor(RoomCellDirections direction)
    {
        switch (direction)
        {
            case RoomCellDirections.Front:
                return _frontWallPrefabs.GetComponentInChildren<WallDoor>() != null;
            case RoomCellDirections.Back:
                return _backWallPrefabs.GetComponentInChildren<WallDoor>() != null;
            case RoomCellDirections.Left:
                return _leftWallPrefabs.GetComponentInChildren<WallDoor>() != null;
            case RoomCellDirections.Right:
                return _rightWallPrefabs.GetComponentInChildren<WallDoor>() != null;
            default:
                return false;
        }
    }
}