using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Boolean
    {
        [NodeMethod("Boolean", "If, then, else", NodeMethodType.Action, "Boolean")]
        static public IEnumerator IfThenDoElseDo(bool test, Node ifTrue, Node ifFalse)
        {
            if (test)
            {
                if (ifTrue != null) yield return ifTrue.Run();
            }
            else
            {
                if (ifFalse != null) yield return ifFalse.Run();
            }
        }

        [NodeMethod("Boolean", "If, then X, else Y", NodeMethodType.Other, "X/Y")]
        static public object IfThenDoElseDo(bool test, object ifTrue, object ifFalse)
        {
            return test ? ifTrue : ifFalse;
        }

        [NodeMethod("Boolean", "Not", NodeMethodType.Condition)]
        static public bool Not(bool boolean)
        {
            return !boolean;
        }

        [NodeMethod("Boolean", "Or", NodeMethodType.Condition, "A or B")]
        static public bool Or(bool a, bool b)
        {
            return a || b;
        }

        [NodeMethod("Boolean", "And", NodeMethodType.Condition, "A and B")]
        static public bool And(bool a, bool b)
        {
            return a && b;
        }

        [NodeMethod("Comparison", "Boolean comparison", NodeMethodType.Condition, "Equals")]
        static public bool BooleanComparison(bool first, bool second, out bool notEqual)
        {
            notEqual = first != second;
            return !notEqual;
        }

        [NodeMethod("Comparison", "String comparison", NodeMethodType.Condition, "Equals")]
        static public bool StringComparison(string first, string second, out bool notEqual)
        {
            notEqual = first != second;
            return !notEqual;
        }

        [NodeMethod("Comparison", "GameObject comparison", NodeMethodType.Condition, "Equals")]
        static public bool GameObjectComparison(GameObject first, GameObject second, out bool notEqual)
        {
            notEqual = first != second;
            return !notEqual;
        }

        public enum NumberComparison
        {
            LessThan,
            LessOrEqual,
            Equal,
            NotEqual,
            MoreOrEqual,
            MoreThan
        }

        [NodeMethod("Comparison", "Real comparison", NodeMethodType.Condition, "Result")]
        static public bool FloatComparison(float first, float second, NumberComparison operation)
        {
            switch (operation)
            {
                case NumberComparison.LessThan: return first < second;
                case NumberComparison.LessOrEqual: return first <= second;
                case NumberComparison.Equal: return first == second;
                case NumberComparison.NotEqual: return first != second;
                case NumberComparison.MoreOrEqual: return first >= second;
                case NumberComparison.MoreThan: return first > second;
            }

            return false;
        }

        [NodeMethod("Comparison", "Integer comparison", NodeMethodType.Condition, "Result")]
        static public bool IntegerComparison(int first, int second, NumberComparison operation)
        {
            switch (operation)
            {
                case NumberComparison.LessThan: return first < second;
                case NumberComparison.LessOrEqual: return first <= second;
                case NumberComparison.Equal: return first == second;
                case NumberComparison.NotEqual: return first != second;
                case NumberComparison.MoreOrEqual: return first >= second;
                case NumberComparison.MoreThan: return first > second;
            }

            return false;
        }
    }
}
