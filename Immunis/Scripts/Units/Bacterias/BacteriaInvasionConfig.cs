using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriaInvasionConfig : MonoBehaviour
{
    [SerializeField] private Transform[] m_spawnPoints;
    [SerializeField] private Transform[] m_targets; // TO EXCHANGE FOR A CUSTOM TARGET CLASS
    [SerializeField] private Bacteria m_bacteria;
    [SerializeField] private int m_bacteriasCount = 10;
    [SerializeField] private BacteriaProperty[] m_properties;
    [SerializeField] private float[] m_propertiesLevels;
    [SerializeField] private BioResources m_resourcesValuesOnDie;
    private Dictionary<BacteriaProperty, float> m_bacteriasProperties;

	private void Awake ()
    {
        int propertiesCount = Mathf.Min(m_properties.Length, m_propertiesLevels.Length);
        m_bacteriasProperties = new Dictionary<BacteriaProperty, float>(propertiesCount);
        for(int i = 0; i < propertiesCount; i++)
        {
            m_bacteriasProperties.Add(m_properties[i], m_propertiesLevels[i]);
        }
    }

    public Bacteria bacteria
    {
        get { return m_bacteria; }
    }

    public int bacteriasCount
    {
        get { return m_bacteriasCount; }
    }

    public Dictionary<BacteriaProperty, float> bacteriasProperties
    {
        get { return m_bacteriasProperties; }
    }

    public Transform[] spawnPoints
    {
        get { return m_spawnPoints; }
    }

    public Transform[] targets
    {
        get { return m_targets; }
    }

    public BioResources resourcesValuesOnDie
    {
        get { return m_resourcesValuesOnDie; }
        set { m_resourcesValuesOnDie = value; }
    }
}
