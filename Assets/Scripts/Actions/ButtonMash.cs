using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class ButtonMash : MonoBehaviour
{
    public int RequiredAmount;
    private int _buttonMashCount;
    public TMP_Text InstructionText;
    private InputAction _action;
    private bool _timeLimitExpired;
    public bool? Result { get; private set; }

    public void Activate(int requiredAmount, string requiredAction, double timeLimit)
    {
        gameObject.SetActive(true);
        RequiredAmount = requiredAmount;
        _buttonMashCount = 0;
        _action = Toolbox.Instance.MainInput.actions[requiredAction];
        _timeLimitExpired = false;
        Result = null;

        Toolbox.Instance.CardGameManager.GameUI.SetTimer((int) timeLimit);
        Toolbox.Instance.MainMachinery.AddBasicMachine(55, HandleTimeLimit(timeLimit));
        Toolbox.Instance.MainMachinery.AddBasicMachine(56, HandleAction());
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

    private Routine HandleTimeLimit(double timeLimit)
    {
        yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, timeLimit,
            breakCondition: () => Result != null || !isActiveAndEnabled);
        _timeLimitExpired = true;
    }

    private Routine HandleAction()
    {
        while (Result == null && isActiveAndEnabled)
        {
            if (_action.WasPerformedThisFrame())
            {
                _buttonMashCount++;
                Debug.Log("button mashed, count: " + _buttonMashCount);
            }

            if (_buttonMashCount >= RequiredAmount)
            {
                Result = true;
                Toolbox.Instance.CardGameManager.GameUI.ResetTimer();
                break;
            }

            if (_timeLimitExpired)
            {
                Result = false;
                break;
            }

            yield return TimeYields.WaitOneFrameX;
        }

        gameObject.SetActive(false);
    }
}
