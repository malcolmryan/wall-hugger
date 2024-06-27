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
    private Vector2 inputDir;
    private ContactPoint2D[] tempContacts = new ContactPoint2D[4];
    private List<ContactPoint2D> contacts = new List<ContactPoint2D>();
    
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
        contacts.Clear();
    }

    void Update()
    {
        inputDir = moveAction.ReadValue<Vector2>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Enter collider = {collision.collider.name}");
        RecordContacts(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log($"Stay collider = {collision.collider.name}");
        RecordContacts(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"Exit collider = {collision.collider.name}");
        RecordContacts(collision);
    }

    private void RecordContacts(Collision2D collision)
    {
        // Note: this is called once per collider, on every physics frame
        // So we need to collect the contacts into a list

        // Resize the array if necessary
        int nContacts = collision.contactCount;
        if (nContacts > tempContacts.Length)
        {
            tempContacts = new ContactPoint2D[nContacts];
        }

        // Get contacts and add them to the list
        collision.GetContacts(tempContacts);
        for (int i = 0; i < nContacts; i++)
        {
            contacts.Add(tempContacts[i]);
        }        
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)inputDir);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)gravityDir);

        Gizmos.color = Color.black;
        for (int i = 0; i < contacts.Count; i++)
        {
            Gizmos.DrawSphere(contacts[i].point, 0.1f);
        }
    }
}
