using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GravityBox.UI
{
    /// <summary>
    /// Simple script to make window or context menu hide when mouse clicked outside of it
    /// </summary>
    public class CloseWindowOnClick : MonoBehaviour, IPointerClickHandler, IUpdateSelectedHandler
    {
        public enum WindowCloseAction
        {
            Hide,  // gameobject.SetActive(false)
            Destroy // Destroy(gameObject)
        }

        public WindowCloseAction closeAction;
        public GameObject windowObject;

        private Event current = new Event();

        private void Awake()
        {
            if (windowObject == null)
                windowObject = gameObject;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == gameObject)
                Close();
        }

        /// <summary>
        /// for this to work gameObject should be set as selected by current EventSystem
        /// but since a lot of things are selectable in UI system this thing is almost unusable
        /// </summary>
        /// <param name="eventData"></param>
        public void OnUpdateSelected(BaseEventData eventData)
        {
            while (Event.PopEvent(current))
            {
                if (current.keyCode == KeyCode.Escape && current.type == EventType.KeyUp)
                {
                    eventData.Use();
                    Close();
                }
            }
        }

        public void Close()
        {
            if (closeAction == WindowCloseAction.Hide)
                windowObject.SetActive(false);
            else
                Destroy(windowObject);
        }
    }
}