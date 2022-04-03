using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class EndingUI : MonoBehaviour
{
    public TMP_Text Ending;
    public TMP_Text Turns;
    public TMP_Text Faith;
    public TMP_Text Sorcery;
    public TMP_Text Artifacts;
    public TMP_Text CardsPlayed;
    public TMP_Text CardsBurnt;
    public TMP_Text CardsSkipped;

    public SpriteRenderer FullBackground;
    public Color FullBackgroundColor;
    public Collider2D RestartButtonCollider;

    public Routine WaitForClickOnRestart()
    {
        var clickAction = Toolbox.Instance.MainInput.actions[Constants.InputActions.Click];
        var mousePosValue = Toolbox.Instance.MainInput.actions[Constants.InputActions.MousePosition];

        while (isActiveAndEnabled)
        {
            var mousePos = Toolbox.Instance.MainCamera.ScreenToWorldPoint(mousePosValue.ReadValue<Vector2>());
            if (clickAction.WasPerformedThisFrame() && RestartButtonCollider.OverlapPoint(mousePos))
            {
                break;
            }

            yield return TimeYields.WaitOneFrameX;
        }
    }

    public Routine FlashBackground()
    {
        FullBackground.color = FullBackgroundColor;

        for (var i = 0; i < 5; i++)
        {
            yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, Mathf.Lerp(0.5f,0.2f, i/5f));
            FullBackground.color = new Color(FullBackground.color.r, FullBackground.color.g, FullBackground.color.b,
                Mathf.Lerp(1, 0, i / 5f));
        }
    }
}
