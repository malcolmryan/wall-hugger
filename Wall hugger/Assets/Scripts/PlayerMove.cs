using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float gravity = 20; // m/s/s
    [SerializeField] private float speed = 5; // m/s
    [SerializeField] private float jumpBufferTime = 0.1f; // seconds
    [SerializeField] private float coyoteTime = 0.1f; // seconds
    [SerializeField] private float jumpSpeed = 1f; // m/s

    private Actions actions;
    private InputAction moveAction;
    private InputAction jumpAction;

    new private Rigidbody2D rigidbody;
    private Vector2 gravityDir;
    private Vector2 inputDir;
    private Vector2 movementDir;
    private ContactPoint2D[] tempContacts = new ContactPoint2D[4];
    private List<ContactPoint2D> contacts = new List<ContactPoint2D>();
    private ContactPoint2D? activeContact = null;

    private float lastJumpTime = float.NegativeInfinity;
    private float lastContactTime = float.NegativeInfinity;

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
        // movement
        float movementScale = inputDir.magnitude;
        movementDir = inputDir.normalized;
        ResolveContacts();
        Vector2 vMove = speed * movementScale * movementDir;

        // do not change the component of movement in the gravity direction
        Vector2 oldV = rigidbody.velocity;
        float vDotG = Vector2.Dot(oldV, gravityDir);
        Vector2 vDown = vDotG * gravityDir;

        // jump
        if (Time.time - lastContactTime < coyoteTime &&
            Time.time - lastJumpTime < jumpBufferTime)
        {
            // jump 'upwards'
            vDown = -jumpSpeed * gravityDir;
            lastJumpTime = float.NegativeInfinity;
        }
        rigidbody.velocity = vDown + vMove;

        rigidbody.AddForce(gravity * gravityDir);

        // clear contacts
        contacts.Clear();
    }

    private void ResolveContacts()
    {
        // remove downward components of the movement vector
        for (int i = 0; i < contacts.Count; i++)
        {
            Vector2 normal = contacts[i].normal;
            float vDotN = Vector2.Dot(movementDir, normal);

            if (vDotN < 0)
            {
                // movement is pointing 'downwards'
                // remove downward component
                movementDir -= vDotN * normal;
            }
        }        

        // find contact point with closest tangent plane
        float minDot = 1f;
        activeContact = null;

        for (int i = 0; i < contacts.Count; i++)
        {
            Vector2 normal = contacts[i].normal;
            float vDotN = Vector2.Dot(movementDir, normal);

            if (vDotN < minDot)
            {
                activeContact = contacts[i];
                minDot = vDotN;
            }
        }

        // set gravity to most recent active contact's 'down' direction
        if (activeContact != null)
        {
            lastContactTime = Time.time;
            gravityDir = -activeContact.Value.normal;
        }

        // movement is always perpendicular to gravity
        float vDotG = Vector2.Dot(movementDir, gravityDir);
        movementDir -= vDotG * gravityDir;
    }

    void Update()
    {
        inputDir = moveAction.ReadValue<Vector2>();
        bool jumpPressed = jumpAction.WasPerformedThisFrame();

        if (jumpPressed) {
            lastJumpTime = Time.time;
        }
    }

#region Collisions
    void OnCollisionEnter2D(Collision2D collision)
    {
        RecordContacts(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        RecordContacts(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
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
#endregion Collisions

#region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)inputDir);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)movementDir);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)gravityDir);

        for (int i = 0; i < contacts.Count; i++)
        {
            ContactPoint2D c = contacts[i];
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(c.point, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(c.point, c.point + c.normal);
        }

        if (activeContact.HasValue)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(activeContact.Value.point, 0.1f);
        }
    }
#endregion Gizmos

}
