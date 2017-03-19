using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(SpeechListener))]
public class SpeechListenerEditor : Editor
{
    private string m_command = "";

    public override void OnInspectorGUI()
    {
        SpeechListener speechListenerTarget = target as SpeechListener;

        if (Application.isPlaying)
        {
            string info = "Impossible d'éditer les paramètres pendant l'utilisation.\r\n\r\nNom de SpeechListener : " + speechListenerTarget.listenerName + "\r\n\r\n-Commandes vocales :\r\n";

            info += "\r\n1) Commandes \"évènement\" :\r\n";
            foreach (string str in speechListenerTarget.eventsCommands) info += "\t- " + str + "\r\n";

            info += "\r\n2) Commandes personnalisées :\r\n";
            foreach (string str in speechListenerTarget.linkedCommands) info += "\t- " + str + "\r\n";

            GUILayout.Label(info, EditorStyles.helpBox);

            return;
        }

        base.OnInspectorGUI();

        #region Events

        if (speechListenerTarget.linkedToEvents != null)
        {
            UnityEvent[] eventsBuffer = speechListenerTarget.events;
            UnityEvent[] newEvents = new UnityEvent[speechListenerTarget.linkedToEvents.Length];

            if(eventsBuffer == null)
            {
                for (int i = 0; i < newEvents.Length; i++) newEvents[i] = new UnityEvent();
                speechListenerTarget.events = newEvents;
            }
            else
            {
                if(eventsBuffer.Length != newEvents.Length)
                {
                    int size = Mathf.Min(newEvents.Length, eventsBuffer.Length);
                    for (int i = 0; i < size; i++) newEvents[i] = eventsBuffer[i];
                    for (int i = size; i < newEvents.Length; i++) newEvents[i] = new UnityEvent();

                    speechListenerTarget.events = newEvents;
                }
            }
        }
        else
        {
            speechListenerTarget.events = null;
        }

        #endregion

        #region Add command

        GUILayout.Label("Ajout de commandes", EditorStyles.boldLabel);

        m_command = GUILayout.TextArea(m_command);
        if (m_command.Length == 0) return;

        string[] linkedCommands = speechListenerTarget.linkedToFunction;
        if (linkedCommands != null)
        {
            foreach (string linkedCommand in linkedCommands) if (linkedCommand.Equals(m_command)) return;
        }

        if (GUILayout.Button("Ajouter commande"))
        {
            MonoScript monoscript = MonoScript.FromMonoBehaviour(speechListenerTarget);
            string path = AssetDatabase.GetAssetPath(monoscript);

            string scriptText = File.ReadAllText(path);
            int i;
            for (i = scriptText.Length - 1; i >= 0; i--)
            {
                if (scriptText[i] == '}')
                {
                    break;
                }
            }

            if (i < 0)
            {
                Debug.LogError("Erreur, le fichier est incorrect !");
                return;
            }
            
            string[] newLinkedCommands;
            if (linkedCommands == null) newLinkedCommands = new string[1];
            else newLinkedCommands = new string[linkedCommands.Length + 1];
            int j;
            for (j = 0; j < linkedCommands.Length; j++) newLinkedCommands[j] = linkedCommands[j];
            newLinkedCommands[j] = m_command;
            speechListenerTarget.linkedToFunction = newLinkedCommands;

            string functionToAppend = FormatForFunction(m_command); //"_"
            functionToAppend = "SpeechRecog_" + functionToAppend + " ()";
            string completeFunctonToAppend = "\r\n\tpublic void " + functionToAppend + "\r\n\t{\r\n\t\tDebug.Log(\"" + m_command + " (default test display)\");\r\n\t}\r\n\t\r\n";

            scriptText = scriptText.Insert(i, completeFunctonToAppend);

            File.WriteAllText(path, scriptText);
        }
        #endregion
    }

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
                if(c == ' ')
                {
                    lastWasSpace = true;
                }
                else if(!char.IsPunctuation(c))
                {
                    char tmp = c;

                    if(lastWasSpace && (c >= 'a') && (c <= 'z'))
                    {
                        tmp = (char) (c + 'A' - 'a');
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
