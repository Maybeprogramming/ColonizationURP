using UnityEngine;

public class FlagVisualProvider
{
    private const float PreviewScale = 0.3f;
    private const string BuildingMeshPath = "BuildingBarn/MeshBuildingBarn";
    private const string BuildingMaterialPath = "BaseModel/BasePreview";

    private readonly GameObject _prefab;
    private GameObject _instance;

    public FlagVisualProvider(GameObject prefab)
    {
        _prefab = prefab;
    }

    public void Show(Vector3 position)
    {
        EnsureInstance();
        _instance.transform.position = position;
        _instance.SetActive(true);
    }

    public void Hide()
    {
        if (_instance != null)
            _instance.SetActive(false);
    }

    public void Destroy()
    {
        if (_instance != null)
        {
            Object.Destroy(_instance);
            _instance = null;
        }
    }

    private void EnsureInstance()
    {
        if (_instance != null)
            return;

        if (_prefab != null)
        {
            _instance = Object.Instantiate(_prefab);
        }
        else
        {
            _instance = new GameObject("BuildingPreview");
            _instance.transform.localScale = Vector3.one * PreviewScale;

            MeshFilter meshFilter = _instance.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = Resources.Load<Mesh>(BuildingMeshPath);

            MeshRenderer meshRenderer = _instance.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>(BuildingMaterialPath);
        }

        _instance.SetActive(false);
    }
}
