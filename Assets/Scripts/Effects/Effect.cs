using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Impl.Pooling;
using Licht.Interfaces.Pooling;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class Effect : MonoBehaviour, IPoolableObject
{
    public Animator Animator;
    public string Animation;
    public float Duration;

    public void Initialize()
    {
    }

    public bool IsActive { get; }
    public bool Deactivate()
    {
        gameObject.SetActive(false);
        return true;
    }

    public bool Activate()
    {
        gameObject.SetActive(true);
        Animator.Play(Animation);
        Toolbox.Instance.Machinery().AddBasicMachine(Expire());
        return true;
    }

    private Routine Expire()
    {
        yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, Duration);
        Deactivate();
    }
}
