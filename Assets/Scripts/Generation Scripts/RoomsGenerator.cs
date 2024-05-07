using UnityEngine;
using InternalRealtimeCSG;
using System.Collections.Generic;
using System;
using System.Linq;
using Utils;

public class RoomsGenerator : MonoBehaviour
{
    #region Public Fields

    public List<GameObject> RoomsCreated = new List<GameObject>();
    public RoomsGenerationScriptableObject RoomsGenerationData;
    public GeneratorsContainer GeneratorsContainer;

    #endregion

    #region Private Fields

    [SerializeField] private List<int> _usedSeeds = new List<int>();
    private SeedsProvider _seedsProvider;

    #endregion

    #region Seeds Management Methods

   /* private void Start()
    {
        GenerateRooms();
    }*/

    private void InitiateSeed()
    {
        while (true)
        {
            int newSeed = _seedsProvider.MainSeed;

            if (!_usedSeeds.Contains(newSeed))
            {
                UnityEngine.Random.InitState(newSeed);
                _usedSeeds.Add(newSeed);
                break;
            }
        }
    }

    #endregion

    #region Methods Called By Buttons

    /// <summary>
    /// This method calls all the methods building the final rooms without painting on surfaces. 
    /// (Creation of the room shells, then the openings in the walls and finally the objects).
    /// </summary>
    public void GenerateRooms()
    {
/*        Debug.Log($"Distance between the two eyes : {Camera.main.stereoSeparation * 2f}");*/
        _seedsProvider = new SeedsProvider();
        InitiateSeed();
        GeneratorsContainer.DatabaseGenerator.InitiateSeed(_seedsProvider);

        EmptyRoomsGeneration();

        GeneratorsContainer.OpeningsGenerator.OpeningsGeneration(RoomsCreated);

         foreach (GameObject room in RoomsCreated)
              GeneratorsContainer.ObjectsGenerator.ObjectsGeneration(room);
    }

    /// <summary>
    /// Applies "paintings" on the surfaces of all rooms.
    /// </summary>
    public void ApplyMaterialsForAllRooms()
    {
        foreach (GameObject room in RoomsCreated)
            ApplyMaterials(room);
    }

    /// <summary>
    /// Destroy all the rooms created in the unity scene.
    /// </summary>
    public void ClearScene()
    {
        if (RoomsCreated.Count > 0)
        {
            foreach (GameObject room in RoomsCreated)
                Destroy(room);
        }

        RoomsCreated.Clear();
        RoomsGenerationData.MaterialsDictionary.Clear();
    }

    #endregion

    #region Empty Rooms Generation Methods

    /// <summary>
    /// Instantiates randomly a quantity choosen by the user of room shells among the prefabricated room shells.
    /// </summary>
    private void EmptyRoomsGeneration()
    {
        ClearScene();
        RoomsGenerationData.MaterialsDictionary.Add("Grounds", RoomsGenerationData.GroundMaterials);
        RoomsGenerationData.MaterialsDictionary.Add("Ceilings", RoomsGenerationData.CeilingMaterials);
        RoomsGenerationData.MaterialsDictionary.Add("Walls", RoomsGenerationData.WallMaterials);

        for (int i = 0; i < RoomsGenerationData.NumberOfEmptyRoomsOnScene; i++)
        {
            int rnd = UnityEngine.Random.Range(0, RoomsGenerationData.EmptyRooms.Count);

            GameObject room = Instantiate(RoomsGenerationData.EmptyRooms[rnd],
                new Vector3(i * RoomsGenerationData.DistanceBetweenRoomsCenters, 0, 0), Quaternion.identity);
            RoomsCreated.Add(room);
        }
    }

    #endregion

    #region Materials Management Methods

    /// <summary>
    /// Applies painting on every surface of a given room.
    /// </summary>
    /// <param name="room"></param>
    public void ApplyMaterials(GameObject room)
    {
        for (int i = 0; i < room.transform.childCount; i++)
        {
            GameObject category = room.transform.GetChild(i).gameObject;

            if (RoomsGenerationData.MaterialsDictionary.Keys.Contains(category.name))
            {
                int rnd = UnityEngine.Random.Range(0, RoomsGenerationData.MaterialsDictionary[category.name].Count);

                for (int j = 0; j < category.transform.childCount; j++)
                {
                    try
                    {
                        category.transform.GetChild(j).gameObject.TryGetComponent(out MeshRenderer meshRenderer);
                        meshRenderer.material = RoomsGenerationData.MaterialsDictionary[category.name][rnd];
                    }
                    catch (Exception)
                    {
                        GameObject brushesMeshRenderer =
                            GetBrushesMeshRenderer(category.transform.GetChild(j).gameObject);
                        brushesMeshRenderer.TryGetComponent(out GeneratedMeshInstance generatedMeshInstance);
                        generatedMeshInstance.RenderMaterial =
                            RoomsGenerationData.MaterialsDictionary[category.name][rnd];
                    }
                }
            }
        }
    }

    #endregion

    #region Information And Objects Getting Methods

    public static float GetOpeningWidth(Vector3 colliderSize)
    {
        return (colliderSize.x > colliderSize.z) ? colliderSize.x : colliderSize.z;
    }

    public float GetWallWidth(GameObject wall)
    {
        return (wall.transform.localScale.x > wall.transform.localScale.z)
            ? wall.transform.localScale.x
            : wall.transform.localScale.z;
    }

    /// <summary>
    /// Returns the object corresponding to one of the 4 categories of a room : Grounds, Ceilings, Walls and Objects.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="wantedCategory"></param>
    /// <returns></returns>
    public static GameObject GetRoomCategory(GameObject room, RoomCategory wantedCategory)
    {
        for (int i = 0; i < room.transform.childCount; i++)
        {
            RoomCategory roomCategory;

            if (Enum.TryParse(room.transform.GetChild(i).gameObject.name, out roomCategory) &&
                roomCategory == wantedCategory)
                return room.transform.GetChild(i).gameObject;
        }

        return null;
    }

    /// <summary>
    /// Returns all the objects contained in a room category of a given room.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="wantedCategory"></param>
    /// <returns></returns>
    public static List<GameObject> GetRoomCategoryObjects(GameObject room, RoomCategory wantedCategory)
    {
        GameObject category = GetRoomCategory(room, wantedCategory);

        List<GameObject> categoryObjects = new List<GameObject>();

        for (int i = 0; i < category.transform.childCount; i++)
            categoryObjects.Add(category.transform.GetChild(i).gameObject);

        return categoryObjects;
    }

    /// <summary>
    /// Gives the total number of doors and windows of a given room.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static int GetNumberOfOpenings(GameObject room)
    {
        int result = 0;

        List<GameObject> walls = GetRoomCategoryObjects(room, RoomCategory.Walls);

        foreach (GameObject wall in walls)
        {
            for (int i = 0; i < wall.transform.childCount; i++)
            {
                if (wall.transform.GetChild(i).gameObject.GetComponent<Opening>())
                    result++;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the component Mesh Renderer of a CSG model.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public GameObject GetBrushesMeshRenderer(GameObject model)
    {
        GameObject brushesMeshRenderer = null;

        for (int i = 0; i < model.transform.childCount; ++i)
        {
            if (model.transform.GetChild(i).name == "[generated-meshes]")
                brushesMeshRenderer = model.transform.GetChild(i).GetChild(0).gameObject;
        }

        return brushesMeshRenderer;
    }

    /// <summary>
    /// Returns the first component Mesh Collider of a CSG model.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static GameObject GetBrushesFirstMeshCollider(GameObject model)
    {
        GameObject brushesMeshCollider = null;

        for (int i = 0; i < model.transform.childCount; ++i)
        {
            GameObject generatedMeshes;

            if (model.transform.GetChild(i).name == "[generated-meshes]")
            {
                generatedMeshes = model.transform.GetChild(i).gameObject;

                for (int j = 0; j < generatedMeshes.transform.childCount; j++)
                {
                    generatedMeshes.transform.GetChild(j).gameObject
                        .TryGetComponent(out MeshCollider meshCollider);
                    if (meshCollider.Equals(null))
                    {
                        brushesMeshCollider = generatedMeshes.transform.GetChild(j).gameObject;
                        break;
                    }
                }

                break;
            }
        }

        return brushesMeshCollider;
    }

    /// <summary>
    /// Verifies if the camera position is located inside one of the walls of a given room.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="nextCameraPosition"></param>
    /// <returns></returns>
    public static bool IsCameraInsideAWall(GameObject room, Vector3 nextCameraPosition)
    {
        List<GameObject> walls = GetRoomCategoryObjects(room, RoomCategory.Walls);
        Vector3 genericWallDimensions = (walls[0].transform.childCount == 0)
            ? walls[0].transform.localScale
            : walls[0].transform.GetChild(0).localScale;
        float wallThickness = (genericWallDimensions.x < genericWallDimensions.z)
            ? genericWallDimensions.x
            : genericWallDimensions.z;

        GameObject genericWall;
        float wallWidth;
        Vector3 wallToCamera;
        float forwardDistanceToWall;
        float sideDistanceToWall;

        foreach (GameObject wall in walls)
        {
            genericWall = (wall.transform.childCount == 0) ? wall : wall.transform.GetChild(0).gameObject;
            wallWidth = (genericWallDimensions.x > genericWallDimensions.z)
                ? genericWallDimensions.x
                : genericWallDimensions.z;
            wallToCamera = nextCameraPosition - genericWall.transform.position;
            forwardDistanceToWall = Vector3.Project(wallToCamera, -genericWall.transform.forward).magnitude;
            sideDistanceToWall = Vector3.Project(wallToCamera, genericWall.transform.right).magnitude;

            if (forwardDistanceToWall <= wallThickness / 2f && sideDistanceToWall < wallWidth / 2f)
                return true;
        }

        return false;
    }

    #endregion
}

public enum RoomCategory
{
    Grounds,
    Ceiling,
    Walls,
    Objects
}