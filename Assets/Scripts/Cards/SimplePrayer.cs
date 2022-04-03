using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class SimplePrayer : Card
{
    public override Routine Draw()
    {
        yield return base.Draw().AsCoroutine();
        Debug.Log("Test Card was drawn!");
    }

    public override Routine PlayCard()
    {
        var action = Toolbox.Instance.ActionsManager.ButtonMashAction;
        action.Activate(20*Level, // change to a different thing
            Constants.InputActions.Clap, Constants.DefaultTimeLimit);

        while (action.Result==null)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        Debug.Log("There's a result: " + action.Result);
        // give rewards? do some animation?

        SetResult(action.Result == true ? CardResult.Success : CardResult.Failure);

        Toolbox.Instance.CardGameManager.PerformAction();
    }
}
