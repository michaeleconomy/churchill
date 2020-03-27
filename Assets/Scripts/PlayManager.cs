using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayManager : MonoBehaviour {
    public static PlayManager instance;

    public float padding;

    public Deck deck;
    public DevilsSix devilsSix;
    public Stack stackPrefab;
    public FinalStack finalStackPrefab;
    public Card cardPrefab;

    public readonly List<Stack> stacks = new List<Stack>();
    public readonly List<Stack> finalStacks = new List<Stack>();
    public readonly List<Card> allCards = new List<Card>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        var tempCard = Instantiate(cardPrefab);

        var cardCol = tempCard.GetComponent<BoxCollider2D>();
        var cardSize = cardCol.bounds.size;
        Destroy(tempCard.gameObject);

        var camera = Camera.main;
        var idealWidth = (cardSize.x + padding) * 10 + padding;
        camera.orthographicSize = idealWidth / (camera.aspect * 2);

        var topLeft = camera.ViewportToWorldPoint(Vector3.up);
        var topRight = camera.ViewportToWorldPoint(new Vector3(1, 1));
        
        var y = topLeft.y - (cardSize.y * .5f + padding);
        deck.transform.position = new Vector3(topLeft.x + padding * 2 + cardSize.x * 1.5f, y);
        devilsSix.transform.position = new Vector3(topLeft.x + padding * 3 + cardSize.x * 3f, y);

        8.Times(i => {
            var stack = Instantiate(finalStackPrefab);
            finalStacks.Add(stack);
            var xOffset = cardSize.x * .5f * i + padding + cardSize.x / 2;
            stack.transform.position = new Vector3(topRight.x - xOffset, y);
            var sortingGroup = stack.GetComponent<SortingGroup>();
            sortingGroup.sortingOrder = 8 - i;
        });

        y -= cardSize.y + padding;
        10.Times(i => {
            var stack = Instantiate(stackPrefab);
            var xOffset = (padding + cardSize.x) * i + padding + cardSize.x / 2;
            stack.transform.position = new Vector3(topLeft.x + xOffset, y);
            stacks.Add(stack);
        });
        2.Times(_ => {
            foreach (var suit in Card.suits) {
                13.Times(i => {
                    var card = Instantiate(cardPrefab);
                    card.suit = suit;
                    card.number = i + 1;
                    card.faceDown = true;
                    card.Refresh();
                    allCards.Add(card);
                });
            }
        });

    }

    public void Deal() {
        StartCoroutine(DoDeal());
    }

    private IEnumerator DoDeal() {
        deck.bottomCard = null;
        devilsSix.bottomCard = null;
        foreach (var stack in stacks) {
            stack.bottomCard = null;
        }
        foreach (var stack in finalStacks) {
            stack.bottomCard = null;
        }
        foreach (var card in allCards.Shuffle()) {
            card.parentCard = null;
            card.cardOnTop = null;
            card.stack = null;
            card.Flip(true);
        }
        foreach (var card in allCards.Shuffle()) {
            yield return StartCoroutine(deck.AddCardSync(card, true, true));
        }
        deck.AlignCollider();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AddDevilsSix());
        for(var i = 0; i < 5; i++) {
            for (var stackIndex = i; stackIndex < 10 - i; stackIndex++) {
                var stack = stacks[stackIndex];
                var card = deck.TopCard();
                if (stackIndex == i || stackIndex == 9 - i) {
                    card.Flip(false);
                }
                yield return StartCoroutine(stack.AddCardSync(card, true));
            }
            yield return StartCoroutine(AddDevilsSix());
        }
    }

    private IEnumerator AddDevilsSix() {
        var card = deck.TopCard();
        card.Flip(false);
        yield return devilsSix.AddCardSync(card, true);
    }


    public void MoreGames() {
        Application.OpenURL("http://www.styrognome.com");
    }
}