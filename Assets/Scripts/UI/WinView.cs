using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinView : MonoBehaviour {
    public Text winCountText;

    public void Show() {
        winCountText.text = "" + Stats.instance.Wins;
        gameObject.SetActive(true);
    }

    public void NewDeal() {
        PlayManager.instance.Deal();
        gameObject.SetActive(false);
    }
}