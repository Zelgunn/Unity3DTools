using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Cell : Unit
{
    [SerializeField] private CellRenderer m_cellRendererPrefab;
    private CellRenderer m_cellRenderer;
    [SerializeField] private BioResources m_baseCost;
    [SerializeField] private Cell[] m_upgrades;

    // VIRTUAL CLASS TO INHERIT
    override protected void Awake ()
    {
        base.Awake();

        m_faction = UnitFaction.Cell;

        CheckTechBuffs();
    }

    private void CheckTechBuffs()
    {
        m_healthPoints = m_maxHealthPoints = m_maxHealthPoints * (1 + CellsManager.staminaBuff);
        m_attackCooldown *= 1 - CellsManager.speedBuff;
        m_armor += CellsManager.armorBuff;
    }

    override protected void Update ()
    {
        base.Update();

        RecoverHealth(m_maxHealthPoints * CellsManager.regenerationBuff);
    }

    public CellRenderer cellRendererPrefab
    {
        get { return m_cellRendererPrefab; }
    }

    public void AssignCellRenderer(CellRenderer cellRenderer)
    {
        m_cellRenderer = cellRenderer;

        m_cellRenderer.transform.SetParent(transform);
        m_cellRenderer.SetCellRenderingMode(CellRenderingMode.Default);
    }

    protected override void OnAttackUnit(Unit unit)
    {
        base.OnAttackUnit(unit);
        m_cellRenderer.decorum.PlayAttack();
    }

    protected override void OnDealDamage(Unit unit, float damagesDealt)
    {
        base.OnDealDamage(unit, damagesDealt);

        RecoverHealth(damagesDealt * CellsManager.leechBuff);
    }

    protected override void OnTakeDamage(Unit source, float damagesTaken)
    {
        base.OnTakeDamage(source, damagesTaken);

        if(!m_isBerserk && (Random.Range(0.0f, 1.0f) <= 0.05f))
        {
            m_isBerserk = true;
            m_attackRate /= 1 + CellsManager.berserkBuff;
        }

        if(m_healthPoints <= 0)
        {
            if(Random.Range(0f, 1f) < CellsManager.immortalBuff)
            {
                m_healthPoints = m_maxHealthPoints;
            }
        }
    }

    public override void Die(Unit killer)
    {
        base.Die(killer);
    }

    protected override void OnDie(Unit killer)
    {
        base.OnDie(killer);

        StartCoroutine(DeathCoroutine());
    }

    protected IEnumerator DeathCoroutine()
    {
        m_cellRenderer.decorum.PlayDeath();

        m_cellRenderer.transform.SetParent(CellPlaceholderManager.singleton.transform);

        yield return new WaitForSeconds(m_cellRenderer.decorum.deathAnimationDuration);

        m_cellRenderer.gameObject.SetActive(false);
    }

    public bool Affordable()
    {
        return GameManager.singleton.CanAffordResources(cost);
    }

    public BioResources cost
    {
        get { return m_baseCost * (1 - CellsManager.biomassEconomyFactor);  }
    }

    public float strengthDamageMultiplier
    {
        get { return 1 + CellsManager.strengthBuff; }
    }

    public float cooperationDamageMultiplier
    {
        get { return 1 + CellsManager.cooperationBuff * Scan(UnitFaction.Cell, m_attackRange, false).Length; }
    }

    public override float attackDamage
    {
        get
        {
            return base.attackDamage * cooperationDamageMultiplier * strengthDamageMultiplier;
        }
    }

    public Cell[] upgrades
    {
        get { return m_upgrades; }
    }

    public bool hasUpgrade
    {
        get { return (m_upgrades != null) && (m_upgrades.Length > 0); }
    }

    public override float timeToDie
    {
        get
        {
            return m_cellRenderer.decorum.deathAnimationDuration;
        }
    }
}
