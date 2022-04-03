using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class Drawing : MonoBehaviour
{
    public Transform ColliderParent;
    private Collider2D[] _colliders;

    public void Activate()
    {
        _colliders = ColliderParent.GetComponentsInChildren<Collider2D>();
    }

    public bool Overlaps(Vector2 pos)
    {
        return _colliders.Any(c => c.OverlapPoint(pos));
    }
}
