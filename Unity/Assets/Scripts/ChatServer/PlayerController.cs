using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotateSpeed = 100f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");        
        float vertical = Input.GetAxis("Vertical");        

        Vector3 moveDirection = transform.forward * vertical;
        transform.position += moveDirection * _moveSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up * horizontal * _rotateSpeed * Time.deltaTime);
    }
}
