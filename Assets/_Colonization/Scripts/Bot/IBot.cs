using UnityEngine;

public interface IBot
{
    Vector3 CurrentBasePosition { get; }
    Resource CurrentResource { get; }

    void GiveResource(Resource resource);
    void SetResourceToMine(Resource resource);
}