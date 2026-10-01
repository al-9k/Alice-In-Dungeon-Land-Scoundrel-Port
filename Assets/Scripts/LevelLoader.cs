using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup faderCanvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;

    private void Start()
    {
        // Automatically fade in when the scene starts
        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.alpha = 1f;
            StartCoroutine(Fade(1f, 0f));
        }
    }

    void OnEnable()
    {
        deckMaster.onRunComplete += TransitionToScene;
    }

    void OnDisable()
    {
        deckMaster.onRunComplete -= TransitionToScene;
    }

    public void TransitionToScene(int sceneNum)
    {
        StartCoroutine(FadeAndLoad(sceneNum));
    }

    private IEnumerator FadeAndLoad(int sceneNum)
    {
        // Block button clicks while fading
        faderCanvasGroup.blocksRaycasts = true;

        // Fade from clear (0) to black (1)
        yield return StartCoroutine(Fade(0f, 1f));

        // Load the new scene
        SceneManager.LoadScene(sceneNum);
    }

    private IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null; // Wait for next frame
        }

        faderCanvasGroup.alpha = targetAlpha;

        // Allow UI clicks again if the panel is fully transparent
        if (targetAlpha == 0f)
        {
            faderCanvasGroup.blocksRaycasts = false;
        }
    }
}