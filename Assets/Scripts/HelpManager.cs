using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class HelpManager : MonoBehaviour
{
    private List<HelpContext> _helpContexts;

    public event OnHelpUpdatedEvent OnHelpUpdated;
    void OnEnable()
    {
        _helpContexts = new List<HelpContext>();
        Toolbox.Instance.MainMachinery.AddBasicMachine(23, HandleHelp());
    }

    private Routine HandleHelp()
    {
        Collider2D currentCollider = null;
        while (isActiveAndEnabled)
        {
            Vector3 mousePos;
            bool overlaps;
            do
            {
                var mousePosValue = Toolbox.Instance.MainInput.actions[Constants.InputActions.MousePosition];
                mousePos = Toolbox.Instance.MainCamera.ScreenToWorldPoint(mousePosValue.ReadValue<Vector2>());
                overlaps = currentCollider != null && currentCollider.OverlapPoint(mousePos);
                if (overlaps) yield return TimeYields.WaitOneFrameX;
            } while (currentCollider != null && overlaps);

            foreach (var help in _helpContexts)
            {
                if (help.Collider.OverlapPoint(mousePos))
                {
                    OnHelpUpdated?.Invoke(help.Name, help.Description);
                    currentCollider = help.Collider;
                    break;
                }

                OnHelpUpdated?.Invoke("", "");
                currentCollider = null;
            }

            yield return TimeYields.WaitOneFrameX;
        }
    }

    public void AddToHelp(HelpContext context)
    {
        if (_helpContexts.Contains(context)) return;
        _helpContexts.Add(context);
    }
    
}

public delegate void OnHelpUpdatedEvent(string name, string description);
