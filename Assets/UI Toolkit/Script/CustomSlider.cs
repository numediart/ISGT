using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CustomSlider : MonoBehaviour
{
    private VisualElement m_Root;
    
    private List<VisualElement> m_Sliders = new List<VisualElement>();
    
    // Start is called before the first frame update
  

    void AddElements()
    {
        foreach (var slider in m_Sliders)
        {
            var dragger = slider.Q<VisualElement>("unity-dragger");
            var m_Bar = new VisualElement();
            dragger.Add(m_Bar);
            m_Bar.name = "Bar";
            m_Bar.AddToClassList("bar");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_Sliders.Count == 0)
        {
            m_Root = GetComponent<UIDocument>().rootVisualElement;
            // Get all sliders using their class selector
            m_Sliders = new List<VisualElement>(m_Root.Query<VisualElement>().Class("slider").ToList());
            Debug.Log(m_Sliders.Count);
            AddElements();
        }
    }
}