using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class InputTarget : MonoBehaviour
    {
        /// <summary>
        /// press event is sent when finger pressed screen or mouse button down.
        /// </summary>
        public InputEvent onPress = new InputEvent();

        /// <summary>
        /// Release event is sent when finger left screen or mouse button up.
        /// </summary>
        public InputEvent onRelease = new InputEvent();

        /// <summary>
        /// Click event is sent when pressed and the released at the same object.
        /// </summary>
        public InputEvent onClick = new InputEvent();

        /// <summary>
        /// Finger or mouse is moving(dragging) on screen. This event is sent to the event target.
        /// </summary>
        public InputEvent onMove = new InputEvent();

        void Start()
        {
            InputDetection.handlers.Add(this);
        }

        void OnDestroy()
        {
            InputDetection.handlers.Remove(this);
        }
    }
}

