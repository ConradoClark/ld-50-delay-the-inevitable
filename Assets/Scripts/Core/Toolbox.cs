using Licht.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

public class Toolbox : BasicToolbox
{
    // Components
    public CardGameManager CardGameManager;
    public PlayerInput MainInput;
    public Camera MainCamera;
    public CardDefaults CardDefaults;
    public ActionsManager ActionsManager;
    public StatsManager StatsManager;
    public EffectsManager EffectsManager;
    public HelpManager HelpManager;
    public ArtifactsManager ArtifactsManager;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        Application.targetFrameRate = 60;
    }

    public new static Toolbox Instance { get; private set; }
}
