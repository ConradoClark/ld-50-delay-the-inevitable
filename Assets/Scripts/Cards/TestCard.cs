using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class TestCard : Card
{
    public override Routine Draw()
    {
        yield return base.Draw().AsCoroutine();
        Debug.Log("Test Card was drawn!");
    }

    public override Routine Play()
    {
        yield return base.Play().AsCoroutine();
        Debug.Log("Test Card was played!");
    }
}
