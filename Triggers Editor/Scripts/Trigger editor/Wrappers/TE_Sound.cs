using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Sound
    {
        [NodeMethod("Sound", "Set clip to play")]
        static public void SetAudioClip(AudioSource audioSource, AudioClip newClip)
        {
            audioSource.clip = newClip;
        }

        [NodeMethod("Sound", "Play")]
        static public void PlayAudioClip(AudioSource audioSource)
        {
            audioSource.Play();
        }

        [NodeMethod("Sound", "Play clip")]
        static public void PlayAudioClip(AudioSource audioSource, AudioClip newClip)
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }

        [NodeMethod("Sound", "Stop")]
        static public void StopAudioClip(AudioSource audioSource)
        {
            audioSource.Stop();
        }

        [NodeMethod("Sound", "Pause")]
        static public void PauseAudioClip(AudioSource audioSource)
        {
            audioSource.Pause();
        }

        [NodeMethod("Sound", "Resume")]
        static public void ResumeAudioClip(AudioSource audioSource)
        {
            audioSource.UnPause();
        }

        [NodeMethod("Sound", "Set volume")]
        static public void SetVolumeIntensity(AudioSource audioSource, float intensity)
        {
            audioSource.volume = intensity;
        }

        [NodeMethod("Sound", "Play sound (once)")]
        static public void PlaySoundOnce(AudioSource audioSource, AudioClip sound)
        {
            audioSource.PlayOneShot(sound);
        }

        [NodeMethod("Sound", "Play sound (at point)")]
        static public void PlayClipAtPoint(AudioClip clip, Vector3 position)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}
