using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TriggerEditor
{
    public class Trigger : ScriptableObject
    {
        [SerializeField] private Routine[] m_events;
        [SerializeField] private Routine[] m_conditions;
        [SerializeField] private Routine[] m_actions;

        [SerializeField] private bool m_enabled = true;

        public Routine[] GetRoutines(Routine.RoutineType type)
        {
            switch(type)
            {
                case Routine.RoutineType.Event: return m_events;
                case Routine.RoutineType.Condition: return m_conditions;
                case Routine.RoutineType.Action: return m_actions;
            }

            return null;
        }

        public int GetRoutinesCount(Routine.RoutineType type)
        {
            switch (type)
            {
                case Routine.RoutineType.Event: return m_events == null ? 0 : m_events.Length;
                case Routine.RoutineType.Condition: return m_conditions == null ? 0 : m_conditions.Length;
                case Routine.RoutineType.Action: return m_actions == null ? 0 : m_actions.Length;
            }

            return 0;
        }

        public void CreateRoutine(MethodInfo finalNodeMethodInfo, string routineName, Routine.RoutineType type)
        {
            Routine newRoutine = Routine.CreateRoutine(finalNodeMethodInfo, routineName, type);
            AddRoutine(newRoutine);
        }

        protected void AddRoutine(Routine routine)
        {
            Routine[] existingRoutines = GetRoutines(routine.type);
            List<Routine> tmpRoutineList;
            if (existingRoutines == null)
            {
                tmpRoutineList = new List<Routine>();
            }
            else
            {
                tmpRoutineList = new List<Routine>(existingRoutines);
            }
            tmpRoutineList.Add(routine);
            switch (routine.type)
            {
                case Routine.RoutineType.Event:
                    m_events = tmpRoutineList.ToArray();
                    break;
                case Routine.RoutineType.Condition:
                    m_conditions = tmpRoutineList.ToArray();
                    break;
                case Routine.RoutineType.Action:
                    m_actions = tmpRoutineList.ToArray();
                    break;
            }
        }

        public bool ContainsRoutine(string routineName)
        {
            return ContainsRoutine(m_events, routineName) ||
                ContainsRoutine(m_conditions, routineName) ||
                ContainsRoutine(m_actions, routineName);
        }

        static public bool ContainsRoutine(Routine[] routines, string routineName)
        {
            if (routines == null) return false;
            foreach(Routine routine in routines)
            {
                if (routine.name == routineName) return true;
            }
            return false;
        }

        public void RemoveRoutine(Routine routine)
        {
            Routine[] existingRoutines = GetRoutines(routine.type);
            if (existingRoutines == null) return;

            List<Routine> tmpRoutineList = new List<Routine>(existingRoutines);
            if (!tmpRoutineList.Contains(routine)) return;
            tmpRoutineList.Remove(routine);

            switch (routine.type)
            {
                case Routine.RoutineType.Event:
                    m_events = tmpRoutineList.ToArray();
                    break;
                case Routine.RoutineType.Condition:
                    m_conditions = tmpRoutineList.ToArray();
                    break;
                case Routine.RoutineType.Action:
                    m_actions = tmpRoutineList.ToArray();
                    break;
            }
        }

        public void DuplicateRoutine(Routine routine)
        {
            AddRoutine(routine.Clone());
        }

        public void SwapRoutines(int index1, int index2, Routine.RoutineType routineType)
        {
            switch(routineType)
            {
                case Routine.RoutineType.Event:
                    SwapRoutines(index1, index2, ref m_events);
                    break;
                case Routine.RoutineType.Condition:
                    SwapRoutines(index1, index2, ref m_conditions);
                    break;
                case Routine.RoutineType.Action:
                    SwapRoutines(index1, index2, ref m_actions);
                    break;
            }
        }

        protected void SwapRoutines(int index1, int index2, ref Routine[] routines)
        {
            if ((index1 < 0) || (index1 >= routines.Length) ||
                (index2 < 0) || (index2 >= routines.Length))
            {
                return;
            }

            Routine tmp = routines[index1];
            routines[index1] = routines[index2];
            routines[index2] = tmp;
        }

        [NodeMethod("Triggers", "Turn on|off", TriggerEditor.NodeMethodType.Action)]
        static public void TurnOnOff(Trigger trigger, bool on)
        {
            trigger.m_enabled = on;
        }

        #region Accessors
        public Routine[] events
        {
            get { return m_events; }
        }

        public Routine[] conditions
        {
            get { return m_conditions; }
        }

        public Routine[] actions
        {
            get { return m_actions; }
        }

        public bool enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }
        #endregion
    }
}