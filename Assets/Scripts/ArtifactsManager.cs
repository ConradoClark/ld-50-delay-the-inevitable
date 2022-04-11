using System;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using TMPro;
using UnityEditor;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class ArtifactsManager : MonoBehaviour
{
    public enum ArtifactEnum
    {
        InnerFaith,
        ChalkOfShadows,
        Solemnity,
        ArtDimension,
        Meditation
    }

    [Serializable]
    public class ArtifactDefinition
    {
        public ArtifactEnum Key;
        public GameObject Artifact;
    }

    [Serializable]
    public class ArtifactLearningRequirements
    {
        public ArtifactEnum Key;
        public Card.StatQuantifier[] Requirements;
    }

    public List<ArtifactEnum> StartingArtifacts;
    public List<ArtifactEnum> CurrentArtifacts { get; private set; }
    public List<ArtifactDefinition> ArtifactDefinitions;
    public List<Artifact> ArtifactReferences { get; private set; } // clumsy but ok

    public event OnArtifactListChangedEvent OnArtifactListChanged;

    public List<ArtifactLearningRequirements> LearnTable; // shouldn't be written anywhere I think. Make the player figure out.

    public TMP_Text ArtifactGainedText;

    public Routine Reset()
    {
        foreach (var art in ArtifactReferences ?? new List<Artifact>())
        {
            Destroy(art.gameObject);
        }

        CurrentArtifacts = new List<ArtifactEnum>();
        ArtifactReferences = new List<Artifact>();
        foreach (var artifact in StartingArtifacts)
        {
            yield return AddArtifact(artifact).AsCoroutine();
        }
        OnArtifactListChanged?.Invoke();
    }

    void OnEnable()
    {
        Toolbox.Instance.StatsManager.OnStatChanged += StatsManager_OnStatChanged;
    }

    public void ShowArtifacts()
    {
        foreach (var art in ArtifactReferences)
        {
            art.gameObject.SetActive(true);
        }
    }

    public void HideArtifacts()
    {
        foreach (var art in ArtifactReferences)
        {
            art.gameObject.SetActive(false);
        }
    }

    private void StatsManager_OnStatChanged(StatsManager.Stat stat, int value)
    {
        Toolbox.Instance.Machinery().AddBasicMachine(HandleLearning());
    }

    private Routine HandleLearning()
    {
        return from learningCondition in LearnTable where !CurrentArtifacts.Contains(learningCondition.Key) let shouldLearn = learningCondition.Requirements.All(r =>
            Toolbox.Instance.StatsManager.Stats.ContainsKey(r.Stat) &&
            Toolbox.Instance.StatsManager.Stats[r.Stat] >= r.Amount) where shouldLearn select AddArtifact(learningCondition.Key, true).AsCoroutine();
    }

    // should this be public?
    public Routine AddArtifact(ArtifactEnum artifact, bool learned=false)
    {
        var artDefinition = ArtifactDefinitions.FirstOrDefault(art => art.Key == artifact);
        if (artDefinition==null) throw new Exception($"Artifact definition not found: {artifact}"); // just in case I forget to update the definitions

        // no problem instantiating them directly here, I guess...
        var obj = Instantiate(artDefinition.Artifact, transform);
        obj.transform.localPosition = new Vector3((CurrentArtifacts.Count % 5) * -0.5f,
            Mathf.FloorToInt(CurrentArtifacts.Count / 5f)*-0.5f, 0);

        var artifactComponent = obj.GetComponent<Artifact>();
        if (learned)
        {
            Toolbox.Instance.Machinery().AddBasicMachine( FlashArtifactText(artifactComponent.Name));
        }

        CurrentArtifacts.Add(artifact);
        ArtifactReferences.Add(artifactComponent);
        yield break;
    }


    private bool _flashingArtifactText;
    private Routine FlashArtifactText(string artifact)
    {
        while (_flashingArtifactText) yield return TimeYields.WaitOneFrameX;

        var text = $"Gained Artifact: {artifact}";
        _flashingArtifactText = true;
        ArtifactGainedText.enabled = true;
        ArtifactGainedText.text = text;

        for (var i = 0; i < 15; i++)
        {
            yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 0.06);
            ArtifactGainedText.text = "";
            yield return TimeYields.WaitSeconds(Toolbox.Instance.MainTimer, 0.06);
            ArtifactGainedText.text = text;
        }

        ArtifactGainedText.enabled = false;
        _flashingArtifactText = false;
    }
}

public delegate void OnArtifactListChangedEvent();
