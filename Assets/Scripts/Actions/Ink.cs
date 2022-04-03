using System.Collections;
using System.Collections.Generic;
using Licht.Interfaces.Pooling;
using UnityEngine;

public class Ink : MonoBehaviour, IPoolableObject
{
    public void Initialize()
    {
        Deactivate();
    }

    public bool IsActive { get; private set; }
    public bool Deactivate()
    {
        gameObject.SetActive(false);
        IsActive = false;
        return true;
    }

    public bool Activate()
    {
        gameObject.SetActive(true);
        IsActive = true;
        return true;
    }
}
