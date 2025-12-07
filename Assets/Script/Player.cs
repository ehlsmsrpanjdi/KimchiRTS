using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
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
    }

    private void Start()
    {
        RClick = input.actions["RClick"];
    }

    private void Update()
    {
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
