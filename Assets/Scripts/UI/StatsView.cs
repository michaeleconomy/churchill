using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsView : MonoBehaviour {

    public Text closeButtonText;
    public StatRow statRowPrefab;
    public GameRecordRow gameRowPrefab;
    public Transform statsList, historyList;

    public GameObject mainMenu;

    public void Show() {
        closeButtonText.text = PlayManager.gameInProgress ? "Back to Game" : "Back to Main Menu";
        statsList.DeleteChildren();
        AddStat("Total Wins", Stats.instance.Wins);
        AddStat("Total Wins* (includes undos)", Stats.instance.TotalWins);
        AddStat("Undo Wins", Stats.instance.UndoWins);
        AddStat("Total Games", Stats.instance.TotalGames);
        AddStat("Overall Win %", Stats.instance.WinRateAllTime);
        AddStat("Recent Win %", Stats.instance.WinRateRecent);
        AddStat("Total Time", Stats.instance.TotalTimeString);
        AddStat("First Game Played", Stats.instance.FirstGamePlayed);
        AddStat("Average Game Length", Stats.instance.AverageGameLength);
        AddStat("Average Game Length (recent)", Stats.instance.AverageGameLengthRecent);
        AddStat("Average Game Length (win)", Stats.instance.AverageGameLengthWin);
        AddStat("Games Played (Rate)", Stats.instance.RateOfGamesPlayed);

        historyList.DeleteChildren();
        for (var i = Stats.instance.Games.Count - 1; i >= 0; i--) {
            var game = Stats.instance.Games[i];
            var row = Instantiate(gameRowPrefab, historyList);
            row.Initialize(game);
        }

        gameObject.SetActive(true);
    }

    public void Close() {
        gameObject.SetActive(false);
        if (!PlayManager.gameInProgress) {
            mainMenu.SetActive(true);
        }
    }

    private void AddStat(string label, System.Object value) {
        var statRow = Instantiate(statRowPrefab, statsList);
        statRow.Initialize(label, value.ToString());
    }
}