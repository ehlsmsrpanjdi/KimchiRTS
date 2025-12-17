using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    [Header("Grid")]
    public GridArea grid;
    public LayerMask groundLayer;

    [Header("Building Size")]
    public int buildingSizeX = 1; // 가로 칸 수
    public int buildingSizeY = 1; // 세로 칸 수

    [Header("Visual")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private MeshRenderer meshRenderer;
    private Vector2Int currentGridPos;
    private bool isValidPlacement;

    public int price = 10;

    ulong playerID;
    Player ownerPlayer;

    [SerializeField] string BuildingName;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Reset()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = transform.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;

        }
    }

    private void Start()
    {
        playerID = GameInstance.Instance.GetPlayerID();
        ownerPlayer = GameInstance.Instance.GetPlayer(playerID);
    }

    void Update()
    {
        FollowMouse();
        UpdateVisual();

        colObjs.RemoveAll(obj => obj == null || !obj.activeSelf);


        // 클릭으로 배치
        if (Input.GetMouseButtonDown(0) && isValidPlacement)
        {
            PlaceBuilding();
        }

        // ESC로 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    void FollowMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            // 새로운 gridArea 존재 여부 체크
            GridArea newGrid = hit.collider.GetComponent<GridArea>();

            // grid가 아직 null → 감지된 grid를 세팅
            if (grid == null && newGrid != null)
            {
                grid = newGrid;
            }
            else if (grid != null && newGrid != grid)
            {
                // 다른 grid로 이동했음 → grid 교체
                grid = newGrid;
            }

            // grid가 없으면 snap 안 함
            if (grid == null)
            {
                transform.position = hit.point;
                isValidPlacement = false;
                return;
            }

            // 마우스 위치(hit.point)를 그리드 좌표로 변환
            Vector3 mouseWorldPos = hit.point;

            // 빌딩 크기의 오프셋 계산 (마우스를 중심에 두기 위해)
            float offsetX = (buildingSizeX - 1) * grid.cellSize * 0.5f;
            float offsetZ = (buildingSizeY - 1) * grid.cellSize * 0.5f;

            // 오프셋을 적용한 위치로 그리드 좌표 계산
            Vector3 adjustedPos = mouseWorldPos - new Vector3(offsetX, 0, offsetZ);
            Vector2Int gridPos = grid.WorldToGrid(adjustedPos);

            // 빌딩 크기를 고려한 유효성 체크
            isValidPlacement = grid.IsAreaAvailable(gridPos.x, gridPos.y, buildingSizeX, buildingSizeY);

            // 그리드 범위 내에 있으면 스냅
            if (gridPos.x >= 0 && gridPos.x < grid.width &&
                gridPos.y >= 0 && gridPos.y < grid.height)
            {
                currentGridPos = gridPos;
                Vector3 snapped = grid.GridToWorldWithSize(gridPos.x, gridPos.y, buildingSizeX, buildingSizeY);
                transform.position = snapped + new Vector3(0, transform.localScale.y / 2, 0);
            }
            else
            {
                // grid 범위 밖이면 마우스 위치 그대로
                transform.position = mouseWorldPos + new Vector3(0, transform.localScale.y / 2, 0);
                isValidPlacement = false;
            }
        }
    }

    void UpdateVisual()
    {
        if (meshRenderer != null)
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            // URP에서는 _BaseColor 사용
            props.SetColor("_BaseColor", isValidPlacement ? validColor : invalidColor);

            float playerDistance = Vector3.Distance(transform.position, ownerPlayer.transform.position);

            if (playerDistance > 10 || colObjs.Count > 0)
            {
                props.SetColor("_BaseColor", invalidColor);
                isValidPlacement = false;
            }

            meshRenderer.SetPropertyBlock(props);
        }
    }

    void PlaceBuilding()
    {
        if (grid == null || !isValidPlacement) return;

        float playerDistance = Vector3.Distance(transform.position, ownerPlayer.transform.position);

        LogHelper.Log(playerDistance.ToString());

        if (playerDistance > 10)
        {
            return;
        }

        if (NetworkManager.Singleton != null)
        {
            Player player = GameInstance.Instance.GetPlayer();

            player.UseResource(price);
        }

        BuildingManager.Instance.PlaceBuildingServerRpc(BuildingName + "Building", transform.position, currentGridPos, buildingSizeX, buildingSizeY, GameInstance.Instance.GetPlayerID());
        // Ghost 오브젝트 제거 (계속 배치하려면 이 줄을 주석 처리)
        Destroy(gameObject);
    }

    void CancelPlacement()
    {
        Debug.Log("Building placement cancelled");
        Destroy(gameObject);
    }


    List<GameObject> colObjs = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if ("Obstacle" == LayerHelper.Instance.GetObjectLayer(other.gameObject))
        {
            colObjs.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ("Obstacle" == LayerHelper.Instance.GetObjectLayer(other.gameObject))
        {
            colObjs.Remove(other.gameObject);
        }
    }

    private void OnDestroy()
    {
        UIManager.Instance.GetUI<BuildSystemUI>().buildingSystemToggleUI.OnToggle();
    }

    // Gizmo로 빌딩이 차지할 영역 표시
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = isValidPlacement ? validColor : invalidColor;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

        Vector3 size = new Vector3(
            buildingSizeX * grid.cellSize,
            0.1f,
            buildingSizeY * grid.cellSize
        );

        Gizmos.DrawCube(transform.position, size);
    }
}