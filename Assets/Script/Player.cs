using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] PlayerInput input;

    public Camera cam;             // RTS 카메라
    public LayerMask groundMask;   // 땅 레이어
    public PlayerMover mover;      // 움직일 유닛

    private InputAction RClick;

    private void Reset()
    {
        input = GetComponent<PlayerInput>();
        mover = GetComponent<PlayerMover>();
        cam = FindAnyObjectByType<Camera>();
        groundMask = LayerHelper.Instance.GetLayerToInt(LayerHelper.GridLayer);
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
        RClick = input.actions["RClick"];

        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.red;
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
}
