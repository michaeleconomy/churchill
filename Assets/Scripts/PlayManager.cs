using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayManager : MonoBehaviour {
    public static PlayManager instance;

    public static bool locked = false,
        gameInProgress = false;

    public float padding;

    public Deck deck;
    public DevilsSix devilsSix;
    public Stack stackPrefab;
    public FinalStack finalStackPrefab;
    public Card cardPrefab;
    public StartView startView;
    public WinView winView;

    public readonly List<Stack> stacks = new List<Stack>();
    public readonly List<Stack> finalStacks = new List<Stack>();
    public readonly List<Card> allCards = new List<Card>();

    private readonly Dictionary<string, Stack> allStacks = new Dictionary<string, Stack>();
    private readonly Dictionary<string, Card> cardsById = new Dictionary<string, Card>();

    private bool foreground = true;

    private void Awake() {
        instance = this;
    }
    void OnApplicationFocus(bool hasFocus) {
        foreground = hasFocus;
    }

    public Stack GetStack(string stackName) {
        return allStacks.GetWithDefault(stackName);
    }
    
    public Card GetCard(string cardName) {
        return cardsById.GetWithDefault(cardName);
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
        
        var y = topLeft.y - (cardSize.y * .5f + padding * 3);
        deck.transform.position = new Vector3(topLeft.x + padding * 2 + cardSize.x * 1.5f, y);
        allStacks.Add(deck.name, deck);
        devilsSix.transform.position = new Vector3(topLeft.x + cardSize.x * 3.2f, y);
        allStacks.Add(devilsSix.name, devilsSix);

        8.Times(i => {
            var stack = Instantiate(finalStackPrefab);
            stack.name = "finalStack" + i;
            allStacks.Add(stack.name, stack);
            finalStacks.Add(stack);
            var xOffset = cardSize.x * .45f * i + padding + cardSize.x / 2;
            stack.transform.position = new Vector3(topRight.x - xOffset, y);
            var sortingGroup = stack.GetComponent<SortingGroup>();
            sortingGroup.sortingOrder = 8 - i;
        });

        y -= cardSize.y + padding * 6;
        10.Times(i => {
            var stack = Instantiate(stackPrefab);
            stack.name = "stack " + (i + 1);
            allStacks.Add(stack.name, stack);
            var xOffset = (padding + cardSize.x) * i + padding + cardSize.x / 2;
            stack.transform.position = new Vector3(topLeft.x + xOffset, y);
            stacks.Add(stack);
        });
        2.Times(j => {
            foreach (var suit in Card.suits) {
                13.Times(i => {
                    var card = Instantiate(cardPrefab, deck.transform.position, Quaternion.identity);
                    card.suit = suit;
                    card.number = i + 1;
                    card.deck = j;
                    card.name = card.ToString();
                    card.faceDown = true;
                    card.Refresh();
                    allCards.Add(card);
                    cardsById.Add(card.ToString(), card);
                });
            }
        });

        GameStateManager.instance.Load();

        StartCoroutine(TrackTime());
    }

    public void Deal() {
        if (locked) {
            return;
        }
        locked = true;
        StartCoroutine(DoDeal());
    }

    private IEnumerator DoDeal() {
        if (gameInProgress) {
            Stats.instance.TrackGameEnd(false);
        }
        gameInProgress = true;
        Stats.instance.TrackGameStart();
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
            card.transform.parent = null;
            card.sortingGroup.sortingOrder = -1;
            card.transform.position = deck.transform.position;
            card.Flip(true);
        }
        GameStateManager.instance.Clear();
        foreach (var card in allCards.Shuffle()) {
            deck.AddCardFast(card, true);
        }
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
        GameStateManager.instance.RecordUndo();
        locked = false;
    }

    private IEnumerator AddDevilsSix() {
        var card = deck.TopCard();
        card.Flip(false);
        yield return devilsSix.AddCardSync(card, true);
    }

    public void MoreGames() {
        Application.OpenURL("http://www.styrognome.com");
    }

    public void IntegrityCheck() {
        foreach (var stack in stacks) {
            Card lastCard = null;
            var count = 0;
            for (var card = stack.bottomCard; card != null; card = card.cardOnTop) {
                if (card.parentCard != lastCard) {
                    Debug.LogWarning("invalid parentCard: " + card.parentCard);
                }
                if (card.stack != stack) {
                    Debug.LogWarning("invalid stack: " + card.stack.name + " expected: " + stack.name);
                }
                lastCard = card;
                count ++;
            }

        }
    }

    public void WinCheck() {
        if (!Winning()) {
            return;
        }
        StartCoroutine(FinishOut());
    }

    private IEnumerator FinishOut() {
        locked = true;
        var cardsLeft = allCards.Where(c => !(c.stack is FinalStack)).ToList();

        while (cardsLeft.Count > 0) {
            for (var i = cardsLeft.Count - 1; i >= 0; i--) {
                var card = cardsLeft[i];
                if (card.cardOnTop != null) {
                    continue;
                }

                foreach (var finalStack in finalStacks) {
                    if (finalStack.CanAdd(card)) {
                        cardsLeft.Remove(card);
                        yield return StartCoroutine(finalStack.AddCardSync(card));
                        break;
                    }
                }
            }
        }

        locked = false;
        gameInProgress = false;
        Stats.instance.TrackGameEnd(true);
        winView.Show();
    }

    private IEnumerator TrackTime() {
        while(true) {
            var lastTime = DateTime.Now;
            yield return new WaitForSeconds(1);
            if (gameInProgress && foreground) {
                var elapsed = DateTime.Now - lastTime;
                var elapsedSecs = (float)elapsed.TotalSeconds;
                if (elapsedSecs > 1) {
                    elapsedSecs = 1;
                }
                Stats.instance.TrackTime(elapsedSecs);
            }
        }
    }

    private bool Winning() {
        foreach (var card in allCards) {
            if (card.faceDown) {
                return false;
            }
            if (card.stack is FinalStack) {
                continue;
            }
            if (!card.Sequential()) {
                return false;
            }
        }
        return true;
    }

    public void NoGameToLoad() {
        startView.DoneLoading();
    }

    public void Load(List<RecordedMove> moves) {
        locked = true;
        foreach (var move in moves) {
            LoadMove(move);
        }
        gameInProgress = true;
        locked = false;
        startView.gameObject.SetActive(false);
    }

    private void LoadMove(RecordedMove move) {
        if (!cardsById.TryGetValue(move.card, out var card)) {
            Debug.LogWarning("error loading: card not found: " + move.card);
            return;
        }

        if (move.flip) {
            card.Flip(false, true);
            return;
        }

        if (!allStacks.TryGetValue(move.newStack, out var stack)) {
            Debug.LogWarning("error loading: stack not found: " + move.newStack);
            return;
        }
        stack.AddCardFast(card, true, true);
    }
}