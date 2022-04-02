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
        action.Activate(10, 
            Constants.InputActions.Clap, Constants.DefaultTimeLimit);

        while (action.Result==null)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        Debug.Log("There's a result: " + action.Result);
        // give rewards? do some animation?

        if (action.Result == true)
        {
            foreach (var increase in StatIncreases)
            {
                   Toolbox.Instance.StatsManager.AddToStat(increase.Stat, increase.Amount);
            }
            SetResult(CardResult.Success);
        }
        else
        {
            // should failure have any consequences other than -1 card?
            SetResult(CardResult.Failure);
        }

        Toolbox.Instance.CardGameManager.PerformAction();
    }
}
