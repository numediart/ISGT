using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

public class FileDialogExplorer : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/FileDialogExplorer")]
    public static void ShowExample()
    {
        FileDialogExplorer wnd = GetWindow<FileDialogExplorer>();
        wnd.titleContent = new GUIContent("FileDialogExplorer");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
    

}
