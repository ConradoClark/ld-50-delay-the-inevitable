using System.Collections;
using System.Collections.Generic;
using Licht.Interfaces.Update;
using UnityEngine;

public class StatsManager : MonoBehaviour, IResettable
{
    public int CurrentTurn { get; private set; } = 1;
    public int Faith { get; private set; } = 1;
    public int Sorcery { get; private set; } = 1;

    public event OnValueChanged OnTurnChanged;
    public event OnValueChanged OnFaithChanged;
    public event OnValueChanged OnSorceryChanged;

    // after an action is performed (play, skip, etc)
    public void IncreaseTurn()
    {
        CurrentTurn++;
        OnTurnChanged?.Invoke(CurrentTurn);
    }

    public void AddToFaith(int value)
    {
        Faith += value;
        OnFaithChanged?.Invoke(Faith);
    }

    public void AddToSorcery(int value)
    {
        Faith += value;
        OnSorceryChanged?.Invoke(Faith);
    }

    public bool Reset()
    {
        CurrentTurn = Faith = Sorcery = 1;
        OnTurnChanged?.Invoke(CurrentTurn);
        OnFaithChanged?.Invoke(Faith);
        OnSorceryChanged?.Invoke(Sorcery);
        return true;
    }
}

public delegate void OnValueChanged(int value);
