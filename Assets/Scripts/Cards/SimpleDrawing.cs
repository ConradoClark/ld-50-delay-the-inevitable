using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class SimpleDrawing : Card
{
    public GameObject Drawing; // select a drawing prefab
    public GameObject Ink; // select an ink prefab

    public override Routine PlayCard()
    {
        var action = Toolbox.Instance.ActionsManager.DrawAction;
        action.Activate(Drawing, Ink, Constants.DefaultTimeLimit);

        while (action.Result == null)
        {
            yield return TimeYields.WaitOneFrameX;
        }

        Debug.Log("There's a result: " + action.Result);
        // give rewards? do some animation?

        SetResult(action.Result == true ? CardResult.Success : CardResult.Failure);

        Toolbox.Instance.CardGameManager.PerformAction();
    }
}
