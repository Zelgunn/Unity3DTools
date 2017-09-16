using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Animator
    {
        [NodeMethod("Animation", "Set animator bool", NodeMethodType.Action)]
        static public void SetAnimatorBool(Animator animator, string id, bool value)
        {
            animator.SetBool(id, value);
        }

        [NodeMethod("Animation", "Set animator float", NodeMethodType.Action)]
        static public void SetAnimatorFloat(Animator animator, string id, float value)
        {
            animator.SetFloat(id, value);
        }

        [NodeMethod("Animation", "Set animator trigger", NodeMethodType.Action)]
        static public void SetAnimatorTrigger(Animator animator, string id)
        {
            animator.SetTrigger(id);
        }

        [NodeMethod("Animation", "Set animator integer", NodeMethodType.Action)]
        static public void SetAnimatorInteger(Animator animator, string id, int value)
        {
            animator.SetInteger(id, value);
        }
    }
}