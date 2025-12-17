using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour, ITakeDamage
{
    PlayerInput input;
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] public PlayerResource playerResource;

    public Camera cam;             // RTS 카메라
    public LayerMask groundMask;   // 땅 레이어
    public PlayerMover mover;      // 움직일 유닛

    private InputAction RClick;

    public ulong playerID;

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

    [SerializeField] HealthBar healthBar;

    private void Reset()
    {
        inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        input = GetComponent<PlayerInput>();
        mover = GetComponent<PlayerMover>();
        groundMask = LayerHelper.Instance.GetLayerToInt(LayerHelper.GridLayer);
        playerResource = GetComponent<PlayerResource>();
        healthBar = GetComponentInChildren<HealthBar>();
    }

    public override void OnNetworkSpawn()
    {
        GameInstance.Instance.AddPlayer(OwnerClientId, this);
        currentHP.OnValueChanged += OnHealthChanged;
        if (healthBar != null)
        {
            healthBar.UpdateHealthPercent(currentHP.Value / maxHP.Value);
        }

        if (IsOwner)
        {
            GameInstance.Instance.SetPlayerID(OwnerClientId);

            input = transform.AddComponent<PlayerInput>();
            input.actions = inputActions;
            input.actions.FindActionMap("RTS");
            input.neverAutoSwitchControlSchemes = true;      // 권장
            input.notificationBehavior =
                PlayerNotifications.InvokeUnityEvents;        // 권장

            UIManager.Instance.NetWorkBinding();

            if (cam == null)
            {
                cam = FollowCamera.Instance.GetComponent<Camera>();
            }
        }
    }

    private void Awake()
    {

        PlayerManager.Instance.AddPlayer(this);
    }

    private void Start()
    {

        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.red;
            RClick = input.actions["RClick"];
        }

        if (!IsOwner)
        {
            mover.GetComponent<NavMeshAgent>().enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (RClick.WasPerformedThisFrame())
        {
            TryMoveToMouse();
        }
    }

    private void TryMoveToMouse()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, groundMask))
        {
            mover.MoveTo(hit.point);
        }
    }

    public bool UseResource(int res)
    {
        return playerResource.TrySpendResource(res);
    }

    private void OnDisable()
    {
        PlayerManager.Instance.RemovePlayer(this);
        GameInstance.Instance.RemovePlayer(OwnerClientId);
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthPercent(newValue / maxHP.Value);
        }
        if (newValue <= 0)
        {
            Destroy(gameObject);
        }
    }

    public bool TakeDamage(float _Amount)
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

    public bool HealHP(float _Amount)
    {
        if (!IsServer) return false;  // 서버에서만 처리

        currentHP.Value += _Amount;

        if (currentHP.Value > maxHP.Value)
        {
            currentHP.Value = maxHP.Value;
        }

        return true;
    }
}
