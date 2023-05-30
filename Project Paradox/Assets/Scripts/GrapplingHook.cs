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
    [SerializeField] LineRenderer lr;
    [SerializeField] float ropeSpeed = 8;
    [SerializeField] [Tooltip("Number of sections rope has")] int quality = 500;
    [SerializeField] [Tooltip("How long the waves last")] float ropeDamper = 14;
    [SerializeField] [Tooltip("unsure")] float strength = 800;
    [SerializeField] [Tooltip("unsure")] float velocity = 15;
    [SerializeField] [Tooltip("Number of waves")] float waveCount = 3;
    [SerializeField] [Tooltip("How big the waves are")] float waveHeight = 1;
    [SerializeField] [Tooltip("what the waves will look like(have both ends at 0)")]AnimationCurve affectCurve;

    private Vector3 currentLrPoint;
    private Vector3 grapplePoint;
    private Spring springRopeCalcs;

    private SpringJoint joint;


    private Quaternion desiredGunRotation;

    public bool isGrappling { get; private set; } = false;


    private void Start()
    {
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
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxGrappleableDistance, grappleableLayers))
        {
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


        }
    }
    public void EndGrapple()
    {
        lr.positionCount = 0;
        springRopeCalcs.Reset();
        Destroy(joint);
    }
    public void Pull(Rigidbody rb, float pullForce)
    {
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
            desiredGunRotation = transform.parent.rotation;
        }
        else
        {
            desiredGunRotation = Quaternion.LookRotation(grapplePoint - gun.transform.position);
        }

        transform.rotation = Quaternion.Lerp(gun.transform.rotation, desiredGunRotation, Time.deltaTime * rotationSpeed);
    }
    public void DrawRope()
    {
        if (!isGrappling)
            return;

        springRopeCalcs.SetDamper(ropeDamper);
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
    }
}
