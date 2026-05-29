using System;
using UnityEngine;

public class ResourceCounter : MonoBehaviour
{
    [SerializeField] private int _count;

    public event Action<int> Added;

    private void Start()
    {
        _count = 0;
    }

    private void OnAdded(int count) =>
        Added?.Invoke(count);

    public void ResourceAddedHandler(Resource _)
    {
        _count++;

        OnAdded(_count);
    }
}