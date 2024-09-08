/**
 *
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class Slime : MonoBehaviour
{

#region Parameters
    [SerializeField] private float maxDistance;
    [SerializeField] private float spreadSpeed = 2;
#endregion 

#region Components
    new private Rigidbody2D rigidbody;
    new private LineRenderer lineRenderer;
#endregion

#region State
    private EdgeCollider2D currentCollider;   
    private List<Vector2> surface; 
    private int? closestSegment;

    private ContactPoint2D[] contacts;
    private LinkedList<Vector2> points;
    private Vector3[] pointsArray;
#endregion

#region Init & Destroy
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();

        surface = new List<Vector2>();
        contacts = new ContactPoint2D[1];
        points = new LinkedList<Vector2>();
        pointsArray = new Vector3[1];
    }
#endregion Init

#region Update
    void Update()
    {
        Vector2 c = contacts[0].point;
        Vector2? closestPoint = null;
        closestSegment = GetClosestSegment(c, out closestPoint);

        if (closestSegment != null) {
            int i = closestSegment.Value;
            Vector2 p = closestPoint.Value;
            points.Clear();
            points.AddLast(p);

            // spread to the right
            float d = maxDistance;
            int next = i + 1;
            d -= Vector2.Distance(p, surface[next]);

            while (d > 0 && next < surface.Count)
            {
                points.AddLast(surface[next]);
                next++;

                if (next < surface.Count) {
                    d -= Vector2.Distance(surface[next-1], surface[next]);
                }
            }
            
            if (d < 0) 
            {
                // E = Sn + d * norm(Sn - Sn-1)
                Vector2 v = surface[next] - surface[next-1];
                Vector2 end = surface[next] + d * v.normalized;
                points.AddLast(end);
            }

            // spread to the left
            d = maxDistance;
            next = i;
            d -= Vector2.Distance(p, surface[next]);
            
            while (d > 0 && next >= 0)
            {
                points.AddFirst(surface[next]);
                next--;

                if (next >= 0) {
                    d -= Vector2.Distance(surface[next+1], surface[next]);
                }
            }

            if (d < 0) 
            {
                // E = Sn + d * norm(Sn - Sn+1)
                Vector2 v = surface[next] - surface[next+1];
                Vector2 end = surface[next] + d * v.normalized;
                points.AddFirst(end);
            }

        }


    }

    private int? GetClosestSegment(Vector2 point, out Vector2? closestPoint)
    {
        int? closestSegment = null;
        closestPoint = null;
        float minDistance = float.PositiveInfinity;

        for (int i = 0; i < surface.Count-1; i++)
        {
            // Q(t) = Si + t v
            // P - Q(t) = P - Si - t v
            //          = u - t v
            // [P - Q(t)].v = 0
            // (u - t v).v = 0
            // u.v = t v.v
            // t = u.v / v.v

            Vector2 u = point - surface[i];
            Vector2 v = surface[i+1] - surface[i];
            float t = Vector2.Dot(u,v) / Vector2.Dot(v,v);

            if (t >= 0 && t < 1) 
            {
                Vector2 p = surface[i] + t * v;
                float d = Vector2.Distance(p,u);
                if (d < minDistance)
                {
                    closestSegment = i;
                    closestPoint = p;
                    minDistance = d;
                }
            }
        }

        return closestSegment;
    }
#endregion Update

#region FixedUpdate
    void FixedUpdate()
    {
        rigidbody.GetContacts(contacts);
    }

    void OnCollisionEnter2D(Collision2D collision) 
    {
        if (collision.collider is EdgeCollider2D)
        {
            currentCollider = (EdgeCollider2D)collision.collider;
            Transform t = currentCollider.transform;
            currentCollider.GetPoints(surface);

            // convert all points to world coords
            for (int i = 0; i < surface.Count; i++)
            {
                surface[i] = t.TransformPoint(surface[i]);
            }
        }
    }
#endregion FixedUpdate

#region Gizmos
    void OnDrawGizmos()
    {
        if (currentCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(contacts[0].point, 0.1f);

            Gizmos.color = Color.yellow;
            for (int i = 0; i < surface.Count-1; i++) 
            {
                Gizmos.DrawLine(surface[i], surface[i+1]);
            }

            Gizmos.color = Color.red;
            Vector2? last = null;
            foreach (Vector2 next in points) 
            {
                if (last.HasValue) {
                    Gizmos.DrawLine(last.Value, next);
                }
                last = next;
            }
        }
    }
#endregion Gizmos
}
