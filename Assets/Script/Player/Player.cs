using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    PlayerInput input;
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] public PlayerResource playerResource;

    public Camera cam;             // RTS 카메라
    public LayerMask groundMask;   // 땅 레이어
    public PlayerMover mover;      // 움직일 유닛

    private InputAction RClick;

    public ulong playerID;

    private void Reset()
    {
        inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        input = GetComponent<PlayerInput>();
        mover = GetComponent<PlayerMover>();
        cam = FindAnyObjectByType<Camera>();
        groundMask = LayerHelper.Instance.GetLayerToInt(LayerHelper.GridLayer);
        playerResource = GetComponent<PlayerResource>();
    }

    public override void OnNetworkSpawn()
    {
        GameInstance.Instance.AddPlayer(OwnerClientId, this);
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
        }
    }

    private void Awake()
    {
        if (cam == null)
        {
            cam = FindAnyObjectByType<Camera>();
        }
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
}
