using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public enum BacteriaProperty
{
    Multiplier,     // Done
    Vicious,
    Resistant,
    Quick,
    Slow,
    Mutant,
    Zombie,
    Big,
    Small,
    Boss,
    Berserker,      // Done
    Killer,
    Kamikaze
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class Bacteria : Unit
{
    protected int m_antigenID;
    protected Dictionary<BacteriaProperty, float> m_properties;
    protected BioResources m_resourcesValue;

    [SerializeField] protected Animator m_overrideAnimator;
    [SerializeField] protected UnitDecorum m_decorum;

    override protected void Awake ()
    {
        base.Awake();
        m_faction = UnitFaction.Bacteria;

        m_decorum.Init(gameObject);
        if(m_overrideAnimator)
        {
            m_decorum.OverrideAnimator(m_overrideAnimator);
        }
    }

    override protected void Update ()
    {
        base.Update();
        UpdateRealtimeProperties();
	}

    #region Bacterias properties
    protected void UpdateRealtimeProperties()
    {
        float multiplierLevel;
        if (HasProperty(BacteriaProperty.Multiplier, out multiplierLevel))
        {
            if (Random.Range(0f, 1f) < (BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Multiplier, multiplierLevel) * Time.deltaTime))
            {
                Bacteria bacteria = Instantiate(this);
                bacteria.m_commandsQueue = new Queue<UnitCommands.UnitCommand>(m_commandsQueue);
                bacteria.transform.SetParent(transform.parent);
                BacteriasManager.singleton.AddBacteriaToInvasion(bacteria);
            }
        }
    }

    public void UpdateStaticProperties()
    {
        float level;
        if (HasProperty(BacteriaProperty.Small, out level))
        {
            transform.localScale *= BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Small, level);
        }

        if (HasProperty(BacteriaProperty.Big, out level))
        {
            transform.localScale *= BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Big, level);
        }

        if (HasProperty(BacteriaProperty.Slow, out level))
        {
            m_navMeshAgent.speed = m_speed * BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Slow, level);
        }

        if (HasProperty(BacteriaProperty.Quick, out level))
        {
            m_navMeshAgent.speed = m_speed * BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Quick, level);
        }

        if(HasProperty(BacteriaProperty.Resistant, out level))
        {
            level = BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Resistant, level);
            m_maxHealthPoints *= level;
            m_healthPoints *= level;
        }
    }

    public void AddProperty(BacteriaProperty property, float value)
    {
        if(m_properties == null)
        {
            m_properties = new Dictionary<BacteriaProperty, float>();
        }

        if(m_properties.ContainsKey(property))
        {
            Debug.LogWarning("Trying to add property to bacteria that already has this property.");
            return;
        }

        m_properties.Add(property, value);
        UpdateStaticProperties();
    }

    public void SetProperties(Dictionary<BacteriaProperty, float> properties)
    {
        m_properties = properties;
        UpdateStaticProperties();
    }

    public bool HasProperty(BacteriaProperty property, out float level)
    {
        if((m_properties == null) || !m_properties.ContainsKey(property))
        {
            level = 0;
            return false;
        }

        level = m_properties[property];
        return true;
    }

    #endregion

    protected override void OnTakeDamage(Unit source, float damagesTaken)
    {
        if(dead)
        {
            return;
        }

        float berserkLevel;
        if(!m_isBerserk && HasProperty(BacteriaProperty.Berserker, out berserkLevel) && (m_healthPoints / m_maxHealthPoints) < 0.3f)
        {
            m_isBerserk = true;
            m_attackRate /= BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Berserker, berserkLevel);
        }
    }

    protected override void OnDealDamage(Unit unit, float damagesDealt)
    {
        if (!unit || unit.dead)
        {
            return;
        }

        float killerLevel;
        if (HasProperty(BacteriaProperty.Killer, out killerLevel))
        {
            if (Random.Range(0f, 1f) < (BacteriasManager.BacteriaPropertyAtLevel(BacteriaProperty.Killer, killerLevel) * Time.deltaTime))
            {
                unit.Die(this);
            }
        }
    }

    public override void Die(Unit killer)
    {
        m_decorum.PlayDeath();
        base.Die(killer);
        OnDie(killer);
    }

    protected override void OnDie(Unit killer)
    {
        base.OnDie(killer);

        float bonusFactor = 1;
        if(TechTree.TechMaxed(TechID.Evol_NoWaste) && (killer == PlayerCore.singleton))
        {
            bonusFactor = 2;
        }

        GameManager.singleton.AddDeadBacteriaResourcesValue(resourcesValue * bonusFactor);
    }

    public BioResources resourcesValue
    {
        get { return m_resourcesValue; }
        set { m_resourcesValue = value; }
    }

    virtual public float systemAdaptationMultiplier
    {
        get { return 1; }
    }

    public override float attackDamage
    {
        get
        {
            return base.attackDamage * systemAdaptationMultiplier;
        }
    }

    protected override void OnAttackUnit(Unit unit)
    {
        base.OnAttackUnit(unit);
        m_decorum.PlayAttack();
    }

    public override float timeToDie
    {
        get
        {
            return m_decorum.deathAnimationDuration;
        }
    }
}
