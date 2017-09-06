using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Boolean
    {
        [NodeMethod("Boolean", "If, then, else",  "Boolean")]
        static public void IfThenDoElseDo(bool test, Node ifTrue, Node ifFalse)
        {
            if (test)
            {
                if (ifTrue != null) ifTrue.Run();
            }
            else
            {
                if (ifFalse != null) ifFalse.Run();
            }
        }

        [NodeMethod("Boolean", "If, then X, else Y", "X/Y")]
        static public object IfThenDoElseDo(bool test, object ifTrue, object ifFalse)
        {
            return test ? ifTrue : ifFalse;
        }
    }
}
