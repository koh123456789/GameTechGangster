using UnityEngine;

public class BossAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Action Clips")]
    public AudioClip swordSwoosh;
    public AudioClip shootingSound;
    public AudioClip deathScream;
    public AudioClip upgradeSound;
    public AudioClip moneyCollect;

    [Header("Looping Clips")]
    public AudioClip retreatGoofyRun;

    // Use this for instant sounds like swings or deaths
    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // This handles the looping logic for movement states
    public void ToggleLoopingSound(bool shouldPlay, AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        if (shouldPlay)
        {
            // Only change the clip if we aren't already playing it
            if (audioSource.clip != clip)
            {
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            // Only stop if the CURRENT clip is the one we want to stop
            if (audioSource.clip == clip && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.clip = null; // Clear it so it doesn't "lock" the source
                audioSource.loop = false;
            }
        }
    }
}