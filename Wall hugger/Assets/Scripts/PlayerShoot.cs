using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] float minDistance = 0.1f;
    [SerializeField] float maxDistance = 20;
    [SerializeField] private LayerMask groundLayer;

    private Actions actions;
    private InputAction aimAction;
    private InputAction shootAction;

    private LineRenderer lineRenderer;
    private Vector3[] linePositions = new Vector3[2];

    private Vector3 aimDir = Vector3.zero;
    private RaycastHit2D hit;

    void Awake()
    {
        actions = new Actions();
        aimAction = actions.aiming.aim;
        shootAction = actions.aiming.shoot;

        lineRenderer = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        actions.aiming.Enable();
    }

    void OnDisable()
    {
        actions.aiming.Disable();
    }

    void Update()
    {
        aimDir = aimAction.ReadValue<Vector2>();

        if (aimDir.sqrMagnitude > minDistance * minDistance)
        {
            lineRenderer.enabled = true;
            hit = Physics2D.Raycast(transform.position, aimDir, maxDistance, groundLayer);

            linePositions[0] = transform.position;
            linePositions[1] = (hit.collider != null ? 
                hit.point : transform.position + aimDir.normalized * maxDistance);
            lineRenderer.SetPositions(linePositions);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)aimDir);

        if (hit.collider != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
    }
#endregion Gizmos

}
