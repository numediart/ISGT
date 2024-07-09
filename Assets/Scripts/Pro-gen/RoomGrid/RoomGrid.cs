using System.Collections.Generic;
using System.Linq;
using Pro_gen.RoomCell;
using UnityEngine;
using Random = System.Random;


namespace Pro_gen.RoomGrid
{
    public class RoomGrid : MonoBehaviour
    {
        private RoomsGenerationScriptableObject _roomsGenerationData;

        //   [SerializeField] private ProceduralPropPlacer _proceduralPropPlacer;
        private Random _gridRandom;
        private RoomCell.RoomCell[,] _grid;

        private Dictionary<RoomCellDirections, List<RoomCell.RoomCell>> _wallSections;
        private Dictionary<RoomCellDirections, int> _wallDoorPerSection;

        public void InitGrid(int roomSeed, RoomsGenerationScriptableObject roomsGenerationData)
        {
            _roomsGenerationData = roomsGenerationData;
            _gridRandom = new Random(roomSeed);
            _roomsGenerationData.width = _gridRandom.Next(2, _roomsGenerationData.MaxRoomWidth);
            //_roomsGenerationData.MaxRoomWidth; // _gridRandom.Next(2, _roomsGenerationData.MaxRoomWidth);
            _roomsGenerationData.height = _gridRandom.Next(2, _roomsGenerationData.MaxRoomHeight);
            //  _roomsGenerationData.MaxRoomHeight; // _gridRandom.Next(2, _roomsGenerationData.MaxRoomHeight);
            _grid = new RoomCell.RoomCell[_roomsGenerationData.width,
                _roomsGenerationData
                    .height]; // initialize the grid with the width and height in pro gen scriptable Object
            _wallSections =
                new
                    Dictionary<RoomCellDirections,
                        List<RoomCell.RoomCell>> // initialize the wall sections with the directions store all the cells that have walls in the same direction
                    {
                        { RoomCellDirections.Front, new List<RoomCell.RoomCell>() },
                        { RoomCellDirections.Back, new List<RoomCell.RoomCell>() },
                        { RoomCellDirections.Left, new List<RoomCell.RoomCell>() },
                        { RoomCellDirections.Right, new List<RoomCell.RoomCell>() }
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
            for (long i = 0; i < _roomsGenerationData.width; i++)
            {
                for (long j = 0; j < _roomsGenerationData.height; j++)
                {
                    _grid[i, j] = Instantiate(_roomsGenerationData.RoomCellPrefab, new Vector3(i * 2.5f, 0, j * 2.5f),
                        Quaternion.identity);
                    _grid[i, j].name = $"RoomCell_{i}_{j}";
                    _grid[i, j].Position = new Vector2(i, j);
                    _grid[i, j].transform.parent = transform;
                }
            }

            GenerateCell(_grid[0, 0]); // generate the room starting from the cell 0,0
        }

        /// <summary>
        /// Generate the room starting from the current cell and visit all the cells that are not visited to generate the room by clearing the walls of the cells
        /// </summary>
        /// <param name="currentCell"></param>
        private void GenerateCell(RoomCell.RoomCell currentCell)
        {
            currentCell.Visit(); // visit the current cell (remove the unvisited block)
            ClearWalls(currentCell); // clear the walls of the current cell according to the position of the cell in the grid
            RoomCell.RoomCell nextCell;
            do
            {
                nextCell = GetNextUnvisitedCell(currentCell); // get the next unvisited cell
                if (nextCell != null)
                {
                    GenerateCell(nextCell); // generate the room starting from the next cell
                }
            } while (nextCell != null); // loop until there is no unvisited cell
        }

        /// <summary>
        /// Get the next unvisited cell around the current cell in the grid (right, left, front, back)
        /// </summary>
        /// <param name="currentCell"></param>
        /// <returns></returns>
        private RoomCell.RoomCell GetNextUnvisitedCell(RoomCell.RoomCell currentCell)
        {
            IEnumerable<RoomCell.RoomCell>
                unvisitedCells = GetUnvisitedCells(currentCell); // get the unvisited cells around the current cell
            return unvisitedCells.FirstOrDefault();
        }

        /// <summary>
        /// Return IEnumerable of unvisited cells around the current cell in the grid (right, left, front, back)
        /// </summary>
        /// <param name="currentCell"></param>
        /// <returns></returns>
        private IEnumerable<RoomCell.RoomCell> GetUnvisitedCells(RoomCell.RoomCell currentCell)
        {
            Vector2
                cellPosition =
                    currentCell.GetCellPosition(); // return the position of the cell in the grid (index x and y)
            if (cellPosition.x + 1 <
                _roomsGenerationData.width) // check if the cell is not in the right corner of the grid
            {
                var cellToRight =
                    _grid[(int)cellPosition.x + 1,
                        (int)cellPosition.y]; // get the cell to the right of the current cell
                if (cellToRight.IsVisited == false)
                {
                    yield return cellToRight; // return the cell if it is not visited
                }
            }

            if (cellPosition.x - 1 >= 0) // check if the cell is not in the left corner of the grid
            {
                var cellToLeft =
                    _grid[(int)cellPosition.x - 1, (int)cellPosition.y]; // get the cell to the left of the current cell
                if (cellToLeft.IsVisited == false)
                {
                    yield return cellToLeft; // return the cell if it is not visited
                }
            }

            if (cellPosition.y + 1 <
                _roomsGenerationData.height) // check if the cell is not in the top corner of the grid
            {
                var cellToFront =
                    _grid[(int)cellPosition.x,
                        (int)cellPosition.y + 1]; // get the cell to the front of the current cell
                if (cellToFront.IsVisited == false)
                {
                    yield return cellToFront; // return the cell if it is not visited
                }
            }

            if (cellPosition.y - 1 >= 0) // check if the cell is not in the bottom corner of the grid
            {
                var cellToBack =
                    _grid[(int)cellPosition.x, (int)cellPosition.y - 1]; // get the cell to the back of the current cell
                if (cellToBack.IsVisited == false)
                {
                    yield return cellToBack; // return the cell if it is not visited
                }
            }
        }

        /// <summary>
        /// Clear the walls of the cell according to the position of the cell in the grid
        /// </summary>
        /// <param name="cell"></param>
        private void ClearWalls(RoomCell.RoomCell cell)
        {
            Vector2
                cellPosition = cell.GetCellPosition(); // return the position of the cell in the grid (index x and y)
            if (cellPosition is { x: 0, y: 0 }) // if the cell is in the top left corner of the grid
            {
                cell.ClearBackWall();
                cell.ClearRightWall();
                _wallSections[RoomCellDirections.Front].Add(cell);
                _wallSections[RoomCellDirections.Left].Add(cell);
            }

            else if (cellPosition is { x: 0, y: > 0 } &&
                     cellPosition.y < _roomsGenerationData.height - 1) // if the cell is in the top of the grid
            {
                cell.ClearBackWall();
                cell.ClearRightWall();
                cell.ClearLeftWall();
                _wallSections[RoomCellDirections.Front].Add(cell);
            }
            else if (cellPosition.x == 0 &&
                     cellPosition.y.Equals(_roomsGenerationData.height -
                                           1)) // if the cell is in the top right corner of the grid
            {
                cell.ClearBackWall();
                cell.ClearLeftWall();
                _wallSections[RoomCellDirections.Front].Add(cell);
                _wallSections[RoomCellDirections.Right].Add(cell);
            }

            else if (cellPosition is { x: > 0, y: 0 } &&
                     cellPosition.x < _roomsGenerationData.width - 1) // if the cell is in the left of the grid
            {
                cell.ClearBackWall();
                cell.ClearRightWall();
                cell.ClearFrontWall();
                _wallSections[RoomCellDirections.Left].Add(cell);
            }

            else if (cellPosition.x.Equals(_roomsGenerationData.width - 1) &&
                     cellPosition.y == 0) // if the cell is in the bottom left corner of the grid
            {
                cell.ClearFrontWall();
                cell.ClearRightWall();
                _wallSections[RoomCellDirections.Back].Add(cell);
                _wallSections[RoomCellDirections.Left].Add(cell);
            }

            else if (cellPosition.x.Equals(_roomsGenerationData.width - 1) &&
                     cellPosition.y < _roomsGenerationData.height - 1 &&
                     cellPosition.y > 0) // if the cell is in the bottom of the grid
            {
                cell.ClearFrontWall();
                cell.ClearRightWall();
                cell.ClearLeftWall();
                _wallSections[RoomCellDirections.Back].Add(cell);
            }
            else if (cellPosition.x.Equals(_roomsGenerationData.width - 1) &&
                     cellPosition.y.Equals(_roomsGenerationData.height -
                                           1)) // if the cell is in the bottom right corner of the grid
            {
                cell.ClearFrontWall();
                cell.ClearLeftWall();
                _wallSections[RoomCellDirections.Back].Add(cell);
                _wallSections[RoomCellDirections.Right].Add(cell);
            }

            else if (cellPosition.x > 0 && cellPosition.x < _roomsGenerationData.width - 1 &&
                     cellPosition.y.Equals(_roomsGenerationData.height - 1)) // if the cell is in the right of the grid
            {
                cell.ClearBackWall();
                cell.ClearFrontWall();
                cell.ClearLeftWall();
                _wallSections[RoomCellDirections.Right].Add(cell);
            }
            else if (cellPosition.x > 0 && cellPosition.x < _roomsGenerationData.width - 1 && cellPosition.y > 0 &&
                     cellPosition.y < _roomsGenerationData.height - 1) // if the cell is in the middle of the grid
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
        public void RandomReplaceActiveWallWithDoorConstraint(Random random)
        {
            foreach (var wallSection in _wallSections) // loop through the wall sections (front, back, left, right)
            {
                int doorPerWallNumber = 0;
                switch (wallSection.Key)
                {
                    case RoomCellDirections.Left:
                        doorPerWallNumber =
                            _roomsGenerationData.DoorPerWallNumber / 10;
                        break;
                    case RoomCellDirections.Right:
                        doorPerWallNumber =
                            _roomsGenerationData.DoorPerWallNumber / 10;
                        break;
                    case RoomCellDirections.Front:
                        doorPerWallNumber =
                            _roomsGenerationData.DoorPerWallNumber / 10;
                        break;
                    case RoomCellDirections.Back:
                        doorPerWallNumber =
                            _roomsGenerationData.DoorPerWallNumber / 10;
                        break;
                }

                int doorNumber = 0;
                RoomCell.RoomCell previousCell = null;
                int attempts = 0;
                for (int j = _wallDoorPerSection[wallSection.Key]; j < doorPerWallNumber; j++)
                {
                    if (attempts > doorPerWallNumber * 2)
                        break;
                    int wallDoorIndex =
                        random.Next(0,
                            _roomsGenerationData.WallDoorPrefabs.Count); // get a random door prefab in the list
                    var wallDoor = _roomsGenerationData.WallDoorPrefabs[wallDoorIndex]; // get the door prefab
                    var cell = wallSection.Value[
                        random.Next(0, wallSection.Value.Count)]; // get a random cell in the wall section
                    if (cell == previousCell) // if the cell is the same as the previous cell
                    {
                        attempts++; // increment the attempts
                        j--; // decrement the j
                        continue; // continue to the next iteration
                    }

                    cell.ReplaceWall(wallSection.Key, wallDoor); // replace the wall with the door
                    doorNumber++;


                    _wallDoorPerSection[wallSection.Key] = doorNumber;
                    previousCell = cell;
                }
            }
            RoomsGenerator.NumberOfDoorsPlaced = _wallDoorPerSection.Sum(x => x.Value);
        }

        public void RandomReplaceActiveWithWindowConstraint(Random random)
        {
            foreach (var wallSection in _wallSections) // loop through the wall sections (front, back, left, right)
            {
                int windowNumberPerWall = 0;

                switch (wallSection.Key)
                {
                    case RoomCellDirections.Left:
                        windowNumberPerWall =
                            _roomsGenerationData.WindowPerWallNumber / 10 * (_roomsGenerationData.width / 2);
                        break;
                    case RoomCellDirections.Right:
                        windowNumberPerWall =
                            _roomsGenerationData.WindowPerWallNumber / 10 * (_roomsGenerationData.width / 2);
                        break;
                    case RoomCellDirections.Front:
                        windowNumberPerWall =
                            _roomsGenerationData.WindowPerWallNumber / 10 * (_roomsGenerationData.height / 2);
                        break;
                    case RoomCellDirections.Back:
                        windowNumberPerWall =
                            _roomsGenerationData.WindowPerWallNumber / 10 * (_roomsGenerationData.height / 2);
                        break;
                }

                int attempts = 0;
                RoomCell.RoomCell previousCell = null; // initialize the previous cell
                for (int j = 0; j < windowNumberPerWall; j++)
                {
                    if (attempts > windowNumberPerWall * 2)
                        break;
                    int wallWindowIndex =
                        random.Next(0,
                            _roomsGenerationData.WallWindowsPrefabs.Count); // get a random window prefab in the list
                    var wallWindow = _roomsGenerationData.WallWindowsPrefabs[wallWindowIndex]; // get the window prefab
                    var cell = wallSection.Value[
                        random.Next(0, wallSection.Value.Count)]; // get a random cell in the wall section
                    if (cell == previousCell) // if the cell is the same as the previous cell
                    {
                        attempts++; // increment the attempts
                        j--; // decrement the j
                        continue; // continue to the next iteration
                    }
                    
                    cell.ReplaceWall(wallSection.Key, wallWindow);

                    previousCell = cell; // set the previous cell to the current cell
                }
            }
            RoomsGenerator.NumberOfWindowsPlaced = _wallSections.Sum(x => x.Value.Count);
        }

        public void ApplyTextures()
        {
            foreach (var wallSection in _wallSections) // loop through the wall sections (front, back, left, right)
            {
                int wallMaterialIndex =
                    _gridRandom.Next(0,
                        _roomsGenerationData.WallMaterials.Count); // get a random wall material in the list

                int windowMaterialIndex =
                    _gridRandom.Next(0,
                        _roomsGenerationData.WindowMaterials.Count); // get a random window material in the list

                foreach (var cell in wallSection.Value) // loop through the cells in the wall section
                {
                    cell.ApplyWallTexture(wallMaterialIndex, wallSection.Key); // apply the texture to the wall
                    cell.ApplyWindowTexture(windowMaterialIndex, wallSection.Key); // apply the texture to the window
                }
            }

            int floorMaterialIndex =
                _gridRandom.Next(0,
                    _roomsGenerationData.FloorMaterials.Count); // get a random floor material in the list
            int ceilingMaterialIndex =
                _gridRandom.Next(0,
                    _roomsGenerationData.CeilingMaterials.Count); // get a random ceiling material in the list


            foreach (var cell in _grid) // loop through the cells in the grid
            {
                cell.ApplyFloorTexture(floorMaterialIndex); // apply the texture to the floor
                cell.ApplyCeilingTexture(ceilingMaterialIndex); // apply the texture to the ceiling
            }
        }


        public List<GameObject> GetAllWalls()
        {
            List<GameObject> walls = new List<GameObject>();
            foreach (KeyValuePair<RoomCellDirections, List<RoomCell.RoomCell>> wallSection in _wallSections)
            {
                foreach (RoomCell.RoomCell cell in wallSection.Value)
                {
                    walls.Add(cell.GetActiveWall(wallSection.Key));
                }
            }

            return walls;
        }
    }
}