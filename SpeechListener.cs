using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
//using System.IO.Ports;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;

public class SpeechListener : MonoBehaviour
{
	[Header("Vocal")]
    [SerializeField] private bool m_useListenerName;
    [SerializeField] private string m_listenerName;

    //Events
	[SerializeField] private string[] m_linkedToEvents;
    [SerializeField] private UnityEvent[] m_events;
    private List<string> m_eventsCommands;

    // Auto functions
	[SerializeField] private string[] m_linkedToFunction;
    private List<string> m_linkedCommands;

	private KeywordRecognizer m_keywordRecognizer;

	private void Awake ()
	{
		int keywordsCount = 0;
        keywordsCount += LinkedToEventCount();
        keywordsCount += LinkedToFunctionCount();

		string[] keywords = new string[keywordsCount];

        m_linkedCommands = new List<string>(LinkedToFunctionCount());
        m_eventsCommands = new List<string>(LinkedToEventCount());

		int index = 0;

        if (m_linkedToEvents != null)
        {
            for (int i = 0; i < m_linkedToEvents.Length; i++)
            {
                if(m_useListenerName)
                {
                    keywords[index] = m_listenerName + " " + m_linkedToEvents[i];
                }
                else
                {
                    keywords[index] = m_linkedToEvents[i];
                }
                m_eventsCommands.Add(keywords[index++]);
            }
        }

        if(m_linkedToFunction != null)
        {
            for (int i = 0; i < m_linkedToFunction.Length; i++)
            {
                if (m_useListenerName)
                {
                    keywords[index] = m_listenerName + " " + m_linkedToFunction[i];
                }
                else
                {
                    keywords[index] = m_linkedToFunction[i];
                }
                m_linkedCommands.Add(keywords[index++]);
            }
        }

		m_keywordRecognizer = new KeywordRecognizer (keywords, ConfidenceLevel.Low);
		m_keywordRecognizer.OnPhraseRecognized += _OnKeywordRecognized;
		m_keywordRecognizer.Start();
	}

	private void _OnKeywordRecognized(PhraseRecognizedEventArgs args)
	{
        if(m_eventsCommands.Contains(args.text))
        {
            m_events[m_eventsCommands.IndexOf(args.text)].Invoke();
        }
        else if (m_linkedCommands.Contains(args.text))
        {
            Invoke("SpeechRecog_" + FormatForFunction(args.text), 0);
        }
	}

    #region Counts
    private int LinkedToEventCount()
    {
        if (m_linkedToEvents == null) return 0;
        return m_linkedToEvents.Length;
    }

    private int LinkedToFunctionCount()
    {
        if (m_linkedToFunction == null) return 0;
        return m_linkedToFunction.Length;
    }
    #endregion

    #region Getters

    public string listenerName
    {
        get { return m_listenerName; }
    }

    public string[] linkedToEvents
    {
        get { return m_linkedToEvents; }
        set { m_linkedToEvents = value; }
    }

    public UnityEvent[] events
    {
        get { return m_events; }
        set { m_events = value; }
    }

    public string[] linkedToFunction
    {
        get { return m_linkedToFunction; }
        set { m_linkedToFunction = value; }
    }

    public List<string> eventsCommands
    {
        get { return m_eventsCommands; }
    }

    public List<string> linkedCommands
    {
        get { return m_linkedCommands; }
    }
    #endregion

    static private string FormatForFunction(string text)
    {
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        bool lastWasSpace = true;
        foreach (char c in normalizedString)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                if (c == ' ')
                {
                    lastWasSpace = true;
                }
                else if (!char.IsPunctuation(c))
                {
                    char tmp = c;

                    if (lastWasSpace && (c >= 'a') && (c <= 'z'))
                    {
                        tmp = (char)(c + 'A' - 'a');
                    }
                    else if (!lastWasSpace && (c >= 'A') && (c <= 'Z'))
                    {
                        tmp = (char)(c - 'A' + 'a');
                    }

                    stringBuilder.Append(tmp);

                    lastWasSpace = false;
                }
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}