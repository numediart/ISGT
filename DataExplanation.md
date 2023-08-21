# Data explanations

The data generated is composed of screenshots in the Photographs folder and corresponding openings data contained in JSON files in the OpeningsData folder.
Each JSON file is related to its screenshot by the name of the file.

## JSON file structure

  

    {
	  "OpeningsData": [
        {
          "Type": "Window",
          "Dimensions": {
            "Height": 1.24748456,
            "Width": 0.7472117,
            "Thickness": 0.0720815659
          },
          "DistanceToCamera": 2.7988503,
          "OpenessDegree": 0.928270459,
          "VisibilityRatio": 0.5299998,
          "BoundingBox": {
            "Origin": [
              0,
              250
            ],
            "Dimension": [
              401,
              626
            ]
          }
        },
        {
          "Type": "Window",
          "Dimensions": {
            "Height": 1.62235975,
            "Width": 1.69758368,
            "Thickness": 0.0498111844
          },
          "DistanceToCamera": 2.32567859,
          "OpenessDegree": 0.5082435,
          "VisibilityRatio": 0.5499998,
          "BoundingBox": {
            "Origin": [
              295,
              553
            ],
            "Dimension": [
              1058,
              527
            ]
          }
        }
      ],
      "CameraRotation": {
        "Right axis rotation": 30.45581,
        "Up axis rotation": 269.191681,
        "Forward axis rotation": 323.978668
      }
    }


Every JSON file is composed of :
- A list of openings data 
- The rotation of the camera around its 3 axes (vertical up axis, horizontal right axis and forward axis).
   In this example : the rotation around the vertical up axis is included in [0°,360°], the rotation around the horizontal right axis is included in [0,45°] and [315°,360°], the rotation around the forward axis is included in [0°,90°] and [270°,360°].

All the openings at least partly visible on the picture are contained in the openingsData list.
For every opening, these informations are given :
 - The type of the opening : "Window" or "Door".
 - The dimensions of the opening in meters (including the structure of the opening). 
 - The distance between the middle of the object camera (0.3m behind the "screen" of the camera, can be changed) and the middle of the opening in meters.
 - The openess degree which is a float number (between 0 and 1) representing the degree to which the opening is open.
 - The ratio of the visible part of the opening compared to the total dimensions (between 0 and 1) with a theoretical accuracy of 0,01.
 - The origin and dimension in pixels of the 2D box surrounding the opening on the picture.
