using UnityEngine;

public interface IBot : IMovable, IGatherer, IConstructor
{
    bool IsBusy { get; }
    IBase OwnerBase { get; }
    Vector3 OwnerBasePosition { get; }
    void SwitchBase(IBase newBase);
}
