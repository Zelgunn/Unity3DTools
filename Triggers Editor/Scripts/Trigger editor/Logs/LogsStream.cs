using UnityEngine;
using System.Collections;
using System.IO;

namespace TriggerEditor
{
    public class LogsStream
    {
        private FileStream m_logsFileStream;
        private StreamWriter m_logsStreamWriter;
        private string m_filename;

        public LogsStream(string logsFolderPath, string logFileName)
        {
            if (!Directory.Exists(logsFolderPath))
            {
                Directory.CreateDirectory(logsFolderPath);
            }

            m_filename = string.Format("{0}/{1} {2}.txt", logsFolderPath, logFileName, System.DateTime.Now.ToString("d_M@HH\\hmm\\mss\\s"));
            m_logsFileStream = new FileStream(m_filename, FileMode.CreateNew);
            m_logsStreamWriter = new StreamWriter(m_logsFileStream);
        }

        public void Log(string log)
        {
            m_logsStreamWriter.WriteLine(string.Format("[{0}] {1}", GetLogTime(), log));
        }

        public void WriteLine(string line)
        {
            m_logsStreamWriter.WriteLine(line);
        }

        public void Close()
        {
            if (m_logsStreamWriter == null) return;

            m_logsStreamWriter.Flush();
            m_logsStreamWriter.Close();
            m_logsFileStream.Close();
        }

        static public string GetLogTime()
        {
            int seconds = Mathf.RoundToInt(Time.timeSinceLevelLoad);
            int mins = seconds / 60;
            seconds %= 60;

            return string.Format("{0}:{1}", mins.ToString("0000"), seconds.ToString("00"));
        }

        static public string GetVector3Log(Vector3 vector)
        {
            return string.Format("({0};{1};{2})",
                vector.x.ToString("0.00"),
                vector.y.ToString("0.00"),
                vector.z.ToString("0.00"));
        }
    }
}
