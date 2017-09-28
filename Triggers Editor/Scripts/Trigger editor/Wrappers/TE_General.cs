using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_General
    {
        [NodeMethod("General", "For", NodeMethodType.Action)]
        static public IEnumerator For(Node action, int times)
        {
            for(int i = 0; i < times; i++)
            {
                yield return action.Run();
            }
        }

        
    }
}