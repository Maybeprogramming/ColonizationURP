using UnityEngine;

public interface IConstructor
{
    bool HasConstructTask { get; set; }
    Vector3 ConstructTargetPosition { get; set; }
}
