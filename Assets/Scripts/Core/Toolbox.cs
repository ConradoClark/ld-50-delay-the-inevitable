using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Impl.Time;
using UnityEngine;
using UnityEngine.InputSystem;

public class Toolbox : MonoBehaviour
{
    public static Toolbox Instance;
     
    // Global
    public BasicMachinery MainMachinery { get; private set; }
    public DefaultTimer MainTimer { get; private set; }

    // Components
    public CardGameManager CardGameManager;
    public PlayerInput MainInput;

    private void OnEnable()
    {
        if (Instance != null) return;
        Instance = this;
        MainMachinery = new BasicMachinery();
        MainTimer = new DefaultTimer(() => Time.deltaTime * 1000);
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        MainMachinery.Update();
        MainTimer.Update();
    }
}
