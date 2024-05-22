using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Pro_gen;
using UnityEditor;
using UnityEngine;
using Random = System.Random;


public class RoomGrid : MonoBehaviour
{
    [SerializeField] private ProGenParams _proGenParams;
    private Random _gridRandom;
    private RoomCell[,] _grid;
    private Dictionary<long, RoomCell> _activeWalls = new Dictionary<long, RoomCell>();
    private long _wallIndex = 0;

    private Dictionary<RoomCellDirections, List<RoomCell>> _wallSections =
        new Dictionary<RoomCellDirections, List<RoomCell>>();

    private Dictionary<RoomCellDirections, int> _wallDoorPerSection;

    private void Start()
    {
        _grid = new RoomCell[_proGenParams.width, _proGenParams.height];
        _gridRandom = new Random(_proGenParams.roomSeed);
        _wallSections.Add(RoomCellDirections.Front, new List<RoomCell>());
        _wallSections.Add(RoomCellDirections.Back, new List<RoomCell>());
        _wallSections.Add(RoomCellDirections.Left, new List<RoomCell>());
        _wallSections.Add(RoomCellDirections.Right, new List<RoomCell>());
        _wallDoorPerSection = new Dictionary<RoomCellDirections, int>
        {
            { RoomCellDirections.Front, 0 },
            { RoomCellDirections.Back, 0 },
            { RoomCellDirections.Left, 0 },
            { RoomCellDirections.Right, 0 }
        };
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for (long i = 0; i < _proGenParams.width; i++)
        {
            for (long j = 0; j < _proGenParams.height; j++)
            {
                _grid[i, j] = Instantiate(_proGenParams.RoomCellPrefab, new Vector3(i * 2.5f, 0, j * 2.5f),
                    Quaternion.identity);
                _grid[i, j].name = $"RoomCell_{i}_{j}";
                _grid[i, j].transform.parent = transform;
            }
        }

        GenerateRoom(_grid[0, 0]);

        //RandomReplaceActiveWallWithDoor();
        RandomReplaceActiveWallWithDoorConstraint();
        RandomReplaceActiveWithWindowConstraint();
    }

    private void GenerateRoom(RoomCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(currentCell);
        RoomCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);
            if (nextCell != null)
            {
                GenerateRoom(nextCell);
            }
        } while (nextCell != null);
    }

    private RoomCell GetNextUnvisitedCell(RoomCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell);
        return unvisitedCells.FirstOrDefault();
    }

    private IEnumerable<RoomCell> GetUnvisitedCells(RoomCell currentCell)
    {
        Vector2 cellPosition = currentCell.GetCellPosition();
        if (cellPosition.x + 1 < _proGenParams.width)
        {
            var cellToRight = _grid[(int)cellPosition.x + 1, (int)cellPosition.y];
            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }

        if (cellPosition.x - 1 >= 0)
        {
            var cellToLeft = _grid[(int)cellPosition.x - 1, (int)cellPosition.y];
            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }

        if (cellPosition.y + 1 < _proGenParams.height)
        {
            var cellToFront = _grid[(int)cellPosition.x, (int)cellPosition.y + 1];
            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        if (cellPosition.y - 1 >= 0)
        {
            var cellToBack = _grid[(int)cellPosition.x, (int)cellPosition.y - 1];
            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(RoomCell currentCell)
    {
        ClearWall(currentCell);
    }

    private void ClearWall(RoomCell cell)
    {
        bool isInCorner = false;
        Vector2 cellPosition = cell.GetCellPosition(); // return the position of the cell in the grid (index x and y)
        if (cellPosition.x == 0 && cellPosition.y == 0)
        {
            cell.ClearBackWall();
            cell.ClearRightWall();
            isInCorner = true;
            _wallSections[RoomCellDirections.Front].Add(cell);
            _wallSections[RoomCellDirections.Left].Add(cell);
        }

        else if (cellPosition is { x: 0, y: > 0 } && cellPosition.y < _proGenParams.height - 1)
        {
            cell.ClearBackWall();
            cell.ClearRightWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Front].Add(cell);
        }
        else if (cellPosition.x == 0 && cellPosition.y == _proGenParams.height - 1)
        {
            cell.ClearBackWall();
            cell.ClearLeftWall();
            isInCorner = true;
            _wallSections[RoomCellDirections.Front].Add(cell);
            _wallSections[RoomCellDirections.Right].Add(cell);
        }

        else if (cellPosition is { x: > 0, y: 0 } && cellPosition.x < _proGenParams.width - 1)
        {
            cell.ClearBackWall();
            cell.ClearRightWall();
            cell.ClearFrontWall();
            _wallSections[RoomCellDirections.Left].Add(cell);
        }

        else if (cellPosition.x == _proGenParams.width - 1 && cellPosition.y == 0)
        {
            cell.ClearFrontWall();
            cell.ClearRightWall();
            isInCorner = true;
            _wallSections[RoomCellDirections.Back].Add(cell);
            _wallSections[RoomCellDirections.Left].Add(cell);
        }

        else if (cellPosition.x == _proGenParams.width - 1 && cellPosition.y < _proGenParams.height - 1 &&
                 cellPosition.y > 0)
        {
            cell.ClearFrontWall();
            cell.ClearRightWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Back].Add(cell);
        }
        else if (cellPosition.x == _proGenParams.width - 1 && cellPosition.y == _proGenParams.height - 1)
        {
            cell.ClearFrontWall();
            cell.ClearLeftWall();
            isInCorner = true;
            _wallSections[RoomCellDirections.Back].Add(cell);
            _wallSections[RoomCellDirections.Right].Add(cell);
        }

        else if (cellPosition.x > 0 && cellPosition.x < _proGenParams.width - 1 &&
                 cellPosition.y == _proGenParams.height - 1)
        {
            cell.ClearBackWall();
            cell.ClearFrontWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Right].Add(cell);
        }
        else if (cellPosition.x > 0 && cellPosition.x < _proGenParams.width - 1 && cellPosition.y > 0 &&
                 cellPosition.y < _proGenParams.height - 1)
        {
            cell.ClearBackWall();
            cell.ClearFrontWall();
            cell.ClearRightWall();
            cell.ClearLeftWall();
            return;
        }

        _activeWalls[_wallIndex++] = cell;
        if (isInCorner)
        {
            _activeWalls[_wallIndex++] = cell;
        }
    }

    private void RandomReplaceActiveWallWithDoorConstraint()
    {
        foreach (var wallSection in _wallSections)
        {
            //check if the door is space with the parameters minDistanceBetweenDoors and maxDistanceBetweenDoors
            int minDistanceBetweenDoors = 2;
            int maxDistanceBetweenDoors = 10;
            int doorNumber = 0;
            var previousCell = wallSection.Value[0];
            for (int j = _wallDoorPerSection[wallSection.Key]; j < _proGenParams.DoorPerWallNumber; j++)
            {
                int wallDoorIndex = _gridRandom.Next(0, _proGenParams.WallDoorPrefabs.Count);
                var wallDoor = _proGenParams.WallDoorPrefabs[wallDoorIndex];
                var cell = wallSection.Value[_gridRandom.Next(0, wallSection.Value.Count)];
                if (cell == previousCell)
                {
                    j--;
                    continue;
                }

                if (doorNumber == 0)
                {
                    cell.ReplaceWall(wallSection.Key, wallDoor);
                    doorNumber++;
                }
                else
                {
                    if ( Vector2.Distance(cell.GetCellPosition(), previousCell.GetCellPosition()) > minDistanceBetweenDoors && Vector3.Distance(cell.GetCellPosition(), previousCell.GetCellPosition()) < maxDistanceBetweenDoors)
                    {
                        cell.ReplaceWall(wallSection.Key, wallDoor);
                        doorNumber++;
                    }
                    else
                    {
                        j--;
                    }
                }

                _wallDoorPerSection[wallSection.Key] = doorNumber;
                previousCell = cell;
            }
        }
    }
    
    private void RandomReplaceActiveWithWindowConstraint()
    {
        foreach (var wallSection in _wallSections)
        {
            int windowNumber = 0;
            RoomCell previousCell = null;
            for (int j = 0; j < _proGenParams.WindowPerWallNumber; j++)
            {
                int wallWindowIndex = _gridRandom.Next(0, _proGenParams.WallWindowsPrefabs.Count);
                var wallWindow = _proGenParams.WallWindowsPrefabs[wallWindowIndex];
                var cell = wallSection.Value[_gridRandom.Next(0, wallSection.Value.Count)];
                //
                if (windowNumber == 0 )
                {
                    
                    cell.ReplaceWall(wallSection.Key, wallWindow);
                    windowNumber++;
                }
                else
                {
                    if(previousCell==null)
                    {
                        j--;
                        continue;
                    }
                    if (Vector2.Distance(cell.GetCellPosition(), previousCell.GetCellPosition()) > 1 && !cell.GetComponentInChildren<WallDoor>())
                    {
                        cell.ReplaceWall(wallSection.Key, wallWindow);
                        windowNumber++;
                    }
                    else
                    {
                        j--;
                    }
                }
                previousCell = cell;
            }
        }

    }

    private void OnDrawGizmos()
    {
        float width = _proGenParams.width * 2.5f;
        float height = _proGenParams.height * 2.5f;
        float spacing = 0.5f;
        float lineThickness = 2.0f; // Change this value to adjust line thickness

        Handles.color = Color.red;

        // Draw vertical lines
        for (float x = 0; x <= width; x += spacing)
        {
            if (x == 0 || x == width || x == spacing || x == width-spacing)
            {
                Handles.color = Color.green; // Exterior vertical lines
            }
            else
            {
                Handles.color = Color.red; // Interior vertical lines
            }
            DrawThickLine(new Vector3(x, 0, 0), new Vector3(x, 0, height), lineThickness);
        }

        // Draw horizontal lines
        for (float z = 0; z <= height; z += spacing)
        {
            if (z == 0 || z == height || z == spacing || z == height - spacing)
            {
                Handles.color = Color.green; // Exterior horizontal lines
            }
            else
            {
                Handles.color = Color.red; // Interior horizontal lines
            }
            DrawThickLine(new Vector3(0, 0, z), new Vector3(width, 0, z), lineThickness);
        }
    }

    private void DrawThickLine(Vector3 start, Vector3 end, float thickness)
    {
        Handles.DrawAAPolyLine(thickness, new Vector3[] { start, end });
    }


}