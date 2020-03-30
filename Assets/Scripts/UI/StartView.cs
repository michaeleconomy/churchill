using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StartView : MonoBehaviour {
    public GameObject loading, dealButton;

    public void DoneLoading() {
        loading.SetActive(false);
        dealButton.SetActive(true);
    }
}