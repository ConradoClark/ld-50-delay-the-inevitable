using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class PlayButton : ActionButton
{
    protected override Routine HandleAction()
    {
        Toolbox.Instance.CardGameManager.RegisterPlayedCard();
        yield return Toolbox.Instance.CardGameManager.DrawnCard.Play().AsCoroutine();
    }
}
