using System;

[Serializable]
public class RecordedMove {
    public string card, newStack, oldStack;
    public bool flip, undoable;
}