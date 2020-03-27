using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevilsSix : Stack {
    public override bool CanAdd(Card card) {
        return false;
    }
}