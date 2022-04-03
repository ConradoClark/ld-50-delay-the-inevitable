using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDefaults : MonoBehaviour
{
    public Sprite BackFaceSprite;
    public Color PrayerCardColor;
    public Color RitualCardColor;
    public Color BonusCardColor;
    public Color DefaultTextColor;

    public Dictionary<Card.CardType, Func<Color>> CardTypeColorMatch;

    void OnEnable()
    {
        CardTypeColorMatch = new Dictionary<Card.CardType, Func<Color>>
        {
            {Card.CardType.Prayer, () => PrayerCardColor},
            {Card.CardType.Ritual, () => RitualCardColor}
        };
    }

}
