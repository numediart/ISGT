using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class InterSceneManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static int LastMaxScreenshots = 0;
    public static int LastMaxRooms = 0;
    public static int LastElapsedTime = 0;
    private VisualElement _lastGenerationInfo;
    public static bool showLastGenerationInfo = true;

    void Awake()
    {
       if(FindObjectsOfType<InterSceneManager>().Length > 1)
            Destroy(gameObject);
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && LastMaxRooms > 0 && showLastGenerationInfo)
        {
            VisualElement root = FindObjectOfType<UIDocument>().rootVisualElement;
            _lastGenerationInfo = root.Q<VisualElement>("LastGenerationInfo");
            _lastGenerationInfo.style.display = DisplayStyle.Flex;
            Label lastMaxScreenshots = _lastGenerationInfo.Q<Label>("LastMaxScreenshots");
            Label lastMaxRooms = _lastGenerationInfo.Q<Label>("LastMaxRooms");
            Label lastElapsedTime = _lastGenerationInfo.Q<Label>("LastElapsedTime");
            lastMaxScreenshots.text = LastMaxScreenshots.ToString();
            lastMaxRooms.text = LastMaxRooms.ToString();
            lastElapsedTime.text = LastElapsedTime/1000 + " s";
        }
        else
        {
            if (_lastGenerationInfo != null)
            {
                _lastGenerationInfo.style.display = DisplayStyle.None;
            }
        }
    }
}