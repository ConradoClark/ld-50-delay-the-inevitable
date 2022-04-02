using System.Collections;
using System.Collections.Generic;
using Licht.Impl.Pooling;
using Licht.Interfaces.Pooling;
using UnityEngine;

public class CardPool : MonoBehaviour, IPoolableObjectFactory<Card>
{
    public int PoolSize;
    public GameObject CardPrefab;

    private int _count;
    private ObjectPool<Card> _objectPool;
    void OnEnable()
    {
        if (_objectPool != null) return;
        _objectPool = new ObjectPool<Card>(PoolSize, this);
        _objectPool.Activate();
    }

    Card IPoolableObjectFactory<Card>.Instantiate()
    {
        var obj = Instantiate(CardPrefab, transform);
        _count++;
        obj.name = obj.name.Replace("(Clone)", $"#{_count}"); 
        obj.SetActive(false);
        var card = obj.GetComponent<Card>();
        card.SpriteRenderer.color = Random.ColorHSV(0f, 1,1f,1f,0.5f,1f);
        return card;
    }

    public bool TryGetCard(out Card card)
    {
        return _objectPool.TryGetFromPool(out card);
    }
}
