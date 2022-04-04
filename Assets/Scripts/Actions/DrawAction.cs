using System;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using Licht.Impl.Pooling;
using Licht.Interfaces.Pooling;
using TMPro;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class DrawAction : DefaultAction, IPoolableObjectFactory<Ink>
{
    private Drawing _drawing;
    private Dictionary<GameObject, ObjectPool<Ink>> _inkPool;

    private bool _isActionAllowed;
    private GameObject _ink;

    public TMP_Text StrokesText;
    public void Activate(GameObject drawing, GameObject ink, int timeLimit)
    {
        _ink = ink;
        _inkPool ??= new Dictionary<GameObject, ObjectPool<Ink>>();
        if (!_inkPool.ContainsKey(ink))
        {
            _inkPool[ink] = new ObjectPool<Ink>(700, this);
            _inkPool[ink].Activate();
        }

        var obj = Instantiate(drawing, transform);
        _drawing = obj.GetComponent<Drawing>();
        if (_drawing == null) throw new Exception($"Object is not a drawing: {obj.name}");

        _isActionAllowed =
            Toolbox.Instance.ArtifactsManager.ArtifactReferences.Any(art => art.AllowsAction(Constants.InputActions.Press));

        var timeLimitOverride =
            Toolbox.Instance.ArtifactsManager.ArtifactReferences.Select(art => art.DrawingTimeLimitOverride).DefaultIfEmpty(0).Max();

        base.ActivateDefaults(Math.Max(timeLimit, timeLimitOverride));
        Toolbox.Instance.MainMachinery.AddBasicMachine(56, HandleAction());

        StrokesText.text = $"{_drawing.MaximumStrokes} stroke(s) left.";
    }

    private Routine HandleAction()
    {
        var spots = new List<Ink>();
        var action = Toolbox.Instance.MainInput.actions[Constants.InputActions.Press];
        var mousePosValue = Toolbox.Instance.MainInput.actions[Constants.InputActions.MousePosition];
        var strokes = 0;
        while (Result == null && isActiveAndEnabled)
        {
            var noSpots = 0;
            var pressed = false;
            Vector3? previousInkPosition = null;
            // started holding
            while (_isActionAllowed && action.IsPressed())
            {
                pressed = true;
                // begin creating ink objects
                // validate colliders

                var mousePos = Toolbox.Instance.MainCamera.ScreenToWorldPoint(mousePosValue.ReadValue<Vector2>());

                if ((previousInkPosition == null
                    || Vector2.Distance(mousePos, previousInkPosition.Value) > 0.03f)
                    )
                {
                    var amountOfInk = previousInkPosition == null ? 1 :
                        Mathf.CeilToInt(Vector2.Distance(mousePos, previousInkPosition.Value) / 0.1f);

                    var pos = new Vector3(mousePos.x, mousePos.y, 0);

                    for (int i = 0; i < amountOfInk; i++)
                    {
                        if (!_inkPool[_ink].TryGetFromPool(out var ink)) continue;
                        var tempPos = Vector3.Lerp(previousInkPosition ?? pos, pos, i / (float)amountOfInk);
                        ink.transform.position = tempPos;
                        spots.Add(ink);
                        noSpots++;
                    }
                    previousInkPosition = pos;
                }

                // Should this have a "grace" period? Should it have feedback?
                if (!_drawing.Overlaps(mousePos))
                {
                    _inkPool[_ink].ReleaseAll(); // get rid of all ink (this should be in one place honestly)
                    Result = false;
                    goto end;
                }

                yield return TimeYields.WaitOneFrameX;
            }

            if (pressed)
            {
                strokes++;
                Debug.Log("number of spots: " + noSpots);
                StrokesText.text = $"{_drawing.MaximumStrokes - strokes} stroke(s) left.";
                // now, how do I test if the drawing is complete...
                if (_drawing.IsComplete(spots))
                {
                    _inkPool[_ink].ReleaseAll(); // get rid of all ink
                    Result = true;
                    break;
                }
            }

            // no more chances
            if (strokes >= _drawing.MaximumStrokes || TimeLimitExpired)
            {
                _inkPool[_ink].ReleaseAll(); // get rid of all ink
                Result = false;
                break;
            }

            yield return TimeYields.WaitOneFrameX;
        }

        end:
        Destroy(_drawing.gameObject); // maybe i should have a pool 
        yield return base.HandleActionEnd().AsCoroutine();
    }

    public Ink Instantiate()
    {
        var ink = Instantiate(_ink, transform);
        return ink.GetComponent<Ink>();
    }
}
