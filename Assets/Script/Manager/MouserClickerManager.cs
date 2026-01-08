using UnityEngine;

public class BuildingClickController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask buildingLayer;

    public static BuildingClickController Instance;

    public BuildingBase selectedBuilding { get; private set; }

    private void Awake()
    {
        mainCamera = Camera.main;
        Instance = this;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // UI 위 클릭 무시
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            BuildingBase building = hit.collider.GetComponentInParent<BuildingBase>();
            if (building != null)
            {
                selectedBuilding = building;
                building.OnClicked();
            }
        }
        else
        {
            UIManager.Instance.GetUI<BuildingUI>().Hide();
        }
    }
}