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
    public TMP_Text Turn;
    public TMP_Text Timer;
    private int _timerCount;
    private bool _isTimerActive;

    void OnDisable()
    {
        Toolbox.Instance.StatsManager.OnTurnChanged -= StatsManager_OnTurnChanged;
    }

    void OnEnable()
    {
        Toolbox.Instance.MainMachinery.AddBasicMachine(88, HandleTimer());
        Toolbox.Instance.StatsManager.OnTurnChanged += StatsManager_OnTurnChanged;
        // this state event could be generic
        Toolbox.Instance.StatsManager.OnFaithChanged += StatsManager_OnFaithChanged;
        Toolbox.Instance.StatsManager.OnSorceryChanged += StatsManager_OnSorceryChanged;
    }

    private void StatsManager_OnFaithChanged(int value)
    {
        Faith.text = value.ToString().PadLeft(2,'0');
    }

    private void StatsManager_OnSorceryChanged(int value)
    {
        Sorcery.text = value.ToString().PadLeft(2, '0');
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
