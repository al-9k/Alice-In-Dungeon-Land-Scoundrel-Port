using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.InputSystem.Composites;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEditor;
using Unity.Cinemachine;


public class deckMaster : MonoBehaviour
{   
    [Header("Deck Appearance Knobs")]
    [SerializeField] float fanAngle = 3;
    [SerializeField] float potrudeOffset = 0.01f;
    [SerializeField] float randomOffset = 0.002f;
    [SerializeField] float moveSpeed = 20;

    [Header("Back Sprite")]
    [SerializeField] Sprite backOfCard;

    [Header("Card Suit Templates")]
    [SerializeField] GameObject[] suitTemplates;

    [Header("Game Slots")]
    [SerializeField] Transform[] playSlots;
    [SerializeField] Transform[] tempSlots;
    [SerializeField] Transform armorSlot;
     [SerializeField] Transform heartSlot;

    [Header("Deck-Wide Audio")]
    [SerializeField] public List<AudioClip> drawClips;
    [SerializeField] public List<AudioClip> playClips;
    [SerializeField] public List<AudioClip> hoverClips;
    [SerializeField] public List<AudioClip> attackClips;
    [SerializeField] public List<AudioClip> healClips;
    [SerializeField] public List<AudioClip> buffClips;
    [SerializeField] public List<AudioClip> discardClips;
    [SerializeField] public AudioClip Beep;
    public AudioSource audioSource;

    [Header("Health Display")]
    public RectTransform healthDisplay;
    public RectTransform countDisplay;
    [SerializeField] private int healthPoints = 20;

    [Header("Run Trackers")]
    private int skullsFractured = 0;
    private int goblinsSlaughtered = 0;
    private int heartsEaten = 0;
    private int shieldsWorn = 0;
    private int shieldsShattered = 0;
    private int roomsSkipped = 0;
    private float runTimer = 0f;

    [Header("Run Meta")]
    private int currentSeed;
    private bool isVictory = false;
    private string deathReason = string.Empty;

    [Header("Private References")]
    private int armorPoints;
    private bool armorisUsed;
    private bool isProcessingDeck;
    private Coroutine healthRoutine;

    /* 
    Various Event Controls
    */
    public static event Action onDrawFinish;
    public static event Action<GameObject> shatterArmor;
    public static event Action onArmorCrack;
    public static event Action<int> onHeal;
    public static event Action onRemove;
    public static event Action enableSkip;
    public static event Action disableSkip;
    public static event Action onTransition;
    public static event Action<int> onRunComplete;

    //UnityEvent OnDeckClear;

    // Card lists 
    public List<GameObject> Deck = new List<GameObject>();
    public List<GameObject> inPlay = new List<GameObject>();
    public GameObject Equipped;

    private void OnEnable() {
        Card.onSkip += ClearRoom;
        Card.onHealthChange += UpdateHealth;
        Card.onArmorChange += OnArmorChange;
        Card.makeSound += PlayRandom;
        CardDissolve.removeFromDeck += RemoveBurntCard;
    }

    private void OnDisable() {
        Card.onSkip -= ClearRoom;
        Card.onHealthChange -= UpdateHealth;
        //Card.onArmorUse -= OnArmorUse;
        Card.onArmorChange -= OnArmorChange;
        Card.makeSound -= PlayRandom;
        CardDissolve.removeFromDeck -= RemoveBurntCard;
    }
    public void PlayRandom(string a)
    {
        if (a == "draw" && drawClips != null && drawClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, drawClips.Count);
            audioSource.PlayOneShot(drawClips[randomIndex]);
        } else if (a == "play" && playClips != null && playClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, playClips.Count);
            audioSource.PlayOneShot(playClips[randomIndex]);
        } else if (a == "hover" && hoverClips != null && hoverClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, hoverClips.Count);
            audioSource.PlayOneShot(hoverClips[randomIndex]);
        } else if (a == "attack" && attackClips != null && attackClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, attackClips.Count);
            audioSource.PlayOneShot(attackClips[randomIndex]);
        } else if (a == "heal" && healClips != null && healClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, healClips.Count);
            audioSource.PlayOneShot(healClips[randomIndex]);
        } else if (a == "buff" && buffClips != null && buffClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, buffClips.Count);
            audioSource.PlayOneShot(buffClips[randomIndex]);
        } else if (a == "discard" && discardClips != null && discardClips.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, discardClips.Count);
            audioSource.PlayOneShot(discardClips[randomIndex]);
        }
    }

    // Helper shuffle function using Fisher-Yates
    void Shuffle(List<GameObject> a, int seed)
    {
        Debug.Log($"Deck size: {Deck.Count}");
        System.Random rng = new System.Random(seed);
        for (int i = a.Count-1; i > 0; i--)
        {   
            // Randomize a number between 0 and i so that the range shrinks each time.
            int rnd = rng.Next(0, i + 1);
            // save the value of current item, otherwise it will overrwrite when we swap values.
            GameObject temp = a[i];
            a[i] = a[rnd];
            a[rnd] = temp;
        }
        // Print result
        for (int i = 0; i < a.Count; i++)
		{
			Debug.Log (a[i]);
		}
    }

    public void CardInitializer(GameObject a, string suit, int i)
    {
        a.GetComponentInChildren<SpriteRenderer>().sprite = backOfCard;
        a.transform.Find("Face").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"{i}_of_{suit}");
        a.GetComponent<Card>().Value = i;
        a.name = $"{a.GetComponent<Card>().Value} of {suit}";
    }

    public void CardRestacker(List<GameObject> a)
    {   Debug.Log($"Reseting {a.Count} cards positions...");
        for (int i = 0; i < a.Count; i++)
        {
            a[i].transform.position = transform.position;
            a[i].transform.rotation = transform.rotation;
        }
        Debug.Log($"Restacking {a.Count} cards...");
        for (int i = 0; i < a.Count; i++)
        {

            SpriteRenderer renderer = a[i].GetComponentInChildren<SpriteRenderer>();
            renderer.sortingOrder = i;
            a[i].transform.Translate((i*potrudeOffset)+UnityEngine.Random.Range(-randomOffset, randomOffset), (i*potrudeOffset)+UnityEngine.Random.Range(-randomOffset, randomOffset), 0);
            a[i].transform.Rotate(0, 0, UnityEngine.Random.Range(-fanAngle, fanAngle));
        }
    }

    private IEnumerator DrawCardsRoutine(int n)
    {   
        if (Deck.Count != 0)
        {   
            audioSource.PlayOneShot(drawClips[UnityEngine.Random.Range(0, drawClips.Count)]);
            for (int i = 0; i < n; i++)
            {
                int lastIndex = Deck.Count - 1;
                GameObject cardObject = Deck[lastIndex];
                Deck.RemoveAt(lastIndex);
                inPlay.Add(cardObject);
                cardObject.GetComponent<CardVisual>().cardIndex = i;
                //StartCoroutine(SlideCardToSlot(cardObject, playSlots[i].position, playSlots[i].rotation, 0.3f, i));
                
                cardObject.GetComponent<CardVisual>().SlideToSlot(moveSpeed, playSlots[i].position, playSlots[i].rotation, playSlots[i].localScale);
                audioSource.PlayOneShot(playClips[UnityEngine.Random.Range(0, playClips.Count)]);
                if (i != n-1)
                {
                    yield return new WaitForSeconds(0.2f);
                } else
                {
                    yield return new WaitForSeconds(0.5f);
                    onDrawFinish.Invoke();
                }
            }
        } else if (Deck.Count == 0)
        {
            Debug.Log("Deck empty, later have an event invoked here.");
        }   
    }

    private IEnumerator RemoveCardsRoutine()
    {   
        if (inPlay.Count == 0) yield break;
        int count = Mathf.Min(inPlay.Count, tempSlots.Length); // Guard against index out of range
        for (int i = 0; i < count; i++)
        {
            //onRemove.Invoke();
            CardVisual visual = inPlay[i].GetComponentInChildren<CardVisual>();
            if (visual != null)
            {
                 visual.SlideToSlot(moveSpeed, tempSlots[i].position, tempSlots[i].rotation, tempSlots[i].localScale);
                 PlayRandom("discard");
                 yield return new WaitForSeconds(0.25f);
            }
        }
        yield return new WaitForSeconds(0.6f); // Adjust duration to match your slide speed
        while (inPlay.Count > 0)
        {
            GameObject cardObject = inPlay[0];
            inPlay.RemoveAt(0);
            Deck.Insert(0, cardObject);
            CardVisual visual = cardObject.GetComponentInChildren<CardVisual>();
        }
        CardRestacker(Deck);
        if (Deck.Count != 0)
            {
                int cardsToDraw = Mathf.Min(4, Deck.Count);
                StartCoroutine(DrawCardsRoutine(cardsToDraw));
                onTransition?.Invoke();
            } else if (Deck.Count == 0 && inPlay.Count == 0)
            {
                Debug.Log("Dungeon Cleared! You Win!");
                // Trigger Victory Screen / End Game state
            }
    }
    private IEnumerator ShatterShield(GameObject a)
    {
        shatterArmor?.Invoke(a);
        yield return new WaitForSeconds(0.5f*Time.deltaTime);
        Destroy(a);
    }
    
     public void OnArmorChange(int a)
    {   
        for (int i = 0; i < inPlay.Count; i++)
        {
            if (inPlay[i].CompareTag("Equipped") && inPlay[i].GetComponent<Card>().Value == a)
            {
                if (Equipped != null)
                { 
                    StartCoroutine(ShatterShield(Equipped));
                }
                Equipped = inPlay[i];
                inPlay.RemoveAt(i);
                Equipped.GetComponent<CardVisual>().SlideToSlot(moveSpeed, armorSlot.position, armorSlot.rotation, armorSlot.localScale, true);
                break;
            }
        }
        CheckDeck();
        shieldsWorn += a;
        Debug.Log($"Armor is Acquired, value: {a}");
        Debug.Log($"Cards in deck: {Deck.Count}");
        armorisUsed = false;
        armorPoints = a;
    }
    private void RemoveBurntCard(GameObject a)
    {
        for (int i = 0; i < inPlay.Count; i++)
        {
            if (inPlay[i] == a)
            {
                int cardValue = a.GetComponent<Card>().Value;
                if (a.CompareTag("Clubs"))
                {
                    if (healthPoints<= 0) {EndRun($"{cardValue} of Skulls", false);} else {skullsFractured += cardValue;}
                }
                else if (a.CompareTag("Spades"))
                {
                    if (healthPoints<= 0) {EndRun($"{cardValue} of Goblins", false);} else {goblinsSlaughtered += cardValue;}
                }
                inPlay.RemoveAt(i);
                break;
            }
        }
        CheckDeck();
    }
    private void UpdateHealth(int a)
    {
        if (a < 0)
        {
            int modifier = (armorPoints >= -a) ? 0 : armorPoints + a;
            healthPoints = Mathf.Clamp(healthPoints + modifier, 0, 20);
            if (healthRoutine != null) { StopCoroutine(healthRoutine); } 
            healthRoutine = StartCoroutine(DynamicHealthDisplay(modifier));
            if (Equipped != null && armorisUsed)
            {
                StartCoroutine(ShatterShield(Equipped));
                Equipped = null;
                shieldsShattered += armorPoints;
                armorPoints = 0;
                armorisUsed = false;
                Debug.Log("Armor is shattered!");
            } else if (Equipped != null && !armorisUsed)
            {
                onArmorCrack?.Invoke();
                armorPoints = Equipped.GetComponent<Card>().Value - 1;
                armorisUsed = true;
                Debug.Log("Armor is damaged!!");
            }
            Debug.Log($"Suffered {modifier} points of damage!");
        } else
        {
            for (int i = 0; i < inPlay.Count; i++)
            {
                if (inPlay[i].GetComponent<Card>().Value == a && inPlay[i].CompareTag("Cups"))
                {
                    GameObject Heart = inPlay[i];
                    inPlay.RemoveAt(i);
                    Heart.GetComponent<CardVisual>().SlideToSlot(moveSpeed, heartSlot.position, heartSlot.rotation, heartSlot.localScale, false, true);
                }
            }
            CheckDeck();
            heartsEaten += a;
            healthPoints = Mathf.Clamp(healthPoints + a, 0, 20);
            if (healthRoutine != null) { StopCoroutine(healthRoutine); } 
            healthRoutine = StartCoroutine(DynamicHealthDisplay(a));
            Debug.Log($"Healed by {a} points!");
        }
        Debug.Log($"Cards in deck: {Deck.Count}");
    }

    public IEnumerator DynamicHealthDisplay(int amount)
    {
        //int startHealth = healthPoints;
        int uiHealth = int.Parse(healthDisplay.GetComponentInChildren<TextMeshProUGUI>().text);
        int targetHealth = healthPoints;
        int totalChange = targetHealth - uiHealth;

        if (totalChange == 0) yield break;

        int direction = totalChange > 0 ? 1 : -1;
        
        if (direction < 0)
        {
            Debug.Log("Dropping health...");
        } else if (direction > 0)
        {
            Debug.Log("Raising health...");
        }

        int steps = Mathf.Abs(totalChange);

        Vector3 originalScale = healthDisplay.GetComponentInChildren<Image>().transform.localScale;
        Vector3 pulseScale = originalScale * 1.2f; // Subtle 20% scale bump

        for (int i = 0; i < steps; i++)
        {
            //healthPoints += direction;
            uiHealth += direction;
            audioSource.PlayOneShot(Beep);
            int spriteIndex = Mathf.Clamp(uiHealth / 2, 0, 10);
            healthDisplay.GetComponentInChildren<TextMeshProUGUI>().text = $"{uiHealth}";

            Image heartImage = healthDisplay.GetComponentInChildren<Image>();
            heartImage.sprite = healthDisplay.GetComponentInChildren<HealthVisual>().Hearts[spriteIndex];

            Transform heartTransform = heartImage.transform;
            float duration = 0.15f; 
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                heartTransform.localScale = Vector3.Lerp(originalScale, pulseScale, elapsed / duration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                heartTransform.localScale = Vector3.Lerp(pulseScale, originalScale, elapsed / duration);
                yield return null;
            }
            heartTransform.localScale = originalScale;        
        }
        healthRoutine = null;
    }

    public void MakeRoom()
    {
        int cardsToDraw = Mathf.Min(4, Deck.Count);
        StartCoroutine(DrawCardsRoutine(cardsToDraw));
    }

    public void ClearRoom()
    {
        roomsSkipped ++ ;
        StartCoroutine(RemoveCardsRoutine());
    }

    public void CheckDeck()
    {
        if (inPlay.Count == 3)
        {
            disableSkip.Invoke();
        }
        StartCoroutine(DeckPlay());
    }

    private void UpdateCounter()
    {
        audioSource.PlayOneShot(Beep);
        countDisplay.GetComponentInChildren<TextMeshProUGUI>().text = $"{Deck.Count + inPlay.Count}";
    }

    private IEnumerator DeckPlay()
    {
        if (isProcessingDeck) yield break;
        UpdateCounter();
        if (inPlay.Count == 1 && Deck.Count > 0)
        {
            isProcessingDeck = true;
            yield return StartCoroutine(RemoveCardsRoutine());
            for (int i = 0; i < inPlay.Count; i++)
            {
                Debug.Log(inPlay[i]);
            }
            isProcessingDeck = false;
            enableSkip.Invoke();
        }
        if (Deck.Count == 0 && inPlay.Count == 0)
            {
                EndRun("Dungeon now runs quiet", true);
                Debug.Log("Dungeon Cleared! You Win!");
                // Trigger Victory Screen / End Game state
            }
    }
 
    public void EndRun(string cause, bool isWinner)
    {
        LastRunData.SaveRun(currentSeed, roomsSkipped, skullsFractured, goblinsSlaughtered, heartsEaten, shieldsWorn, shieldsShattered, runTimer, isWinner, cause);
        onRunComplete?.Invoke(0);
    }

    public void GiveUp()
    {
        LastRunData.SaveRun(currentSeed, roomsSkipped, skullsFractured, goblinsSlaughtered, heartsEaten, shieldsWorn, shieldsShattered, runTimer, false, "Their own sword");
        onRunComplete?.Invoke(0);
    }

    void Start()
    {
        LastRunData.ResetData();
        currentSeed = UnityEngine.Random.Range(1000, 9999);

        for (int i = 2; i < 15; i++)
        {
            GameObject spadeCard = Instantiate(suitTemplates[0], gameObject.transform);
            GameObject clubCard = Instantiate(suitTemplates[1], gameObject.transform);
            CardInitializer(spadeCard, "spades", i);
            CardInitializer(clubCard, "clubs", i);
            Deck.Add(spadeCard);
            Deck.Add(clubCard);
        };

        for (int i = 2; i < 11; i++)
        {
            GameObject pentacleCard = Instantiate(suitTemplates[2], gameObject.transform);
            GameObject cupCard = Instantiate(suitTemplates[3], gameObject.transform);
            CardInitializer(pentacleCard, "pentacles", i);
            CardInitializer(cupCard, "cups", i);
            Deck.Add(pentacleCard);
            Deck.Add(cupCard);
        };
        // Do the shuffle!
        Shuffle(Deck, currentSeed);
        CardRestacker(Deck);
        MakeRoom();
    }

    private void Update()
    {
        runTimer += Time.deltaTime;
    }
}
