using TMPro;
using UnityEngine;

public class ResourceCounterView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    private void Awake() =>
        _label ??= GetComponent<TextMeshProUGUI>();

    public void CountUpdateHandler(int count)
    {
        _label.text = $"- {count} -";
    }
}