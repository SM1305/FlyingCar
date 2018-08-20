using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverThrustersV3 : MonoBehaviour
{
    public bool debugRaycast;
    [Header("[Scene Objects]")]
    public Rigidbody car;
    public GameObject carModel;
    public GameObject[] Thrusters;

    [Header("[Scene Particles/Lights]")]
    public ParticleSystem dust;
    public float dustHeight;
    public ParticleSystem exhaust;
    public float exhaustEmissionAmount;
    public float exhaustStartLifetime;
    public Light[] lights;

    [Header("[Hover bits]")]
    public float hoverAltitude;
    public float hoverStrength;
    public float carGravity;
    public float groundedHeight;
    public bool grounded;
    public float controlReduction;

    [Header("-Steering bits-")]
    [Header("[Drive bits]")]
    public float velocity = 0f;
    public float maxVelocity = 0f;
    public float Acceleration = 0f;
    [Header("[Turning bits]")]
    public float turnStrength = 0f;
    public float maxTurnStrength = 0f;
    public float turnAcceleration = 0f;
    [Header("[Banking bits]")]
    public float bankRotationAngle;
    public float bankRotationAcceleration;
    public float bankRotationSpeed;

    private Vector3 objectNormal;

    private float exhaustEmission = 0;
    private float dustEmission;


    void Start()
    {
        car = GetComponent<Rigidbody>();
        car.centerOfMass = Vector3.down;
        exhaust.startLifetime = 0;
    }

    void Update()
    {
        //////STEERING
        //forward
        if ((Input.GetKey(KeyCode.W) && velocity < maxVelocity))
            velocity += Acceleration * Time.deltaTime;
        //reverse
        else if ((Input.GetKey(KeyCode.S) && velocity > -maxVelocity))
            velocity -= Acceleration * Time.deltaTime;
        //not forward or reverse
        else
            velocity = Mathf.Lerp(velocity, 0f, 10f * Time.deltaTime);

        //left
        if (Input.GetKey(KeyCode.A))
            turnStrength -= turnAcceleration * Time.deltaTime;
        //right
        else if (Input.GetKey(KeyCode.D))
            turnStrength += turnAcceleration * Time.deltaTime;
        //not left or right
        else
            turnStrength = Mathf.Lerp(turnStrength, 0f, 10f * Time.deltaTime);

        //no input
        if (!Input.anyKey)
            car.drag = 2f;
        else
            car.drag = 1f;

        //keep within bounds
        if (velocity >= maxVelocity)
            velocity = maxVelocity;
        if (velocity <= -maxVelocity)
            velocity = -maxVelocity;

        if (turnStrength >= maxTurnStrength)
            turnStrength = maxTurnStrength;
        if (turnStrength <= -maxTurnStrength)
            turnStrength = -maxTurnStrength;



        //exhaust particle effect
        if (Input.GetKey(KeyCode.W))
        {
            exhaustStartLifetime = (velocity / maxVelocity) / 3;
            if (exhaustStartLifetime > 0.2f)
                exhaustStartLifetime = 0.2f;
            exhaust.startLifetime = exhaustStartLifetime;
        }
        if (!Input.GetKey(KeyCode.W))
        {
            exhaustStartLifetime = Mathf.Lerp(exhaustStartLifetime, 0f, 2f * Time.deltaTime);
            exhaust.startLifetime = exhaustStartLifetime;
        }

        //brake lights
        if (Input.GetKey(KeyCode.S) && velocity > 0)
        {
            lights[0].range = 0.33f;
            lights[1].range = 0.33f;
        }
        else
        {
            lights[0].range = 0f;
            lights[1].range = 0f;
        }

        //reverse lights
        if (Input.GetKey(KeyCode.S) && velocity < 0)
        {

            lights[2].range = 0.33f;
            lights[3].range = 0.33f;
        }
        else
        {
            lights[2].range = 0f;
            lights[3].range = 0f;
        }

        //dust particle system
        RaycastHit dustHit;
        if (Physics.Raycast(car.transform.position, -Vector3.up, out dustHit, dustHeight))
        {
            Vector3 dustPos = dustHit.point;
            dustPos.y = dustPos.y + 0.5f;
            dust.transform.position = dustPos;
        }

        //grounded bool
        if (Physics.Raycast(car.transform.position, -Vector3.up, out dustHit, groundedHeight))
        {
            grounded = true;
            Debug.DrawRay(car.transform.position, -transform.up * groundedHeight, Color.yellow);
        }
        else
        {
            grounded = false;
        }
    }


    //physics
    void FixedUpdate()
    {
        //reduce user control if above grounded altitude
        if (!grounded)
        {
            turnStrength = turnStrength / controlReduction;
            bankRotationAcceleration = bankRotationAcceleration / controlReduction;
        }


        //drive forward-back
        car.AddForce(transform.forward * velocity * Time.deltaTime);

        //turn
        car.AddRelativeTorque(Vector3.up * turnStrength * Time.deltaTime);

        //rotate car relative to floor
        RaycastHit modelRay;
        if (Physics.Raycast(carModel.transform.position, -transform.up, out modelRay, (hoverAltitude + 1)))
        {
            objectNormal = modelRay.normal.normalized;
        }

        //bank during turn
        Vector3 newBank = carModel.transform.eulerAngles;
        newBank.z = Mathf.SmoothDampAngle(newBank.z, Input.GetAxis("Horizontal") * -bankRotationAngle, ref bankRotationAcceleration, bankRotationSpeed);
        carModel.transform.eulerAngles = newBank;


        //////HOVERING
        RaycastHit thrusterHit;
        for (int i = 0; i < Thrusters.Length; i++)
        {
            var thrustPosition = Thrusters[i];

            //draw raycast in white if not detecting object below car
            if (debugRaycast)
                Debug.DrawRay(thrustPosition.transform.position, -transform.up * hoverAltitude);

            //raycast below thrusters
            if (Physics.Raycast(thrustPosition.transform.position, -transform.up, out thrusterHit, hoverAltitude))
            {
                car.AddForceAtPosition(Vector3.up * hoverStrength * (1f - (thrusterHit.distance / hoverAltitude)), thrustPosition.transform.position);

                //draw raycast in red if detecting object below car
                if (debugRaycast)
                    Debug.DrawRay(thrustPosition.transform.position, -transform.up * hoverAltitude, Color.red);

            }
            else
            {
                if (transform.position.y > thrustPosition.transform.position.y)
                {
                    car.AddForceAtPosition(thrustPosition.transform.up * carGravity, thrustPosition.transform.position);
                }
                else
                {
                    car.AddForceAtPosition(thrustPosition.transform.up * -carGravity, thrustPosition.transform.position);
                }
            }
        }
    }
}