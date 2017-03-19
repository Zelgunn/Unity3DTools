using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshObstacle))]
public class PlayerCore : Unit
{
    static private PlayerCore s_singleton;

    [Header("Attaque")]
    [SerializeField] private Transform m_rightHand;
    [SerializeField] private Transform m_leftHand;
    [Header("Tech")]
    [SerializeField] private float m_regenerationPercentageAfterWave = 0.1f;
    [SerializeField] private float m_thornsDamagePerRank = 1f;
    [SerializeField] private float m_techDamageBonusFactor = 0.25f;
    [SerializeField] private float m_techLeechFactor = 0.25f;
    [Header("Decor colors")]
    [Header("Wall")]
    [SerializeField] private MeshRenderer[] m_wallRenderers;
    [SerializeField] private Color m_wallStartColor;
    [SerializeField] private Color m_wallEndColor;
    [Header("Skybox")]
    [SerializeField] private Color m_skyboxStartColor;
    [SerializeField] private Color m_skyboxEndColor;
    [Header("Ambient Lightning")]
    [SerializeField] private Color m_ambientSkyStartColor;
    [SerializeField] private Color m_ambientSkyEndColor;
    [SerializeField] private Color m_ambientEquatorStartColor;
    [SerializeField] private Color m_ambientEquatorEndColor;
    [SerializeField] private Color m_ambientGroundStartColor;
    [SerializeField] private Color m_ambientGroundEndColor;

    protected override void Awake()
    {
        base.Awake();
        s_singleton = this;
        m_faction = UnitFaction.Cell;
    }

    protected override void Update()
    {
        if(!GameManager.singleton.invasionPhase)
        {
            return;
        }

        base.Update();

        if(InputsManager.singleton.rightTriggerDown)
        {
            FireProjectile(m_rightHand);
        }

        if(TechTree.TechMaxed(TechID.Evol_DoubleHanded) && InputsManager.singleton.leftTriggerDown)
        {
            FireProjectile(m_leftHand);
        }
    }

    private void UpdateDecorColor()
    {
        float ratio = m_healthPoints / m_maxHealthPoints;

        for(int i = 0; i < m_wallRenderers.Length; i++)
        {
            m_wallRenderers[i].material.color = Color.Lerp(m_wallEndColor, m_wallStartColor, ratio);
        }

        RenderSettings.skybox.color = Color.Lerp(m_skyboxEndColor, m_skyboxStartColor, ratio);
        RenderSettings.ambientSkyColor = Color.Lerp(m_ambientSkyEndColor, m_ambientSkyStartColor, ratio);
        RenderSettings.ambientEquatorColor = Color.Lerp(m_ambientEquatorEndColor, m_ambientEquatorStartColor, ratio);
        RenderSettings.ambientGroundColor = Color.Lerp(m_ambientGroundEndColor, m_ambientGroundStartColor, ratio);
    }

    protected override Projectile FireProjectile(Transform projectileBaseTransform)
    {
        Projectile projectile = base.FireProjectile(projectileBaseTransform);

        if (TechTree.TechMaxed(TechID.Evol_Patriot))
        {
            projectile.factionMask = UnitFaction.Cell;
        }
        else
        {
            projectile.factionMask = UnitFaction.None;
        }

        return projectile;
    }

    public void RegenerateAfterWave()
    {
        RecoverHealth(TechTree.TechRank(TechID.Evol_Regeneration) * m_regenerationPercentageAfterWave * m_maxHealthPoints);
    }

    public void EvolveToFortress()
    {
        m_maxHealthPoints *= 2;
        m_healthPoints *= 2;
    }

    protected override void OnTakeDamage(Unit source, float damagesTaken)
    {
        base.OnTakeDamage(source, damagesTaken);

        if(source == null)
        {
            return;
        }

        int thornsRank = TechTree.TechRank(TechID.Evol_Thorns);
        if(thornsRank > 0)
        {
            source.TakeDamage(this, m_thornsDamagePerRank * thornsRank);
        }

        UpdateDecorColor();
    }

    public float bastionDamageMultiplier
    {
        get { return 1 + TechTree.TechRank(TechID.Evol_Bastion) * m_techDamageBonusFactor; }
    }

    public override float attackDamage
    {
        get { return base.attackDamage * bastionDamageMultiplier; }
    }

    protected override void OnDealDamage(Unit unit, float damagesDealt)
    {
        base.OnDealDamage(unit, damagesDealt);

        if(TechTree.TechRank(TechID.Evol_Defense) > 0)
        {
            RecoverHealth(damagesDealt * TechTree.TechRank(TechID.Evol_Defense) * m_techLeechFactor);
        }
    }

    static public PlayerCore singleton
    {
        get { return s_singleton; }
    }
}
