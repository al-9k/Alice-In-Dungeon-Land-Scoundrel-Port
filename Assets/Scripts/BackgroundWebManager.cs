using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(MeshRenderer))]
public class BackgroundWebManager : MonoBehaviour
{
    [Header("Video File Names (e.g. cardCarpet.mp4)")]
    [SerializeField] private string[] backgroundFileNames;
    [SerializeField] private float fadeDuration = 1.0f;

    private VideoPlayer videoPlayer;
    private Material quadMaterial;
    private bool isTransitioning;
    private int currentIndex = -1;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        quadMaterial = GetComponent<MeshRenderer>().material;

        // Force source to URL mode for WebGL compatibility
        videoPlayer.source = VideoSource.Url;

        if (backgroundFileNames.Length > 0)
        {
            currentIndex = GetRandomUniqueIndex();
            videoPlayer.url = GetVideoUrl(backgroundFileNames[currentIndex]);
            videoPlayer.Prepare();
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
        if (!isTransitioning && backgroundFileNames.Length > 0)
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

        // 2. Pick a NEW random video file & Prepare via URL
        currentIndex = GetRandomUniqueIndex();
        videoPlayer.url = GetVideoUrl(backgroundFileNames[currentIndex]);
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

    private string GetVideoUrl(string fileName)
    {
        // Points directly to Assets/StreamingAssets/Backgrounds/fileName.mp4 in editor and builds
        return Path.Combine(Application.streamingAssetsPath, "Backgrounds", fileName);
    }

    private int GetRandomUniqueIndex()
    {
        if (backgroundFileNames.Length <= 1) return 0;

        int nextIndex;
        do
        {
            nextIndex = UnityEngine.Random.Range(0, backgroundFileNames.Length);
        } 
        while (nextIndex == currentIndex);

        return nextIndex;
    }
}