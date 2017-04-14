using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool selected = false;

    //Do this when the selectable UI object is selected.
    public void OnPointerDown(PointerEventData eventData)
    {
        selected = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        selected = false;
    }
}