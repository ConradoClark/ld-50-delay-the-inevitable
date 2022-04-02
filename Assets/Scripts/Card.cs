using System;
using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Pooling;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public abstract class Card : MonoBehaviour, IPoolableObject
{
    public enum CardType
    {
        Prayer,
        Circle,
        Ritual
    }

    public SpriteRenderer SpriteRenderer;
    public string Name;
    public CardType Type;
    public string Description;
    private Sprite _cardSprite;

    public virtual Routine Draw()
    {
        yield return SlideIn().Combine(Reveal().AsCoroutine());
    }

    public virtual Routine Play()
    {
        var ui = Toolbox.Instance.CardGameManager.CardUI;
        var cardPlayEffect = PlayEffect().AsCoroutine();

        yield return cardPlayEffect;
    }

    public void Initialize()
    {
        _cardSprite = SpriteRenderer.sprite;
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
        SpriteRenderer.sprite = Toolbox.Instance.CardDefaults.BackFaceSprite;
        return true;
    }

    protected Routine PlayEffect()
    {
        var motion = EasingYields.Lerp(
            f => transform.position = new Vector3(transform.position.x, f, transform.position.z),
            () => transform.position.y, 1f,
            transform.position.y + 0.4f, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);

        var bounceEffect = EasingYields.Lerp(
            f => transform.localScale = new Vector3(f, f, transform.localScale.z),
            () => transform.localScale.x, 0.35f,
            1.5f, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        var bounceBackEffect = EasingYields.Lerp(
            f => transform.localScale = new Vector3(f, f, transform.localScale.z),
            () => transform.localScale.x, 0.35f,
            1.25f, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);

        yield return motion.Combine(
            bounceEffect.Then(bounceBackEffect));
    }

    protected IEnumerable<Action> SlideIn()
    {
        return EasingYields.Lerp(
            f => transform.position =
                new Vector3(f, transform.position.y, transform.position.z),
            () => transform.position.x, 0.75f, transform.position.x - 2.5f,
            EasingYields.EasingFunction.QuadraticEaseOut,
            Toolbox.Instance.MainTimer);
    }

    protected Routine Reveal()
    {
        var flipMotion = EasingYields.Lerp(
            f => transform.localScale = new Vector3(f, transform.localScale.y, transform.localScale.z),
            () =>  transform.localScale.x, 0.45f, 0, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return flipMotion;
        SpriteRenderer.sprite = _cardSprite;

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
