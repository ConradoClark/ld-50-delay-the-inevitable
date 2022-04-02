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

    public override Routine Play()
    {
        yield return base.Play().AsCoroutine();
        var action = Toolbox.Instance.ActionsManager.ButtonMashAction;
        action.Activate(10, 
            Constants.InputActions.Clap, Constants.DefaultTimeLimit);

        while (action.Result==null)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        Debug.Log("There's a result: " + action.Result);
        // give rewards? do some animation?

        Toolbox.Instance.CardGameManager.PerformAction();

        Debug.Log($"card {gameObject.name} is gone");
        gameObject.SetActive(false);
    }
}
