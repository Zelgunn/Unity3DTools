using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    public class TriggerLogger : MonoBehaviour
    {
        private LogsStream m_logsStream;

        private void Start()
        {
            TriggersManager.manager.triggerRan += Manager_triggerRan;
            TriggersManager.manager.eventOccurred += Manager_eventOccurred;
            TriggersManager.manager.conditionTested += Manager_conditionTested;
            TriggersManager.manager.actionBeginRun += Manager_actionBeginRun;
        }

        private void Manager_triggerRan(object sender, TriggerRunEventArgs e)
        {
            Log(e.trigger);
        }

        private void Manager_eventOccurred(object sender, EventOccurrenceArgs e)
        {
            LogEvent(e.occurringEvent);
        }

        private void Manager_conditionTested(object sender, ConditionTestEventArgs e)
        {
            LogCondition(e.testedCondition, e.testResult);
        }

        private void Manager_actionBeginRun(object sender, ActionRunEventArgs e)
        {
            LogAction(e.action);
        }

        private void Log(Trigger trigger)
        {
            Log(string.Format("+TRIGGER : {0}", trigger.name));
        }

        private void LogEvent(Routine occurringEvent)
        {
            Log(string.Format("|-EVENT : \"{0}\"", occurringEvent.name));
        }

        private void LogCondition(Routine testedCondition, bool testResult)
        {
            Log(string.Format("|-CONDITION : \"{0}\" was {1}", testedCondition.name, testResult));
        }

        private void LogAction(Routine action)
        {
            Log(string.Format("|-ACTION : \"{0}\"", action.name));
        }

        private void Log(string log)
        {
            if(m_logsStream == null)
            {
                m_logsStream = new LogsStream("Triggers (Logs)", "Triggers");
            }

            m_logsStream.Log(log);
        }

        private void OnApplicationQuit()
        {
            if (m_logsStream != null)
            {
                m_logsStream.Close();
            }
        }
    }
}
