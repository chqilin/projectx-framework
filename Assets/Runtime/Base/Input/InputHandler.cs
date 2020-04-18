using UnityEngine;
using UnityEngine.Events;

namespace ProjectX
{
    [RequireComponent(typeof(InputTarget))]
    public class InputHandler : MonoBehaviour
    {
        [System.Serializable]
        public class Handler : UnityEvent<InputTouch>
        { }

        public Handler onPress = new Handler();
        public Handler onRelease = new Handler();
        public Handler onClick = new Handler();
        public Handler onMove = new Handler();

        private InputTarget mTarget = null;

        void Awake()
        {
            this.mTarget = this.GetComponent<InputTarget>();
            this.mTarget.onPress.Attach(touch => this.onPress.Invoke(touch));
            this.mTarget.onRelease.Attach(touch => this.onRelease.Invoke(touch));
            this.mTarget.onClick.Attach(touch => this.onClick.Invoke(touch));
            this.mTarget.onMove.Attach(touch => this.onMove.Invoke(touch));
        }
    }
}

