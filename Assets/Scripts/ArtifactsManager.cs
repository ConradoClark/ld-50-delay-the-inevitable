using System;
using System.Collections.Generic;
using System.Linq;
using Licht.Impl.Orchestration;
using UnityEditor;
using UnityEngine;
using Routine = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Action>>;

public class ArtifactsManager : MonoBehaviour
{
    public enum ArtifactEnum
    {
        InnerFaith,
        ChalkOfShadows,
        Solemnity
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

    public Routine Reset()
    {
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

    private void StatsManager_OnStatChanged(StatsManager.Stat stat, int value)
    {
        Toolbox.Instance.MainMachinery.AddBasicMachine(99, HandleLearning());
    }

    private Routine HandleLearning()
    {
        return from learningCondition in LearnTable where !CurrentArtifacts.Contains(learningCondition.Key) let shouldLearn = learningCondition.Requirements.All(r =>
            Toolbox.Instance.StatsManager.Stats.ContainsKey(r.Stat) &&
            Toolbox.Instance.StatsManager.Stats[r.Stat] >= r.Amount) where shouldLearn select AddArtifact(learningCondition.Key).AsCoroutine();
    }

    // should this be public?
    public Routine AddArtifact(ArtifactEnum artifact)
    {
        var artDefinition = ArtifactDefinitions.FirstOrDefault(art => art.Key == artifact);
        if (artDefinition==null) throw new Exception($"Artifact definition not found: {artifact}"); // just in case I forget to update the definitions

        // no problem instantiating them directly here, I guess...
        var obj = Instantiate(artDefinition.Artifact, transform);
        obj.transform.localPosition = new Vector3((CurrentArtifacts.Count % 5) * -0.5f,
            Mathf.FloorToInt(CurrentArtifacts.Count / 5f)*-0.5f, 0);
        // TODO: maybe some effects later

        CurrentArtifacts.Add(artifact);
        ArtifactReferences.Add(obj.GetComponent<Artifact>());
        yield break;
    }
}

public delegate void OnArtifactListChangedEvent();
