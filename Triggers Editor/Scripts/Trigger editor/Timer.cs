using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    public class Timer : MonoBehaviour
    {
        protected bool m_stopped = true;
        protected float m_time = 30f;
        protected float m_elapsedTime = 0f;
        protected bool m_ends = false;
        protected bool m_repeat = false;

        private void Awake()
        {

        }

        private void Update()
        {
            if (m_stopped) return;

            m_elapsedTime += Time.deltaTime;
            if(m_time <= m_elapsedTime)
            {
                if(m_ends)
                {
                    StopTimer();
                    if(m_repeat)
                    {
                        StartTimer(m_time, true);
                    }
                }
                else
                {
                    m_ends = true;
                }
            }
        }

        [NodeMethod("Timer", "Start timer", NodeMethodType.Action)]
        public void StartTimer(float time, bool repeat)
        {
            StopTimer();
            m_stopped = false;
            m_time = time;
            m_repeat = repeat;
        }

        [NodeMethod("Timer", "Stop", NodeMethodType.Action)]
        public void StopTimer()
        {
            m_stopped = true;
            m_ends = false;
            m_elapsedTime = 0;
        }

        [NodeMethod("Timer", "Timer ends", NodeMethodType.Event)]
        public bool TimerEnds()
        {
            return m_ends;
        }

        [NodeMethod("Time", "Wait", NodeMethodType.Action)]
        static public IEnumerator WaitCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
        }

        [NodeMethod("Time", "Time elapsed", NodeMethodType.Other, "Seconds")]
        static public float TimeElapsed()
        {
            return Time.timeSinceLevelLoad;
        }

        [NodeMethod("Time", "After N seconds", NodeMethodType.Condition)]
        static public bool AfterNSecondsInScene(float seconds)
        {
            return Time.timeSinceLevelLoad >= seconds;
        }
    }
}

