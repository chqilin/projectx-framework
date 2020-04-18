using UnityEngine;
using System.Collections.Generic;

namespace ProjectX
{
    public class AudioChannel
    {
        public AudioSource source = null;
        public SimpleFSM<AudioSource> states = null;

        public AudioChannel(AudioSource source)
        {
            this.source = source;
            this.states = new SimpleFSM<AudioSource>();
        }

        #region Life Circle
        public void Init()
        {
            this.states.Init(this.source);
        }
        public void Quit()
        {
            this.states.Quit();
        }
        public void Update(float elapse)
        {
            this.states.Update(elapse);
        } 
        #endregion

        #region Public Methods
        public void Play(AudioClip clip, bool loop = false, float delay = 0.0f)
        {
            this.source.clip = clip;
            this.source.loop = loop;
            this.source.PlayDelayed(delay);
        }
        public void Play(AudioClip clip, bool loop, float volume, float pitch, float delay = 0.0f)
        {
            this.source.clip = clip;
            this.source.loop = loop;
            this.source.volume = volume;
            this.source.pitch = pitch;
            this.source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Vector3 pos, bool loop = false, float delay = 0.0f)
        {
            this.source.transform.position = pos;
            this.source.clip = clip;
            this.source.loop = loop;
            this.source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Vector3 pos, bool loop, float volume, float pitch, float delay = 0.0f)
        {
            this.source.transform.position = pos;
            this.source.clip = clip;
            this.source.loop = loop;
            this.source.volume = volume;
            this.source.pitch = pitch;
            this.source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Transform target, bool loop = false, float delay = 0.0f)
        {
            this.source.transform.parent = target;
            this.source.transform.localPosition = Vector3.zero;
            this.source.clip = clip;
            this.source.loop = loop;
            this.source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Transform target, bool loop, float volume, float pitch, float delay = 0.0f)
        {
            this.source.transform.parent = target;
            this.source.transform.localPosition = Vector3.zero;
            this.source.clip = clip;
            this.source.loop = loop;
            this.source.volume = volume;
            this.source.pitch = pitch;
            this.source.PlayDelayed(delay);
        }

        public void PlayOneShot(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            this.source.pitch = pitch;
            this.source.PlayOneShot(clip, volume);
        }

        public void Stop()
        {
            this.source.Stop();
        }

        public void ChangeVolume(float volume, float time)
        {
            this.states.Start(new AudioState_ChangeVolume(volume, time));
        }

        public void FadeIn(float time)
        {
            this.states.Start(new AudioState_ChangeVolume(1.0f, time));
        }

        public void FadeOut(float time)
        {
            this.states.Start(new AudioState_ChangeVolume(0.0f, time));
        }

        public void CrossFade(AudioClip clip, bool loop, float outTime, float inTime)
        {
            this.states.Start(new AudioState_CrossFade(clip, loop, outTime, inTime));
        }

        public void PlaySequence(List<AudioClip> clips, float minInterval, float maxInterval)
        {
            this.states.Start(new AudioState_PlaySequence(clips, minInterval, maxInterval));
        }

        public void PlayRandom(List<AudioClip> clips, float minInterval, float maxInterval)
        {
            this.states.Start(new AudioState_PlayRandom(clips, minInterval, maxInterval));
        }
        #endregion

        #region Audio State
        public abstract class AudioState : SimpleFSM<AudioSource>.State
        { }

        public class AudioState_ChangeVolume : AudioState
        {
            private float mVolumeF = 0.0f;
            private float mVolumeT = 0.0f;
            private float mTime = 1.0f;

            public AudioState_ChangeVolume(float volume, float time)
            {
                this.mVolumeF = this.subject.volume;
                this.mVolumeT = volume;
                this.mTime = Mathf.Max(time, 0.001f);
            }

            public override void Update(float elapse)
            {
                base.Update(elapse);

                float percent = this.time / this.mTime;
                if (percent > 1.0f)
                    return;
                this.subject.volume = Mathf.Lerp(this.mVolumeF, this.mVolumeT, percent);
            }
        }

        public class AudioState_CrossFade : AudioState
        {
            private AudioClip mClip = null;
            private bool mLoop = false;
            private float mFadeOutTime = 1.0f;
            private float mFadeInTime = 1.0f;

            private float mFadeOutCumu = 0.0f;
            private float mFadeInCumu = 0.0f;
            private int mStage = 0;

            public AudioState_CrossFade(AudioClip clip, bool loop, float outTime, float inTime)
            {
                this.mClip = clip;
                this.mLoop = loop;
                this.mFadeOutTime = Mathf.Max(outTime, 0.001f); ;
                this.mFadeInTime = Mathf.Max(inTime, 0.001f);

                this.mFadeOutCumu = 0.0f;
                this.mFadeInCumu = 0.0f;
                this.mStage = 1; // fade out
            }

            public override void Update(float elapse)
            {
                base.Update(elapse);

                if (this.mStage == 1) // fade out
                {
                    this.mFadeOutCumu += elapse;
                    float percent = this.mFadeOutCumu / this.mFadeOutTime;
                    this.subject.volume = 1 - percent;
                    if (percent >= 1.0f)
                    {
                        this.subject.clip = this.mClip;
                        this.subject.loop = this.mLoop;
                        this.subject.Play();
                        this.mStage = 2; // fade-in
                    }
                }
                else if (this.mStage == 2) // fade in
                {
                    this.mFadeInCumu += elapse;
                    float percent = this.mFadeInCumu / this.mFadeInTime;
                    this.subject.volume = percent;
                    if (percent >= 1.0f)
                    {
                        this.mStage = 0;
                    }
                }
            }
        }

        public class AudioState_PlaySequence : AudioState
        {
            private List<AudioClip> mClips = new List<AudioClip>();
            private float mMinInterval = 1.0f;
            private float mMaxInterval = 4.0f;

            private int mIndex = 0;
            private float mInterval = 1.0f;
            private float mCumulant = 0.0f;

            public AudioState_PlaySequence(List<AudioClip> clips, float minInterval, float maxInterval)
            {
                this.mClips = clips;
                this.mMinInterval = minInterval;
                this.mMaxInterval = maxInterval;

                this.mIndex = 0;
                this.mInterval = Random.Range(this.mMinInterval, this.mMaxInterval);
                this.mCumulant = 0.0f;
                this.PlayNext();
            }

            public override void Update(float elapse)
            {
                base.Update(elapse);

                if (this.subject.isPlaying)
                    return;

                if (this.mCumulant < this.mInterval)
                {
                    this.mCumulant += elapse;
                }
                else
                {
                    this.mInterval = Random.Range(this.mMinInterval, this.mMaxInterval);
                    this.mCumulant = 0.0f;
                    this.PlayNext();
                }
            }

            void PlayNext()
            {
                if (this.mClips.Count <= 0)
                    return;
                if (this.mIndex < 0 || this.mIndex >= this.mClips.Count)
                    return;
                this.subject.clip = this.mClips[this.mIndex];
                this.subject.loop = false;
                this.subject.Play();

                this.mIndex++;
                if (this.mIndex >= this.mClips.Count)
                {
                    this.mIndex = this.mIndex % this.mClips.Count;
                }
            }
        }

        public class AudioState_PlayRandom : AudioState
        {
            private List<AudioClip> mClips = new List<AudioClip>();
            private float mMinInterval = 1.0f;
            private float mMaxInterval = 4.0f;

            private float mInterval = 1.0f;
            private float mCumulant = 0.0f;

            public AudioState_PlayRandom(List<AudioClip> clips, float minInterval, float maxInterval)
            {
                this.mClips = clips;
                this.mMinInterval = minInterval;
                this.mMaxInterval = maxInterval;

                this.mInterval = Random.Range(this.mMinInterval, this.mMaxInterval);
                this.mCumulant = 0.0f;
                this.PlayRandom();
            }

            public override void Update(float elapse)
            {
                base.Update(elapse);
                
                if (this.subject.isPlaying)
                    return;

                if (this.mCumulant < this.mInterval)
                {
                    this.mCumulant += elapse;
                }
                else
                {
                    this.mInterval = Random.Range(this.mMinInterval, this.mMaxInterval);
                    this.mCumulant = 0.0f;
                    this.PlayRandom();
                }
            }

            void PlayRandom()
            {
                if (this.mClips.Count <= 0)
                    return;
                int index = Random.Range(0, this.mClips.Count);
                this.subject.clip = this.mClips[index];
                this.subject.Play();
            }
        }
        #endregion
    }

    public class AudioPlayback : MonoBehaviour
    {
        public AudioSource audioSourcePrefab = null;
        public int audioSourceCacheSize = 1;

        private List<AudioSource> mFreeSources = new List<AudioSource>();        
        private List<AudioSource> mAutoSources = new List<AudioSource>();
        private List<AudioSource> mDeadSources = new List<AudioSource>();
        private List<AudioChannel> mChannels = new List<AudioChannel>();

        #region Public Methods
        public void Play(AudioClip clip, bool loop = false, float delay = 0.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.clip = clip;
            source.loop = loop;
            source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, bool loop, float volume, float pitch, float delay = 0.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.pitch = pitch;
            source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Vector3 pos, bool loop = false, float delay = 0.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.transform.position = pos;
            source.clip = clip;
            source.loop = loop;
            source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Vector3 pos, bool loop, float volume, float pitch, float delay = 0.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.transform.position = pos;
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.pitch = pitch;
            source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Transform target, bool loop = false, float delay = 0.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.transform.parent = target;
            source.transform.localPosition = Vector3.zero;
            source.clip = clip;
            source.loop = loop;
            source.PlayDelayed(delay);
        }

        public void Play(AudioClip clip, Transform target, bool loop, float volume, float pitch, float delay = 0.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.transform.parent = target;
            source.transform.localPosition = Vector3.zero;
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.pitch = pitch;
            source.PlayDelayed(delay);
        }

        public void PlayOneShot(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            AudioSource source = this.AcquireSouce();
            source.pitch = pitch;
            source.PlayOneShot(clip, volume);
        }

        public AudioChannel AcquireChannel()
        {
            AudioSource source = this.AcquireSouce(false);
            AudioChannel channel = new AudioChannel(source);
            channel.Init();
            this.mChannels.Add(channel);
            return channel;
        }

        public void ReleaseChannel(AudioChannel channel)
        {
            channel.Quit();
            this.mChannels.Remove(channel);
        }
        #endregion

        #region Unity Life Circle
        void Awake()
        {
            if (this.audioSourcePrefab == null)
            {
                GameObject go = new GameObject("__AudioSource__");
                go.transform.parent = this.transform;

                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.clip = null;

                this.audioSourcePrefab = source;
            }

            while (this.mFreeSources.Count < this.audioSourceCacheSize)
            {
                AudioSource source = this.InstantiateSource();
                this.mFreeSources.Add(source);
            }
        }

        void Update()
        {
            float elapse = Time.deltaTime;

            this.mDeadSources.Clear();
            for (int i = 0; i < this.mAutoSources.Count; i++)
            {
                AudioSource source = this.mAutoSources[i];
                if (!source.isPlaying)
                {
                    this.mDeadSources.Add(source);
                }
            }
            for (int i = 0; i < this.mDeadSources.Count; i++)
            {
                AudioSource source = this.mDeadSources[i];
                this.mAutoSources.Remove(source);
                this.ReleaseSource(source);
            }

            for (int i = 0; i < this.mChannels.Count; i++)
            {
                AudioChannel channel = this.mChannels[i];
                channel.Update(elapse);
            }
        }
        #endregion

        #region Private Methods
        AudioSource AcquireSouce(bool autoRelease = true)
        {
            AudioSource source = null;
            if (this.mFreeSources.Count > 0)
            {
                source = this.mFreeSources[0];
                this.mFreeSources.RemoveAt(0);
                return source;
            }

            source = this.InstantiateSource();
            if (autoRelease)
            {
                this.mAutoSources.Add(source);
            }
            return source;
        }

        void ReleaseSource(AudioSource source)
        {
            source.transform.parent = this.transform;
            this.mFreeSources.Add(source);
        }

        AudioSource InstantiateSource()
        {
            AudioSource source = Object.Instantiate<AudioSource>(this.audioSourcePrefab);
            source.transform.parent = this.transform;
            source.playOnAwake = false;
            return source;
        }
        #endregion
    }
}
