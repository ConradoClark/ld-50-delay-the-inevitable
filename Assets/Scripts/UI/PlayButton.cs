using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class PlayButton : MonoBehaviour
{
    public Collider2D Collider;
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
        var mousePosValue = Toolbox.Instance.MainInput.actions[Constants.InputActions.MousePosition];
        while (!_hasClicked)
        {
            var mousePos = Toolbox.Instance.MainCamera.ScreenToWorldPoint(mousePosValue.ReadValue<Vector2>());
            if (clickAction.WasPerformedThisFrame() && Collider.OverlapPoint(mousePos))
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
