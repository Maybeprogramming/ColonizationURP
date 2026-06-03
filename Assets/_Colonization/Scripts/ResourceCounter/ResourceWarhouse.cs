using System;
using UnityEngine;

public class ResourceWarhouse : MonoBehaviour
{
    [SerializeField] private int _count;    

    public event Action<int> Changed;
    public event Action<int> Spend;

    private void Start()
    {
        _count = 0;
    }

    private void OnChangeCount(int count) =>
        Changed?.Invoke(count);

    public void ResourceChangedHandler(Resource _)
    {
        _count++;

        OnChangeCount(_count);
    }

    public bool TrySpendResource(int count)
    {
        if (count > 0 && _count >= count)
        {
            _count -= count;

            Spend?.Invoke(count);
            Changed?.Invoke(_count);

            return true;
        }

        return false;
    }    
}