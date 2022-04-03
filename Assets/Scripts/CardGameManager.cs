using System;
using System.Collections.Generic;
using System.Linq;
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
    public List<CardDefinition> FullPool;
    public int InitialDrawingCardCount = 10;

    public Queue<Card> CurrentDeck;
    public Card DrawnCard;
    public CardUI CardUI;
    public GameUI GameUI;
    public EndingUI EndingUI;

    private bool _hitEndGameTrigger;
    private bool _actionTriggered;
    private bool _actionPerformed;
    private bool _addCardToNextReward;

    private int _playedCards;
    private int _skippedCards;
    private int _burntCards;

    public event OnDeckChangedEvent OnDeckChanged;

    public Ending EndingAsterisk;

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
        var stats = Toolbox.Instance.StatsManager;
        while (IsGameActive)
        {
            ResetCounters();
            stats.Reset();
            yield return Toolbox.Instance.ArtifactsManager.Reset().AsCoroutine();
            yield return GenerateDeck().AsCoroutine();

            while (IsGameActive)
            {
                yield return DrawCard().AsCoroutine();

                if (_hitEndGameTrigger)
                {
                    yield return ShowEnding().AsCoroutine();
                    break;
                }

                yield return ShowCardUI().AsCoroutine();
                yield return WaitForActionTriggered().AsCoroutine();
                yield return HideCardActions().AsCoroutine(); // be careful with timing
                yield return WaitForActionPerformed().AsCoroutine();
                stats.IncreaseTurn();
                yield return HideCardUI().AsCoroutine();
                yield return WaitAllCardEffects();
                yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 0.25);
            }
        }
    }
    
    private Routine ShowEnding()
    {
        var ending = GetEnding();
        ending.gameObject.SetActive(true);
        ending.transform.parent.gameObject.SetActive(true); // lazy
        
        GameUI.gameObject.SetActive(false);
        Toolbox.Instance.ArtifactsManager.HideArtifacts();

        var flash = EndingUI.FlashBackground().AsCoroutine();
        var stats = ShowEndingStats(ending).AsCoroutine();

        yield return flash.Combine(stats);
        yield return EndingUI.WaitForClickOnRestart().AsCoroutine();

        _hitEndGameTrigger = false;
        GameUI.gameObject.SetActive(true);
        Toolbox.Instance.ArtifactsManager.ShowArtifacts();
        ending.gameObject.SetActive(false);
        ending.transform.parent.gameObject.SetActive(false); // lazy
    }

    private Routine ShowEndingStats(Ending ending)
    {
        EndingUI.Ending.text = ending.Name;
        EndingUI.Turns.text = Toolbox.Instance.StatsManager.CurrentTurn.ToString().PadLeft(3, '0');
        EndingUI.Faith.text = Toolbox.Instance.StatsManager.Stats[StatsManager.Stat.Faith]
            .ToString().PadLeft(2, '0');
        EndingUI.Sorcery.text = Toolbox.Instance.StatsManager.Stats[StatsManager.Stat.Faith]
            .ToString().PadLeft(2, '0');

        EndingUI.Artifacts.text = Toolbox.Instance.ArtifactsManager.CurrentArtifacts.Count
            .ToString().PadLeft(2, '0');

        EndingUI.CardsPlayed.text = _playedCards.ToString().PadLeft(3, '0');
        EndingUI.CardsBurnt.text = _burntCards.ToString().PadLeft(3, '0');
        EndingUI.CardsSkipped.text = _skippedCards.ToString().PadLeft(3, '0');
        yield break;
    }

    private Ending GetEnding()
    {
        // Ending *: <30 turns - ended too quick, 
        if (Toolbox.Instance.StatsManager.CurrentTurn < 30) return EndingAsterisk;

        // Ending A: blablabla
        return EndingAsterisk;
    }

    public void RegisterPlayedCard()
    {
        _playedCards++;
    }

    public void RegisterSkippedCard()
    {
        _skippedCards++;
    }

    private void ResetCounters()
    {
        _playedCards = _skippedCards = _burntCards = 0;
    }

    public void AddCardToNextReward()
    {
        _addCardToNextReward = true;
    }

    public void ReleaseCard(Card card)
    {
        // I should compare this to something other than the name...
        var cardPool = StartingPool.Concat(FullPool ?? new List<CardDefinition>()).First(def => def.Name == card.OriginalName);
        cardPool.Pool.ReleaseCard(card);
        DrawnCard = null;
    }

    private IEnumerable<Action> WaitAllCardEffects()
    {
        while (DrawnCard != null)
        {
            yield return TimeYields.WaitOneFrame;
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
        if (_addCardToNextReward)
        {
            DrawnCard.AddTemporaryCardReward();
            _addCardToNextReward = false;
        }
        OnDeckChanged?.Invoke(CurrentDeck.Count);
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

    public Routine AddCardsToDeck(int amount)
    {
        var cards =
            new WeightedDice<CardDefinition>(StartingPool.Concat(FullPool), new DefaultRandomGenerator());

        for (var i = 0; i < amount; i++)
        {
            var selectedCard = cards.Generate();
            if (selectedCard.Pool.TryGetCard(out var card))
            {
                yield return AddCard(card, i).AsCoroutine();
            }
        }
    }

    public Routine AddDrawnCardToDeck()
    {
        yield return AddCard(DrawnCard, 0, false).AsCoroutine();
        DrawnCard = null;
    }

    private Routine AddCard(Card card, int index, bool doEffect = true)
    {
        CurrentDeck.Enqueue(card);
        OnDeckChanged?.Invoke(CurrentDeck.Count);
        if (!doEffect) yield break;

        var delay = (1f- Mathf.Clamp(index,0,10)*0.1f)*0.03f + 0.15f;
        yield return card.SlideIntoDeck(delay).AsCoroutine();
    }

    #endregion
}

public delegate void OnDeckChangedEvent(int deckSize);
