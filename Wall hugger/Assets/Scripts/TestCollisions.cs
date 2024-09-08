using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollisions : MonoBehaviour
{
    private new Rigidbody2D rigidbody;
    private ContactPoint2D[] tempContacts = new ContactPoint2D[4];
    private List<ContactPoint2D> contacts = new List<ContactPoint2D>();

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private float Sign(float x) 
    {
        if (x < 0)
        {
            return -1;
        }
        else if (x > 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }

    void FixedUpdate()
    {
        GameStats.PlayerSpeedX.Value = Mathf.Abs(rigidbody.velocity.x);
        GameStats.PlayerSpeedY.Value = Sign(rigidbody.velocity.y);
        GameStats.PlayerPosX.Value = Mathf.Abs(rigidbody.position.x);
        GameStats.PlayerPosY.Value = Mathf.Abs(rigidbody.position.y);
        GameStats.NContacts.Value = contacts.Count;
        contacts.Clear();
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
        // ignore
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
        for (int i = 0; i < contacts.Count; i++)
        {
            ContactPoint2D c = contacts[i];
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(c.point, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(c.point, c.point + c.normal);
        }
    }
#endregion Gizmos
}
