using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class DefaultAction : MonoBehaviour
{
    public TMP_Text InstructionText;
    public bool? Result { get; protected set; }
    protected bool TimeLimitExpired;

    public virtual void ActivateDefaults(int timeLimit)
    {
        Result = null;
        TimeLimitExpired = false;
        gameObject.SetActive(true);
        Toolbox.Instance.CardGameManager.GameUI.SetTimer(timeLimit);
        Toolbox.Instance.MainMachinery.AddBasicMachine(55, HandleTimeLimit(timeLimit));
        Toolbox.Instance.MainMachinery.AddBasicMachine(57, FlashText());
    }

    private Routine FlashText()
    {
        var originalText = InstructionText.text;
        while (isActiveAndEnabled)
        {
            yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 0.1);
            InstructionText.text = "";
            yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 0.1);
            InstructionText.text = originalText;
        }

        InstructionText.text = originalText;
    }
    protected Routine HandleActionEnd()
    {
        gameObject.SetActive(false);
        yield break;
    }
    protected Routine HandleTimeLimit(double timeLimit)
    {
        yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, timeLimit,
            breakCondition: () => Result != null || !isActiveAndEnabled);
        TimeLimitExpired = true;
    }

}
