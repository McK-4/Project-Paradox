using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingHook : MonoBehaviour
{
    [Header("For Grappling")]
    [SerializeField] Transform gunTipTransform;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform playerTransform;
    [SerializeField] float maxGrappleableDistance;
    [SerializeField] LayerMask grappleableLayers;
    [SerializeField] LayerMask hitableLayers;
    [Header("Joint Settings (Spring Joint)")]
    [SerializeField] float maxDistancePercent = 0.8f;
    [SerializeField] float minDistancePercent = 0.25f;
    [SerializeField] float spring = 4.5f;
    [SerializeField] float damper = 7f;
    [SerializeField] float massScale = 4.5f;
    [Header("Object Looks")]
    [SerializeField] Transform gun;
    [SerializeField] float rotationSpeed;
    [Header("Rope Looks")]
    [SerializeField] [Tooltip("How quickly it goes out")] float ropeSpeed = 8;
    [SerializeField] [Tooltip("Number of sections rope has")] int quality = 500;
    [SerializeField] [Tooltip("How quickly the waves will grow and shrink")] float ropeDamper = 14;
    [SerializeField] [Tooltip("How quickly the waves shrink as they go further")] float strength = 1 / 15;
    [SerializeField] [Tooltip("What the max height of the waves will be")] float velocity = 15;
    [SerializeField] [Tooltip("The distance between each wave")] float waveDistance = 15f;
    [SerializeField] [Tooltip("Changes how it is represented on a graph")] int xRange = 500;

    LineRenderer lr;
    private Vector3 currentLrPoint;
    private Vector3 grapplePoint;
    private float currentVel;

    private SpringJoint joint;


    private Quaternion desiredGunRotation;

    public bool isGrappling { get { return joint != null; } }


    private void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        lr.enabled = false;
    }
    private void Update()
    {
        RotateGun();
    }
    private void LateUpdate()
    {
        DrawRope();
    }

    public void StartGrapple()
    {
        //seeing if we hit something
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxGrappleableDistance, hitableLayers))
        {
            //checking if grappalable
            if (grappleableLayers == (grappleableLayers | (1 << hit.transform.gameObject.layer)))
            {
                currentVel = 0;
                lr.positionCount = quality + 1;
                currentLrPoint = gunTipTransform.position;

                grapplePoint = hit.point;
                joint = playerTransform.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                float distanceFromPoint = Vector3.Distance(playerTransform.position, grapplePoint);

                joint.maxDistance = distanceFromPoint * maxDistancePercent;
                joint.minDistance = distanceFromPoint * minDistancePercent;

                joint.spring = spring;
                joint.damper = damper;
                joint.massScale = massScale;

                lr.enabled = true;
            }
        }
    }
    public void EndGrapple()
    {
        lr.positionCount = 0;
        lr.enabled = false;
        Destroy(joint);
    }
    public void Pull(Rigidbody rb, float pullForce)
    {
        //Debug.Log("Pulling");
        Vector3 pullDirection = grapplePoint - playerTransform.position;
        rb.AddForce(pullDirection.normalized * pullForce);

        float distanceFromPoint = Vector3.Distance(playerTransform.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * maxDistancePercent;
        joint.minDistance = distanceFromPoint * minDistancePercent;
    }
    public void RotateGun()
    {

        if (!isGrappling)
        {
            desiredGunRotation = gun.parent.transform.rotation;
        }
        else
        {
            desiredGunRotation = Quaternion.LookRotation(grapplePoint - gun.transform.position);
        }

        gun.rotation = Quaternion.Lerp(gun.transform.rotation, desiredGunRotation, Time.deltaTime * rotationSpeed);

    }
    public void DrawRope()
    {
        if (!isGrappling)
            return;

        Vector3 ropeRight = gun.right;

        for (int i = 0; i < quality + 1; i++)
        {
            float x = ((float)i / (quality + 1)) * xRange;

            float V = (-strength * x + currentVel) > 0 ? (-strength * x + currentVel) : 0;

            float rightMultiplier = V * Mathf.Sin(x / waveDistance);


            float distanceOut = (Vector3.Distance(currentLrPoint, gunTipTransform.position) / (quality + 1)) * i;

            Vector3 newPoint = gunTipTransform.position +
                ((currentLrPoint - gunTipTransform.position).normalized * distanceOut) + (ropeRight * rightMultiplier);
            lr.SetPosition(i, newPoint);
        }

        currentLrPoint = Vector3.MoveTowards(currentLrPoint, grapplePoint, Time.deltaTime * ropeSpeed);

        if (currentLrPoint != grapplePoint)
        {
            currentVel = Mathf.Clamp(currentVel + Time.deltaTime * ropeDamper, 0, velocity);
        }
        else
        {
            currentVel = Mathf.Clamp(currentVel - Time.deltaTime * ropeDamper, 0, velocity);
        }
    }
}
