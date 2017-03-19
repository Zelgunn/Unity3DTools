using UnityEngine;
using System.Collections;

public enum GamePhase
{
    OutsideBody,
    Wound,
    Preparation,
    Invasion
}

[System.Serializable]
public struct BioResources
{
    public float biomass;

    public static BioResources operator +(BioResources a, BioResources b)
    {
        BioResources result = new BioResources
        {
            biomass = a.biomass + b.biomass
        };

        return result;
    }

    public static BioResources operator -(BioResources a, BioResources b)
    {
        BioResources result = new BioResources
        {
            biomass = a.biomass - b.biomass
        };

        return result;
    }

    public static BioResources operator *(BioResources a, float b)
    {
        BioResources result = new BioResources
        {
            biomass = a.biomass * b
        };

        return result;
    }

    public static BioResources operator /(BioResources a, float b)
    {
        BioResources result = new BioResources
        {
            biomass = a.biomass / b
        };

        return result;
    }

    public static bool operator >(BioResources a, BioResources b)
    {
        return (a.biomass > b.biomass);
    }

    public static bool operator <(BioResources a, BioResources b)
    {
        return (a.biomass < b.biomass);
    }

    public static bool operator >=(BioResources a, BioResources b)
    {
        return (a.biomass >= b.biomass);
    }

    public static bool operator <=(BioResources a, BioResources b)
    {
        return (a.biomass <= b.biomass);
    }

    public static bool operator ==(BioResources a, BioResources b)
    {
        return (a.biomass == b.biomass);
    }

    public static bool operator !=(BioResources a, BioResources b)
    {
        return (a.biomass != b.biomass);
    }

    public override bool Equals(object o)
    {
        try
        {
            return (this == (BioResources)o);
        }
        catch
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return (int)biomass;
    }

    public override string ToString()
    {
        return biomass.ToString();
    }
}

public class GameManager : MonoBehaviour
{
    static private GameManager s_singleton;
    private GamePhase m_gamePhase = GamePhase.Preparation;
    [Header("Room scaling")]
    [SerializeField] private SteamVR_PlayArea m_roomScale;
    [SerializeField] private SteamVR_Camera m_camera;
    [SerializeField] private float m_preparationPhasePlayerScaleRatio = 1.0f;
    [SerializeField] private float m_invasionPhasePlayerScaleRatio = 0.25f;
    [SerializeField] private float m_scaleTransitionTime = 1.0f;
    [Header("Player parameters")]
    [SerializeField] private BioResources m_playerResources;
    [SerializeField] private float m_cellularDivisionFactor = 0.1f;
    [SerializeField] private float m_biomassRecoveryBonusFactor = 0.1f;
    [SerializeField] private float m_techReductionFactor = 0.1f;
    [Header("AI parameters")]
    [SerializeField] private float m_attackPointDistanceEpsilon = 0.1f;
    [Header("Tech")]
    [SerializeField] private TechTree m_techTree;
    private bool m_forceStartInvasion = false;

    private void Awake ()
    {
        s_singleton = this;
    }

    private void Start()
    {
        m_gamePhase = GamePhase.OutsideBody;
        SetGameArea(m_gamePhase);
        StartCoroutine(OutsideBodyEnterCoroutine());
        //m_gamePhase = GamePhase.Preparation;
        //StartCoroutine(PreparationEnterCoroutine());
    }

    private void Update()
    {
        //m_roomScale.transform.Rotate(Vector3.up, 45 * Time.deltaTime * Input.GetAxis("Horizontal"), Space.World);

        
    }

    #region Game phasing
    private void SetGamePhase(GamePhase phase)
    {
        StartCoroutine(EnterGamePhase(phase));
    }

    private IEnumerator EnterGamePhase(GamePhase phase)
    {
        yield return ExitGamePhase(m_gamePhase);
        m_gamePhase = phase;

        SoundManager.PlayMusic(phase);

        switch(phase)
        {
            case GamePhase.OutsideBody:
                yield return OutsideBodyEnterCoroutine();
                break;
            case GamePhase.Wound:
                yield return  WoundEnterCoroutine();
                break;
            case GamePhase.Preparation:
                yield return  PreparationEnterCoroutine();
                break;
            case GamePhase.Invasion:
                yield return InvasionEnterCoroutine();
                break;
        }
    }

    private IEnumerator ExitGamePhase(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.OutsideBody:
                yield return OutsideBodyExitCoroutine();
                break;
            case GamePhase.Wound:
                yield return WoundExitCoroutine();
                break;
            case GamePhase.Preparation:
                yield return PreparationExitCoroutine();
                break;
            case GamePhase.Invasion:
                yield return InvasionExitCoroutine();
                break;
        }
    }

    #region Phases : Entering
    private IEnumerator OutsideBodyEnterCoroutine()
    {
        while (!PlayerCursor.singleton) yield return null;
        PlayerCursor.singleton.gameObject.SetActive(false);
        StartCoroutine(OutsideBodyCoroutine());
    }

    private IEnumerator WoundEnterCoroutine()
    {
        PlayerCursor.singleton.gameObject.SetActive(true);
        StartCoroutine(WoundCoroutine());
        yield return null;
    }

    private IEnumerator PreparationEnterCoroutine()
    {
        yield return SetPlayerScaleRatioCoroutine(m_preparationPhasePlayerScaleRatio);
        PlayerCursor.singleton.gameObject.SetActive(true);
        UpdaterUI.Show(true);

        StartCoroutine(PreparationCoroutine());
    }

    private IEnumerator InvasionEnterCoroutine()
    {
        PlayerCursor.singleton.gameObject.SetActive(false);
        CellsManager.singleton.SpawnCells();
        m_techTree.Show(false, m_scaleTransitionTime / m_techTree.depth);
        yield return SetPlayerScaleRatioCoroutine(m_invasionPhasePlayerScaleRatio);

        yield return BacteriasManager.singleton.StartInvasionCoroutine();
        StartCoroutine(InvasionCoroutine());
    }
    #endregion

    #region Phases : Progress
    private IEnumerator OutsideBodyCoroutine()
    {
        SetGamePhase(GamePhase.Wound);
        yield return null;
    }

    private IEnumerator WoundCoroutine()
    {
        while(!WoundManager.singleton.woundClean)
        {
            yield return null;
        }

        SetGamePhase(GamePhase.Preparation);
    }

    private IEnumerator PreparationCoroutine()
    {
        bool startInvasion = false;

        while (!startInvasion)
        {
            startInvasion = (InputsManager.singleton.leftTriggerDown && ControllerUI.singleton.readyMenu) || m_forceStartInvasion;
            yield return null;
        }

        m_forceStartInvasion = false;

        SetGamePhase(GamePhase.Invasion);
    }

    private IEnumerator InvasionCoroutine()
    {
        bool victory = false;
        bool defeat = false;
        while (!victory && !defeat)
        {
            victory = BacteriasManager.singleton.livingBacteriasCount == 0;
            defeat = PlayerCore.singleton.dead;
            yield return null;
        }

        if (victory && defeat)
        {
            Debug.Log("Draw !");
        }
        else if (victory)
        {
            Debug.Log("Victory !");
        }
        else
        {
            Debug.Log("Defeat !");
        }

        if(victory)
        {
            PlayerCore.singleton.RegenerateAfterWave();
            m_playerResources *= 1 + m_cellularDivisionFactor * TechTree.TechRank(TechID.Adapt_CellDivision);
        }

        SetGamePhase(GamePhase.Preparation);
    }
    #endregion

    #region Phases : Exiting
    private IEnumerator OutsideBodyExitCoroutine()
    {
        yield return ChangeGameAreaCoroutine(GamePhase.OutsideBody, GamePhase.Wound);
    }

    private IEnumerator WoundExitCoroutine()
    {
        yield return ChangeGameAreaCoroutine(GamePhase.Wound, GamePhase.Preparation);
    }

    private IEnumerator PreparationExitCoroutine()
    {
        UpdaterUI.Show(false);
        yield return null;
    }

    private IEnumerator InvasionExitCoroutine()
    {
        BacteriasManager.singleton.StopInvasion();
        CellPlaceholderManager.singleton.ReclaimCellRenderers();
        CellsManager.singleton.RemoveCells();

        yield return null;
    }
    #endregion

    private IEnumerator ChangeGameAreaCoroutine(GamePhase from, GamePhase to)
    {
        yield return PreviewManager.singleton.PreviewSceneCoroutine(from, to);

        SetGameArea(to);
    }

    private void SetGameArea(GamePhase phase)
    {
        Transform roomScaleTargetTransform = GameArea.GetGameArea(phase).sceneRoomScaleTransform;
        m_roomScale.transform.position = roomScaleTargetTransform.position;
        m_roomScale.transform.rotation = roomScaleTargetTransform.rotation;
        m_roomScale.transform.localScale = roomScaleTargetTransform.localScale;
        m_camera.camera.nearClipPlane = roomScaleTargetTransform.localScale.x / 5;
        GameArea.GetGameArea(phase).ApplyAmbientLightning();
    }

    private IEnumerator SetPlayerScaleRatioCoroutine(float ratio)
    {
        float currentRatio = m_roomScale.transform.localScale.x;
        float time = 0;
        while (time < m_scaleTransitionTime)
        {
            m_roomScale.transform.localScale = Vector3.one * Mathf.Lerp(currentRatio, ratio, time / m_scaleTransitionTime);
            time += Time.deltaTime;
            yield return null;
        }
        m_roomScale.transform.localScale = Vector3.one * ratio;
    }

    #endregion

    #region Resources (Biomasss, ...)

    public bool PayResources(BioResources cost)
    {
        if(!CanAffordResources(cost))
        {
            return false;
        }

        m_playerResources -= cost;

        return true;
    }

    public bool CanAffordResources(BioResources cost)
    {
        return cost <= m_playerResources;
    }

    public void AddDeadBacteriaResourcesValue(BioResources resourcesValue)
    {
        m_playerResources += resourcesValue * (1 + m_biomassRecoveryBonusFactor * TechTree.TechRank(TechID.Adapt_BiomassRecovery));
        SoundManager.PlayDigestionSoundEffect();
    }

    public void AddResources(BioResources resources)
    {
        m_playerResources += resources;
    }

    public float techReductionFactor
    {
        get { return m_techReductionFactor * TechTree.TechRank(TechID.Adapt_FastEvolution); }
    }

    static public BioResources playerResources
    {
        get { return s_singleton.m_playerResources; }
    }
    #endregion

    #region Get/Set
    static public GameManager singleton
    {
        get { return s_singleton; }
    }

    //public GamePhase gamePhase
    //{
    //    get { return m_gamePhase; }
    //}

    public bool outsideBodyPhase
    {
        get
        {
            return m_gamePhase == GamePhase.OutsideBody;
        }
    }

    public bool woundPhase
    {
        get
        {
            return m_gamePhase == GamePhase.Wound;
        }
    }

    public bool insideBodyPhase
    {
        get
        {
            return preparationPhase || invasionPhase;
        }
    }

    public bool preparationPhase
    {
        get
        {
            return m_gamePhase == GamePhase.Preparation;
        }
    }

    public bool invasionPhase
    {
        get
        {
            return m_gamePhase == GamePhase.Invasion;
        }
    }

    static public float attackPointDistanceEpsilon
    {
        get { return s_singleton.m_attackPointDistanceEpsilon; }
    }

    new static public Camera camera
    {
        get { return s_singleton.m_camera.camera; }
    }
    #endregion

    public void StartInvasionNow()
    {
        m_forceStartInvasion = true;
    }

    public void Mrlrlglrglglg()
    {
        print("Mrlgglrglrlglglg");
    }

    public void ShowTechTree()
    {
        ControllerUI.singleton.menu = ControllerUI.ControllerUIMenus.TechTreeMenu;
    }

    public void ShowMenu(int menu)
    {
        ControllerUI.singleton.menu = (ControllerUI.ControllerUIMenus) menu;
    }
}
