using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neutrophil : Cell
{
    public override void DamageUnit(Unit unit)
    {
        if(unit.dead)
        {
            return;
        }
        base.DamageUnit(unit);
        if(unit.dead)
        {
            Die(this);
        }
    }
}
