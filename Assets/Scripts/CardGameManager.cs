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
    public int InitialDrawingCardCount = 10;

    public List<Card> CurrentDeck;

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

        Toolbox.Instance.MainMachinery.AddBasicMachine(100, GameLoop());
        yield break;
    }

    private Routine GameLoop()
    {
        while (IsGameActive)
        {
            yield return DrawCards().AsCoroutine();

            while (IsGameActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }
        }
    }

    private Routine DrawCards()
    {
        CurrentDeck = new List<Card>();
        var cards =
            new WeightedDice<CardDefinition>(StartingPool, new DefaultRandomGenerator());

        for (var i = 0; i < InitialDrawingCardCount; i++)
        {
            var selectedCard = cards.Generate();
            if (selectedCard.Pool.TryGetCard(out var card))
            {
                Debug.Log($"Added card [{card.name}] to deck.");
                CurrentDeck.Add(card);
            }

        }
        yield break;
    }
}
