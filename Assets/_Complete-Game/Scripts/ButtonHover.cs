using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string ButtonTextName;
    private Color defaultColor = new Color(0, 0, 0);
    private Color hoverTextColor = new Color(240, 200, 0);
    // Start is called before the first frame update
    void Start()
    {
        Text buttonText = GameObject.Find(ButtonTextName).GetComponent<Text>();
        buttonText.color = defaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Text buttonText = GameObject.Find(ButtonTextName).GetComponent<Text>();
        buttonText.color = hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
     {
        Text buttonText = GameObject.Find(ButtonTextName).GetComponent<Text>();
        buttonText.color = defaultColor;  
     }
}
