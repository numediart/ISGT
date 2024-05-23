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

    private Dictionary<RoomCellDirections, List<RoomCell>> _wallSections;
    private Dictionary<RoomCellDirections, int> _wallDoorPerSection;

    private void Start()
    {
        _grid = new RoomCell[_proGenParams.width,
            _proGenParams.height]; // initialize the grid with the width and height in pro gen scriptable Object
        _gridRandom = new Random(_proGenParams.roomSeed); // initialize the random seed
        _wallSections =
            new
                Dictionary<RoomCellDirections,
                    List<RoomCell>> // initialize the wall sections with the directions store all the cells that have walls in the same direction
                {
                    { RoomCellDirections.Front, new List<RoomCell>() },
                    { RoomCellDirections.Back, new List<RoomCell>() },
                    { RoomCellDirections.Left, new List<RoomCell>() },
                    { RoomCellDirections.Right, new List<RoomCell>() }
                };
        _wallDoorPerSection =
            new
                Dictionary<RoomCellDirections, int> // initialize the wall door per section with the directions store the number of doors in the same direction
                {
                    { RoomCellDirections.Front, 0 },
                    { RoomCellDirections.Back, 0 },
                    { RoomCellDirections.Left, 0 },
                    { RoomCellDirections.Right, 0 }
                };
        GenerateGrid(); // generate the grid
    }

    /// <summary>
    /// Generate the grid of the room with the width and height in the pro gen scriptable object
    /// </summary>
    private void GenerateGrid()
    {
        for (long i = 0; i < _proGenParams.width; i++)
        {
            for (long j = 0; j < _proGenParams.height; j++)
            {
                _grid[i, j] = Instantiate(_proGenParams.RoomCellPrefab, new Vector3(i * 2.5f, 0, j * 2.5f),
                    Quaternion.identity);
                _grid[i, j].name = $"RoomCell_{i}_{j}";
                _grid[i, j].Position = new Vector2(i, j);
                _grid[i, j].transform.parent = transform;
            }
        }

        GenerateRoom(_grid[0, 0]); // generate the room starting from the cell 0,0

        RandomReplaceActiveWallWithDoorConstraint(); // replace the active wall with door with the constraint of the distance between the doors
        RandomReplaceActiveWithWindowConstraint(); // replace the active wall with window with the constraint of the distance between the windows
        ApplyTextures(); // apply the textures to the walls 
    }

    /// <summary>
    /// Generate the room starting from the current cell and visit all the cells that are not visited to generate the room by clearing the walls of the cells
    /// </summary>
    /// <param name="currentCell"></param>
    private void GenerateRoom(RoomCell currentCell)
    {
        currentCell.Visit(); // visit the current cell (remove the unvisited block)
        ClearWalls(currentCell); // clear the walls of the current cell according to the position of the cell in the grid
        RoomCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currentCell); // get the next unvisited cell
            if (nextCell != null)
            {
                GenerateRoom(nextCell); // generate the room starting from the next cell
            }
        } while (nextCell != null); // loop until there is no unvisited cell
    }

    /// <summary>
    /// Get the next unvisited cell around the current cell in the grid (right, left, front, back)
    /// </summary>
    /// <param name="currentCell"></param>
    /// <returns></returns>
    private RoomCell GetNextUnvisitedCell(RoomCell currentCell)
    {
        IEnumerable<RoomCell>
            unvisitedCells = GetUnvisitedCells(currentCell); // get the unvisited cells around the current cell
        return unvisitedCells.FirstOrDefault();
    }

    /// <summary>
    /// Return IEnumerable of unvisited cells around the current cell in the grid (right, left, front, back)
    /// </summary>
    /// <param name="currentCell"></param>
    /// <returns></returns>
    private IEnumerable<RoomCell> GetUnvisitedCells(RoomCell currentCell)
    {
        Vector2 cellPosition = currentCell.GetCellPosition();// return the position of the cell in the grid (index x and y)
        if (cellPosition.x + 1 < _proGenParams.width)// check if the cell is not in the right corner of the grid
        {
            var cellToRight = _grid[(int)cellPosition.x + 1, (int)cellPosition.y];// get the cell to the right of the current cell
            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight;// return the cell if it is not visited
            }
        }

        if (cellPosition.x - 1 >= 0)// check if the cell is not in the left corner of the grid
        {
            var cellToLeft = _grid[(int)cellPosition.x - 1, (int)cellPosition.y];// get the cell to the left of the current cell
            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;// return the cell if it is not visited
            }
        }

        if (cellPosition.y + 1 < _proGenParams.height)// check if the cell is not in the top corner of the grid
        {
            var cellToFront = _grid[(int)cellPosition.x, (int)cellPosition.y + 1];// get the cell to the front of the current cell
            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;// return the cell if it is not visited
            }
        }

        if (cellPosition.y - 1 >= 0)// check if the cell is not in the bottom corner of the grid
        {
            var cellToBack = _grid[(int)cellPosition.x, (int)cellPosition.y - 1];// get the cell to the back of the current cell
            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;// return the cell if it is not visited
            }
        }
    }

    /// <summary>
    /// Clear the walls of the cell according to the position of the cell in the grid
    /// </summary>
    /// <param name="cell"></param>
    private void ClearWalls(RoomCell cell)
    {
        Vector2 cellPosition = cell.GetCellPosition(); // return the position of the cell in the grid (index x and y)
        if (cellPosition is { x: 0, y: 0 }) // if the cell is in the top left corner of the grid
        {
            cell.ClearBackWall();
            cell.ClearRightWall();
            _wallSections[RoomCellDirections.Front].Add(cell);
            _wallSections[RoomCellDirections.Left].Add(cell);
        }

        else if (cellPosition is { x: 0, y: > 0 } &&
                 cellPosition.y < _proGenParams.height - 1) // if the cell is in the top of the grid
        {
            cell.ClearBackWall();
            cell.ClearRightWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Front].Add(cell);
        }
        else if (cellPosition.x == 0 &&
                 cellPosition.y.Equals(_proGenParams.height - 1)) // if the cell is in the top right corner of the grid
        {
            cell.ClearBackWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Front].Add(cell);
            _wallSections[RoomCellDirections.Right].Add(cell);
        }

        else if (cellPosition is { x: > 0, y: 0 } &&
                 cellPosition.x < _proGenParams.width - 1) // if the cell is in the left of the grid
        {
            cell.ClearBackWall();
            cell.ClearRightWall();
            cell.ClearFrontWall();
            _wallSections[RoomCellDirections.Left].Add(cell);
        }

        else if (cellPosition.x.Equals(_proGenParams.width - 1) &&
                 cellPosition.y == 0) // if the cell is in the bottom left corner of the grid
        {
            cell.ClearFrontWall();
            cell.ClearRightWall();
            _wallSections[RoomCellDirections.Back].Add(cell);
            _wallSections[RoomCellDirections.Left].Add(cell);
        }

        else if (cellPosition.x.Equals(_proGenParams.width - 1) && cellPosition.y < _proGenParams.height - 1 &&
                 cellPosition.y > 0) // if the cell is in the bottom of the grid
        {
            cell.ClearFrontWall();
            cell.ClearRightWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Back].Add(cell);
        }
        else if (cellPosition.x.Equals(_proGenParams.width - 1) &&
                 cellPosition.y.Equals(_proGenParams.height -
                                       1)) // if the cell is in the bottom right corner of the grid
        {
            cell.ClearFrontWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Back].Add(cell);
            _wallSections[RoomCellDirections.Right].Add(cell);
        }

        else if (cellPosition.x > 0 && cellPosition.x < _proGenParams.width - 1 &&
                 cellPosition.y.Equals(_proGenParams.height - 1)) // if the cell is in the right of the grid
        {
            cell.ClearBackWall();
            cell.ClearFrontWall();
            cell.ClearLeftWall();
            _wallSections[RoomCellDirections.Right].Add(cell);
        }
        else if (cellPosition.x > 0 && cellPosition.x < _proGenParams.width - 1 && cellPosition.y > 0 &&
                 cellPosition.y < _proGenParams.height - 1) // if the cell is in the middle of the grid
        {
            cell.ClearBackWall();
            cell.ClearFrontWall();
            cell.ClearRightWall();
            cell.ClearLeftWall();
        }
    }

    /// <summary>
    ///  Replace the active wall with door with the constraint of the distance between the doors
    /// </summary>
    private void RandomReplaceActiveWallWithDoorConstraint()
    {
        foreach (var wallSection in _wallSections) // loop through the wall sections (front, back, left, right)
        {
            //check if the door is space with the parameters minDistanceBetweenDoors and maxDistanceBetweenDoors
            int minDistanceBetweenDoors = 2; // should be in pro gen scriptable object 
            int maxDistanceBetweenDoors = 10; // should be in pro gen scriptable object
            int doorNumber = 0;
            RoomCell previousCell = null;
            for (int j = _wallDoorPerSection[wallSection.Key]; j < _proGenParams.DoorPerWallNumber; j++)
            {
                int wallDoorIndex =
                    _gridRandom.Next(0, _proGenParams.WallDoorPrefabs.Count); // get a random door prefab in the list
                var wallDoor = _proGenParams.WallDoorPrefabs[wallDoorIndex]; // get the door prefab
                var cell = wallSection.Value[
                    _gridRandom.Next(0, wallSection.Value.Count)]; // get a random cell in the wall section
                if (cell == previousCell) // if the cell is the same as the previous cell
                {
                    j--; // decrement the j
                    continue; // continue to the next iteration
                }

                if (doorNumber ==
                    0) // if doornumber is 0 (the first Door) there is no need to check Distance and if there is already a Door in the cell
                {
                    cell.ReplaceWall(wallSection.Key, wallDoor); // replace the wall with the door
                    doorNumber++;
                }
                else
                {
                    if (Vector2.Distance(cell.GetCellPosition(), previousCell.GetCellPosition()) >
                        minDistanceBetweenDoors &&
                        Vector3.Distance(cell.GetCellPosition(), previousCell.GetCellPosition()) <
                        maxDistanceBetweenDoors) // check if the distance between the current cell and the previous cell is between the minDistanceBetweenDoors and maxDistanceBetweenDoors
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
        foreach (var wallSection in _wallSections) // loop through the wall sections (front, back, left, right)
        {
            int windowNumber = 0; // initialize the window number
            RoomCell previousCell = null; // initialize the previous cell
            for (int j = 0; j < _proGenParams.WindowPerWallNumber; j++)
            {
                int wallWindowIndex =
                    _gridRandom.Next(0,
                        _proGenParams.WallWindowsPrefabs.Count); // get a random window prefab in the list
                var wallWindow = _proGenParams.WallWindowsPrefabs[wallWindowIndex]; // get the window prefab
                var cell = wallSection.Value[
                    _gridRandom.Next(0, wallSection.Value.Count)]; // get a random cell in the wall section
                if (cell == previousCell) // if the cell is the same as the previous cell
                {
                    j--; // decrement the j
                    continue; // continue to the next iteration
                }

                if (windowNumber ==
                    0) // if window number is 0 (the first window) there is no need to check Distance and if there is already a window in the cell
                {
                    cell.ReplaceWall(wallSection.Key, wallWindow);
                    windowNumber++;
                }
                else
                {
                    if (Vector2.Distance(cell.GetCellPosition(), previousCell.GetCellPosition()) > 1 &&
                        !cell.GetComponentInChildren<WallDoor>())// check if the distance between the current cell and the previous cell is greater than 1 and there is no door in the cell Hardcoded value should be in the pro gen scriptable object
                    {
                        cell.ReplaceWall(wallSection.Key, wallWindow);// replace the wall with the window
                        windowNumber++;// increment the window number
                    }
                    else
                    {
                        j--;
                    }
                }

                previousCell = cell;// set the previous cell to the current cell
            }
        }
    }
    
    private void ApplyTextures()
    {
        foreach (var wallSection in _wallSections) // loop through the wall sections (front, back, left, right)
        {
            int wallMaterialIndex =
                _gridRandom.Next(0, _proGenParams.WallMaterials.Count); // get a random wall material in the list
            foreach (var cell in wallSection.Value) // loop through the cells in the wall section
            {
                cell.ApplyTexture(wallMaterialIndex, wallSection.Key); // apply the texture to the wall
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
            if (x == 0 || x.Equals(width) || x.Equals(spacing) || x.Equals(width - spacing))
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
            if (z == 0 || z.Equals(height) || z.Equals(spacing) || z.Equals(height - spacing))
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