
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FactorySoundEffect : MonoBehaviour
{
    [SerializeField] private AudioClip soundClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlaySound()
    {
        if (audioSource == null || soundClip == null)
            return;

        audioSource.PlayOneShot(soundClip);
    }
}
