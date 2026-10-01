using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements.Experimental;



public class Card : MonoBehaviour
{    
    [SerializeField] public int Value = 0;
    [SerializeField] public bool  isEquipped;
    [SerializeField] private GameObject crackOverlay;

    // private values for the card
    private int Damage;
    private int Shield;
    private int Heal;
    public static event Action onSkip;
    public static event Action<CardDissolve> onAttack;
    public static event Action<Card> onEquip; 
    public UnityEvent onEnter;
    public UnityEvent onExit;
    private bool CanSkip = true;

    // Events
    public static event Action<int> onHealthChange;
    public static event Action<int> onArmorChange;
    public static event Action<string> makeSound;

    //public static event Action onArmorUse;
    //public static event Action<Card> onArmorReplace;

    private void OnEnable() {
        deckMaster.enableSkip += EnableSkip;
        deckMaster.disableSkip += DisableSkip;
    }

    private void OnDisable() {
        deckMaster.enableSkip -= EnableSkip;
        deckMaster.disableSkip -= DisableSkip;

    }

    void EnableSkip()
    {
        if (CompareTag("Special") && !CanSkip)
        {
            GetComponent<CardVisual>().FlipCard();
            CanSkip = true;
        }
    }
    
    void DisableSkip()
    {
        if (CompareTag("Special") && CanSkip)
        {
            GetComponent<CardVisual>().FlipCard();
            CanSkip = false;
        }
    }

    void OnMouseDown()
    {
        // Early return guard clause, to prevent clicking when paused.
        if (Time.timeScale == 0f) return;

        // Early guard to prevent clicking cards on stack.
        if (GetComponent<CardVisual>().isFlipped == false && !CompareTag("Special")) return;

        if (CompareTag("Special") && CanSkip) 
        {
            GetComponent<CardVisual>().FlipCard();
            CanSkip = false;
            onSkip.Invoke();
        } else if (CompareTag("Special") && !CanSkip)
        {
            Debug.Log("You can't Skip");
        } else if (CompareTag("Clubs") || CompareTag("Spades"))
        {
            onHealthChange?.Invoke(-Value);
            onAttack?.Invoke(GetComponentInChildren<CardDissolve>());
            makeSound.Invoke("attack");
            //onArmorUse?.Invoke();
            
        } else if (CompareTag("Cups"))
        {
            onHealthChange?.Invoke(Value);
            makeSound.Invoke("heal");
        } else if (CompareTag("Pentacles"))
        {   
            //(isEquipped = true) ? onArmorReplace.Invoke(this) : isEquipped = true;
            gameObject.tag = "Equipped";
            if (isEquipped)
            {
                //onArmorReplace.Invoke(this);
            } else
            {
                isEquipped = true;
            }
            gameObject.tag = "Equipped";
            onArmorChange?.Invoke(Value);
            makeSound.Invoke("buff");
        }
    }

    void OnMouseEnter()
    {
        onEnter.Invoke();
    }

    void OnMouseExit()
    {
        onExit.Invoke();
    }

    void Start() // start of frame we check the inspector tag of the gameObject.
    {
        if (gameObject.CompareTag("Cups"))
        {
            Heal = Value;
        } else if (gameObject.CompareTag("Pentacles"))
        {
            Shield = Value;
        } else if (gameObject.CompareTag("Spades") || gameObject.CompareTag("Clubs"))
        {
            Damage = Value;
        }
    }
}
