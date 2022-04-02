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
    private bool _actionTriggered;
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
            yield return WaitForActionTriggered().AsCoroutine();
            yield return HideCardActions().AsCoroutine(); // be careful with timing
            yield return WaitForActionPerformed().AsCoroutine();
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

    private Routine HideCardActions()
    {
        yield return CardUI.HideActions().AsCoroutine();
    }

    private Routine HideCardUI()
    {
        yield return CardUI.Hide().AsCoroutine();
    }

    private Routine WaitForActionTriggered()
    {
        _actionTriggered = false;
        while (!_actionTriggered)
        {
            yield return TimeYields.WaitOneFrameX;
        }
    }

    private Routine WaitForActionPerformed()
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
        _actionTriggered = true;
    }

    public void PerformAction()
    {
        _actionPerformed = true;
    }

    private Routine DrawCard()
    {
        if (CurrentDeck.Count == 0)
        {
            _hitEndGameTrigger = true;
            yield break;
        }

        DrawnCard = CurrentDeck.Dequeue();
        yield return DrawnCard.Draw().AsCoroutine();
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

        yield return card.SlideIntoDeck(delay).AsCoroutine();
    }

    #endregion
}
