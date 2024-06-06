using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Data_Classes;
using UnityEngine.SceneManagement;
using Utils;

public class MainMenuController : MonoBehaviour
{   
    private const string ResourcesDirectory = "/Resources";
    private const string JsonFileExtension = ".json";

    public VisualElement RootUi;
    public VisualElement SettingsUi;

    public Button StartButton;
    public Button ExitButton;
    public Button SettingsButton;

    public Button ReturnButton;
    public Button ApplyButton;
    public Button BrowsePathButton;

    private string _presetDataPath=null;
    
    public static PresetData PresetData;
    private void Awake()
    {
        RootUi = GetComponent<UIDocument>().rootVisualElement;
        RootUi.Q<VisualElement>("Menu").style.display = DisplayStyle.Flex;
        SettingsUi = RootUi.Q<VisualElement>("SettingsMenu");
        SettingsUi.style.display = DisplayStyle.None;
    }

    private void OnEnable()
    {
        StartButton = RootUi.Q<Button>("StartButton");
        ExitButton = RootUi.Q<Button>("ExitButton");
        SettingsButton = RootUi.Q<Button>("SettingsButton");

        ReturnButton = SettingsUi.Q<Button>("CancelButton");
        ApplyButton = SettingsUi.Q<Button>("ApplyButton");
        BrowsePathButton = SettingsUi.Q<VisualElement>("BrowsePathContainer").Q<Button>("BrowsePath");

       ReturnButton.clicked += OnReturnButtonClicked;
        ApplyButton.clicked += OnApplyButtonClicked;
        BrowsePathButton.clicked += OnBrowsePathButtonClicked;

        StartButton.clicked += OnStartButtonClicked;
        ExitButton.clicked += OnExitButtonClicked;
        SettingsButton.clicked += OnSettingsButtonClicked;
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("Start Button Clicked" + SceneManager.sceneCount);
        RootUi.Q<VisualElement>("Menu").style.display = DisplayStyle.None;
        
        if(_presetDataPath!=null)
            PresetData = JsonConvert.DeserializeObject<PresetData>(File.ReadAllText(_presetDataPath));
        // Load main scene
        SceneManager.LoadScene(SceneManager.sceneCount);//Load Procedural Generation Scene
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit Button Clicked");
        Application.Quit();
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings Button Clicked");
        RootUi.Q<VisualElement>("Menu").style.display = DisplayStyle.None;
        SettingsUi.style.display = DisplayStyle.Flex;
        GetPresetDataFilename();
    }

    private void OnReturnButtonClicked()
    {
        SettingsUi.style.display = DisplayStyle.None;
        RootUi.Q<VisualElement>("Menu").style.display = DisplayStyle.Flex;
    }

    private void OnApplyButtonClicked()
    {
        PresetData presetData = new PresetData(
            SettingsUi.Q<SliderInt>("MaxWidth").value,
            SettingsUi.Q<SliderInt>("MaxDepth").value,
            SettingsUi.Q<SliderInt>("PropsRatio").value,
            SettingsUi.Q<SliderInt>("WindowRatio").value,
            SettingsUi.Q<SliderInt>("DoorRatio").value,
            SettingsUi.Q<IntegerField>("ScreenshotNumber").value,
            SettingsUi.Q<IntegerField>("RoomNumber").value,
            SettingsUi.Q<Label>("PathLabel").text);

        string presetDataJson = JsonConvert.SerializeObject(presetData);
        Debug.Log("Preset Data: " + presetDataJson);

        if (!Directory.Exists(Application.dataPath + ResourcesDirectory))
            Directory.CreateDirectory(Application.dataPath + ResourcesDirectory);

        var files = GetFiles(Application.dataPath + ResourcesDirectory);
        var fileName = $"presetData_{files.Length}{JsonFileExtension}";
        File.WriteAllBytes(Application.dataPath + $"{ResourcesDirectory}/{fileName}",
            Encoding.ASCII.GetBytes(presetDataJson));
    }

    private void OnBrowsePathButtonClicked()
    {
        Debug.Log("Browse Path Button Clicked");
        FolderDialog folderDialog = gameObject.AddComponent<FolderDialog>();
        string path = folderDialog.OpenFolderDialog("Select Folder");
        Debug.Log("Path: " + path);
        SettingsUi.Q<Label>("PathLabel").text = path;
    }

    // Get preset data from json files
    private void GetPresetDataFilename()
    {
        string path = Application.dataPath + ResourcesDirectory;
        var files = GetFiles(path);

        // Sort files by index in filename
        Array.Sort(files, (x, y) =>
            Int32.Parse(x.Name.Split("_")[1].Split(".")[0])
                .CompareTo(Int32.Parse(y.Name.Split("_")[1].Split(".")[0])));

        RadioButtonGroup radioButtonGroup = SettingsUi.Q<RadioButtonGroup>("PresetRadio");
        radioButtonGroup.Clear();

        foreach (var file in files)
        {
            RadioButton radioButton = new RadioButton
            {
                text = file.Name,
                name = file.Name,
                value = false
            };
            radioButton.RegisterCallback<ClickEvent>(evt =>
            {
                _presetDataPath = path + "/" + radioButton.name;
                Debug.Log("Selected Preset Data: " + _presetDataPath);
            });
            radioButtonGroup.hierarchy.Add(radioButton);
        }
    }

    private FileInfo[] GetFiles(string path)
    {
        // Get all json files in the directory
        DirectoryInfo dir = new DirectoryInfo(path);
        return dir.GetFiles($"presetData_*{JsonFileExtension}");
    }
}