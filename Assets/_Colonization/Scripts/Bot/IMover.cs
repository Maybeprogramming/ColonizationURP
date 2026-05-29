using UnityEngine;

public interface IMover
{
    bool IsMovingComplete();
    void MoveTo(Vector3 target);
    void Stop();
}