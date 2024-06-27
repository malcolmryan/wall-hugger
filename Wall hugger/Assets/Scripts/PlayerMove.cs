using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float gravity = 10;
    [SerializeField] private float speed = 5;

    private Actions actions;
    private InputAction moveAction;
    private InputAction jumpAction;

    new private Rigidbody2D rigidbody;
    private Vector2 gravityDir;

    void Awake()
    {
        actions = new Actions();
        moveAction = actions.movement.move;
        jumpAction = actions.movement.jump;

        rigidbody = GetComponent<Rigidbody2D>();
        gravityDir = (Vector2)(-transform.up).normalized;
    }

    void OnEnable()
    {
        actions.movement.Enable();
    }

    void OnDisable()
    {
        actions.movement.Disable();
    }

    void FixedUpdate()
    {
        rigidbody.AddForce(gravity * gravityDir);
    }

    void Update()
    {
        
    }
}
