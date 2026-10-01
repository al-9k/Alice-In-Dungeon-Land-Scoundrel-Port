using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(MeshRenderer))]
public class BackgroundSelector : MonoBehaviour
{
    [SerializeField] private VideoClip[] backgrounds;
    [SerializeField] private float fadeDuration = 1.0f;

    private VideoPlayer videoPlayer;
    private Material quadMaterial;
    private bool isTransitioning;
    private int currentIndex = -1;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        quadMaterial = GetComponent<MeshRenderer>().material;

        if (backgrounds.Length > 0)
        {
            currentIndex = GetRandomUniqueIndex();
            videoPlayer.clip = backgrounds[currentIndex];
        }
    }

    private void OnEnable()
    {
        deckMaster.onTransition += HandleTransition;
    }

    private void OnDisable()
    {
        deckMaster.onTransition -= HandleTransition;
    }

    private void HandleTransition()
    {
        if (!isTransitioning && backgrounds.Length > 0)
        {
            StartCoroutine(TransitionRoutine());
        }
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        // 1. Fade Quad to Black
        float elapsed = 0f;
        Color originalColor = Color.white;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            quadMaterial.color = Color.Lerp(originalColor, Color.black, elapsed / fadeDuration);
            yield return null;
        }
        quadMaterial.color = Color.black;

        // 2. Pick a NEW random video clip & Prepare
        currentIndex = GetRandomUniqueIndex();
        videoPlayer.clip = backgrounds[currentIndex];
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // 3. Play & Fade back to White
        videoPlayer.Play();

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            quadMaterial.color = Color.Lerp(Color.black, originalColor, elapsed / fadeDuration);
            yield return null;
        }
        quadMaterial.color = originalColor;

        isTransitioning = false;
    }

    private int GetRandomUniqueIndex()
    {
        // If there's only 1 or 0 clips, repeat prevention isn't possible
        if (backgrounds.Length <= 1) return 0;

        int nextIndex;
        do
        {
            nextIndex = UnityEngine.Random.Range(0, backgrounds.Length);
        } 
        while (nextIndex == currentIndex);

        return nextIndex;
    }
}