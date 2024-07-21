using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float adhere = 20; // m/s/s
    [SerializeField] private float gravity = 10; // m/s/s
    [SerializeField] private float speed = 5; // m/s
    [SerializeField] private float jumpBufferTime = 0.1f; // seconds
    [SerializeField] private float coyoteTime = 0.5f; // seconds
    [SerializeField] private float jumpSpeed = 1f; // m/s
    [SerializeField] private float minDownSpeed = 1f; // m/s

    private Actions actions;
    private InputAction moveAction;
    private InputAction jumpAction;

    new private Rigidbody2D rigidbody;
    private Vector2 gravityDir = Vector2.down;
    private Vector2 adhereDir;
    private Vector2 inputDir;
    private Vector2 movementDir;
    private ContactPoint2D[] tempContacts = new ContactPoint2D[4];
    private List<ContactPoint2D> contacts = new List<ContactPoint2D>();
    private ContactPoint2D? activeContact = null;

    private float lastJumpTime = float.NegativeInfinity;
    private float lastContactTime = float.NegativeInfinity;

    private bool OnGround {
        get 
        {
            return Time.time - lastContactTime < coyoteTime;
        }
    }

    private Vector2 ForceDir {
        get
        {
            float t = Mathf.Clamp01((Time.time - lastContactTime) / coyoteTime);
            return Vector2.Lerp(adhereDir, gravityDir, t).normalized;
        }
    }

    void Awake()
    {
        actions = new Actions();
        moveAction = actions.movement.move;
        jumpAction = actions.movement.jump;

        rigidbody = GetComponent<Rigidbody2D>();
        adhereDir = (Vector2)(-transform.up).normalized;

        Debug.Log("Awake");
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
        float vDotG = Vector2.Dot(oldV, ForceDir);
        Vector2 vDown = vDotG * ForceDir;

        // jump
        if (OnGround && Time.time - lastJumpTime < jumpBufferTime)
        {
            // jump 'upwards'
            vDown = -jumpSpeed * adhereDir;
            lastJumpTime = float.NegativeInfinity;
            lastContactTime = float.NegativeInfinity;
        }


        rigidbody.velocity = vDown + vMove;

        rigidbody.AddForce(adhere * ForceDir);

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
        float minDot = float.PositiveInfinity;
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

        if (activeContact != null)
        {
            // set adhere direction to most recent active contact's 'down' direction
            lastContactTime = Time.time;
            adhereDir = -activeContact.Value.normal;
        }

        // movement is always perpendicular to gravity/adhere force
        float vDotG = Vector2.Dot(movementDir, ForceDir);
        movementDir -= vDotG * ForceDir;
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
            // often a contact will be recorded for a frame while we are
            // moving away from the surface, so 
            // ignore contacts that we are already moving away from
            Vector2 v = rigidbody.velocity;
            float vDotN = Vector2.Dot(v, tempContacts[i].normal);

            if (vDotN <= 0)
            {
                contacts.Add(tempContacts[i]);
            }
        }        
    }
#endregion Collisions

#region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = (OnGround ? Color.magenta : Color.black);
        Gizmos.DrawSphere(transform.position, 0.1f);

        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, transform.position + (Vector3)inputDir);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)movementDir);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)adhereDir);

        Gizmos.color = Color.green;
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
