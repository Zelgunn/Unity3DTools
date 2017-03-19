using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum UnitFaction
{
    None        = 0,
    Cell        = 1,
    Bacteria    = 2,
    All         = -1
}

[System.Serializable]
public struct UnitDecorum
{
    [Header("Sounds")]
    [SerializeField] private AudioClip m_idleSound;
    [SerializeField] private AudioClip m_attackSound;
    [SerializeField] private AudioClip m_deathSound;

    [Header("Animations")]
    [SerializeField] private string m_attackTriggerName;
    private int m_attackTriggerHash;
    [SerializeField] private string m_deathTriggerName;
    [SerializeField] private float m_deathAnimationDuration;
    private int m_deathTriggerHash;

    private Animator m_animator;
    private AudioSource m_audioSource;

    public void Init(GameObject gameObject)
    {
        m_animator = gameObject.GetComponent<Animator>();
        m_audioSource = gameObject.GetComponent<AudioSource>();

        m_attackTriggerHash = Animator.StringToHash(m_attackTriggerName);
        m_deathTriggerHash = Animator.StringToHash(m_deathTriggerName);
    }

    public void OverrideAnimator(Animator animator)
    {
        m_animator = animator;
    }

    public void PlayIdle()
    {
        m_audioSource.clip = m_idleSound;
        m_audioSource.Play();
    }

    public void PlayAttack()
    {
        m_audioSource.PlayOneShot(m_attackSound);
        m_animator.SetTrigger(m_attackTriggerHash);
    }

    public void PlayDeath()
    {
        m_audioSource.PlayOneShot(m_deathSound);
        m_animator.SetBool(m_deathTriggerHash, true);
    }

    public void Revive()
    {
        m_animator.SetBool(m_deathTriggerHash, false);
    }

    public float deathAnimationDuration
    {
        get { return m_deathAnimationDuration; }
    }
}

public class Unit : MonoBehaviour
{
    protected float m_healthPoints;
    [Header("Status")]
    [SerializeField] protected float m_maxHealthPoints = 100;
    [SerializeField] protected float m_resource = 0;
    [SerializeField] protected float m_armor = 0;
    [Header("Attack")]
    [SerializeField] protected float m_baseAttackDamage = 10;
    [SerializeField] protected float m_attackRate = 0.5f;
    protected float m_attackCooldown = 0;
    protected bool m_isBerserk = false;
    [SerializeField] protected float m_scanRange = 10;
    [Header("Projectiles")]
    [SerializeField] protected float m_attackRange = 5;
    [SerializeField] protected Projectile m_projectilePrefab;
    [SerializeField] protected float m_projectileVelocity = 20;
    [SerializeField] protected Transform m_projectileBaseTransform;
    [Header("Misc")]
    [SerializeField] protected float m_speed = 2;
    [SerializeField] protected Sprite m_icon;
    protected NavMeshAgent m_navMeshAgent;
    protected UnitFaction m_faction;
    protected Queue<UnitCommands.UnitCommand> m_commandsQueue = new Queue<UnitCommands.UnitCommand>();
    protected float m_waitTimer = 0;
    public UnitCommands.UnitCommandType currentCommand = UnitCommands.UnitCommandType.Stop;
    public bool targetInRange = false;

    virtual protected void Awake ()
    {
        m_healthPoints = m_maxHealthPoints;
        m_attackCooldown = m_attackRate;
        
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        if(m_navMeshAgent)
        {
            m_navMeshAgent.speed = m_speed;
        }
    }

    virtual protected void Update ()
    {
        m_attackCooldown += Time.deltaTime;
        ResolveCommandsQueue();
        if (m_commandsQueue.Count > 0) currentCommand = m_commandsQueue.Peek().type;
        else currentCommand = UnitCommands.UnitCommandType.WaitForCondition;
    }

    #region Protected functions

    virtual protected void ResolveCommandsQueue()
    {
        if(m_commandsQueue.Count == 0)
        {
            Unit closestFoe = ScanClosest(~m_faction, m_scanRange);
            AttackUnit(closestFoe);
            return;
        }

        UnitCommands.UnitCommand currentCommand = m_commandsQueue.Peek();

        bool currentCommandResolved = false;

        switch(currentCommand.type)
        {
            case UnitCommands.UnitCommandType.AttackArea:
                currentCommandResolved = AttackArea((Rect)currentCommand.parameters);
                break;
            case UnitCommands.UnitCommandType.AttackPoint:
                currentCommandResolved = AttackPoint((Vector3)currentCommand.parameters);
                break;
            case UnitCommands.UnitCommandType.AttackUnit:
                currentCommandResolved = AttackUnit((Unit)currentCommand.parameters);
                break;
            case UnitCommands.UnitCommandType.Stop:
                currentCommandResolved = true;
                break;
            case UnitCommands.UnitCommandType.Wait:
                currentCommandResolved = Wait((float)currentCommand.parameters);
                break;
            case UnitCommands.UnitCommandType.WaitForCondition:
                currentCommandResolved = ((UnitCommands.WaitForConditionDelegate)currentCommand.parameters)();
                break;
        }

        if (currentCommandResolved)
        {
            m_navMeshAgent.Stop();
            m_commandsQueue.Dequeue();
        }
    }

    #region Commands

    virtual protected bool AttackArea(Rect area)
    {
        AttackPoint(area.center);

        Vector2 xzPosition = new Vector2(transform.position.x, transform.position.z);
        return area.Contains(xzPosition);
    }

    virtual protected bool AttackPoint(Vector3 point)
    {
        Unit closestFoe = ScanClosest(~m_faction, m_scanRange);
        if (closestFoe)
        {
            AttackUnit(closestFoe);
        }
        else
        {
            m_navMeshAgent.SetDestination(point);
            m_navMeshAgent.Resume();
        }

        return (DistanceTo(point) < GameManager.attackPointDistanceEpsilon);
    }

    virtual protected bool AttackUnit(Unit unit)
    {
        if(unit == null)
        {
            return true;
        }

        if (DistanceTo(unit) < m_attackRange)
        {
            m_navMeshAgent.Stop();
            if (m_attackRate <= m_attackCooldown)
            {
                m_attackCooldown = 0;
                if(m_projectilePrefab && m_projectileBaseTransform)
                {
                    FireProjectile(m_projectileBaseTransform);
                }
                else
                {
                    DamageUnit(unit);
                }
                OnAttackUnit(unit);
            }
        }
        else
        {
            if(m_navMeshAgent.destination != unit.transform.position)
            {
                m_navMeshAgent.SetDestination(unit.transform.position);
                m_navMeshAgent.Resume();
            }
        }

        return ((unit == null) || (unit.dead));
    }

    virtual protected Projectile FireProjectile(Transform projectileBaseTransform)
    {
        Projectile projectile = Instantiate(m_projectilePrefab);

        projectile.source = this;
        projectile.factionMask = m_faction;

        projectile.transform.position = projectileBaseTransform.position;
        projectile.transform.right = -projectileBaseTransform.forward;
        projectile.transform.localScale = Vector3.Scale(projectileBaseTransform.lossyScale, projectile.transform.localScale);

        projectile.rigidbody.AddForce(projectileBaseTransform.forward * m_projectileVelocity, ForceMode.VelocityChange);

        return projectile;
    }

    virtual protected bool Wait(float time)
    {
        if(m_waitTimer > time)
        {
            m_waitTimer = 0;
            return true;
        }
        m_waitTimer += Time.deltaTime;
        return false;
    }

    virtual protected bool Stop()
    {
        m_navMeshAgent.Stop();
        return true;
    }

    #endregion

    #region Scans
    protected Unit ScanClosest(UnitFaction factionMask, float maxRange, bool includeDeadUnits = false)
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        float closestUnitDistance = -1;
        Unit closestUnit = null;

        for (int i = 0; i < allUnits.Length; i++)
        {
            Unit unit = allUnits[i];
            if(unit == this)
            {
                continue;
            }

            if(unit.dead && !includeDeadUnits)
            {
                continue;
            }

            bool maskContainsUnitFaction = (unit.faction & factionMask) == unit.faction;
            if(!maskContainsUnitFaction)
            {
                continue;
            }

            float distance = DistanceTo(unit);
            if (distance > maxRange)
            {
                continue;
            }

            if (closestUnitDistance < 0)
            {
                closestUnitDistance = distance;
                closestUnit = unit;
            }
            else if (distance < closestUnitDistance)
            {
                closestUnit = unit;
            }
        }

        return closestUnit;
    }

    protected Unit[] Scan(UnitFaction factionMask, float maxRange, bool includeDeadUnits = false)
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        List<Unit> foundUnits = new List<Unit>(allUnits.Length);

        for (int i = 0; i < allUnits.Length; i++)
        {
            Unit unit = allUnits[i];

            if (unit == this)
            {
                continue;
            }

            if (unit.dead && !includeDeadUnits)
            {
                continue;
            }

            bool maskContainsUnitFaction = (unit.faction & factionMask) == unit.faction;
            if (!maskContainsUnitFaction)
            {
                continue;
            }

            float distance = DistanceTo(unit);
            if (distance > maxRange)
            {
                continue;
            }

            foundUnits.Add(unit);
        }

        return foundUnits.ToArray();
    }

    protected float DistanceTo(Vector3 point)
    {
        Vector3 selfPosition = transform.position;
        selfPosition.y = 0;

        return (selfPosition - point).magnitude;
    }

    protected float DistanceTo(Unit other)
    {
        Vector3 otherPosition = other.transform.position;
        otherPosition.y = 0;

        return DistanceTo(otherPosition);
    }

    protected bool IsHostile(Unit other)
    {
        return other.faction != m_faction;
    }
    #endregion

    #endregion

    #region Public functions
    public void AddCommand(UnitCommands.UnitCommand command, bool clearQueue = false)
    {
        if(clearQueue)
        {
            m_commandsQueue.Clear();
        }
        m_commandsQueue.Enqueue(command);
    }

    public void AttackPosition(Vector3 position)
    {
        UnitCommands.UnitCommand command = new UnitCommands.UnitCommand
        {
            type = UnitCommands.UnitCommandType.AttackPoint,
            parameters = position
        };
        AddCommand(command);
    }

    public void AttackPositionImmediate(Vector3 position)
    {
        UnitCommands.UnitCommand command = new UnitCommands.UnitCommand
        {
            type = UnitCommands.UnitCommandType.AttackPoint,
            parameters = position
        };
        AddCommand(command, true);
    }

    virtual public void DamageUnit(Unit unit)
    {
        float damageDealt = unit.TakeDamage(this, attackDamage);
        OnDealDamage(unit, damageDealt);
    }

    public float TakeDamage(Unit source, float amount)
    {
        if(dead)
        {
            return 0;
        }

        amount -= m_armor;
        float damagesTaken = Mathf.Max(Mathf.Min(amount, m_healthPoints), 0);
        m_healthPoints -= damagesTaken;
        OnTakeDamage(source, damagesTaken);

        if(m_healthPoints <= 0)
        {
            m_healthPoints = 0;
            Die(source);
        }

        return damagesTaken;
    }

    public void RecoverHealth(float amount)
    {
        if (dead)
        {
            return;
        }

        float healthRecovered = Mathf.Max(Mathf.Min(amount, m_maxHealthPoints - m_healthPoints), 0);
        m_healthPoints += healthRecovered;
        OnRecoverHealth(healthRecovered);
    }

    virtual public void Die(Unit killer)
    {
        OnDie(killer);
        Destroy(gameObject, timeToDie);
    }
    #endregion

    #region Events
    virtual protected void OnAttackUnit(Unit unit)
    {

    }

    virtual protected void OnTakeDamage(Unit source, float damagesTaken)
    {

    }

    virtual protected void OnRecoverHealth(float recoveredHealth)
    {

    }

    virtual protected void OnDealDamage(Unit unit, float damagesDealt)
    {

    }

    virtual protected void OnDie(Unit killer)
    {

    }
    #endregion

    #region Get/Set
    public float maxHealthPoints
    {
        get { return m_maxHealthPoints; }
    }

    public float healthPoints
    {
        get { return m_healthPoints; }
    }

    public bool dead
    {
        get { return m_healthPoints <= 0; }
    }

    public float ressource
    {
        get { return m_resource; }
    }

    virtual public float attackDamage
    {
        get { return m_baseAttackDamage; }
    }

    public float attackRate
    {
        get { return m_attackRate; }
    }
    
    public UnitFaction faction
    {
        get { return m_faction; }
    }

    virtual public float timeToDie
    {
        get { return 0; }
    }

    public Sprite icon
    {
        get { return m_icon; }
    }
    #endregion
}
