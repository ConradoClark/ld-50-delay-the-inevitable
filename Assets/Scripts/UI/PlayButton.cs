using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class PlayButton : MonoBehaviour
{
    private bool _hasClicked;
    void OnEnable()
    {
        _hasClicked = false;
        Toolbox.Instance.MainMachinery.AddBasicMachine(33, HandleClick());
    }

    private Routine HandleClick()
    {
        var manager = Toolbox.Instance.CardGameManager;
        var clickAction = Toolbox.Instance.MainInput.actions[Constants.InputActions.Click];
        while (!_hasClicked)
        {
            if (clickAction.WasPerformedThisFrame())
            {
                _hasClicked = true;
                manager.TriggerAction();
                // start the current card effect
                yield return manager.DrawnCard.Play().AsCoroutine();
            }

            yield return TimeYields.WaitOneFrameX;
        }

        while (isActiveAndEnabled)
        {
            yield return TimeYields.WaitOneFrameX;
        }
    }
}
