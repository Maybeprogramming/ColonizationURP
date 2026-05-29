using UnityEngine;

[RequireComponent(typeof(ScanAnimation))]
public class ScanAnimator : MonoBehaviour
{
    [SerializeField] private ScanAnimation _animation;
    [SerializeField] private GameObject _animationObject;

    private void Awake() =>    
        _animation = GetComponent<ScanAnimation>();

    public void Run()
    {
        _animationObject.SetActive(true);
        _animation.Run();
    }

    public void Stop()
    {
        _animation.Stop();
        _animationObject.SetActive(false);
    }
}
