using System;
using System.IO;
using System.Text;
using Data_Classes;
using Newtonsoft.Json;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using Button = UnityEngine.UIElements.Button;

/// <summary>
///  Settings menu controller class to handle the settings menu UI and events
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    private const string ResourcesDirectory = "/Resources";

    private const string JsonFileExtension = ".json";
    
    private VisualElement _rootVisualElement;
    
    private UnsignedIntegerField _numberOfRoomsToGenerate;
    private UnsignedIntegerField _screenshotsCountPerRoom;
    
    private EnumField _roomMaxSize;
    private EnumField _propsDensity;
    private EnumField _windowDensity;
    private EnumField _doorDensity;
    
    private SliderInt _maxSizeSlider;
    private SliderInt _propsDensitySlider;
    private SliderInt _windowDensitySlider;
    private SliderInt _doorDensitySlider;
    
    //Camera settings
    private SliderInt _XSlider;
    private SliderInt _YSlider;
    private SliderInt _ZSlider;     
    
    private SliderInt _fieldOfViewSlider;
    private SliderInt _isoSlider;
    private Slider _apertureSlider;
    private Slider _focusDistanceSlider;
    
    //Precision settings
    private EnumField _raycastAmount;
    
    private SliderInt _raycastAmountSlider;
    
    private Label _pathLabel;
    
    private Button _browseButton;
    private Button _applyButton;
    private Button _cancelButton;

    private DropdownField _presetDropdown;

    void Awake()
    {
        _rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        
        _presetDropdown = _rootVisualElement.Q<DropdownField>("PresetDropdown");
        _roomMaxSize = _rootVisualElement.Q<EnumField>("RoomSize");
        _maxSizeSlider = _rootVisualElement.Q<SliderInt>("MaxSizeSlider");
        _propsDensity = _rootVisualElement.Q<EnumField>("PropsDensity");
        _propsDensitySlider = _rootVisualElement.Q<SliderInt>("PropsDensitySlider");
        _windowDensity = _rootVisualElement.Q<EnumField>("WindowsDensity");
        _windowDensitySlider = _rootVisualElement.Q<SliderInt>("WindowsDensitySlider");
        _doorDensity = _rootVisualElement.Q<EnumField>("DoorsDensity");
        _doorDensitySlider = _rootVisualElement.Q<SliderInt>("DoorsDensitySlider");
        _pathLabel = _rootVisualElement.Q<Label>("PathLabel");
        _browseButton = _rootVisualElement.Q<Button>("BrowsePath");
        _applyButton = _rootVisualElement.Q<Button>("ApplyButton");
        _cancelButton = _rootVisualElement.Q<Button>("CancelButton");
        _screenshotsCountPerRoom = _rootVisualElement.Q<UnsignedIntegerField>("ScreenshotsCountPerRoom");
        _numberOfRoomsToGenerate = _rootVisualElement.Q<UnsignedIntegerField>("RoomNumber");
        _fieldOfViewSlider = _rootVisualElement.Q<SliderInt>("FOVSlider");
        _isoSlider = _rootVisualElement.Q<SliderInt>("ISOSlider");
        _apertureSlider = _rootVisualElement.Q<Slider>("ApertureSlider");
        _focusDistanceSlider = _rootVisualElement.Q<Slider>("FocusDistanceSlider");
        _XSlider = _rootVisualElement.Q<SliderInt>("XSlider");
        _YSlider = _rootVisualElement.Q<SliderInt>("YSlider");
        _ZSlider = _rootVisualElement.Q<SliderInt>("ZSlider");
        _raycastAmount = _rootVisualElement.Q<EnumField>("RaycastAmount");
        _raycastAmountSlider = _rootVisualElement.Q<SliderInt>("RaycastAmountSlider");
        
        // Hide sliders for manual inputs
        _maxSizeSlider.style.display = DisplayStyle.None;
        _propsDensitySlider.style.display = DisplayStyle.None;
        _windowDensitySlider.style.display = DisplayStyle.None;
        _doorDensitySlider.style.display = DisplayStyle.None;
        _raycastAmountSlider.style.display = DisplayStyle.None;

        // Register callbacks for manual inputs
        _roomMaxSize.RegisterValueChangedCallback(evt =>
        {
            _maxSizeSlider.style.display = (RoomMaxSize)evt.newValue == RoomMaxSize.ManualInput ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        _propsDensity.RegisterValueChangedCallback(evt =>
        {
            _propsDensitySlider.style.display = (PropsDensity)evt.newValue == PropsDensity.ManualInput ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        _windowDensity.RegisterValueChangedCallback(evt =>
        {
            _windowDensitySlider.style.display = (WindowDensity)evt.newValue == WindowDensity.ManualInput ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        _doorDensity.RegisterValueChangedCallback(evt =>
        {
            _doorDensitySlider.style.display = (DoorDensity)evt.newValue == DoorDensity.ManualInput ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        _raycastAmount.RegisterValueChangedCallback(evt =>
        {
            _raycastAmountSlider.style.display = (RaycastAmount)evt.newValue == RaycastAmount.ManualInput ? DisplayStyle.Flex : DisplayStyle.None;
        });
        
        if (!Directory.Exists(Application.dataPath + ResourcesDirectory))
            Directory.CreateDirectory(Application.dataPath + ResourcesDirectory);

        GetPresetDataFilename();

        if (MainMenuController.PresetData != null)
            _presetDropdown.value = MainMenuController.PresetDataFilename;
        
        // Load the preset filename from PlayerPrefs if it exists
        string savedPresetFilename = PlayerPrefs.GetString("PresetDataFilename", string.Empty);
        if (!string.IsNullOrEmpty(savedPresetFilename) && _presetDropdown.choices.Contains(savedPresetFilename) && File.Exists(Application.dataPath + ResourcesDirectory + "/" + savedPresetFilename))
        {
            _presetDropdown.value = savedPresetFilename;
        }
    }

    /// <summary>
    ///  OnEnable method to add event handlers to the UI elements
    /// </summary>
    private void OnEnable()
    {
        _browseButton.clicked += OnBrowseButtonClicked;
        _applyButton.clicked += OnApplyButtonClicked;
        _cancelButton.clicked += OnCancelButtonClicked;
        _presetDropdown.RegisterValueChangedCallback(OnPresetDropdownValueChanged);
    }

    /// <summary>
    ///  Event handler for the dropdown field value changed event
    /// </summary>
    /// <param name="changeEvent"></param>
    private void OnPresetDropdownValueChanged(ChangeEvent<string> changeEvent)
    {
        string path = Application.dataPath + ResourcesDirectory;
        
        if (!File.Exists(path + "/" + changeEvent.newValue))
            return;
        
        string presetDataJson = File.ReadAllText(path + "/" + changeEvent.newValue);
        MainMenuController.PresetData = JsonConvert.DeserializeObject<PresetData>(presetDataJson);

        bool isSizeManualInput = !Enum.IsDefined(typeof(RoomMaxSize), MainMenuController.PresetData.MaxWidth);
        bool isPropsManualInput = !Enum.IsDefined(typeof(PropsDensity), MainMenuController.PresetData.PropsRatio);
        bool isWindowsManualInput = !Enum.IsDefined(typeof(WindowDensity), MainMenuController.PresetData.WindowRatio);
        bool isDoorsManualInput = !Enum.IsDefined(typeof(DoorDensity), MainMenuController.PresetData.DoorRatio);
        bool isRaycastManualInput = !Enum.IsDefined(typeof(RaycastAmount), MainMenuController.PresetData.RaycastAmount);
        
        _pathLabel.text = MainMenuController.PresetData.ExportPath;
        
        _roomMaxSize.value = isSizeManualInput ? RoomMaxSize.ManualInput : (RoomMaxSize)MainMenuController.PresetData.MaxWidth;
        _maxSizeSlider.value = MainMenuController.PresetData.MaxWidth;
        
        _propsDensity.value = isPropsManualInput ? PropsDensity.ManualInput : (PropsDensity)MainMenuController.PresetData.PropsRatio;
        _propsDensitySlider.value = MainMenuController.PresetData.PropsRatio;
        
        _windowDensity.value = isWindowsManualInput ? WindowDensity.ManualInput : (WindowDensity)MainMenuController.PresetData.WindowRatio;
        _windowDensitySlider.value = MainMenuController.PresetData.WindowRatio;
        
        _doorDensity.value = isDoorsManualInput ? DoorDensity.ManualInput : (DoorDensity)MainMenuController.PresetData.DoorRatio;
        _doorDensitySlider.value = MainMenuController.PresetData.DoorRatio;
        
        _screenshotsCountPerRoom.value = (uint)MainMenuController.PresetData.ScreenshotsCountPerRoom;
        _numberOfRoomsToGenerate.value = (uint)MainMenuController.PresetData.NumberOfRoomsToGenerate;
        
        _fieldOfViewSlider.value = MainMenuController.PresetData.FieldOfView;
        _isoSlider.value = MainMenuController.PresetData.ISO;
        _apertureSlider.value = MainMenuController.PresetData.Aperture;
        _focusDistanceSlider.value = MainMenuController.PresetData.FocusDistance;
        
        _XSlider.value = MainMenuController.PresetData.MaxRotation.x;
        _YSlider.value = MainMenuController.PresetData.MaxRotation.y;
        _ZSlider.value = MainMenuController.PresetData.MaxRotation.z;
        
        _raycastAmount.value = isRaycastManualInput ? RaycastAmount.ManualInput : (RaycastAmount)MainMenuController.PresetData.RaycastAmount;
        _raycastAmountSlider.value = MainMenuController.PresetData.RaycastAmount;

        MainMenuController.PresetDataFilename = changeEvent.newValue;
        
        // Save the preset filename to PlayerPrefs
        PlayerPrefs.SetString("PresetDataFilename", changeEvent.newValue);
        PlayerPrefs.Save();
    }

    /// <summary>
    ///  Click event handler for the browse button to select a folder it will open a folder dialog //TODO: Make it work on Linux and Mac (if not a text field should be used)
    /// </summary>
    private void OnBrowseButtonClicked()
    {
        // Add the FolderDialog component to the gameObject to open a folder dialog
        FolderDialog folderDialog = gameObject.AddComponent<FolderDialog>();
        string path = folderDialog.OpenFolderDialog("Select a folder");
        _pathLabel.text = path;
    }

    /// <summary>
    ///  Click event handler for the apply button
    /// </summary>
    private void OnApplyButtonClicked()
    {
        string path = _pathLabel.text;
        
        bool isSizeManualInput = (int)(RoomMaxSize)_roomMaxSize.value == (int)RoomMaxSize.ManualInput;
        bool isPropsManualInput = (int)(PropsDensity)_propsDensity.value == (int)PropsDensity.ManualInput;
        bool isWindowsManualInput = (int)(WindowDensity)_windowDensity.value == (int)WindowDensity.ManualInput;
        bool isDoorsManualInput = (int)(DoorDensity)_doorDensity.value == (int)DoorDensity.ManualInput;
        bool isRaycastManualInput = (int)(RaycastAmount)_raycastAmount.value == (int)RaycastAmount.ManualInput;
        
        PresetData presetData = new PresetData(
            isSizeManualInput ? _maxSizeSlider.value : (int)(RoomMaxSize)_roomMaxSize.value,
            isSizeManualInput ? _maxSizeSlider.value : (int)(RoomMaxSize)_roomMaxSize.value, 
            isPropsManualInput ? _propsDensitySlider.value : (int)(PropsDensity)_propsDensity.value,
            isWindowsManualInput ? _windowDensitySlider.value : (int)(WindowDensity)_windowDensity.value,
            isDoorsManualInput ? _doorDensitySlider.value : (int)(DoorDensity)_doorDensity.value,
            (int)_screenshotsCountPerRoom.value, 
            (int)_numberOfRoomsToGenerate.value, 
            path,
            _fieldOfViewSlider.value,
            _isoSlider.value,
            _apertureSlider.value,
            _focusDistanceSlider.value,
            new Vector3Int(_XSlider.value, _YSlider.value, _ZSlider.value),
            isRaycastManualInput ? _raycastAmountSlider.value : (int)(RaycastAmount)_raycastAmount.value
            );
        
        
        string presetDataJson = JsonConvert.SerializeObject(presetData);

        // Save the preset data to a json file in the Resources directory with a unique name based on the number of files in the directory
        var files = GetFiles(Application.dataPath + ResourcesDirectory);
        var fileName = $"presetData_{files.Length}{JsonFileExtension}";

        File.WriteAllBytes(Application.dataPath + $"{ResourcesDirectory}/{fileName}",
            Encoding.ASCII.GetBytes(presetDataJson));

        // Update the dropdown with the new preset data file
        _presetDropdown.choices.Add(fileName);
        _presetDropdown.value = fileName;
        // Update the preset data in the MainMenuController
        MainMenuController.PresetDataFilename = fileName;
        // Hide the settings menu and show the main menu to the user
        _rootVisualElement.Q<VisualElement>("SettingsMenu").style.display = DisplayStyle.None;
        _rootVisualElement.Q<VisualElement>("Menu").style.display = DisplayStyle.Flex;
        Destroy(this); // Destroy the SettingsMenuController component to prevent multiple instances
    }

    /// <summary>
    ///   Get the preset data filename from the Resources directory and add it to the dropdown field and set the value to the default preset data file if it exists
    /// </summary>
    private void GetPresetDataFilename()
    {
        string path = Application.dataPath + ResourcesDirectory;
        var files = GetFiles(path);
        if (files.Length == 0)
        {
            CreateDefaultPresetData();
            return;
        }

        // Sort files by index in filename
        Array.Sort(files, (x, y) =>
        {
            if (x.Name.Split("_")[1] == "default.json" || y.Name.Split("_")[1] == "default.json")
                return 1;

            return 1 + Int32.Parse(x.Name.Split("_")[1].Split(".")[0])
                .CompareTo(Int32.Parse(y.Name.Split("_")[1].Split(".")[0]));
        });
        foreach (var file in files)
        {
            _presetDropdown.choices.Add(file.Name);
            _presetDropdown.value = file.Name;
        }
    }

    /// <summary>
    ///  Get all json files in the directory with the specified path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private FileInfo[] GetFiles(string path)
    {
        // Get all json files in the directory
        DirectoryInfo dir = new DirectoryInfo(path);
        return dir.GetFiles($"presetData_*{JsonFileExtension}");
    }

    /// <summary>
    ///  Click event handler for the cancel button
    /// </summary>
    private void OnCancelButtonClicked()
    {
        _rootVisualElement.Q<VisualElement>("SettingsMenu").style.display = DisplayStyle.None;
        _rootVisualElement.Q<VisualElement>("Menu").style.display = DisplayStyle.Flex;
        if(InterSceneManager.LastMaxRooms > 0)
            _rootVisualElement.Q<VisualElement>("LastGenerationInfo").style.display = DisplayStyle.Flex;
        Destroy(this);
    }

    /// <summary>
    ///  Create a default preset data file with the specified values and add it to the dropdown field
    /// </summary>
    public void CreateDefaultPresetData()
    {
        string presetDataJson = JsonConvert.SerializeObject(MainMenuController.PresetData);

        if (!Directory.Exists(Application.dataPath + ResourcesDirectory))
            Directory.CreateDirectory(Application.dataPath + ResourcesDirectory);

        var files = GetFiles(Application.dataPath + ResourcesDirectory);
        var fileName = $"presetData_default{JsonFileExtension}";
        File.WriteAllBytes(Application.dataPath + $"{ResourcesDirectory}/{fileName}",
            Encoding.ASCII.GetBytes(presetDataJson));
        _presetDropdown.choices.Add(fileName);
        _presetDropdown.value = fileName;
    }
}
