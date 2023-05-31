using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    LineRenderer lr;
    [SerializeField] float ropeSpeed = 8;
    [SerializeField] [Tooltip("Number of sections rope has")] int quality = 500;
    [SerializeField] [Tooltip("How long the waves last")] float ropeDamper = 14;
    [SerializeField] [Tooltip("unsure")] float strength = 800;
    [SerializeField] [Tooltip("unsure")] float velocity = 15;
    [SerializeField] [Tooltip("Number of waves")] float waveCount = 3;
    [SerializeField] [Tooltip("How big the waves are")] float waveHeight = 1;
    [SerializeField] [Tooltip("what the waves will look like(have both ends at 0)")] AnimationCurve affectCurve;

    private Vector3 currentLrPoint;
    private Vector3 grapplePoint;
    private Spring springRopeCalcs;

    private SpringJoint joint;


    private Quaternion desiredGunRotation;

    public bool isGrappling { get { return joint != null; } }


    private void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        lr.enabled = false;
        springRopeCalcs = new Spring();
        springRopeCalcs.SetTarget(0);
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
            if (grappleableLayers == (grappleableLayers | (1 << hit.transform.gameObject.layer))){
                lr.positionCount = quality + 1;
                springRopeCalcs.SetVelocity(velocity);
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
        springRopeCalcs.Reset();
        Destroy(joint);
    }
    public void Pull(Rigidbody rb, float pullForce)
    {
        Debug.Log("Pulling");
        Vector3 pullDirection = grapplePoint - playerTransform.position;
        rb.AddForce(pullDirection.normalized * pullForce);

        float distanceFromPoint = Vector3.Distance(playerTransform.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * maxDistancePercent;
        joint.minDistance = distanceFromPoint * minDistancePercent;
    }

    public void RotateGun()
    {
        if (gun != null)
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
    }
    public void DrawRope()
    {
        if (!isGrappling)
            return;

        /*springRopeCalcs.SetDamper(ropeDamper);
        springRopeCalcs.SetStrength(strength);
        springRopeCalcs.Update(Time.deltaTime);

        Vector3 ropeUp = Quaternion.LookRotation((grapplePoint - gunTipTransform.position).normalized) * Vector3.up;

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = (float)i / quality;
            Vector3 offset = ropeUp * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * springRopeCalcs.Value * affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(grapplePoint, currentLrPoint, delta) + offset);
        }

        currentLrPoint = Vector3.Lerp(currentLrPoint, grapplePoint, Time.deltaTime * ropeSpeed);
        */
        lr.positionCount = 2;
        lr.SetPosition(0, gunTipTransform.position);
        lr.SetPosition(1, currentLrPoint);
        currentLrPoint = Vector3.Lerp(currentLrPoint, grapplePoint, Time.deltaTime * ropeSpeed);

    }
}
