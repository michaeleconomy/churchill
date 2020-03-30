using System;
using UnityEngine;

[Serializable]
public class CardBack {
    public int winsToUnlock;
    public Sprite backSprite;

    public bool Locked {
        get { return Stats.instance.Wins < winsToUnlock; }
    }

    public int WinsNeeded {
        get { return winsToUnlock - Stats.instance.Wins; }
    }

    public bool Selected {
        get { return backSprite == CardBacks.instance.selectedCardback; }
    }
}