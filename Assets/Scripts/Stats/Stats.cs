using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour {

    public static Stats instance;

    private const int numRecent = 100;
    private History history;

    private void Awake() {
        instance = this;
        Load();
    }
    
    public int Wins {
        get {
            return history.records.Count(r => r.LegitWin);
        }
    }

    public int TotalWins {
        get { return history.records.Count(r => r.win); }
    }

    public int UndoWins {
        get { return history.records.Count(r => r.win && r.hardUndo); }
    }

    public int Undos {
        get {
            return PlayerPrefs.GetInt("undos", 0);
        }
    }

    public float TimeElapsed {
        get {
            return PlayerPrefs.GetFloat("time", 0);
        }
    }

    public int TotalGames {
        get {
            return Games.Count;
        }
    }

    public int TotalTime {
        get {
            return Games.Sum(g => g.timeSpent);
        }
    }

    public string TotalTimeString {
        get {
            return FormatDuration(TotalTime);
        }
    }

    public string FirstGamePlayed {
        get {
            var game = Games.First();
            if (game == null) {
                return DateTime.Now.ToString("M/d/y");
            }
            return game.StartTime().ToString("M/d/y");
        }
    }

    public string AverageGameLength {
        get {
            var avg = TotalGames == 0 ? 0 : TotalTime / TotalGames;
            return FormatDuration(avg);
        }
    }

    public string AverageGameLengthRecent {
        get {
            var recent = RecentGames();
            var time = recent.Sum(g => g.timeSpent);
            var avg = recent.Count() == 0 ? 0 : time / recent.Count();
            return FormatDuration(avg);
        }
    }

    public string AverageGameLengthWin {
        get {
            var wins = Games.Where(g => g.win);
            var time = wins.Sum(g => g.timeSpent);
            var winsCount = wins.Count();
            var avg = winsCount == 0 ? 0 : time / winsCount;
            return FormatDuration(avg);
        }
    }

    public string RateOfGamesPlayed {
        get {
            var game = Games.First();
            if (game == null) {
                return "0 games/day";
            }
            var duration = DateTime.UtcNow - game.StartTime();
            var dRate = TotalGames / duration.TotalDays;
            if (dRate > 1) {
                return dRate.ToString("F1") + " games/day";
            }

            var wRate = dRate * 7;
            if (wRate > 1) {
                return wRate.ToString("F1") + " games/week";
            }

            var mRate = dRate * 30.5;
            return mRate.ToString("F1") + " games/month";
        }
    }

    private string FormatDuration(int secs) {
        var s = "";
        var time = (int)secs;
        var hours = time / (60 * 60);
        s += hours + ":";
        var minutes = (time % (60 * 60)) / 60;
        s += minutes / 10;
        s += minutes % 10  + ":";
        var seconds = time % 60;
        s += seconds / 10;
        s += seconds % 10;
        return s;
    }

    public string WinRateAllTime {
        get { return Percentage(Wins, TotalGames); }
    }

    private string Percentage(int numerator, int divisor) {
        var p = divisor == 0 ? 0f : (float)numerator / divisor;
        return p.ToString("P");
    }


    public string WinRateRecent {
        get { return Percentage(RecentWins, RecentGames().Count()); }
    }

    public int RecentWins {
        get { return RecentGames().Count(g => g.LegitWin); }
    }

    private IEnumerable<GameRecord> RecentGames() {
        return Games.TakeLast(numRecent);
    }

    public List<GameRecord> Games {
        get { return history.records; }
    }

    public void TrackGameStart() {
        PlayerPrefs.SetString("start", CurrentTimeSerialized());
    }

    public void TrackUndo() {
        PlayerPrefs.SetInt("undos", Undos + 1);
    }

    public void TrackHardUndo() {
        PlayerPrefs.SetString("hardundo", "true");
    }

    public void TrackTime(float secs) {
        PlayerPrefs.SetFloat("time", TimeElapsed + secs);
    }

    public void TrackGameEnd(bool win) {
        var record = new GameRecord {
            win = win,
            hardUndo = PlayerPrefs.GetString("hardundo", null) == "true",
            undos = Undos,
            timeSpent = Mathf.CeilToInt(TimeElapsed),
            start = PlayerPrefs.GetString("start", null),
            end = CurrentTimeSerialized(),
        };
        history.records.Add(record);
        Save();
        PlayerPrefs.DeleteKey("undos");
        PlayerPrefs.DeleteKey("hardundo");
        PlayerPrefs.DeleteKey("start");
        PlayerPrefs.DeleteKey("time");
    }

    private string CurrentTimeSerialized() {
        return DateTime.UtcNow.ToString();
    }

    private void Save() {
        var json = JsonUtility.ToJson(history);
        File.WriteAllText(HistoryPath(), json);
    }

    private void Load() {
        try {
            var json = File.ReadAllText(HistoryPath());
            history =  JsonUtility.FromJson<History>(json);
            return;
        }
        catch (FileNotFoundException) { }
        catch (Exception e) {
            Debug.LogWarning(e);
        }
        history = new History();
    }

    private string HistoryPath() {
        return Application.persistentDataPath + "/history.json";
    }
}