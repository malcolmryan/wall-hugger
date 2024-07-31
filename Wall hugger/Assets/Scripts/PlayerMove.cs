using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WordsOnPlay.Utils;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float stickyAdhere = 100; // m/s/s
    [SerializeField] private float slimyAdhere = 5; // m/s/s
    [SerializeField] private float gravity = 10; // m/s/s
    [SerializeField] private float speed = 5; // m/s
    [SerializeField] private float jumpBufferTime = 0.1f; // seconds
    [SerializeField] private float coyoteTime = 0.5f; // seconds
    [SerializeField] private float jumpSpeed = 1f; // m/s
    [SerializeField] private float minDownSpeed = 1f; // m/s
    [SerializeField] private Vector2 vAirAdjustment; // m/s
    [SerializeField] private LayerMask stickyLayer;
    [SerializeField] private LayerMask slimyLayer;

    private Actions actions;
    private InputAction moveAction;
    private InputAction jumpAction;

    new private Rigidbody2D rigidbody;
    private Vector2 gravityDir = Vector2.down;
    private Vector2 adhereDir;
    private Vector2 inputDir;
    private Vector2 movementDir;
    private Vector2 velocity;
    private ContactPoint2D[] tempContacts = new ContactPoint2D[4];
    private List<ContactPoint2D> contacts = new List<ContactPoint2D>();
    private ContactPoint2D? activeContact = null;
    private ContactPoint2D? lastContact = null;
    
    private float lastJumpTime = float.NegativeInfinity;
    private float lastContactTime = float.NegativeInfinity;

    private bool OnGround {
        get 
        {
            return Time.time - lastContactTime < coyoteTime;
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

        if (OnGround)
        {
            Vector2 vMove = speed * movementScale * movementDir;

            // do not change the component of movement in the gravity direction
            Vector2 oldV = rigidbody.velocity;
            float vDotG = Vector2.Dot(oldV, adhereDir);
            Vector2 vDown = vDotG * adhereDir;

            // jump
            if (OnGround && Time.time - lastJumpTime < jumpBufferTime)
            {
                // jump 'upwards'
                vDown = -jumpSpeed * adhereDir;
                lastJumpTime = float.NegativeInfinity;
                lastContactTime = float.NegativeInfinity;
            }

            velocity = vDown + vMove;
            rigidbody.velocity = velocity;
            bool isSlimy = slimyLayer.Contains(lastContact.Value.collider.gameObject);
            float adhere = isSlimy ? slimyAdhere : stickyAdhere;    
            rigidbody.AddForce(stickyAdhere * adhereDir);
        }

        // apparently there is not ForceMode2D.Acceleration
        rigidbody.AddForce(gravity * gravityDir * rigidbody.mass);

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
                lastContact = activeContact;
                minDot = vDotN;
            }
        }

        if (activeContact != null)
        {
            // set adhere direction to most recent active contact's 'down' direction
            lastContactTime = Time.time;
            adhereDir = -activeContact.Value.normal;
        }
        else if (lastContact != null) 
        {
            // adhere to the nearest point on the collider we last touched
            Vector2 p = lastContact.Value.collider.ClosestPoint(rigidbody.position);
            adhereDir = (p - rigidbody.position).normalized;
        }

        // movement is always perpendicular to gravity/adhere force
        float vDotG = Vector2.Dot(movementDir, adhereDir);
        movementDir -= vDotG * adhereDir;
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

            if (vDotN <= 0.01) // small epsilon to ignore micro movements
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
