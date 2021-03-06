using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class SkipButton : ActionButton
{
    protected override Routine HandleAction()
    {
        Toolbox.Instance.CardGameManager.RegisterSkippedCard();
        yield return Toolbox.Instance.CardGameManager.DrawnCard.Skip().AsCoroutine();
    }
}
