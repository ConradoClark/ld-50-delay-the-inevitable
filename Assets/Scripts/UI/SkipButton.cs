using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class SkipButton : ActionButton
{
    protected override Routine HandleAction()
    {
        yield return Toolbox.Instance.CardGameManager.DrawnCard.Skip().AsCoroutine();
    }
}
