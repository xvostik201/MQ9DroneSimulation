using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Terrain deformer")]
    [SerializeField] private float _radius = 15f;
    [SerializeField] private float _depth = 9f;

    [Header("Damage settings")]
    [SerializeField] private float _damage = 100f;
    [SerializeField, Range(0f, 1f)] private float _minDamageMultiplier = 0.5f;

    [Header("Explosion force")]
    [SerializeField] private float _explosionForce = 500f;
    [SerializeField] private float _explosionUpModifier = 0.5f;
    [SerializeField] private ForceMode _forceMode = ForceMode.Impulse;

    [Header("Explosion FX")]
    [SerializeField] private GameObject _particlePrefab;

    [Header("NavMesh obstalce")]
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private Vector3 _obstacleOffset = new Vector3(0, -1, 0);

    private Rigidbody _rb;
    private bool _isFirstHit = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (_rb.velocity.sqrMagnitude > 0.01f)
        {
            transform.forward = _rb.velocity.normalized;
        }
    }

    public void Shoot(float force)
    {
        _rb.AddForce(transform.forward * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isFirstHit) return;
        _isFirstHit = false;

        Vector3 hitPoint = collision.contacts[0].point;

        Instantiate(_particlePrefab, hitPoint, Quaternion.identity);

        ApplyFragmentationDamage(hitPoint);
        ApplyExplosionForce(hitPoint);

        Terrain terrain = collision.gameObject.GetComponent<Terrain>();

        TerrainDeformer.CreateCrater(terrain, hitPoint, _radius, _depth, 1);

        Instantiate(_obstaclePrefab, hitPoint + _obstacleOffset, Quaternion.identity);

        Destroy(gameObject);
    }


    private void ApplyFragmentationDamage(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, _radius);
        foreach (var col in colliders)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                float distance = Vector3.Distance(center, col.transform.position);
                float distance01 = Mathf.Clamp01(distance / _radius);

                float damageMultiplier = Mathf.Lerp(1f, _minDamageMultiplier, distance01);
                float finalDamage = _damage * damageMultiplier;

                damageable.TakeDamage(finalDamage);
            }
        }
    }

    private void ApplyExplosionForce(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, _radius);
        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && !rb.isKinematic && rb.GetComponent<Bullet>() == null) 
            {
                rb.AddExplosionForce(_explosionForce, center, _radius, _explosionUpModifier, _forceMode);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, _radius);
    }
#endif
}
