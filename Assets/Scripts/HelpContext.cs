using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpContext : MonoBehaviour
{
    public string Name;
    public string Description;
    public Collider2D Collider;

    void Start()
    {
        Toolbox.Instance.HelpManager.AddToHelp(this);
    }
}
