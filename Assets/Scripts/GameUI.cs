using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class GameUI : MonoBehaviour
{
    public TMP_Text Timer;
    private int _timerCount;
    private bool _isTimerActive;
    void OnEnable()
    {
        Toolbox.Instance.MainMachinery.AddBasicMachine(88, HandleTimer());
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
