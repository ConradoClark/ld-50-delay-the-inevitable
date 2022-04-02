using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class CardUI : MonoBehaviour
{
    public Transform ActionsGroup;
    public TMP_Text CardName;
    public TMP_Text CardDescription;

    public Routine Show()
    {
        this.gameObject.SetActive(true);
        ActionsGroup.gameObject.SetActive(true);

        var card = Toolbox.Instance.CardGameManager.DrawnCard;
        if (card == null) yield break; // should never happen
        CardName.text = card.Name;
        CardDescription.text = card.Description;
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
}
