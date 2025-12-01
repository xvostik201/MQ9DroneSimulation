using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshMover : MonoBehaviour
{
    [SerializeField] private float _agentSpeed = 1.5f;
    [SerializeField] private float _agentRunSpeed = 2.5f;
    private NavMeshAgent _agent;
    private NavMeshObstacle _obstacle;

    private void Awake()
    {
        _obstacle = GetComponent<NavMeshObstacle>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _agentSpeed;
    }

    public void MoveTo(Vector3 destination)
    {
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        _agent.isStopped = true;
    }

    public void Deactivate()
    {
        if (_agent != null)
        {
            _agent.velocity = Vector3.zero;
            _agent.updateRotation = false;
            _agent.enabled = false;
        }
            _obstacle.enabled = true;
    }

    public void SetRunAgentSpeed()
    {
        _agent.speed = _agentRunSpeed;
    }

    public bool HasPath => _agent.hasPath;

    public float CurrentSpeed => _agent.velocity.magnitude;
}
