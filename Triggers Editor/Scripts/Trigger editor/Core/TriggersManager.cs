using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TriggerEditor
{
    public class TriggersManager : MonoBehaviour
    {
        static private TriggersManager s_singleton;
        static private bool s_sceneInitialized = false;

        [SerializeField] private Trigger[] m_triggers;

        public event EventHandler<TriggerRunEventArgs> triggerRan;
        public event EventHandler<EventOccurrenceArgs> eventOccurred;
        public event EventHandler<ConditionTestEventArgs> conditionTested;
        public event EventHandler<ActionRunEventArgs> actionBeginRun;
        public event EventHandler<ActionRunEventArgs> actionEndRun;

        private void Update()
        {
            for(int i = 0; i < m_triggers.Length; i++)
            {
                Trigger trigger = m_triggers[i];
                if (!trigger.enabled) continue;

                Routine occuringEvent;

                if (EvalutateTriggerEvents(trigger, out occuringEvent))
                {
                    OnTriggerRan(trigger);
                    OnEventOccurred(occuringEvent);

                    bool allConditionsMet = EvaluateTriggerConditions(trigger);
                    
                    if (allConditionsMet)
                    {
                        ProcessTriggerActions(trigger);
                    }
                }
            }

            s_sceneInitialized = true;
        }

        private bool EvalutateTriggerEvents(Trigger trigger, out Routine occuringEvent)
        {
            Routine[] events = trigger.events;
            for (int i = 0; i < events.Length; i++)
            {
                if ((bool)events[i].Evaluate())
                {
                    occuringEvent = events[i];
                    return true;
                }
            }

            occuringEvent = null;
            return false;
        }

        private bool EvaluateTriggerConditions(Trigger trigger)
        {
            Routine[] conditions = trigger.conditions;
            bool result = true;

            for (int i = 0; i < conditions.Length; i++)
            {
                bool testResult = (bool)conditions[i].Evaluate();
                OnConditionTested(conditions[i], testResult);

                if (!testResult)
                {
                    result = false;
                }
            }

            return result;
        }

        private void ProcessTriggerActions(Trigger trigger)
        {
            StartCoroutine(ProcessTriggerActionsCoroutine(trigger));
        }

        private IEnumerator ProcessTriggerActionsCoroutine(Trigger trigger)
        {
            Routine[] actions = trigger.actions;
            for (int i = 0; i < actions.Length; i++)
            {
                OnActionBeginRun(actions[i]);
                yield return actions[i].Run();
                OnActionEndRun(actions[i]);
            }
        }

        protected void OnTriggerRan(Trigger trigger)
        {
            if (triggerRan == null) return;

            TriggerRunEventArgs eventArgs = new TriggerRunEventArgs
            {
                trigger = trigger
            };

            triggerRan(null, eventArgs);
        }

        protected void OnEventOccurred(Routine occurringEvent)
        {
            if (eventOccurred == null) return;

            EventOccurrenceArgs eventArgs = new EventOccurrenceArgs
            {
                occurringEvent = occurringEvent
            };

            eventOccurred(null, eventArgs);
        }

        protected void OnConditionTested(Routine testedCondition, bool testResult)
        {
            if (conditionTested == null) return;

            ConditionTestEventArgs eventArgs = new ConditionTestEventArgs
            {
                testedCondition = testedCondition,
                testResult = testResult
            };

            conditionTested(null, eventArgs);
        }

        protected void OnActionBeginRun(Routine action)
        {
            if (actionBeginRun == null) return;

            ActionRunEventArgs eventArgs = new ActionRunEventArgs
            {
                action = action
            };

            actionBeginRun(null, eventArgs);
        }

        protected void OnActionEndRun(Routine action)
        {
            if (actionEndRun == null) return;

            ActionRunEventArgs eventArgs = new ActionRunEventArgs
            {
                action = action
            };

            actionEndRun(null, eventArgs);
        }

        #region Node methods
        [NodeMethod("Time", "Scene starts",  "Boolean")]
        static public bool SceneStarts()
        {
            return !s_sceneInitialized;
        }

        [NodeMethod("Time", "Scene initialized",  "Boolean")]
        static public bool SceneInitialized()
        {
            return s_sceneInitialized;
        }

        [NodeMethod("Triggers", "Run trigger")]
        public void RunTrigger(Trigger trigger)
        {
            ProcessTriggerActions(trigger);
        }
        #endregion

        #region Protected functions
        protected bool _ContainsTrigger(string triggerName)
        {
            if (m_triggers == null) return false;

            for(int i = 0; i < m_triggers.Length; i++)
            {
                if(triggerName == m_triggers[i].name)
                {
                    return true;
                }
            }

            return false;
        }

        protected void _CreateTrigger(string triggerName)
        {
            Trigger trigger = Trigger.CreateInstance<Trigger>();
            trigger.name = triggerName;

            if (m_triggers == null)
            {
                m_triggers = new Trigger[1];
                m_triggers[0] = trigger;
            }
            else
            {
                List<Trigger> tmpTriggerList = new List<Trigger>(m_triggers);
                tmpTriggerList.Add(trigger);
                m_triggers = tmpTriggerList.ToArray();
            }
        }

        protected void _RemoveTrigger(Trigger trigger)
        {
            if (m_triggers == null) return;

            List<Trigger> tmpTriggerList = new List<Trigger>(m_triggers);
            
            for(int i = 0; i < tmpTriggerList.Count; i++)
            {
                if(tmpTriggerList[i] == trigger)
                {
                    DestroyImmediate(trigger);
                    tmpTriggerList.RemoveAt(i);
                    break;
                }
            }

            m_triggers = tmpTriggerList.ToArray();
        }

        protected Trigger _GetTrigger(string triggerName)
        {
            if (m_triggers == null) return null;

            for (int i = 0; i < m_triggers.Length; i++)
            {
                if (triggerName == m_triggers[i].name)
                {
                    return m_triggers[i];
                }
            }

            return null;
        }

        protected string[] _GetTriggersNames()
        {
            if (m_triggers == null) return new string[0];
            string[] triggersNames = new string[m_triggers.Length];
            for(int i = 0; i < m_triggers.Length; i++)
            {
                triggersNames[i] = m_triggers[i].name;
            }
            return triggersNames;
        }
        #endregion

        #region Static functions
        static public bool TriggerNameIsValid(string triggerName)
        {
            if (triggerName == null) return false;
            if (triggerName.Length == 0) return false;
            return !manager._ContainsTrigger(triggerName);
        }

        static public bool ContainsTrigger(string triggerName)
        {
            return manager._ContainsTrigger(triggerName);
        }

        static public void CreateTrigger(string triggerName)
        {
            manager._CreateTrigger(triggerName);
        }

        static public void RemoveTrigger(Trigger trigger)
        {
            manager._RemoveTrigger(trigger);
        }

        static public Trigger GetTrigger(string triggerName)
        {
            return manager._GetTrigger(triggerName);
        }

        static public string[] GetTriggersNames()
        {
            return manager._GetTriggersNames();
        }
        #endregion

        #region Static Accessors
        static public TriggersManager manager
        {
            get
            {
                if(s_singleton == null)
                {
                    s_singleton = FindObjectOfType<TriggersManager>();
                    if(s_singleton == null)
                    {
                        s_singleton = new GameObject("Triggers manager").AddComponent<TriggersManager>();
                    }
                }

                return s_singleton;
            }
        }

        static public Trigger[] triggers
        {
            get { return manager.m_triggers; }
        }
        #endregion
    }

    public class TriggerRunEventArgs : EventArgs
    {
        public Trigger trigger;
    }

    public class EventOccurrenceArgs : EventArgs
    {
        public Routine occurringEvent;
    }

    public class ConditionTestEventArgs : EventArgs
    {
        public Routine testedCondition;
        public bool testResult;
    }

    public class ActionRunEventArgs : EventArgs
    {
        public Routine action;
    }
}