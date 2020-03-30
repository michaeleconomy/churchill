using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DictionaryExtensions {
    public static int Increment<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int amount = 1) {
        var val = dictionary.GetWithDefault(key) +
            amount;
        dictionary[key] = val;
        return val;
    }


    public static int Decrement<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int amount = 1) {
        return dictionary.Increment(key, -amount);
    }


    public static int GetWithDefault<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int def = 0) {
        if (!dictionary.TryGetValue(key, out var val)) {
            return def;
        }
        return val;
    }


    public static KeyValuePair<TKey, TValue>? Pop<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) {
        if (dictionary.Empty()) {
            return null;
        }
        var pair = dictionary.First();
        dictionary.Remove(pair.Key);
        return pair;
    }


    public static TVal GetWithDefault<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, TKey key) where TVal : class {
        return dictionary.GetWithDefault(key, null);
    }


    public static TVal GetWithDefault<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, TKey key, TVal def) {
        if (!dictionary.TryGetValue(key, out var val)) {
            return def;
        }
        return val;
    }


    public static void AddAll<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, Dictionary<TKey, TVal> other) {
        foreach (var pair in other) {
            dictionary.Add(pair.Key, pair.Value);
        }
    }

}
