using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Licht.Interfaces.Update;
using UnityEngine;

public class StatsManager : MonoBehaviour, IResettable
{
    public enum Stat
    {
        Faith,
        Sorcery
    }
    public int CurrentTurn { get; private set; } = 1;

    public Dictionary<Stat, int> Stats = new Dictionary<Stat, int>
    {
        {Stat.Faith, 1},
        {Stat.Sorcery, 1}
    };

    public event OnValueChanged OnTurnChanged;
    public event OnStatChanged OnStatChanged;

    // after an action is performed (play, skip, etc)
    public void IncreaseTurn()
    {
        CurrentTurn++;
        OnTurnChanged?.Invoke(CurrentTurn);
    }

    public void AddToStat(Stat stat, int value)
    {
        if (!Stats.ContainsKey(stat)) return;
        Stats[stat] += value;
        OnStatChanged?.Invoke(stat, Stats[stat]);
    }
    public bool Reset()
    {
        CurrentTurn = 1;
        foreach (var stat in Stats.Keys.ToArray())
        {
            Stats[stat] = 1;
        }
        OnTurnChanged?.Invoke(CurrentTurn);
        
        return true;
    }
}

public delegate void OnValueChanged(int value);
public delegate void OnStatChanged(StatsManager.Stat stat, int value);
