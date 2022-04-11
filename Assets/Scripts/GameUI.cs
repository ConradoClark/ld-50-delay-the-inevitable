using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class GameUI : MonoBehaviour
{
    public TMP_Text Faith;
    public TMP_Text Sorcery;
    public TMP_Text Cards;
    public TMP_Text Turn;
    public TMP_Text Timer;
    private int _timerCount;
    private bool _isTimerActive;

    public TMP_Text HelpCaption;
    public TMP_Text HelpText;

    void OnDisable()
    {
        Toolbox.Instance.StatsManager.OnTurnChanged -= StatsManager_OnTurnChanged;
        Toolbox.Instance.StatsManager.OnStatChanged -= StatsManager_OnStatChanged;
        Toolbox.Instance.CardGameManager.OnDeckChanged -= CardGameManager_OnDeckChanged;
        Toolbox.Instance.HelpManager.OnHelpUpdated -= HelpManager_OnHelpUpdated;
    }

    void OnEnable()
    {
        Toolbox.Instance.Machinery().AddBasicMachine(HandleTimer());
        Toolbox.Instance.StatsManager.OnTurnChanged += StatsManager_OnTurnChanged;
        Toolbox.Instance.StatsManager.OnStatChanged += StatsManager_OnStatChanged;
        Toolbox.Instance.CardGameManager.OnDeckChanged += CardGameManager_OnDeckChanged;
        Toolbox.Instance.HelpManager.OnHelpUpdated += HelpManager_OnHelpUpdated;
        HelpCaption.text = HelpText.text = "";
    }

    private void HelpManager_OnHelpUpdated(string caption, string description)
    {
        HelpCaption.text = caption;
        HelpText.text = description;
    }

    private void CardGameManager_OnDeckChanged(int deckSize)
    {
        Cards.text = "x " + deckSize.ToString().PadLeft(2, '0');
    }

    private void StatsManager_OnStatChanged(StatsManager.Stat stat, int value)
    {
        switch (stat)
        {
            case StatsManager.Stat.Faith:
                Faith.text = value.ToString().PadLeft(2, '0');
                break;
            case StatsManager.Stat.Sorcery:
                Sorcery.text = value.ToString().PadLeft(2, '0');
                break;
        }
    }

    private void StatsManager_OnTurnChanged(int value)
    {
        Turn.text = value.ToString();
    }

    public void SetTimer(int seconds)
    {
        _isTimerActive = true;
        _timerCount = seconds;
    }

    public void ResetTimer()
    {
        _isTimerActive = false;
        Timer.text = "-";
    }

    public Routine HandleTimer()
    {
        Timer.text = "-";
        while (isActiveAndEnabled)
        {
            while (!_isTimerActive)
            {
                yield return TimeYields.WaitOneFrameX;
            }

            var count = _timerCount;
            do
            {
                Timer.text = count.ToString();
                yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 1);
                count--;
            } while (count > 0 && _isTimerActive);

            _isTimerActive = false;
            Timer.text = "-";
        }
    }
}
