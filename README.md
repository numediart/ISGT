# Virtual Indoor Scene Generator

## How to use the project

What do you need to use the project :

 - Unity Hub
 - Unity Editor 2020.3.25f1
 - Git (optional)

### Installing and opening the project

The first step is to clone the repository or to download the folders in a .zip file. You should then have these folders in a global folder on your computer : 

![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img1.png)

The second step is to execute Unity Hub and open the project by clicking on the "Open" button and by choosing the folder containing the project folders (Assets, Packages...).
Once the project is open in Unity Editor, you will see this window :
![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img2.png)

### Changing some generation parameters

In the section "Project" you can see the structure of the folder "Assets". If you click on the "Data" folder, you will visualize 3 objects named "RoomsGeneration", "ObjectsGeneration" and "DatabaseGeneration". The object "RoomsGeneration" contains objects and constant values used for the general process of rooms generation, the object "ObjectsGeneration" contains constant values and objects used for the objects generation in the different empty rooms and the object "DatabaseGeneration" contains constant values used for the construction of the database. To display and be able to modify the parameters, you need to click on one of these 3 objects and then you will be able to see and modify the corresponding parameters in the section "Inspector".

**Explanation of the different parameters :**

 - **RoomsGenerationData :**
	 - Empty Rooms : List of the different pre-fabricated empty rooms.
	 - Windows : List of the different windows.
	 - Doors : List of the different doors.
	 - Ground Materials : List of the textures for grounds.
	 - Cieling Materials : List of the textures for cielings.
	 - Wall Materials : List of the textures for walls.
	 - Window Structure Materials : List of the textures for window structures.
	 - Door Frame Materials : List of the textures for door frames.
	 - Door Materials : List of the textures for doors.
	 - Number Of Empty Rooms On Scene : Quantity of rooms to place on the scene.
	 - Distance Between Rooms Centers : Distance (in meters) between the centers of each room on the scene.
	 - Minimum Distance Between Borders : Minimum distance in width and height between two openings and between an opening and the edge of a wall.
	 - Window Per Wall Minimum Number : Minimum number of windows to place on each wall.
	 - Window Per Wall Maximum Number : Maximum number of windows to place on each wall.
	 - Door Per Room Minimum Number : Minimum number of doors to place in each room.
	 - Door Per Room Maximum Number : Maximum number of doors to place in each room.
	 - Door Per Wall Maximum Number : Maximum number of doors allowed on one wall.
	 
 - **ObjectsGenerationData :**
	 - Objects : List of the decorative objects and furnitures to place in the rooms. 
	 - Minimum Distance Between Objects : Minimum distance (in meters) between each object to place.
	 - Objects Minimum Number Per Room : Minimum number of objects to place in each room.
	 - Objects Maximum Number Per Room : Maximum number of objects to place in each room.
	 - Objects Layout JSON File Path : File path of the AI model output JSON file containing the objects layout description.
	 
 - **DatabaseGenerationData :**
	 - Screenshots Number Per Room : Number of screenshots to take in each room.
	 - Camera Minimum Distance From Wall : Minimum distance (in meters) between the camera and the walls.
	 - Camera Minimum Distance From Ground And Cieling : Minimum distance (in meters) between the camera and the ground and cieling.
	 - Camera Minimum Distance From Objects : Minimum distance (in meters) between the camera and the objects.
	 - Maximum Camera X Rotation : The maximum rotation (in degrees) of the camera around its right axis.
	 - Maximum Camera Y Rotation : The maximum rotation (in degrees) of the camera around its up axis.
	 - Maximum Camera Z Rotation : The maximum rotation (in degrees) of the camera around its forward axis.

### Using the camera stereo mode to take screenshots with both eyes

You can use the camera stereo mode to take a screenshot with the left eye and one with the right eye from every view point of the camera. 
To do so, you will have to install the Unity Mock HMD pkug-in in the project. For the installation, you need to open the Edit>Project Settings window and click on the last line of the settings called "XR Plug-in Management". Then you can click on the case "Initialize XR on Startup" and on the case "Unity Mock HMD". This will install the Unity Mock HMD plug-in and it will add a field on the camera component of the camera on the scene. 
The second step is to click on the "Main camera" in the section "Hierarchy", visualize the fields of the camera component of the camera in the section "Inspector" and change the last field of this component called "Target eye" from "None (Main Display)" to "Both".
The last step is to go in the DatabaseGenerator script that you can open by clicking on the file Assets>Scripts>Generation_Scripts>DatabaseGenerator.cs in the section "Project". Go to the line 99 and follow the instructions given by the comments in lines 99 and 102. The distance between the two eyes is equal to 4,4cm but you can verify this information by uncommenting the line 57 of the RoomsGenerator.cs file and executing the project in the Untity Editor (the information will be displayed in the "Console" section).

If you want to come back to a "normal" behaviour and not using the camera stereo mode, you simply have to revert the changes you made by commenting and commenting some parts of the DatabaseGenerator.cs file and you need to remove the "Unity Mock HMD" plug-in. To uninstall this plug-in, you have to open the Window>Package Manager window, click on the "MockHMD XR Plugin" line and click on the "Remove" button.

### Generating the database

The database generation is done in two steps :
- The generation of the rooms (with windows and doors on the walls and objects generated on the ground).
- The generation of the database (screenshots and data collected by the camera from different view points).

There are several objects in the section "Hierarchy" :
![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img3.png)

If you click on the object named "ObjectsGenerator", you will see the field "Generation Type" with a 2 options dropdown menu : "Random Generation" and "Realistic Generation With AI Model". If you choose the first option, objects will be randomly generated on the floor of the room with a random quantity (between a minimum number and a maximum number of objects). If you choose the second option, objects will be placed in the room according to the JSON file generated by an AI model and containing the objects layout description.

If you click on the object named "RoomsGenerator", you will see several fields in the section "Inspector" including 3 buttons :
 - "Generate Rooms" : Instantiate a specific amount of empty rooms on the scene, create and randomly place the openings (doors and windows) on the walls and randomly place objects on the ground. Clicking on this button if rooms have already been created will delete the existing rooms and create new ones.
 - "Apply Materials" : Applies textures and colors on the ground, the cieling and the walls.
 - "Clear Scene" : Delete the rooms created. 

The final step to generate the database, once you have clicked on the "Generate Rooms" button and the rooms have been created, is to click on the (little image) button located at the top center of the Unity Editor window. The image and data collection will begin and you will be able to visualize the different view points from every room.

The images are stored in the folder "Photographs" and the openings data is stored in JSON files contained in the "OpeningsData" folder.
These folders are located at the same level as the "Assets" folder and the other project folders :

![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img4.png)

To be able to keep the correspondence between an image and the corresponding JSON file with openings data, the image and its corresponding JSON file have the same name.

## How to add new assets

### Adding new empty rooms

An empty room has a specific structure. It's an object containing 4 other objects : the objects named "Grounds", "Cielings", "Walls" and "Objects". Every 3D object that constitutes a part of the room has to be a child object of the appropriate "category" object (Grounds, Cielings or Walls).
![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img5.png)

Building a special shaped room using other 3D objects than the basic Unity 3D objects is possible but only using the package RealtimeCSG contained in the project (how to use this package : [Updates | RealtimeCSG](https://realtimecsg.com/))

Finally, once you've build your new empty room with grounds, cielings and full walls on the Unity scene, you can drag and drop the empty room object from the scene to the folder Assets>Prefabs>Rooms that you can open and visualize in the section "Project". Then, the final step is to add the empty room object to the "Rooms" list contained in the RoomsGenerationData. To do so, you need to open the "Data" folder and click on the RoomsGenerationData in the "Project" section of the Editor. It is now possible to visualize the different fields of the object RoomsGenerationData, especially the field "Rooms" in the "Inspector" section. You can now lock the "Inspector" section with the padlock at the top right corner of the section, open the Assets>Prefabs>Rooms folder in the "Project" section and ultimately drag the new empty room and drop it in the "Rooms" field of the RoomsGenerationData in the "Inspector" section.

### Adding new objects

Decorative objects and furnitures need to have the layer Object. They also need to have a 3D bounding box object as child object with the name "BoundingBox" and the right dimensions (the 3D box must surround the entire object)
![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img6.png)

Like for the empty rooms, once the object is fully created, you just need to drag and drop the new object from the scene to the folder Assets>Prefabs>Objects. Then, the final step is to add the object to the "Objects" list contained in the ObjectsGenerationData. To do so, you need to open the "Data" folder and click on the ObjectsGenerationData in the "Project" section of the Editor. It is now possible to visualize the different fields of the object ObjectsGenerationData, especially the field "Objects" in the "Inspector" section. You can now lock the "Inspector" section with the padlock at the top right corner of the section, open the Assets>Prefabs>Objects folder in the "Project" section and ultimately drag the new object and drop it in the "Objects" field of the ObjectsGenerationData in the "Inspector" section.

### Adding new materials 

All the textures and painting to apply on the grounds, cielings and walls are "Material" type objects. Once you have created or imported a new material to the project, you will have to drag and drop this material from the folder you have put it to the corresponding materials list contained in the "Materials" part of the  RoomsGenerationData. To do so, you need to open the "Data" folder and click on the RoomsGenerationData in the "Project" section of the Editor. It is now possible to visualize the different fields of the object RoomsGenerationData, especially the fields of the "Materials" part in the "Inspector" section. You can now lock the "Inspector" section with the padlock at the top right corner of the section, open the folder in which you have put the material in the "Project" section and ultimately drag the new material and drop it in the correct materials list of the RoomsGenerationData in the "Inspector" section.

### Adding new openings (windows or doors)

All the openings are objects with a "Box Collider" component (3D box surrounding the entire object) and an "Opening" component. First of all, you will have to add an empty object with the tag "Opening Ratio Sphere Parent Object" as a child of the main object. You will then have to create an empty object as a child of the main object and and place it at the pivot point position. Once it is done, you need to put the object that corresponds to the moving part of the opening as a child of the object previously created. To finalize the construction of the opening, you will need to add the components "Box Collider" and "Opening" to the main object and fill all the fields of the "Opening" component. In the first field, you describe the way the opening can be opened (the way the moving part of the opening can be moved) : Translation or Rotation. In the second field, you choose in which direction the moving part translates (for a translation) or what is the rotation axis of the moving part (for a rotation) : Up, Down, Right, Left. The third field has to be filled with the object containing the moving part of the opening by dragging and dropping it from the "Hierarchy" section to the "Inspector" section. Finally, you can fill the two last fields of the "Opening" component by dragging and dropping the "RoomsGenerationData" object and the "ObjectsGenerationData" object from the folder "Data" in the section "Project" to the corresponding fields in the section "Inspector".
Like before, once the opening is fully created, you just need to drag and drop the new opening from the "Hierarchy" to the folder Assets>Prefabs>Windows or Assets>Prefabs>Doors. Then, the final step is to add the object to the "Doors" or "Windows" list contained in the RoomsGenerationData. To do so, you need to open the "Data" folder and click on the RoomsGenerationData in the "Project" section of the Editor. It is now possible to visualize the different fields of the object RoomsGenerationData, especially the fields "Windows" and "Doors" in the "Inspector" section. You can now lock the "Inspector" section with the padlock at the top right corner of the section, open the Assets>Prefabs>Windows or Assets>Prefabs>Doors folder in the "Project" section and ultimately drag the new object and drop it in the "Doors" or "Windows" field of the RoomsGenerationData in the "Inspector" section.

![enter image description here](https://github.com/A-Sereuse/UnityIndoorSceneViewsGenerator/blob/main/ProjectImages/img7.png)
