using System;
using UnityEngine;


public static class VectorExtensions {
    public static Vector2 In2d(this Vector3 v) {
        return (Vector2)v;
    }
    
    public static Vector3 In3d(this Vector2 v) {
        return (Vector3)v;
    }

    public static float Distance(this Vector3 v, Vector3 other) {
        return Vector3.Distance(v, other);
    }
}