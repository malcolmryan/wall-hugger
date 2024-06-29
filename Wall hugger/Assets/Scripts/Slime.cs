using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    [SerializeField] private int nPoints = 20;
    [SerializeField] private float minRadius = 1;
    [SerializeField] private float maxRadius = 2;
    [SerializeField] private float smoothing = 0;
    [SerializeField] private LayerMask groundLayer;

    private MeshFilter meshFilter;
    private Mesh mesh;

    private float[] distance = null;
    private float[] smoothDistance = null;
    private Vector3[] vertices;
    private int[] tris;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
    }

    void Start()
    {
        InitMesh();
    } 

    void Update()
    {
        UpdateMesh();        
    }

    private void InitMesh()
    {
        distance = new float[nPoints];
        smoothDistance = new float[nPoints];
        vertices = new Vector3[nPoints+1];
        vertices[0] = Vector3.zero;   // center
        for (int i = 1; i <= nPoints; i++)
        {
            float angle = (i-1) * 360f / nPoints;           
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);;
            
            distance[i-1] = minRadius;
            smoothDistance[i-1] = minRadius;
            vertices[i] = q * (minRadius * Vector3.right);
        }
        mesh.vertices = vertices;

        tris = new int[3 * nPoints];
        int k = 0;
        for (int i = 0; i < nPoints; i++)
        {
            tris[k++] = 0;
            tris[k++] = i+1;
            tris[k++] = (i+1) % nPoints + 1;
        }
        mesh.triangles = tris;
    }

    private void UpdateMesh()
    {
        // raycast to calculate distances
        for (int i = 0; i < nPoints; i++)
        {
            float angle = i * 360f / nPoints;                   
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = q * Vector3.right;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxRadius, groundLayer);

            distance[i] = (hit.collider == null ? minRadius : hit.distance);
        }

        // apply a kernel to smooth
        for (int i = 0; i < nPoints; i++)
        {
            if (distance[i] > minRadius)
            {
                smoothDistance[i] = distance[i];
            }
            else
            {
                int left = (i - 1 + nPoints) % nPoints;
                int right = (i + 1) % nPoints;

                smoothDistance[i] = distance[i] * 0.5f;
                smoothDistance[i] += distance[left] * 0.25f;
                smoothDistance[i] += distance[right] * 0.25f;
            }

        }

        // update mesh
        for (int i = 1; i <= nPoints; i++)
        {
            float angle = (i-1) * 360f / nPoints;                   
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = q * Vector3.right;

            // TODO make this framerate indept
            float t = 1- smoothing;
            // vertices[i] = Vector3.Lerp(vertices[i], smoothDistance[i-1] * dir, t);
            vertices[i] = smoothDistance[i-1] * dir;
        }
        mesh.vertices = vertices;

    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) 
        {
            return;
        }

        Gizmos.color = Color.yellow;

        for (int i = 0; i < nPoints; i++)
        {
            float angle = i * 360f / nPoints;                   
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = q * Vector3.right;

            Gizmos.DrawLine(transform.position, transform.position + distance[i] * dir);
        }
    }

}
