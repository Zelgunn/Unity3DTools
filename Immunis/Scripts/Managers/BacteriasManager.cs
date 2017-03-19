using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriasManager : MonoBehaviour
{
    static private BacteriasManager s_singleton;

    [SerializeField] private float m_timeBetweenBacteriasSpawn = 0.02f;
    [SerializeField] private int m_bacteriasCountSpawnAtATime = 2;

    [Header("Bacterias' properties")]
    [SerializeField] private float m_multiplierChancesFactor = 0.01f;
    [SerializeField] private float m_smallBacteriaScaleFactor = 0.9f;
    [SerializeField] private float m_bigBacteriaScaleFactor = 1.1f;
    [SerializeField] private float m_quickBacteriaSpeedFactor = 1.1f;
    [SerializeField] private float m_slowBacteriaSpeedFactor = 1.1f;
    [SerializeField] private float m_bacteriaResistanceFactor = 1.1f;
    [SerializeField] private float m_bacteriaBerserkFactor = 1.1f;
    [SerializeField] private float m_bacteriaKillerChanceFactor = 1.05f;

    [Header("Tech")]
    [SerializeField] private int m_reductionPerAnticipationRank = 1;
    [SerializeField] private float m_antiBacteriaFactor = 0.25f;
    [SerializeField] private float m_antiEColiFactor = 0.25f;
    [SerializeField] private float m_antiPollenFactor = 0.25f;

    private BacteriaInvasionConfig[] m_invasionsConfigurations;
    private int m_currentInvasionIndex = 0;
    private List<Bacteria> m_invasion;

    private void Awake ()
    {
        s_singleton = this;
        m_invasionsConfigurations = GetComponentsInChildren<BacteriaInvasionConfig>();
	}
	
    public IEnumerator StartInvasionCoroutine()
    {
        BacteriaInvasionConfig config = m_invasionsConfigurations[m_currentInvasionIndex++];
        m_invasion = new List<Bacteria>(config.bacteriasCount);
        Transform[] spawnPoints = config.spawnPoints; int spawnPointsCount = spawnPoints.Length;
        Transform[] targets = config.targets; int targetsCount = targets.Length;
        int bacteriasCount = config.bacteriasCount - m_reductionPerAnticipationRank * TechTree.TechRank(TechID.Evol_Anticipation);

        int i = 0;
        while(i < bacteriasCount)
        {
            for(int j = 0; j < m_bacteriasCountSpawnAtATime; j++)
            {
                Bacteria bacteria = Instantiate(config.bacteria);
                bacteria.transform.SetParent(transform);
                bacteria.transform.position = spawnPoints[i%spawnPointsCount].position;
                bacteria.AttackPosition(targets[i%targetsCount].position);
                bacteria.SetProperties(config.bacteriasProperties);
                bacteria.resourcesValue = config.resourcesValuesOnDie;
                m_invasion.Add(bacteria);
                i++;
            }
            yield return new WaitForSeconds(m_timeBetweenBacteriasSpawn);
        }
    }

    public void StopInvasion()
    {
        for(int i = 0; i < m_invasion.Count; i++)
        {
            Bacteria bacteria = m_invasion[i];
            if(bacteria)
            {
                Destroy(bacteria.gameObject);
            }
        }

        m_invasion = null;
    }

    static public BacteriasManager singleton
    {
        get { return s_singleton; }
    }

    public void AddBacteriaToInvasion(Bacteria bacteria)
    {
        m_invasion.Add(bacteria);
    }

    public int livingBacteriasCount
    {
        get
        {
            int result = 0;

            foreach (Bacteria bacteria in m_invasion)
            {
                if (bacteria && !bacteria.dead)
                {
                    result++;
                }
            }

            return result;
        }
    }

    public BacteriaInvasionConfig nextInvasionConfig
    {
        get { return m_invasionsConfigurations[m_currentInvasionIndex]; }
    }

    public int nextInvasionBacteriasCount
    {
        get { return m_invasionsConfigurations[m_currentInvasionIndex].bacteriasCount; }
    }

    #region Bacteria Properties
    static public float BacteriaPropertyAtLevel(BacteriaProperty property, float level)
    {
        switch(property)
        {
            case BacteriaProperty.Multiplier:
                level = s_singleton.m_multiplierChancesFactor * level;
                break;
            case BacteriaProperty.Small:
                level = s_singleton.m_smallBacteriaScaleFactor * level;
                break;
            case BacteriaProperty.Big:
                level = s_singleton.m_bigBacteriaScaleFactor * level;
                break;
            case BacteriaProperty.Slow:
                level = s_singleton.m_slowBacteriaSpeedFactor * level;
                break;
            case BacteriaProperty.Quick:
                level = s_singleton.m_quickBacteriaSpeedFactor * level;
                break;
            case BacteriaProperty.Resistant:
                level = s_singleton.m_bacteriaResistanceFactor * level;
                break;
            case BacteriaProperty.Berserker:
                level = s_singleton.m_bacteriaBerserkFactor * level;
                break;
            case BacteriaProperty.Killer:
                level = s_singleton.m_bacteriaKillerChanceFactor * level;
                break;
        }
        return level;
    }
    #endregion

    #region Tech
    static public float antiBacteriaFactor
    {
        get
        {
            return s_singleton.m_antiBacteriaFactor * TechTree.TechRank(TechID.Adapt_AntiBacteria);
        }
    }

    static public float antiEColiFactor
    {
        get
        {
            return s_singleton.m_antiEColiFactor * TechTree.TechRank(TechID.Adapt_AntiEColi);
        }
    }

    static public float antiPollenFactor
    {
        get
        {
            return s_singleton.m_antiPollenFactor * TechTree.TechRank(TechID.Adapt_AntiPollen);
        }
    }
     #endregion
}
