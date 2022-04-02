using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Impl.Time;
using UnityEngine;

public class Toolbox : MonoBehaviour
{
    public static Toolbox Instance;
     
    // Global
    public BasicMachinery MainMachinery;
    public DefaultTimer MainTimer;

    // Components

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
