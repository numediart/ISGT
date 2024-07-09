using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Utils;

public class InGameMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    public static Label ScreenshotValueLabel { get; set; }
    public static Label RoomValueLabel { get; set; }
    public static Label ElapsedTimeValueLabel { get; set; }
    public static Label ETAValueLabel { get; set; }

    public static ProgressBar ProgressBar { get; set; }
    public static Label ProgressLabel { get; set; }

    public static int ElapsedTime { get; set; }
    private void Awake()
    {
        ScreenshotValueLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("ScreenshotValueLabel");
        RoomValueLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("RoomValueLabel");
        ElapsedTimeValueLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("ElapsedTimeValueLabel");
        ETAValueLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("ETAValueLabel");
        ProgressBar = GetComponent<UIDocument>().rootVisualElement.Q<ProgressBar>("ProgressBar");
        ProgressLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("RoomIdLabel");
    }

    void Start()
    {
        ScreenshotValueLabel.text = "0";
        RoomValueLabel.text = "0";
        ElapsedTimeValueLabel.text = "0 s";
        ETAValueLabel.text = "0 s";
        ProgressLabel.text = "0 / 100%";
        ProgressBar.value = 0;
    }

    private void Update()
    {
        InterSceneManager.LastElapsedTime = ElapsedTime;
        InterSceneManager.LastMaxRooms = Int32.Parse(RoomValueLabel.text.Split("/")[0]);
        InterSceneManager.LastMaxScreenshots = Int32.Parse(ScreenshotValueLabel.text.Split("/")[0]);

    }
}