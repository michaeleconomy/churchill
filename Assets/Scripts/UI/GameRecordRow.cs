using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRecordRow : MonoBehaviour {
    public Text outcomeText, dateText, durationText, undosText;

    public void Initialize(GameRecord game) {
        outcomeText.text = game.Letter;
        dateText.text = game.EndTime().ToString("M/d/y H:mm");
        durationText.text = (game.timeSpent / 60) + "min " + (game.timeSpent % 60) + " sec";
        undosText.text = game.undos + " undos";
    }
}