using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class ButtonMash : DefaultAction
{
    public int RequiredAmount;
    private int _buttonMashCount;
    public TMP_Text Counter;
    private InputAction _action;
    private bool _mashing;
    private bool _isActionAllowed;

    public SpriteRenderer Pressed;

    public void Activate(int requiredAmount, string requiredAction, int timeLimit)
    {
        var timeLimitOverride =
            Toolbox.Instance.ArtifactsManager.ArtifactReferences.Select(art => art.PrayerTimeLimitOverride)
                .DefaultIfEmpty(0).Max();

        base.ActivateDefaults(Math.Max(timeLimit, timeLimitOverride));
        RequiredAmount = requiredAmount;
        _buttonMashCount = 0;
        _action = Toolbox.Instance.MainInput.actions[requiredAction];
        _isActionAllowed = Toolbox.Instance.ArtifactsManager.ArtifactReferences.Any(art => art.AllowsAction(requiredAction));

        Counter.text = requiredAmount.ToString().PadLeft(3, '0');
        Toolbox.Instance.Machinery().AddBasicMachine( HandleAction());
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
                Toolbox.Instance.Machinery().AddBasicMachine( MashEffect());
            }

            if (_buttonMashCount >= RequiredAmount)
            {
                Result = true;
                break;
            }

            if (TimeLimitExpired)
            {
                Result = false;
                break;
            }

            yield return TimeYields.WaitOneFrameX;
        }

        yield return base.HandleActionEnd().AsCoroutine();
    }
}
