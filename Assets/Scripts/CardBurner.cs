using System;
using System.Collections;
using UnityEngine;

public class CardDissolve : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float dissolveDuration = 3f;
    [SerializeField] private float startCutoff = 4f;
    [SerializeField] private float endCutoff = -1f;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    // Shader property ID ("_CutoffHeight" is the standard Reference name in Shader Graph)
    private static readonly int CutoffHeightID = Shader.PropertyToID("_CutoffHeight");
    private MaterialPropertyBlock propBlock;

    public static event Action<GameObject> removeFromDeck;

    void OnEnable()
    {
        Card.onAttack += BurnCard;
    }

    void OnDisable()
    {
        Card.onAttack -= BurnCard;
    }

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        // Auto-grab renderers if left unassigned
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    [ContextMenu("Burn Card")]
    public void BurnCard(CardDissolve a)
    {
        Debug.Log("Attempting to burn enemy...");
        if (a != null && a == this)
        {
            if (Application.isPlaying)
            {
                removeFromDeck.Invoke(transform.parent.gameObject);
                StartCoroutine(DissolveRoutine());
            }
            else
            {
                Debug.LogWarning("[CardDissolve] Inspector testing works best during Play Mode!");
            }
        }
    }

    private IEnumerator DissolveRoutine()
    {
        float elapsed = 0f;

        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / dissolveDuration);

            // Lerp cutoff height from 0 to -5
            float currentCutoff = Mathf.Lerp(startCutoff, endCutoff, progress);

            // Apply value to all child sprite renderers
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null) continue;

                spriteRenderers[i].GetPropertyBlock(propBlock);
                propBlock.SetFloat(CutoffHeightID, currentCutoff);
                spriteRenderers[i].SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        ApplyCutoffValue(endCutoff);

        Destroy(transform.parent.gameObject);
    }

    private void ApplyCutoffValue(float val)
    {
        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;

            spriteRenderers[i].GetPropertyBlock(propBlock);
            propBlock.SetFloat(CutoffHeightID, val);
            spriteRenderers[i].SetPropertyBlock(propBlock);
        }
    }
}

// Custom Editor to render the "Burn Card" button directly in the Inspector
