using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(Health))]
public abstract class Unit : MonoBehaviour, IDamageable
{
    [Header("Ragdoll")]
    [SerializeField] private Rigidbody[] _ragdollBones;
    [SerializeField] private Rigidbody _mainRb;
    [SerializeField] private Collider _mainCollider;
    [Header("Impact Death")]
    [SerializeField] private float _impactDeathSpeed = 10f;
    [Header("Movement")]
    [SerializeField] private Transform _target;
    [SerializeField] private NavMeshMover _mover;
    [Header("Flee Settings")]
    [SerializeField] private float _fleeRadius = 10f;

    private Vector3 _fleePoint;
    public Vector3 FleePoint => _fleePoint;
    private bool _isFleeing;
    private Animator _anim;
    private Health _health;
    private bool _isDead;

    protected virtual void Awake()
    {
        _anim = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _health.OnDeath += Die;
        DisableRagdoll();
        _isFleeing = false;
    }

    private void Update()
    {
        if (_isDead) return;
        if (_target == null) return;
        if (_isFleeing)
        {
            _mover.MoveTo(_fleePoint);
        }
        else
        {
            _mover.MoveTo(_target.position);
        }
        float speed = _mover.CurrentSpeed;
        _anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    public virtual void TakeDamage(float amount)
    {
        if (_isDead) return;
        _health.TakeDamage(amount);

        if (!_isDead)
        {
            Vector2 randCircle = Random.insideUnitCircle.normalized * _fleeRadius;
            Vector3 randDir = new Vector3(randCircle.x, 0f, randCircle.y);

            Vector3 candidate = transform.position + randDir;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, _fleeRadius, NavMesh.AllAreas))
                _fleePoint = hit.position;
            else
                _fleePoint = transform.position;

            _isFleeing = true;
            _mover.SetRunAgentSpeed();
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (_isDead) return;
        if (collision.relativeVelocity.magnitude >= _impactDeathSpeed)
            Die();
    }

    protected virtual void Die()
    {
        _anim.enabled = false;
        _mover.Deactivate();
        _mainRb.isKinematic = false;
        _isDead = true;
        EnableRagdoll();
        _mainRb.isKinematic = true;
        _mainCollider.isTrigger = true;
        Destroy(this);
    }

    private void EnableRagdoll()
    {
        foreach (var rb in _ragdollBones)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }

    private void DisableRagdoll()
    {
        foreach (var rb in _ragdollBones)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }
}
