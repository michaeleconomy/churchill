using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameRecord {
    public bool win, hardUndo;
    public string start, end;
    public int undos, timeSpent;

    public string Letter {
        get {
            if (hardUndo && win) {
                return "U";
            }
            return win ? "W" : "L";
        }
    }
    public bool LegitWin {
        get { return win && !hardUndo; }
    }
    public DateTime StartTime() {
        if (!DateTime.TryParse(start, out var time)) {
            Debug.Log("error parsing time: " + start);
            return DateTime.Now;
        }
        return time.ToLocalTime();
    }

    public DateTime EndTime() {
        if (!DateTime.TryParse(end, out var time)) {
            Debug.Log("error parsing time: " + end);
            return DateTime.Now;
        }
        return time.ToLocalTime();
    }
}