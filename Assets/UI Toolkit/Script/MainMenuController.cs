using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Data_Classes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    VisualElement _rootUi;
    [SerializeField] private VisualTreeAsset _settingsMenuAsset;
    public static PresetData PresetData;
    public static string PresetDataFilename;
    private void Awake()
    {
        _rootUi = GetComponent<UIDocument>().rootVisualElement;
        _rootUi.Q<VisualElement>("Menu").style.display = DisplayStyle.Flex;
        
        // Load the PresetData from the file in playerprefs
        if (PlayerPrefs.HasKey("PresetDataFilename"))
        {
            PresetDataFilename = PlayerPrefs.GetString("PresetDataFilename");
            string path = Application.dataPath + "/Resources/" + PresetDataFilename;
            if (File.Exists(path))
            {
                string presetDataJson = File.ReadAllText(path);
                PresetData = JsonConvert.DeserializeObject<PresetData>(presetDataJson);
            }
        }
        
        if (PresetData == null)
        {
            // Create a default PresetData
             PresetData =
                new PresetData(10, 10, 40, 20, 20, 10, 10, UnityEngine.Device.Application.dataPath + "/Export", 90, 200, 16f, 10f, new Vector3Int(0, 180, 0), 1000);
        }
        
    }

    private void OnEnable()
    {
        _rootUi.Q<Button>("StartButton").clicked += OnStartButtonClicked;
        _rootUi.Q<Button>("ExitButton").clicked += OnExitButtonClicked;
        _rootUi.Q<Button>("SettingsButton").clicked += OnSettingsButtonClicked;
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("Start Button Clicked");
        SceneManager.LoadScene(1);// Load the ProceduralGeneration scene
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit Button Clicked");
        Application.Quit(0);//Exit the application with code 0 (success)
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings Button Clicked");
        _rootUi.Q<VisualElement>("Menu").style.display = DisplayStyle.None;
        if(_rootUi.Q<VisualElement>("SettingsMenu") != null)
            _rootUi.Q<VisualElement>("SettingsMenu").RemoveFromHierarchy();
        _rootUi.Q<VisualElement>("Panel").Add(_settingsMenuAsset.CloneTree());
        _rootUi.Q<VisualElement>("SettingsMenu").style.display = DisplayStyle.Flex;
        gameObject.AddComponent<SettingsMenuController>();
        _rootUi.Q<VisualElement>("LastGenerationInfo").style.display = DisplayStyle.None;
    }
}