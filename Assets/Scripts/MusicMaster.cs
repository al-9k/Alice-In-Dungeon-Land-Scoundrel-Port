using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomBGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private bool playOnStart = true;

    private AudioSource audioSource;
    private int lastTrackIndex = -1;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (playOnStart && tracks.Length > 0)
        {
            PlayNextTrack();
        }
    }

    private void Update()
    {
        // Automatically queue the next song once the current one ends
        if (!audioSource.isPlaying && tracks.Length > 0)
        {
            PlayNextTrack();
        }
    }

    public void PlayNextTrack()
    {
        if (tracks.Length == 0) return;

        int nextIndex = lastTrackIndex;

        // Avoid repeating the same track back-to-back if there are multiple songs
        if (tracks.Length > 1)
        {
            while (nextIndex == lastTrackIndex)
            {
                nextIndex = Random.Range(0, tracks.Length);
            }
        }
        else
        {
            nextIndex = 0;
        }

        lastTrackIndex = nextIndex;
        audioSource.clip = tracks[nextIndex];
        audioSource.Play();
    }
}