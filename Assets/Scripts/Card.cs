using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Pooling;
using Licht.Unity.Extensions;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements.Experimental;
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
    protected int Level = 1;
    public string OriginalName { get; protected set; }
    public int MaxLevel = 1;

    [Serializable]
    public class StatQuantifier
    {
        public StatsManager.Stat Stat;
        public int Amount;
    }

    public List<StatQuantifier> StatIncreases;
    public List<StatQuantifier> TemporaryStatIncreases;
    protected List<StatQuantifier> OriginalStatIncreases;

    public bool CanSkip()
    {
        return Level < MaxLevel;
    }

    public bool CanBurn()
    {
        return false;
    }

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
        var cardIncreases =
            tempCardIncreases.Select(inc => inc.Amount).DefaultIfEmpty(0).Sum()
            + StatIncreases.Where(inc => inc.Stat == StatsManager.Stat.Card).Select(inc => inc.Amount)
                .DefaultIfEmpty(0).Sum();

        switch (_result)
        {
            case CardResult.Success:
                foreach (var increase in StatIncreases.Concat(TemporaryStatIncreases)
                    .Where(stat => stat.Stat != StatsManager.Stat.Card))
                {
                    Toolbox.Instance.StatsManager.AddToStat(increase.Stat, increase.Amount);
                }
                if (!tempCardIncreases.Any()) Toolbox.Instance.CardGameManager.AddCardToNextReward();

                var successMessage = Toolbox.Instance.CardGameManager.CardUI.ShowSuccessMessage().AsCoroutine();
                var success= SuccessEffect().AsCoroutine();

                yield return success.Combine(successMessage);

                if (cardIncreases > 0)
                {
                    yield return Toolbox.Instance.CardGameManager.AddCardsToDeck(cardIncreases).AsCoroutine();
                }
                break;
            case CardResult.Failure:
                var failureMessage = Toolbox.Instance.CardGameManager.CardUI.ShowFailureMessage().AsCoroutine();
                var failure = FailureEffect().AsCoroutine();
                yield return failure.Combine(failureMessage);
                break;
            default: break;
        }

        Toolbox.Instance.Machinery().AddBasicMachine(Toolbox.Instance.CardGameManager.CardUI.HideResultMessage().AsCoroutine());
        TemporaryStatIncreases.Clear();
        Toolbox.Instance.CardGameManager.ReleaseCard(this);
    }

    public Routine Skip()
    {
        yield return SlideBack().Combine(FlipSkipEffect().AsCoroutine());
        Evolve();
        SetResult(CardResult.Skip);
        yield return Toolbox.Instance.CardGameManager.AddDrawnCardToDeck().AsCoroutine();
        Toolbox.Instance.CardGameManager.PerformAction();
    }

    protected virtual void ResetLevel()
    {
        Level = 1;
        Name = OriginalName;
        StatIncreases.Clear();
        StatIncreases.AddRange(OriginalStatIncreases.Select(s=>
            new StatQuantifier
            {
                Stat = s.Stat,
                Amount = s.Amount,
            }));
    }

    protected virtual void Evolve()
    {
        Level++;
        Name = $"{OriginalName} {new string(Enumerable.Repeat('+', Level - 1).ToArray())}";
        foreach (var statIncrease in TemporaryStatIncreases)
        {
            var native = StatIncreases.FirstOrDefault(inc => inc.Stat == statIncrease.Stat);
            if (native == null) StatIncreases.Add(statIncrease);
            else native.Amount += statIncrease.Amount;
        }

        // should I always double it? It seems like this could be abused
        // unless I make it risky (difficult cards, etc)
        foreach (var statIncrease in StatIncreases)
        {
            statIncrease.Amount *= 2;
        }
        TemporaryStatIncreases.Clear();
    }

    protected IEnumerable<Action> SlideBack()
    {
        return transform.GetAccessor().Position.X
            .Increase(2.75f)
            .Over(0.75f)
            .Easing(EasingYields.EasingFunction.QuadraticEaseOut)
            .Build();
    }

    protected Routine FlipSkipEffect()
    {
        var flip = false;
        for (var i = 0; i < 3; i++)
        {
            var flipMotion = transform.GetAccessor().LocalScale.X
                .SetTarget(0f)
                .Over(0.15f)
                .Easing(EasingYields.EasingFunction.CubicEaseIn)
                .Build();

            var flipBack = transform.GetAccessor().LocalScale.X
                .SetTarget(1)
                .Over(0.15f)
                .Easing(EasingYields.EasingFunction.CubicEaseOut)
                .Build();

            yield return flipMotion;

            SpriteRenderer.sprite = flip ? _cardSprite : Toolbox.Instance.CardDefaults.BackFaceSprite;
            flip = !flip;

            yield return flipBack;
        }
    }

    public void AddTemporaryCardReward()
    {
        TemporaryStatIncreases.Add(new StatQuantifier
        {
            Stat = StatsManager.Stat.Card,
            Amount = 1
        });
    }

    public void Initialize()
    {
        _cardSprite = SpriteRenderer.sprite;
        _originalPosition = transform.position;
        OriginalStatIncreases = new List<StatQuantifier>(StatIncreases);
        OriginalName = Name;
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
        ResetLevel();
        gameObject.SetActive(true);
        IsActive = true;
        transform.rotation = Quaternion.identity;
        SpriteRenderer.sprite = Toolbox.Instance.CardDefaults.BackFaceSprite;
        return true;
    }

    protected void SetResult(CardResult result)
    {
        _result = result;
    }

    protected Routine SuccessEffect()
    {
        var motion = transform.GetAccessor().Position.Y
            .Increase(0.25f)
            .Over(1f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .Build();

        var fade = SpriteRenderer.GetAccessor().Color.A
            .SetTarget(0f)
            .Over(1f)
            .Easing(EasingYields.EasingFunction.CubicEaseIn)
            .Build();

        yield return motion.Combine(fade);
    }

    protected Routine FailureEffect()
    {
        var rotate = TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 1f, delta =>
        {
            transform.Rotate(0, 0, (float)delta * 0.5f);
        });

        var resize = transform.GetAccessor()
            .UniformScale()
            .SetTarget(0f)
            .Over(1f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .Build();

        var fade = SpriteRenderer.GetAccessor().Color.A
            .SetTarget(0f)
            .Over(1f)
            .Easing(EasingYields.EasingFunction.CubicEaseIn)
            .Build();

        yield return rotate.Combine(resize).Combine(fade);
    }

    protected Routine PlayEffect()
    {
        var motion = transform.GetAccessor().Position.Y
            .Increase(0.4f)
            .Over(1f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .Build();

        var bounceTemplate = transform.GetAccessor().UniformScale()
            .Over(0.35f);

        var bounce = bounceTemplate.SetTarget(1.5f)
            .Easing(EasingYields.EasingFunction.CubicEaseIn)
            .Build();

        var bounceBack = bounceTemplate.SetTarget(1.25f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .Build();

        yield return motion.Combine(
            bounce.Then(bounceBack));
    }

    protected IEnumerable<Action> SlideIn()
    {
        return transform.GetAccessor()
            .Position.X
            .Decrease(2.75f)
            .Over(0.75f)
            .Easing(EasingYields.EasingFunction.QuadraticEaseOut)
            .Build();
    }

    protected Routine Reveal()
    {
        var flipTemplate = transform.GetAccessor()
            .LocalScale.X
            .Over(0.45f);

        var flip = flipTemplate.SetTarget(0f).Easing(EasingYields.EasingFunction.CubicEaseIn).Build();
        var flipBack = flipTemplate.SetTarget(1f).Easing(EasingYields.EasingFunction.CubicEaseOut).Build();

        yield return flip;
        SpriteRenderer.sprite = _cardSprite;
        yield return flipBack;
    }

    public Routine SlideIntoDeck(float delay)
    {
        var slideIntoDeck = transform.GetAccessor().Position.Y
            .Decrease(0.75f)
            .Over(delay)
            .Easing(EasingYields.EasingFunction.CubicEaseIn)
            .Build();

        var swoop = transform.GetAccessor().Position.Y
            .Decrease(0.35f)
            .Over(delay * 0.4f)
            .Easing(EasingYields.EasingFunction.CubicEaseOut)
            .Build();

        yield return slideIntoDeck;
        Toolbox.Instance.Machinery().AddBasicMachine(swoop);
    }

}
