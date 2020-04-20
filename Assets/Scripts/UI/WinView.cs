using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.iOS;

public class WinView : MonoBehaviour {
    public Text winCountText;

    private const int promptReviewThreshold = 3;

    public void Show() {
        winCountText.text = "" + Stats.instance.Wins;
        gameObject.SetActive(true);
        PromptReview();
    }

    private bool PromptReview() {
        if (Stats.instance.Wins < promptReviewThreshold) {
            return false;
        }
        if (PlayerPrefs.HasKey("promptedForReview")) {
            return false;
        }
        PlayerPrefs.SetString("promptedForReview", "true");
        DialogView.Show("Are you enjoying Solitaire: Winston Churchill?", new List<string> { "Yes", "No" },  (option) => {
            if (option == 0) {
#if UNITY_IOS
                if (!Device.RequestStoreReview()) {
                    DialogView.Prompt("Glad to hear it!");
                }
#elif UNITY_ANDROID
                DialogView.Confirm("Would you care to write a review?", () => {
                    Application.OpenURL("market://details?id=com.styrognome.churchill");
                });
#else
                UIDialog.Alert("Glad to hear it!");
#endif
            }
            else {
                DialogView.Confirm("Would you like to send us feedback so we can improve?", () => {
                    Application.OpenURL("http://www.styrognome.com/contact");
                });
            }
        });
        return true;
    }

    public void NewDeal() {
        PlayManager.instance.Deal();
        gameObject.SetActive(false);
    }
}