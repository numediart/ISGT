using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CustomSlider : MonoBehaviour
{
    private VisualElement root;
    
    private List<VisualElement> sliders = new List<VisualElement>();
    
    // Start is called before the first frame update
  

    void AddElements()
    {
        foreach (var slider in sliders)
        {
            var dragger = slider.Q<VisualElement>("unity-dragger");
            var bar = new VisualElement();
            dragger.Add(bar);
            bar.name = "Bar";
            bar.AddToClassList("bar");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(sliders.Count == 0)
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            // Get all sliders using their class selector
            sliders = new List<VisualElement>(root.Query<VisualElement>().Class("slider").ToList());
            AddElements();
        }
    }
}