using UnityEngine;
using System.Collections;

public class Valery : Bacteria
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override float systemAdaptationMultiplier
    {
        get
        {
            return base.systemAdaptationMultiplier * (1 - BacteriasManager.antiBacteriaFactor);
        }
    }
}
