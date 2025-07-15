using System.Collections.Generic;

[System.Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public SerializableKeyValuePair()
    { }

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public KeyValuePair<TKey, TValue> ToKeyValuePair()
    {
        return new KeyValuePair<TKey, TValue>(Key, Value);
    }
}