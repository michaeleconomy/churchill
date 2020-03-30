using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatRow : MonoBehaviour {
    public Text labelText, valueText;

    public void Initialize(string label, string value) {
        labelText.text = label;
        valueText.text = value;
    }
}