using UnityEngine;
using UnityEngine.AI;

public class PlayerMover : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
}
