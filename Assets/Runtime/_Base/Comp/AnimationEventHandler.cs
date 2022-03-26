using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class AnimationEventHandler : MonoBehaviour
    {
        public delegate void Handler(string evt);
        public Handler onAnimEvent = null;

        void AnimEvent(string evt)
        {
            if (this.onAnimEvent != null)
            {
                this.onAnimEvent(evt);
            }
        }

        void AnimBegin(string anim)
        {
            this.AnimEvent("AnimBegin=" + anim);
        }

        void AnimEnd(string anim)
        {
            this.AnimEvent("AnimEnd=" + anim);
        }

        void AnimFocus(string focus)
        {
            this.AnimEvent("AnimFocus=" + focus);
        }

        void AnimCombo(string combo)
        {
            this.AnimEvent("AnimCombo=" + combo);
        }

        void AnimEffect(string effect)
        {
            this.AnimEvent("AnimEffect=" + effect);
        }

        void AnimSound(string sound)
        {
            this.AnimEvent("AnimSound=" + sound);
        }
    }
}
