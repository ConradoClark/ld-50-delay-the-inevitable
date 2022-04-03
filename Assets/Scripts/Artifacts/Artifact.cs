using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    // maybe they could have props that are checked by actions?
    // this can be super hardcoded for all I care
    public bool AllowsClapping;
    public bool AllowsDrawing;
    public int ClappingEffectivenessModifier;

    public bool AllowsAction(string action) // this is a bit sketchy
    {
        return action switch
        {
            Constants.InputActions.Clap => AllowsClapping,
            Constants.InputActions.Press => AllowsDrawing,
            _ => false
        };
    }
}
