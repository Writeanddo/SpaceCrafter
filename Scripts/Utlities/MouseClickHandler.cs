using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Detects when the right or left mouse button has been pressed
/// while the mouse is on this object
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class MouseClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Triggered on Left Mouse Button clicked
    /// </summary>
    public delegate void OnLeftClick();
    public OnLeftClick OnLeftClickEvent;

    /// <summary>
    /// Triggered on Right Mouse Button clicked
    /// </summary>
    public delegate void OnRightClick();
    public OnRightClick OnRightClickEvent;

    /// <summary>
    /// Triggered when mouse enters the object
    /// </summary>
    public delegate void OnMouseEnter();
    public OnLeftClick OnMouseEnterEvent;

    /// <summary>
    /// Triggered when mouse exist the object
    /// </summary>
    public delegate void OnMouseExit();
    public OnRightClick OnMouseExitEvent;

    /// <summary>
    /// Dispatches on mouse button click events
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left) {
            OnLeftClickEvent?.Invoke();
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClickEvent?.Invoke();
        }
    }

    /// <summary>
    /// Dispatches on mouse enter event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnterEvent?.Invoke();
    }

    /// <summary>
    /// Dispatches on mouse exit event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExitEvent?.Invoke();
    }
}
