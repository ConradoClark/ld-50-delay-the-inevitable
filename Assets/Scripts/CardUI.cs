using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class CardUI : MonoBehaviour
{
    public Routine Show()
    {
        this.gameObject.SetActive(true);
        yield break;
    }

    public Routine Hide()
    {
        this.gameObject.SetActive(false);
        yield break;
    }
}
