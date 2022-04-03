using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Pooling;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public abstract class Card : MonoBehaviour, IPoolableObject
{
    public enum CardType
    {
        Prayer,
        Circle,
        Ritual
    }

    public enum CardResult
    {
        Unknown,
        Success,
        Failure,
        Burn,
        Skip,
    }

    public SpriteRenderer SpriteRenderer;
    public string Name;
    public CardType Type;
    public string Description;
    public string Requirements;
    private Sprite _cardSprite;
    private CardResult _result;
    private Vector3? _originalPosition;

    [Serializable]
    public class StatIncrease
    {
        public StatsManager.Stat Stat;
        public int Amount;
    }

    public List<StatIncrease> StatIncreases;
    public List<StatIncrease> TemporaryStatIncreases;

    public virtual Routine Draw()
    {
        yield return SlideIn().Combine(Reveal().AsCoroutine());
    }

    public virtual Routine PlayCard()
    {
        yield break;
    }

    public Routine Play()
    {
        yield return PlayEffect().AsCoroutine();
        yield return PlayCard().AsCoroutine();

        var tempCardIncreases = TemporaryStatIncreases.Where(stat => stat.Stat == StatsManager.Stat.Card).ToArray();

        switch (_result)
        {
            case CardResult.Success:
                foreach (var tempInc in TemporaryStatIncreases.Where(stat => stat.Stat != StatsManager.Stat.Card))
                {
                    Toolbox.Instance.StatsManager.AddToStat(tempInc.Stat, tempInc.Amount);
                }
                if (!tempCardIncreases.Any()) Toolbox.Instance.CardGameManager.AddCardToNextReward();
                yield return SuccessEffect().AsCoroutine();
                foreach (var tempInc in tempCardIncreases)
                {
                    if (tempInc.Stat == StatsManager.Stat.Card)
                        yield return Toolbox.Instance.CardGameManager.AddCardsToDeck(tempInc.Amount).AsCoroutine();
                }
                break;
            case CardResult.Failure:
                yield return FailureEffect().AsCoroutine();
                break;
            default: break;
        }

        TemporaryStatIncreases.Clear();
        Toolbox.Instance.CardGameManager.ReleaseCard(this);
    }

    public void AddTemporaryCardReward()
    {
        TemporaryStatIncreases.Add(new StatIncrease
        {
            Stat = StatsManager.Stat.Card,
            Amount=1
        });
    }

    public void Initialize()
    {
        _cardSprite = SpriteRenderer.sprite;
        _originalPosition = transform.position;
    }

    public bool IsActive { get; set; }
    public bool Deactivate()
    {
        transform.localScale = Vector3.one;
        _result = CardResult.Unknown;
        SpriteRenderer.color = Color.white;
        gameObject.SetActive(false);
        IsActive = false;
        return true;
    }

    public bool Activate()
    {
        if (_originalPosition != null) transform.position = _originalPosition.Value;
        gameObject.SetActive(true);
        IsActive = true;
        SpriteRenderer.sprite = Toolbox.Instance.CardDefaults.BackFaceSprite;
        return true;
    }

    protected void SetResult(CardResult result)
    {
        _result = result;
    }

    protected Routine SuccessEffect()
    {
        var motion = EasingYields.Lerp(
            f => transform.position = new Vector3(transform.position.x, f, transform.position.z),
            () => transform.position.y, 1f,
            transform.position.y + 0.25f, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);

        var fade = EasingYields.Lerp(
            f => SpriteRenderer.color = 
                new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, f),
            () => SpriteRenderer.color.a, 1f,
            0f, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return motion.Combine(fade);
    }

    protected Routine FailureEffect()
    {
        var rotate = TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 1f, delta =>
        {
            transform.Rotate(0, 0, (float) delta * 0.5f);
        });

        var resize = EasingYields.Lerp(
            f => transform.localScale = new Vector3(f, f, transform.localScale.z),
            () => transform.localScale.x, 1f,
            0f, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);

        var fade = EasingYields.Lerp(
            f => SpriteRenderer.color =
                new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, f),
            () => SpriteRenderer.color.a, 1f,
            0f, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return rotate.Combine(resize).Combine(fade);
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
            () => transform.position.x, 0.75f, transform.position.x - 2.75f,
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
