using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Pooling;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public abstract class Card : MonoBehaviour, IPoolableObject
{
    public SpriteRenderer SpriteRenderer;

    public virtual Routine Draw()
    {
        yield return SlideIn().Combine(Reveal().AsCoroutine());
    }

    public virtual Routine Play()
    {
        yield break;
    }

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


    protected IEnumerable<Action> SlideIn()
    {
        return EasingYields.Lerp(
            f => transform.position =
                new Vector3(f, transform.position.y, transform.position.z),
            () => transform.position.x, 0.75f, transform.position.x - 1.5f,
            EasingYields.EasingFunction.QuadraticEaseOut,
            Toolbox.Instance.MainTimer);
    }

    protected Routine Reveal()
    {
        var flipMotion = EasingYields.Lerp(
            f => transform.localScale = new Vector3(f, transform.localScale.y, transform.localScale.z),
            () =>  transform.localScale.x, 0.45f, 0, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return flipMotion;
        SpriteRenderer.color = Color.cyan; // TODO: this is just for testing

        var flipBack = EasingYields.Lerp(
            f => transform.localScale = new Vector3(f,transform.localScale.y, transform.localScale.z),
            () => transform.localScale.x, 0.45f, 1, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);
        yield return flipBack;
    }

    public Routine SlideIntoDeck(float delay)
    {
        var motion = EasingYields.Lerp(
            f => transform.position = new Vector3(transform.position.x, f, transform.position.z),
            () => transform.position.y, delay,
            transform.position.y - 0.75f, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return motion.Combine(WaitAndSwoop(delay).AsCoroutine());
    }

    protected Routine WaitAndSwoop(float delay)
    {
        yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, delay);
        var motion = EasingYields.Lerp(
            f => transform.position = new Vector3(transform.position.x, f, transform.position.z),
            () => transform.position.y, delay * 0.4f,
            transform.position.y - 0.35f, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);
        Toolbox.Instance.MainMachinery.AddBasicMachine(200, motion);
    }
}
