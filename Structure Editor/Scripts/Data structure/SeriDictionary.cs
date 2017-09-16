using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SeriDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private TKey[] m_keys;
    [SerializeField] private TValue[] m_values;
    private Dictionary<TKey, TValue> m_dictionary = new Dictionary<TKey, TValue>();

    public Dictionary<TKey, TValue> dictionary
    {
        get { return m_dictionary; }
        set { m_dictionary = value; }
    }

    public void OnAfterDeserialize()
    {
        if(m_keys == null)
        {
            m_dictionary = new Dictionary<TKey, TValue>();
        }
        else
        {
            m_dictionary = new Dictionary<TKey, TValue>(m_keys.Length);
            for (int i = 0; i < m_keys.Length; i++)
            {
                m_dictionary.Add(m_keys[i], m_values[i]);
            }
        }
    }

    public void OnBeforeSerialize()
    {
        if(m_dictionary == null)
        {
            m_keys = null;
            m_values = null;
        }
        else
        {
            m_keys = new List<TKey>(m_dictionary.Keys).ToArray();
            m_values = new List<TValue>(m_dictionary.Values).ToArray();
        }
    }
}