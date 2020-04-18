using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class CameraCompositorDayAndNight : MonoBehaviour
    {
        public Color[] timeColors = new Color[]
        {
            new Color32(50, 40, 80, 255),
            new Color32(128, 120, 80, 255),
            new Color32(144, 144, 144, 255),
            new Color32(100, 50, 10, 255),
            new Color32(50, 40, 80, 255)
        };
        public int timeCircle = 240;

        private int mLastTimeIndex = 0;
        private int mLastTimePoint = 0;

        void Update()
        {
            if (Camera.main == null)
                return;
            CameraCompositor compositor = Camera.main.GetComponent<CameraCompositor>();
            if (compositor == null || compositor.material == null)
                return;

            var now = System.DateTime.Now;
            int timeClock = now.Hour * 3600 + now.Minute * 60 + now.Second;
            int timeFrags = this.timeCircle / (this.timeColors.Length - 1);
            int timeIndex = timeClock / timeFrags % (this.timeColors.Length - 1);
            int timePoint = timeClock % timeFrags;
            if (timeIndex == this.mLastTimeIndex && timePoint == this.mLastTimePoint)
                return;
            this.mLastTimeIndex = timeIndex;
            this.mLastTimePoint = timePoint;
            Color color = Color.Lerp(this.timeColors[timeIndex], this.timeColors[timeIndex + 1], (timePoint + 0.0f) / timeFrags);
            compositor.color = color;
        }
    }
}
