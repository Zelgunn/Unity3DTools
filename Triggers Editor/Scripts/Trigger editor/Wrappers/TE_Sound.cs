using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Sound
    {
        [NodeMethod("Sound", "Set clip to play", NodeMethodType.Action)]
        static public void SetAudioClip(AudioSource audioSource, AudioClip newClip)
        {
            audioSource.clip = newClip;
        }

        [NodeMethod("Sound", "Play", NodeMethodType.Action)]
        static public void PlayAudioClip(AudioSource audioSource)
        {
            audioSource.Play();
        }

        [NodeMethod("Sound", "Play clip", NodeMethodType.Action)]
        static public void PlayAudioClip(AudioSource audioSource, AudioClip newClip)
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }

        [NodeMethod("Sound", "Stop", NodeMethodType.Action)]
        static public void StopAudioClip(AudioSource audioSource)
        {
            audioSource.Stop();
        }

        [NodeMethod("Sound", "Pause", NodeMethodType.Action)]
        static public void PauseAudioClip(AudioSource audioSource)
        {
            audioSource.Pause();
        }

        [NodeMethod("Sound", "Resume", NodeMethodType.Action)]
        static public void ResumeAudioClip(AudioSource audioSource)
        {
            audioSource.UnPause();
        }

        [NodeMethod("Sound", "Set volume", NodeMethodType.Action)]
        static public void SetVolumeIntensity(AudioSource audioSource, float intensity)
        {
            audioSource.volume = intensity;
        }

        [NodeMethod("Sound", "Play sound (once)", NodeMethodType.Action)]
        static public void PlaySoundOnce(AudioSource audioSource, AudioClip sound)
        {
            audioSource.PlayOneShot(sound);
        }

        [NodeMethod("Sound", "Play sound (at point)", NodeMethodType.Action)]
        static public void PlayClipAtPoint(AudioClip clip, Vector3 position)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}
