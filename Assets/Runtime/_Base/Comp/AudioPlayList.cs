using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    [RequireComponent(typeof(AudioPlayback))]
    public class AudioPlayList : MonoBehaviour
    {
        public enum PlayMode
        {
            Sequence,
            Random
        }
        public PlayMode playMode = PlayMode.Sequence;
        public float minInterval = 1.0f;
        public float maxInterval = 4.0f;
        public bool autoPlaying = true;
        public List<AudioClip> clips = new List<AudioClip>();

        private AudioChannel mChannel = null;

        public void Play()
        {
            if (this.mChannel == null)
                return;

            if (this.playMode == PlayMode.Sequence)
            {
                this.mChannel.PlaySequence(this.clips, this.minInterval, this.maxInterval);
            }
            else if (this.playMode == PlayMode.Random)
            {
                this.mChannel.PlayRandom(this.clips, this.minInterval, this.maxInterval);
            }
        }

        void Start()
        {
            if (this.mChannel == null)
            {
                AudioPlayback playback = this.GetComponent<AudioPlayback>();
                this.mChannel = playback.AcquireChannel();
            }

            if (this.autoPlaying)
            {
                this.Play();
            }
        }
    }
}
