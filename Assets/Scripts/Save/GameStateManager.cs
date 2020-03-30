using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameStateManager : MonoBehaviour {
    public static GameStateManager instance;

    public Button undoButton;

    private GameState state = new GameState();

    private List<RecordedMove> Moves {
        get { return state.moves; }
    }

    private void Awake() {
        instance = this;
    }

    public void Clear() {
        state = new GameState();
        RefreshUndoButton();
    }

    public void RecordMove(Card card, Stack oldStack, Stack newStack) {
        Moves.Add(new RecordedMove{
            card = card.ToString(),
            newStack = newStack.ToString(),
            oldStack = oldStack?.ToString()
        });
    }
    public void RecordFlip(Card card) {
        Moves.Add(new RecordedMove{
            card = card.ToString(),
            flip = true
        });
    }

    public void RecordUndo() {
        var move = Moves.Last();
        if (move == null) {
            Debug.LogWarning("Can't record undo, no moves");
            return;
        }
        move.undoable = true;
        RefreshUndoButton();
        Save();
    }
    private RecordedMove PrevUndo() {
        var last = Moves.Last();
        if (last == null) {
            return null;
        }
        return Moves.Last(m => m.undoable && m != last);
    }
    public bool CanUndo() {
        return PrevUndo() != null;
    }
    
    public bool IsUndoHard() {
        if (Moves.Count < 2) {
            return false;
        }
        return !Moves[Moves.Count -2].undoable;
    }

    public void UndoClick() {
        if (!CanUndo()) {
            Debug.LogWarning("Can't undo!");
            return;
        }
        if (IsUndoHard()) {
            DialogView.Confirm("New cards have been revealed. This game will not 'count' as a win if you undo.\n" +
                "(will be counted as a loss in statistics)\n"+
                "Do you still wish to undo?", Undo);
            return;
        }
        Undo();
    }

    private void RefreshUndoButton() {
        undoButton.interactable = CanUndo();
    }

    private void Undo() {
        if (PlayManager.locked) {
            return;
        }
        Stats.instance.TrackUndo();

        if (IsUndoHard()) {
            Stats.instance.TrackHardUndo();
        }
        PlayManager.locked = true;
        StartCoroutine(DoUndo());
    }

    private IEnumerator DoUndo() {
        var prevUndo = PrevUndo();
        if (prevUndo == null) {
            Debug.LogWarning("Can't undo, no undos");

            PlayManager.locked = false;
            yield break;
        }

        while (Moves.Last() != prevUndo) {
            var move = Moves.Last();
            Moves.Remove(move);
            yield return StartCoroutine(UndoStep(move));
        }
        Save();
        RefreshUndoButton();
        PlayManager.locked = false;
    }

    private IEnumerator UndoStep(RecordedMove move) {
        if (move == null) {
            Debug.LogWarning("can't undo a null move");
            yield break;
        }
        var card = PlayManager.instance.GetCard(move.card);
        if (card == null) {
            Debug.LogWarning("card not found: " + move.card);
            yield break;
        }
        if (move.flip) {
            card.Flip(true, true);
            yield break;
        }
        var stack = PlayManager.instance.GetStack(move.oldStack);
        if (stack == null) {
            Debug.LogWarning("oldStack not found: " + move.oldStack);
            yield break;
        }
        yield return StartCoroutine(stack.AddCardSync(card, true, true));
    }

    public void Load() {
        try {
            var json = File.ReadAllText(FileName());
            var loadedState = JsonUtility.FromJson<GameState>(json);
            PlayManager.instance.Load(loadedState.moves);
            state = loadedState;
            RefreshUndoButton();
            return;
        }
        catch (FileNotFoundException) { }
        catch (Exception e) {
            Debug.LogWarning(e);
        }
        state = new GameState();
        PlayManager.instance.NoGameToLoad();
    }

    private void Save() {
        var json = JsonUtility.ToJson(state);
        File.WriteAllText(FileName(), json);
    }

    private string FileName() {
        return Application.persistentDataPath + "/cards.json";
    }
}
