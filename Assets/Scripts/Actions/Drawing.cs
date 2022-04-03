using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class Drawing : MonoBehaviour
{
    [Serializable]
    public class InkableCollider2D
    {
        public int AmountRequiredToPaint;
        public Collider2D Collider;
    }

    public int MaximumStrokes;
    public InkableCollider2D[] Colliders;

    public bool Overlaps(Vector2 pos)
    {
        return Colliders.Any(c => c.Collider.OverlapPoint(pos));
    }

    public bool IsComplete(IEnumerable<Ink> spots)
    {
        // is this slow?
        return Colliders.All(c =>
        {
            var amount = 0;
            foreach (var spot in spots)
            {
                if (c.Collider.OverlapPoint(spot.transform.position))
                {
                    amount++;
                }

                if (amount >= c.AmountRequiredToPaint) return true;
            }

            return false;
        });
    }
}
