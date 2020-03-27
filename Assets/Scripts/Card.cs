using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {
    public static string spade = "♠",
        heart = "♥",
        club = "♣",
        diamond = "♦";
    
    public static string[] suits = new[] {spade, heart, club, diamond};

    public bool faceDown = true;
    public Color blackColor, redColor;
    public Sprite backSprite, frontSprite;
    public Collider2D fullCollider, topCollider;

    public TextMesh[] suitTexts, numberTexts;
    public float speed, doubleClickSpeed;

    [NonSerialized]
    public int number;
    [NonSerialized]
    public string suit;

    [NonSerialized]
    public Card cardOnTop, parentCard;

    [NonSerialized]
    public Stack stack;

    [NonSerialized]
    public SortingGroup sortingGroup;

    private Vector2 oldPostition;

    private SpriteRenderer spriteRenderer;
    private DateTime lastClickAt = DateTime.Now;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sortingGroup = GetComponent<SortingGroup>();
    }

    public string StyledNumber() {
        if (number == 1) {
            return "A";
        }
        if (number == 11) {
            return "J";
        }
        if (number == 12) {
            return "Q";
        }
        if (number == 13) {
            return "K";
        }
        return number.ToString();
    }

    public void Flip(bool faceDown) {
        this.faceDown = faceDown;
        Refresh();
    }

    public bool Black {
        get { return suit == spade || suit == club; }
    }

    private Color GetColor() {
        return Black ? blackColor : redColor;
    }

    protected bool Sequential() {
        if (faceDown) {
            return false;
        }
        if (cardOnTop == null) {
            return true;
        }
        return cardOnTop.number + 1 == number && cardOnTop.Black != Black && cardOnTop.Sequential();
    }

    public void Refresh() {
        if (faceDown) {
            fullCollider.enabled = false;
            topCollider.enabled = false;
            spriteRenderer.sprite = backSprite;
            foreach (var suitText in suitTexts) {
                suitText.text = "";
            }
            foreach (var numberText in numberTexts) {
                numberText.text = "";
            }
            return;
        }
        fullCollider.enabled = true;
        UpdateTopCollider();
        spriteRenderer.sprite = frontSprite;
        foreach (var suitText in suitTexts) {
            suitText.text = suit;
            suitText.color = GetColor();
        }
        foreach (var numberText in numberTexts) {
            numberText.text = StyledNumber();
            numberText.color = GetColor();
        }
    }

    public void UpdateTopCollider() {
        topCollider.enabled = Sequential();
    }

    public void Return() {
        StartCoroutine(MoveTo(oldPostition));
    }

    public IEnumerator MoveTo(Vector2 pos) {
        var heading = pos - transform.position.In2d();
        var distance = heading.magnitude;
        var direction = heading / distance;
        while(true) {
            var moveDistance = speed * Time.deltaTime;
            distance -= moveDistance;
            if (distance <= 0) {
                break;
            }
            transform.position += direction.In3d() * moveDistance;

            yield return null;
        }
        transform.position = pos;
        oldPostition = pos;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        sortingGroup.sortingOrder = 99;
        transform.parent = null;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var worldPos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        sortingGroup.sortingOrder = stack.Count();

        transform.parent = parentCard?.transform ?? stack.transform;
        var newStack = GetOverlappingStack();
        if (newStack == null || newStack == stack) {
            Return();
            return;
        }
        newStack.AddCard(this);
    }

    private Stack GetOverlappingStack() {
        var results = new List<Collider2D>();
        var colEnabled = fullCollider.enabled;
        fullCollider.enabled = true;
        fullCollider.OverlapCollider(new ContactFilter2D(), results);
        fullCollider.enabled = colEnabled;
        Debug.Log("overlapping colliders found: " + results.Select(c => c.name).Join());
        var cards = results.Select(r => r.GetComponent<Card>()).OfType<Card>();
        var stacks = cards.Select(c => c.stack).Union(
            results.Select(r => r.GetComponent<Stack>()).OfType<Stack>());
        if (stacks.Empty()) {
            Debug.Log("no overlapping stacks found");
            return null;
        }
        var closest = stacks.First();
        var closestDistance = closest.transform.position.Distance(transform.position);
        foreach (var stack in stacks) {
            var distance = stack.transform.position.Distance(transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = stack;
            }
        }
        Debug.Log("closest: " + closest.name + closest.TopCard());
        return closest;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        var now = DateTime.Now;
        var diff = now - lastClickAt;
        if (diff.TotalSeconds <= doubleClickSpeed) {
            DoubleClick();
        }
        lastClickAt = now;
    }


    private void DoubleClick() {
        Debug.Log("dbl click");
    }

    public override string ToString() {
        return number + suit;
    }
}