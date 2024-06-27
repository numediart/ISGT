# Indoor Scene Generator Toolkit

ISGT is a Unity project aimed at generating realistic virtual rooms. They are then used to create datasets filled with screenshots and the information about each window or door in the image. These datasets can be used to train image recognition AIs. 

## How to use 

To use the software, you only need to download the latest realease, unzip it and start ISGT.exe .<br>
You canchange the settings and the output folder via the menu, and then start the process. It can be stopped at any point by pressing escape.
By default, screenshots and JSONs are saved at /ISGT_Data/Export/

## Generated data

Each screenshot is matched with a Json containing info about the room, the seeds, the camera settings and about each opening in the image, namely doors and windows. Here is an example : 
```js
{
  "SeedsData": {
    "DatabaseSeed": 522732214,
    "RoomsSeed": -320021519,
    "OpeningsSeed": 723920686,
    "ObjectsSeed": 1119647337
  },
  "CameraData": {
    "FieldOfView": 52.2338448,
    "NearClipPlane": 0.3,
    "FarClipPlane": 1000.0,
    "ViewportRectX": 0.0,
    "ViewportRectY": 0.0,
    "ViewportRectWidth": 1920,
    "ViewportRectHeight": 1080,
    "Depth": -1.0,
    "IsOrthographic": false
  },
  "ScreenshotData": {
    "OpeningsData": [
      {
        "Type": "Window",
        "Dimensions": {
          "Height": 1.603815,
          "Width": 1.13412368,
          "Thickness": 0.10204263
        },
        "DistanceToCamera": 7.12805271,
        "RotationQuaternionFromCamera": {
          "w": 0.457306623,
          "x": -0.004237673,
          "y": 0.8892608,
          "z": 0.008240416
        },
        "OpenessDegree": 0.6515185,
        "VisibilityRatio": 0.9289916,
        "BoundingBox": {
          "Origin": [
            1118,
            454
          ],
          "Dimension": [
            118,
            205
          ]
        },
        "VisibilityBoundingBox": {
          "Origin": [
            1120,
            458
          ],
          "Dimension": [
            116,
            200
          ]
        }
      },
    ],
    "CameraRotation": {
      "w": 0.5645235,
      "x": 0.0,
      "y": 0.825417,
      "z": 0.0
    }
  }
}
```

## Open source

ISGT is fully open-source, you can fork this repository and start helping us improve the tool. To work on this project, you will need Unity version 2022.3.26f1 or higher.  <br>

A more detailed documentation is on the way. 
