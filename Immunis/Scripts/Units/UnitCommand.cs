using UnityEngine;

namespace UnitCommands
{
    public enum UnitCommandType
    {
        AttackUnit,
        AttackArea,
        AttackPoint,
        Stop,
        Wait,
        WaitForCondition
    }

    public delegate bool WaitForConditionDelegate();

    public struct UnitCommand
    {
        public UnitCommandType type;
        public object parameters;
    }

    //public struct AttackUnitCommand
    //{
    //    public Unit target;
    //}

    //public struct AttackAreaCommand
    //{
    //    public Rect target;
    //}

    //public struct AttackPointCommand
    //{
    //    public Vector3 target;
    //}

    //public struct WaitCommand
    //{
    //    public float time;
    //}
}




