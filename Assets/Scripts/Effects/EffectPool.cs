using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Pooling;
using Licht.Interfaces.Pooling;
using UnityEngine;

public class EffectPool : MonoBehaviour, IPoolableObjectFactory<Effect>
{
    public int PoolSize;
    private ObjectPool<Effect> _pool;
    public GameObject Prefab;

    void OnEnable()
    {
        if (_pool != null) return;
        _pool = new ObjectPool<Effect>(PoolSize, this);
        _pool.Activate();
    }

    Effect IPoolableObjectFactory<Effect>.Instantiate()
    {
        var obj = Instantiate(Prefab, transform);
        var effect = obj.GetComponent<Effect>();
        effect.Deactivate();
        return effect;
    }

    public bool TryGetEffect(out Effect effect)
    {
        return _pool.TryGetFromPool(out effect);
    }
}
