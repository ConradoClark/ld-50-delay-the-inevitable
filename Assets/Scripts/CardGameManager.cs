using System;
using System.Collections.Generic;
using Licht.Impl.Generation;
using Licht.Impl.Orchestration;
using Licht.Interfaces.Generation;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class CardGameManager : MonoBehaviour
{
    [Serializable]
    public struct CardDefinition : IWeighted<float>
    {
        public string Name;
        public CardPool Pool;
        public int weight;
        public float Weight => weight;
    }

    public bool IsGameActive { get; private set; }
    public List<CardDefinition> StartingPool;
    public int InitialDrawingCardCount = 10;

    public Queue<Card> CurrentDeck;
    public Card DrawnCard;
    public CardUI CardUI;

    private bool _hitEndGameTrigger;
    private bool _actionPerformed;

    public class DefaultRandomGenerator : IGenerator<int, float>
    {
        public int Seed { get; set; }

        public float Generate()
        {
            return UnityEngine.Random.Range(0f, 1f);
        }
    }

    void OnEnable()
    {
        Toolbox.Instance.MainMachinery.AddBasicMachine(1, StartGame());
    }

    public Routine StartGame()
    {
        IsGameActive = true;
        _hitEndGameTrigger = false;

        Toolbox.Instance.MainMachinery.AddBasicMachine(100, GameLoop());
        yield break;
    }

    private Routine GameLoop()
    {
        while (IsGameActive)
        {
            yield return GenerateDeck().AsCoroutine();
            yield return DrawCard().AsCoroutine();
            yield return ShowCardUI().AsCoroutine();
            yield return WaitForAction().AsCoroutine();
            yield return HideCardUI().AsCoroutine();
            while (IsGameActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }
        }
    }

    private Routine ShowCardUI()
    {
        yield return CardUI.Show().AsCoroutine();
    }

    private Routine HideCardUI()
    {
        yield return CardUI.Hide().AsCoroutine();
    }

    private Routine WaitForAction()
    {
        _actionPerformed = false;
        while (!_actionPerformed)
        {
            yield return TimeYields.WaitOneFrameX;
        }
    }

    // TODO: Make a better way to trigger events or w/e?
    public void TriggerAction()
    {
        _actionPerformed = true;
    }

    // TODO: Move card effects to the card script file.
    private Routine DrawCard()
    {
        if (CurrentDeck.Count == 0)
        {
            _hitEndGameTrigger = true;
            yield break;
        }

        DrawnCard = CurrentDeck.Dequeue();
        DrawnCard.Draw();
        yield return EasingYields.Lerp(
            f => DrawnCard.transform.position =
                new Vector3(f, DrawnCard.transform.position.y, DrawnCard.transform.position.z),
            () => DrawnCard.transform.position.x, 0.75f, DrawnCard.transform.position.x - 1.5f,
            EasingYields.EasingFunction.QuadraticEaseOut,
            Toolbox.Instance.MainTimer).Combine(ShowCard(DrawnCard).AsCoroutine());
    }

    private Routine ShowCard(Card card)
    {
        var flipMotion = EasingYields.Lerp(
            f => card.transform.localScale = new Vector3(f, card.transform.localScale.y, card.transform.localScale.z),
            () => card.transform.localScale.x, 0.45f, 0, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return flipMotion;
        card.SpriteRenderer.color = Color.cyan;

        var flipBack = EasingYields.Lerp(
            f => card.transform.localScale = new Vector3(f, card.transform.localScale.y, card.transform.localScale.z),
            () => card.transform.localScale.x, 0.45f, 1, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);
        yield return flipBack;
    }


    #region Deck Generation
    private Routine GenerateDeck()
    {
        CurrentDeck = new Queue<Card>();
        var cards =
            new WeightedDice<CardDefinition>(StartingPool, new DefaultRandomGenerator());

        for (var i = 0; i < InitialDrawingCardCount; i++)
        {
            var selectedCard = cards.Generate();
            if (selectedCard.Pool.TryGetCard(out var card))
            {
                yield return AddCard(card, i).AsCoroutine();
            }
        }
        yield break;
    }

    private Routine AddCard(Card card, int index)
    {
        Debug.Log($"Added card [{card.name}] to deck.");
        CurrentDeck.Enqueue(card);

        float delay = (1f- Mathf.Clamp(index,0,10)*0.1f)*0.03f + 0.15f;

        var motion = EasingYields.Lerp(
            f => card.transform.position = new Vector3(card.transform.position.x, f, card.transform.position.z),
            () => card.transform.position.y, delay,
            card.transform.position.y - 0.75f, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return motion.Combine(WaitAndSwoop(card, delay).AsCoroutine());
    }

    private Routine WaitAndSwoop(Card card, float delay)
    {
        yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, delay);
        var motion = EasingYields.Lerp(
            f => card.transform.position = new Vector3(card.transform.position.x, f, card.transform.position.z),
            () => card.transform.position.y, delay*0.4f,
            card.transform.position.y - 0.35f, EasingYields.EasingFunction.CubicEaseOut, Toolbox.Instance.MainTimer);
        Toolbox.Instance.MainMachinery.AddBasicMachine(200, motion);
    }
    #endregion
}
