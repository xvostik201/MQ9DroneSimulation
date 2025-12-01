using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
public class EnemyTank : MonoBehaviour, IDamageable
{
    [Header("Turret Settings")]
    [SerializeField] private Transform _turretTransform;
    [SerializeField] private Rigidbody _turretRigidbody;
    [SerializeField] private float _detachForce = 200f;

    [Header("Death")] 
    [SerializeField] private GameObject _destroyedTankPrefab;
    [SerializeField] private ParticleSystem _flameParticle;

    [SerializeField] private Transform _target;

    [SerializeField] private NavMeshMover _mover;

    private Health _health;
    private Rigidbody _bodyRb;

    private void Awake()
    {
        _bodyRb = GetComponent<Rigidbody>();    
        _health = GetComponent<Health>();
        _health.OnDeath += Die;

        if (_turretRigidbody != null)
            _turretRigidbody.isKinematic = true;
    }

    private void Update()
    {
        
        if (_health.IsDead) return;
        if(_target == null) return;
        _mover.MoveTo(_target.position);
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= Die;
    }

    public void TakeDamage(float amount)
    {
        _health.TakeDamage(amount);
    }

    private void Die()
    {
        _mover.Deactivate();

        _bodyRb.isKinematic = false;

        if (_turretTransform != null && _turretRigidbody != null)
        {
            _turretTransform.parent = null;
            _turretRigidbody.isKinematic = false;

            _turretRigidbody.velocity = Vector3.zero;
            _turretRigidbody.angularVelocity = Vector3.zero;

            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);
            float actualForce = _detachForce * 0.5f;
            _turretRigidbody.AddForce(randomDir * actualForce, ForceMode.VelocityChange);

            _flameParticle.Play();
        }
    }
}
