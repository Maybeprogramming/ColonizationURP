using UnityEngine;
using UnityEngine.Serialization;

public class ToolsProvider : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("_hummer")] private Hammer _hammer;

    private void OnEnable()
    {
        Disable();
    }

    [ContextMenu("Enable")]
    public void Enable()
    {
        _hammer.gameObject.SetActive(true);
    }

    [ContextMenu("Disable")]
    public void Disable()
    {
        _hammer.gameObject.SetActive(false);
    }
}
