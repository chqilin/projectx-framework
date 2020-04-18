using UnityEngine;

namespace ProjectX
{
    public class AudioModule : AppModule
    {
        private bool mAsyncAsset = false;
        private AudioListener mListener = null;
        private AudioPlayback mPlayback = null;
        private AudioChannel mMusicChannel = null;
        private AudioChannel mSoundChannel = null;

        private AudioClip mLastMusic = null;
        private AudioClip mLastSound = null;

        public AudioClip lastPlayedMusic
        {
            get { return this.mLastMusic; }
        }

        public AudioClip lastPlayedSound
        {
            get { return this.mLastSound; }
        }

        #region Life Circle
        public override bool Init()
        {
            GameObject audio = new GameObject("__Audio__");
            StableObject stable = XUtility.FindOrCreateComponent<StableObject>(audio);
            stable.isStable = true;

            this.mListener = XUtility.FindOrCreateComponent<AudioListener>(audio);
            if (this.mListener == null)
                return false;

            this.mPlayback = XUtility.FindOrCreateComponent<AudioPlayback>(audio);
            if (this.mPlayback == null)
                return false;

            this.mMusicChannel = this.mPlayback.AcquireChannel();
            this.mSoundChannel = this.mPlayback.AcquireChannel();

            return true;
        }

        public override void Quit()
        {
            this.mPlayback.ReleaseChannel(this.mSoundChannel);
            this.mPlayback.ReleaseChannel(this.mMusicChannel);
        }

        public override void Loop(float elapse)
        {
            if (Camera.main != null && this.mListener != null)
            {
                this.mListener.transform.position = Camera.main.transform.position;
                this.mListener.transform.rotation = Camera.main.transform.rotation;
                this.mListener.transform.localScale = Vector3.one;
            }
        }
        #endregion

        public void ChangeMusicVolume(float volume, float time = 1.0f)
        {
            this.mMusicChannel.ChangeVolume(volume, time);
        }

        public void ChangeSoundVolume(float volume, float time = 1.0f)
        {
            this.mSoundChannel.ChangeVolume(volume, time);
        }

        public void PlayMusic(string musicName, bool loop = true)
        {
            if (string.IsNullOrEmpty(musicName))
                return;

            if (this.mMusicChannel.source.clip != null && this.mMusicChannel.source.clip.name == musicName)
                return;

            if (this.mAsyncAsset)
            {
                App.assets.LoadAssetAsync<AudioClip>("Audio", musicName, clip =>
                {
                    this.PlayMusic(clip, loop);
                });
            }
            else
            {
                AudioClip clip = App.assets.LoadAsset<AudioClip>("Audio", musicName);
                this.PlayMusic(clip, loop);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {           
            if (clip == null)
                return;
            this.mLastMusic = this.mMusicChannel.source.clip;
            this.mMusicChannel.Play(clip, loop);
        }

        public void FadeToMusic(string musicName, bool loop = true, float outTime = 1.0f, float inTime = 1.0f)
        {
            if (string.IsNullOrEmpty(musicName))
                return;
            if (this.mMusicChannel.source.clip != null && this.mMusicChannel.source.clip.name == musicName)
                return;
            if (this.mAsyncAsset)
            {
                App.assets.LoadAssetAsync<AudioClip>("Audio", musicName, clip =>
                {
                    this.FadeToMusic(clip, loop, outTime, inTime);
                });
            }
            else
            {
                AudioClip clip = App.assets.LoadAsset<AudioClip>("Audio", musicName);
                this.FadeToMusic(clip, loop, outTime, inTime);
            }
        }

        public void FadeToMusic(AudioClip clip, bool loop = true, float outTime = 1.0f, float inTime = 1.0f)
        {

            if (clip == null)
                return;
            this.mLastMusic = this.mMusicChannel.source.clip;
            this.mMusicChannel.CrossFade(clip, loop, outTime, inTime);
        }

        public void StopMusic()
        {
            this.mMusicChannel.Stop();
        }

        public bool IsPlayingMusic(string name)
        {
            if (!this.mMusicChannel.source.isPlaying || this.mMusicChannel.source.clip == null)
                return false;
            return this.mMusicChannel.source.clip.name == name;
        }

        public void PlaySound(string soundName, bool oneshot = true, float volume = 1.0f, float pitch = 1.0f)
        {
            if (string.IsNullOrEmpty(soundName))
                return;
            if (this.mAsyncAsset)
            {
                App.assets.LoadAssetAsync<AudioClip>("Audio", soundName, clip =>
                {
                    this.PlaySound(clip, oneshot, volume, pitch);
                });
            }
            else
            {
                AudioClip clip = App.assets.LoadAsset<AudioClip>("Audio", soundName);
                this.PlaySound(clip, oneshot, volume, pitch);
            }
        }

        public void PlaySound(AudioClip clip, bool oneshot = true, float volume = 1.0f, float pitch = 1.0f)
        {
            this.mLastSound = this.mSoundChannel.source.clip;
            if (clip != null)
            {
                if (oneshot)
                {
                    this.mSoundChannel.PlayOneShot(clip, volume);
                }
                else
                {
                    this.mSoundChannel.Play(clip, false, volume, pitch);
                }
            }
        }

        public void StopSound()
        {
            this.mSoundChannel.Stop();
        }

        public bool IsPlayingSound(string name)
        {
            if (!this.mSoundChannel.source.isPlaying || this.mSoundChannel.source.clip == null)
                return false;
            return this.mSoundChannel.source.clip.name == name;
        }
    }
}
