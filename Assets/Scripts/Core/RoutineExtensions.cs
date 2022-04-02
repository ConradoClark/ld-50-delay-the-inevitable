using System;
using System.Collections.Generic;
using System.Linq;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public static class RoutineExtensions
{
    public static IEnumerable<Action> AsCoroutine(this Routine routine)
    {
        return routine.SelectMany(s => s);
    }
}

