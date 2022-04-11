using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class ActionButton : MonoBehaviour
{
    public Collider2D Collider;
    private bool _hasClicked;

    public Transform ButtonBlock;
    public HelpContext HelpContext;

    private string _originalHelpDescription;

    void OnEnable()
    {
        _hasClicked = false;
        Toolbox.Instance.Machinery().AddBasicMachine(HandleClick());
        _originalHelpDescription = HelpContext.Description;
    }

    public void UnblockButton()
    {
        if (ButtonBlock == null) return;
        ButtonBlock.gameObject.SetActive(false);
    }

    public void BlockButton(string reason)
    {
        if (ButtonBlock == null) return;
        ButtonBlock.gameObject.SetActive(true);
        HelpContext.Description = reason;
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

                yield return HandleAction().AsCoroutine();
            }

            yield return TimeYields.WaitOneFrameX;
        }

        while (isActiveAndEnabled)
        {
            yield return TimeYields.WaitOneFrameX;
        }
    }

    protected virtual Routine HandleAction()
    {
        yield break;
    }
}
