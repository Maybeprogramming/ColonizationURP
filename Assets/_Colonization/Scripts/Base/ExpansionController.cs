using System;
using UnityEngine;

public class ExpansionController
{
    public event Action NewBaseBuilt;

    public Vector3 FlagPosition { get; set; }

    public bool HasConstructNewBase { get; private set; }

    public void AssignFlag(Vector3 position)
    {
        FlagPosition = position;
        HasConstructNewBase = true;
    }

    public void Cancel()
    {
        HasConstructNewBase = false;
    }

    public void ClearFlag()
    {
        HasConstructNewBase = false;
        NewBaseBuilt?.Invoke();
    }
}
