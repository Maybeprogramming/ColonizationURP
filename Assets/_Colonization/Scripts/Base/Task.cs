using UnityEngine;

namespace CollectorBots.Scheduler
{
    public class Task
    {
        public Task(Resource resource, Vector3 basePosition)
        {
            Resource = resource;
            BasePosition = basePosition;
        }

        public Resource Resource { get; }
        public Vector3 BasePosition { get; }
        public Vector3 ResourcePosition => Resource.transform.position;
        public float Distance =>  (BasePosition - ResourcePosition).sqrMagnitude;
    }
}
