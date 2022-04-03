using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public TMP_Text Counter;
    private InputAction _action;
    private bool _timeLimitExpired;
    private bool _mashing;
    private bool _isActionAllowed;
    public bool? Result { get; private set; }

    public SpriteRenderer Pressed;

    public void Activate(int requiredAmount, string requiredAction, double timeLimit)
    {
        gameObject.SetActive(true);
        RequiredAmount = requiredAmount;
        _buttonMashCount = 0;
        _action = Toolbox.Instance.MainInput.actions[requiredAction];
        _timeLimitExpired = false;
        _isActionAllowed = Toolbox.Instance.ArtifactsManager.ArtifactReferences.Any(art => art.AllowsAction(requiredAction));
        Result = null;

        Counter.text = requiredAmount.ToString().PadLeft(3, '0');
        Toolbox.Instance.CardGameManager.GameUI.SetTimer((int)timeLimit);
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


    private Routine MashEffect()
    {
        if (_mashing) yield break;
        _mashing = true;

        for (var i = 0; i < 3; i++)
        {
            if (!Toolbox.Instance.EffectsManager.ButtonMashEffectPool.TryGetEffect(out var effect)) continue;
            effect.transform.position = (Vector3)Random.insideUnitCircle * 0.3f + Pressed.transform.position;
            effect.transform.Rotate(0, 0, Random.Range(0, 360));
            effect.transform.localScale = new Vector3(2, 2, 1);
        }

        Pressed.enabled = true;
        yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 0.1);
        Pressed.enabled = false;
        _mashing = false;
    }

    private Routine HandleAction()
    {
        var effectiveness = Toolbox.Instance.ArtifactsManager
            .ArtifactReferences.Select(art => art.ClappingEffectivenessModifier)
            .DefaultIfEmpty(0)
            .Max();

        while (Result == null && isActiveAndEnabled)
        {
            if (_isActionAllowed && _action.WasPerformedThisFrame())
            {
                _buttonMashCount+= effectiveness+1;
                Counter.text = (RequiredAmount - _buttonMashCount).ToString().PadLeft(3, '0');
                Toolbox.Instance.MainMachinery.AddBasicMachine(44, MashEffect());
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
