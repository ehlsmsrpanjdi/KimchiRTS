using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BuildingBase : NetworkBehaviour, ITakeDamage, IPoolObj
{
    [Header("Building Info")]
    public int sizeX = 1;
    public int sizeY = 1;

    [Header("Grid Reference")]
    public GridArea grid;
    public Vector2Int gridPosition;

    [Header("Navigation")]
    public bool blockNavigation = true; // 네비게이션 차단 여부
    public float carveHeight = 2f; // Carving 높이
    [Range(0.8f, 1.0f)]
    public float carveSizeMultiplier = 0.95f; // Carving 크기 조절 (그리드보다 약간 작게)

    private MeshRenderer meshRenderer;
    private NavMeshObstacle navMeshObstacle;

    [SerializeField] HealthBar healthBar;

    public NetworkVariable<ulong> BuildingOwnerId = new NetworkVariable<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> currentHP = new NetworkVariable<float>(
    100,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);

    public NetworkVariable<float> maxHP = new NetworkVariable<float>(
100,
NetworkVariableReadPermission.Everyone,
NetworkVariableWritePermission.Server
);

    private void Reset()
    {
        NetworkObject net = GetComponent<NetworkObject>();
        if (net == null)
        {
            transform.AddComponent<NetworkObject>();
        }
        healthBar = GetComponentInChildren<HealthBar>();
    }

    protected virtual void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        healthBar = GetComponentInChildren<HealthBar>();
    }

    protected virtual void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        //LogHelper.Log($"{buildingName} initialized at grid position: {gridPosition}");

        grid = GridArea.Instance;
        // 색상을 원래대로 복원
        if (meshRenderer != null)
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetColor("_Color", Color.white);
            meshRenderer.SetPropertyBlock(props);
        }

        // NavMesh 설정
        if (blockNavigation)
        {
            SetupNavMeshObstacle();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 여기서 등록! (NetworkVariable 준비된 후)
        currentHP.OnValueChanged += OnHealthChanged;

        // 초기 체력바 설정
        if (healthBar != null)
        {
            healthBar.UpdateHealthPercent(currentHP.Value / maxHP.Value);
        }
    }

    public void SetGridInfo(Vector2Int gridPos, int width, int height)
    {
        grid = GridArea.Instance;
        gridPosition = gridPos;
        sizeX = width;
        sizeY = height;
    }

    // NavMeshObstacle 설정
    void SetupNavMeshObstacle()
    {
        if (navMeshObstacle != null)
            Destroy(navMeshObstacle);

        navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
        navMeshObstacle.carving = true;
        navMeshObstacle.carveOnlyStationary = true;
        navMeshObstacle.shape = NavMeshObstacleShape.Box;

        // 그리드 크기 기준으로 정확하게 설정
        if (grid != null)
        {
            // center는 로컬 좌표 (0,0,0)
            navMeshObstacle.center = Vector3.zero;

            navMeshObstacle.size = Vector3.one;
            // size는 그리드 크기에 정확히 맞춤
            //navMeshObstacle.size = new Vector3(
            //    sizeX * grid.cellSize * carveSizeMultiplier,
            //    carveHeight,
            //    sizeY * grid.cellSize * carveSizeMultiplier
            //);

            //Debug.Log($"NavMeshObstacle set to grid size: {navMeshObstacle.size} (Grid: {sizeX}x{sizeY}, CellSize: {grid.cellSize})");
        }
        else
        {
            Debug.LogWarning("Grid is null, using default obstacle size");
            navMeshObstacle.center = Vector3.zero;
            navMeshObstacle.size = new Vector3(1, carveHeight, 1);
        }
    }

    //// 네비게이션 차단 활성화/비활성화
    //public void SetNavigationBlocking(bool block)
    //{
    //    blockNavigation = block;

    //    if (block && navMeshObstacle == null)
    //    {
    //        SetupNavMeshObstacle();
    //    }
    //    else if (!block && navMeshObstacle != null)
    //    {
    //        RemoveNavMeshObstacle();
    //    }
    //}

    // NavMeshObstacle 제거
    void RemoveNavMeshObstacle()
    {
        if (navMeshObstacle != null)
        {
            Destroy(navMeshObstacle);
            navMeshObstacle = null;
        }
    }

    // 빌딩 제거
    public void RemoveBuilding()
    {
        if (grid != null)
        {
            grid.RemoveBuilding(gridPosition.x, gridPosition.y, sizeX, sizeY);
        }

        RemoveNavMeshObstacle();

        // NavMeshObstacle도 함께 제거됨
        PoolManager.Instance.Push(gameObject);
    }




    protected new void OnDestroy()
    {
        LogHelper.LogWarrning("이거 호출되면 안되는디");
        // 오브젝트가 파괴될 때 그리드에서도 제거
        if (grid != null)
        {
            grid.RemoveBuilding(gridPosition.x, gridPosition.y, sizeX, sizeY);
        }

        // NavMeshObstacle 정리
        RemoveNavMeshObstacle();
    }

    // 디버그용 Gizmo
    private void OnDrawGizmosSelected()
    {
        if (grid == null) return;

        // 차지하는 그리드 영역
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(
            sizeX * grid.cellSize,
            0.2f,
            sizeY * grid.cellSize
        );
        Gizmos.DrawWireCube(transform.position, size);

        // NavMesh Carving 영역 (실제 obstacle 크기)
        if (blockNavigation && navMeshObstacle != null)
        {
            Gizmos.color = Color.red;
            Vector3 navSize = navMeshObstacle.size;
            Vector3 navCenter = transform.TransformPoint(navMeshObstacle.center);
            Gizmos.DrawWireCube(navCenter, navSize);
        }
        else if (blockNavigation)
        {
            // obstacle이 아직 생성 안됐을 때 예상 크기
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Vector3 navSize = new Vector3(
                sizeX * grid.cellSize * carveSizeMultiplier,
                carveHeight,
                sizeY * grid.cellSize * carveSizeMultiplier
            );
            Gizmos.DrawWireCube(transform.position, navSize);
        }

        // 차지하는 그리드 칸 표시
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Vector3 cellCenter = grid.GridToWorld(gridPosition.x + x, gridPosition.y + y);
                Vector3 cellSize = new Vector3(grid.cellSize * 0.9f, 0.1f, grid.cellSize * 0.9f);
                Gizmos.DrawCube(cellCenter, cellSize);
            }
        }
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthPercent(newValue / maxHP.Value);
        }
        if(newValue <= 0)
        {
            RemoveBuilding();
        }
    }


    public virtual bool TakeDamage(float _Amount)
    {
        if (!IsServer) return false;  // 서버에서만 처리

        currentHP.Value -= _Amount;


        if (currentHP.Value < 0)
        {
            currentHP.Value = 0;
            return true;
        }

        else
        {
            return false;
        }
    }

    public virtual bool HealHP(float _Amount)
    {
        if (!IsServer) return false;  // 서버에서만 처리

        currentHP.Value += _Amount;

        if (currentHP.Value > maxHP.Value)
        {
            currentHP.Value = maxHP.Value;
        }

        return true;
    }

    public virtual void OnPush()
    {
        RemoveBuilding();
    }

    public virtual void OnPop()
    {
    }
}