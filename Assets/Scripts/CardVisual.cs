using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class CardVisual : MonoBehaviour
{   
    [SerializeField] float manualTiltAmount = 20f;
    [SerializeField] float autoTiltAmount = 30f;
    [SerializeField] float tiltSpeed = 20f;
    [SerializeField] GameObject[] Faces;
    [SerializeField] public int cardIndex = 0;
    [SerializeField] private GameObject crackVisual;
    [Header("Blink Effect")]
    public float blinkDuration = 1f;
    public float blinkFrequency = 10f;

    private bool isHovering;
    public bool isFlipped;
    [SerializeField] private bool inPlay;
    

    void OnEnable()
    {
        deckMaster.onDrawFinish += OnDrawFinish;
        deckMaster.onArmorCrack += ShowArmorDamage;
        deckMaster.onRemove += RemoveFromPlay;
    }

    void OnDisable()
    {
        deckMaster.onDrawFinish -= OnDrawFinish;
        deckMaster.onArmorCrack -= ShowArmorDamage;
        deckMaster.onRemove -= RemoveFromPlay;
    }

    public void OnEnter()
    {
        isHovering = true;
        //makeSound?.Invoke("hover");
    }

    public void OnExit()
    {
        isHovering = false;
    }

    public void OnDrawFinish()
    {
        if (inPlay) {
            isFlipped = (CompareTag("Special") || CompareTag("Equipped")) ? isFlipped : !isFlipped;
        }
    }

    public void ShowArmorDamage()
    {
        StartCoroutine(ArmorDamage());
    }
    
    private IEnumerator ArmorDamage()
    {
        if (!gameObject.CompareTag("Equipped")) yield break;

        if (crackVisual != null)
        {
            crackVisual.SetActive(true);
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0) yield break;

        float elapsed = 0f;
        float interval = 1f / Mathf.Max(1f, blinkFrequency);
        bool visible = true;

        while (elapsed < blinkDuration)
        {
            visible = !visible;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }
    }
    public void FlipCard()
    {
        isFlipped = !isFlipped;
    }

    public void SlideToSlot(float moveSpeed, Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, bool isArmor = false, bool isHeart = false, bool isRemoving = false)
    {   
        float distance = Vector3.Distance(transform.position, targetPos);
    
        // Prevent division by zero if already at target
        if (distance <= 0.001f) return; 

        float duration = distance / moveSpeed;

        if (isHeart)
        {
            StartCoroutine(SlideRoutine(targetPos, duration, true, true, true, targetScale, targetRot));
        } else  if (!isArmor && !isHeart) {
            cardIndex = 0;
            inPlay = !inPlay;
            if (isFlipped) isFlipped = !isFlipped;
            StartCoroutine(SlideRoutine(targetPos, duration, true, true, false, targetScale, targetRot));      
        }
        else
        {
            StartCoroutine(SlideRoutine(targetPos, duration));
        }      
    }

    public void ResetVisual()
    {
        cardIndex = 0;
        inPlay = !inPlay;
        if (isFlipped)
        {
            isFlipped = !isFlipped;
        }
    }

    private IEnumerator SlideRoutine(Vector3 targetPos ,float duration, bool scale = false, bool rotate = false, bool destroy = false, Vector3 targetScale = default, Quaternion targetRot = default)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {  
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float smoothPercent = Mathf.Sin(percent * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, targetPos, smoothPercent);
            if (rotate) transform.rotation = Quaternion.Lerp(startRot, targetRot, smoothPercent);
            if (scale) transform.localScale = Vector3.Lerp(startScale, targetScale, smoothPercent);

            yield return null;
        }
        
        transform.position = targetPos;
        if (scale) transform.localScale = targetScale;
        if (rotate) transform.rotation = targetRot;
        if (destroy) Destroy(gameObject);
    }

    private void CardTilt()
    {   
        if (inPlay || gameObject.CompareTag("Special")) {
    
            Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float sine = Mathf.Sin(Time.time + cardIndex) * (isHovering ? .2f : 1);
            float cosine = Mathf.Cos(Time.time + cardIndex) * (isHovering ? .2f : 1);
            
            float tiltX = isHovering ? (offset.y * -1 * manualTiltAmount) : 0;
            float tiltY = isHovering ? (offset.x * -1 * manualTiltAmount) : 0;

            float targetX = tiltX + (sine * autoTiltAmount);
            float targetY = tiltY + (cosine * autoTiltAmount);

            float lerpX = Mathf.LerpAngle(transform.eulerAngles.x, targetX, tiltSpeed * Time.deltaTime);
            float lerpY = Mathf.LerpAngle(transform.eulerAngles.y, targetY + (isFlipped ? 180f : 0f), tiltSpeed * Time.deltaTime);

            transform.eulerAngles = new Vector3(lerpX, lerpY, 0);
        }
    }

    private void RemoveFromPlay()
    {
        
    }

    void Update()
    {
        CardTilt();            
    }
}

   