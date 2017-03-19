using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    static private SoundManager s_singleton;
    private AudioSource m_audioSource;
    [Header("Musics")]
    [SerializeField] private AudioClip m_outsideBodyMusic;
    [SerializeField] private AudioClip m_onWoundMusic;
    [SerializeField] private AudioClip m_preparationMusic;
    [SerializeField] private AudioClip m_invasionMusic;

    [Header("Sound effects")]
    [SerializeField] private AudioClip[] m_cellSpawnSoundEffects;
    [SerializeField] private AudioClip m_errorSoundEffect;
    [SerializeField] private AudioClip[] m_digestionSoundEffects;
    [SerializeField] private AudioClip m_researchSoundEffect;

    private void Awake ()
    {
        s_singleton = this;
        m_audioSource = GetComponent<AudioSource>();
    }

    private void _PlayMusic(GamePhase phase)
    {
        AudioClip music = m_outsideBodyMusic;

        switch (phase)
        {
            case GamePhase.OutsideBody:
                music = m_outsideBodyMusic;
                break;
            case GamePhase.Wound:
                music = m_onWoundMusic;
                break;
            case GamePhase.Preparation:
                music = m_preparationMusic;
                break;
            case GamePhase.Invasion:
                music = m_invasionMusic;
                break;
        }

        if((music == m_audioSource.clip) && (m_audioSource.isPlaying))
        {
            return;
        }

        m_audioSource.clip = music;
        m_audioSource.Play();
        m_audioSource.loop = true;
    }

    private void _PlaySoundEffect(AudioClip clip)
    {
        m_audioSource.PlayOneShot(clip);
    }

    private void _PlaySoundEffect(AudioClip[] clips)
    {
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        _PlaySoundEffect(randomClip);
    }

    static public void PlayMusic(GamePhase phase)
    {
        s_singleton._PlayMusic(phase);
    }

    static public void PlayCellSpawnSoundEffect()
    {
        s_singleton._PlaySoundEffect(s_singleton.m_cellSpawnSoundEffects);
    }

    static public void PlayErrorSoundEffect()
    {
        s_singleton._PlaySoundEffect(s_singleton.m_errorSoundEffect);
    }

    static public void PlayDigestionSoundEffect()
    {
        s_singleton._PlaySoundEffect(s_singleton.m_digestionSoundEffects);
    }

    static public void PlayResearchSoundEffect()
    {
        s_singleton._PlaySoundEffect(s_singleton.m_researchSoundEffect);
    }
}
