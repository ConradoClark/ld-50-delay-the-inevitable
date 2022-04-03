using System;
using System.Linq;
using Licht.Impl.Generation;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using UnityEngine;
using Random = UnityEngine.Random;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class SimpleDrawing : Card
{
    [Serializable]
    public class DrawingDefinition : IWeighted<float>
    {
        public int Level;
        public GameObject Drawing;
        public float Weight { get; } = 1;
    }

    public DrawingDefinition[] Drawings; // select a drawing prefab
    public GameObject Ink; // select an ink prefab

    public override Routine PlayCard()
    {
        var drawings = Drawings.Where(d => Level == d.Level).ToArray();
        var action = Toolbox.Instance.ActionsManager.DrawAction;
        if (drawings.Length == 0) throw new Exception($"There should be at least one drawing at level {Level}: {gameObject.name}");

        var rng = new WeightedDice<DrawingDefinition>(drawings, new CardGameManager.DefaultRandomGenerator());
        var randomDrawing= rng.Generate();

        action.Activate(randomDrawing.Drawing, Ink, Constants.DefaultTimeLimit);

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
