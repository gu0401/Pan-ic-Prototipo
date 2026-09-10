using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Sons")]
    public AudioClip pickupSound;
    public AudioClip placeSound;
    public AudioClip mixSound;
    public AudioClip ovenSound;
    public AudioClip finishedSound;
    public AudioClip trashSound;
    public AudioClip buttonSound;
    public AudioClip errorSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void TestPickupSound()
    {
        PlaySFX(pickupSound);
    }
}