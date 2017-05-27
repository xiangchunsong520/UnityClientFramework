/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class EventTriggerListener : EventTrigger
{
    public Action<GameObject> onClick;
    public Action<GameObject> onDown;
    public Action<GameObject> onEnter;
    public Action<GameObject> onExit;
    public Action<GameObject> onUp;
    public Action<GameObject> onSelect;
    public Action<GameObject> onUpdateSelect;
    /*OnBeginDrag
        OnCancel
        OnDeselect
        OnDrag
        OnDrop
        OnEndDrag
        OnInitializePotentialDrag
        OnMove
        OnScroll
        OnSubmit
        */

    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null)
            listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
    }
    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }
    /*public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }
    public override void OnCancel(BaseEventData eventData)
    {
        base.OnCancel(eventData);
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    }
    public override void OnDrop(PointerEventData eventData)
    {
        base.OnDrop(eventData);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
    }
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
    }
    public override void OnMove(AxisEventData eventData)
    {
        base.OnMove(eventData);
    }
    public override void OnScroll(PointerEventData eventData)
    {
        base.OnScroll(eventData);
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
    }*/
}