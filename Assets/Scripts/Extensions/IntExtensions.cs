using System;

public static class IntExtensions {
    public static void Times(this int times, Action<int> action) {
        for(var i = 0; i < times; i++) {
            action(i);
        }
    }
}