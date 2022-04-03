using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class CardUI : MonoBehaviour
{
    public Transform ActionsGroup;
    public TMP_Text CardName;
    public TMP_Text CardDescription;
    public TMP_Text CardRequirements;

    public TMP_Text CardResultFeedback;

    public Transform StatsGroup;
    public TMP_Text FaithIncrease;
    public TMP_Text SorceryIncrease;
    public TMP_Text CardIncrease;
    public TMP_Text CardBonusSign;
    public Collider2D CardBonusSignCollider; // tweak

    private TMP_Text[] _texts;
    private SpriteRenderer[] _sprites;

    public ActionButton BurnButton;
    public ActionButton SkipButton;

    public Routine Show()
    {
        this.gameObject.SetActive(true);
        ActionsGroup.gameObject.SetActive(true);

        var card = Toolbox.Instance.CardGameManager.DrawnCard;
        if (card == null) yield break; // should never happen

        if (card.CanSkip()) SkipButton.UnblockButton();
        else SkipButton.BlockButton("This card has reached its maximum level.");
        
        if (card.CanBurn()) BurnButton.UnblockButton();
        else BurnButton.BlockButton("This function is still unimplemented");


        CardName.text = card.Name;
        CardName.color = Toolbox.Instance.CardDefaults.CardTypeColorMatch.ContainsKey(card.Type)
            ? Toolbox.Instance.CardDefaults.CardTypeColorMatch[card.Type]()
            : Toolbox.Instance.CardDefaults.DefaultTextColor;

        CardDescription.text = card.Description;
        CardRequirements.text = card.Requirements;
        CardRequirements.color = Toolbox.Instance.CardDefaults.CardTypeColorMatch.ContainsKey(card.Type)
            ? Toolbox.Instance.CardDefaults.CardTypeColorMatch[card.Type]()
            : Toolbox.Instance.CardDefaults.DefaultTextColor;

        var faithIncrease = GetStatIncrease(StatsManager.Stat.Faith, card);
        FaithIncrease.text = faithIncrease.ToString().PadLeft(2, '0');

        var sorceryIncrease = GetStatIncrease(StatsManager.Stat.Sorcery, card);
        SorceryIncrease.text = sorceryIncrease.ToString().PadLeft(2, '0');

        var cardIncrease = GetStatIncrease(StatsManager.Stat.Card, card);
        CardIncrease.text = cardIncrease.ToString().PadLeft(2, '0');

        AdjustCardIncreasesText(card);

        var targetX = StatsGroup.position.x;
        StatsGroup.position = new Vector3(StatsGroup.position.x + 0.5f, StatsGroup.position.y, StatsGroup.position.z);
        Toolbox.Instance.MainMachinery.AddBasicMachine(44, AnimateStats(targetX));
    }

    private Routine AnimateStats(float targetX)
    {
        var slideStats = EasingYields.Lerp(f =>
                StatsGroup.position = new Vector3(f, StatsGroup.position.y, StatsGroup.position.z),
            () => StatsGroup.position.x, 0.35f, targetX, EasingYields.EasingFunction.QuadraticEaseOut,
            Toolbox.Instance.MainTimer);

        _texts ??= StatsGroup.GetComponentsInChildren<TMP_Text>();
        _sprites ??= StatsGroup.GetComponentsInChildren<SpriteRenderer>();
        foreach (var text in _texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }

        foreach (var sprite in _sprites)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
        }

        var fadeInTexts = EasingYields.Lerp(f =>
            {
                foreach (var text in _texts)
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, f);
                }
                foreach (var sprite in _sprites)
                {
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, f);
                }
            }, () => _texts.First().color.a, 0.15f,1f, EasingYields.EasingFunction.CubicEaseInOut,
            Toolbox.Instance.MainTimer);

        yield return slideStats.Combine(fadeInTexts);
    }

    private int GetStatIncrease(StatsManager.Stat stat, Card card)
    {
        return card.StatIncreases.Concat(card.TemporaryStatIncreases)
            .Where(s => s.Stat == stat)
            .Select(s=>s.Amount)
            .DefaultIfEmpty(0)
            .Sum();
    }

    private void AdjustCardIncreasesText(Card card)
    {
        var hasBonusCard = card.TemporaryStatIncreases.Any(stat => stat.Stat == StatsManager.Stat.Card && stat.Amount > 0);
        CardBonusSign.color = CardIncrease.color = hasBonusCard
            ? Toolbox.Instance.CardDefaults.BonusCardColor
            : Toolbox.Instance.CardDefaults.DefaultTextColor;

        CardBonusSign.enabled = hasBonusCard;
        CardBonusSignCollider.enabled = hasBonusCard;
    }

    public Routine HideActions()
    {
        ActionsGroup.gameObject.SetActive(false);
        yield break;
    }

    public Routine Hide()
    {
        this.gameObject.SetActive(false);
        yield break;
    }

    public Routine ShowSuccessMessage()
    {
        CardResultFeedback.color = Color.black;
        CardResultFeedback.text = "SUCCESS!";
        yield break;
    }

    public Routine ShowFailureMessage()
    {
        CardResultFeedback.color = Color.black;
        CardResultFeedback.text = "FAILURE...";
        yield break;
    }

    public Routine HideResultMessage()
    {
        var fade = EasingYields.Lerp(
            f => CardResultFeedback.color =
                new Color(CardResultFeedback.color.r, CardResultFeedback.color.g, CardResultFeedback.color.b, f),
            () => CardResultFeedback.color.a, 0.3f,
            0f, EasingYields.EasingFunction.CubicEaseIn, Toolbox.Instance.MainTimer);

        yield return fade;
        CardResultFeedback.text = "";
    }
}
