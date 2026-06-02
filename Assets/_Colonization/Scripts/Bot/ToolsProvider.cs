using UnityEngine;

public class ToolsProvider : MonoBehaviour
{
    [SerializeField] private Hummer _hummer;

    private void OnEnable()
    {
        Disable();
    }

    [ContextMenu("Enable")]
    public void Enable()
    {
        _hummer.gameObject.SetActive(true);
    }

    [ContextMenu("Disable")]
    public void Disable()
    {
        _hummer.gameObject.SetActive(false);
    }
}