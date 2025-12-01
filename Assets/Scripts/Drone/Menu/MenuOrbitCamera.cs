using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOrbitCamera : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private Transform _cam;
    [SerializeField] private Transform _target;
    private void Update()
    {
        transform.Rotate(Vector3.up * _speed * Time.deltaTime, Space.World);  
        _cam.LookAt(_target);
    }
}
