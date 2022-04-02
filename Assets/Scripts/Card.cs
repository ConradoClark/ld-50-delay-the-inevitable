using System.Collections;
using System.Collections.Generic;
using Licht.Interfaces.Pooling;
using UnityEngine;

public abstract class Card : MonoBehaviour, IPoolableObject
{
    public SpriteRenderer SpriteRenderer;
    public abstract void Draw();
    public void Initialize()
    {
    }

    public bool IsActive { get; set; }
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
