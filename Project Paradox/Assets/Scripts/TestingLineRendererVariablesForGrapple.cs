using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class TestingLineRendererVariablesForGrapple : MonoBehaviour
{
    [SerializeField] [Tooltip("How quickly it goes out")] float ropeSpeed = 8;
    [SerializeField] [Tooltip("Number of sections rope has")] int quality = 500;
    [SerializeField] [Tooltip("How long the waves last")] float ropeDamper = 14;
    [SerializeField] [Tooltip("How quickly the waves shrink as the go further")] float strength = 1 / 15;
    [SerializeField] [Tooltip("The max height of the waves")] float velocity = 15f;
    [SerializeField] [Tooltip("The distance between each wave")] float waveDistance = 15f;
    [SerializeField] [Tooltip("Changes how it is represented on a graph")] int xRange = 500;

    [SerializeField] Transform gunTipTransform;
    LineRenderer lr;
    private Vector3 currentLrPoint;
    [SerializeField] Vector3 grapplePoint;
    private float currentVel;
    // Start is called before the first frame update
    private void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        lr.positionCount = quality + 1;
        
    }
    private void LateUpdate()
    {
        currentVel = velocity;
        DrawRope();
    }
    public void DrawRope()
    {

        Vector3 ropeRight = new Vector3(0,0,1);

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

        currentLrPoint = Vector3.Lerp(currentLrPoint, grapplePoint, Time.deltaTime * ropeSpeed);

        //if (currentLrPoint != grapplePoint)
        //{
        //    currentVel += Time.deltaTime * ropeDamper;
        //}
        //else
        //{
        //    currentVel -= Time.deltaTime * ropeDamper;
        //}

    }
}
