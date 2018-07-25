using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarThrust : MonoBehaviour
{
    public Rigidbody car;
    public GameObject[] Thrusters;

    [Header("[Hover bits]")]
    public float hoverAltitude;
    public float hoverStrength;
    public bool debugRaycast;

    [Header("[Steering bits]")]
    public float velocity = 0f;
    public float maxVelocity = 0f;
    public float Acceleration = 0f;

    public float turnStrength = 0f;
    public float maxTurnStrength = 0f;
    public float turnAcceleration = 0f;


        void Start ()
    {
        car = GetComponent<Rigidbody>();
	}
	
	void Update ()
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
    }


    //physics
    void FixedUpdate()
    {
        //drive forward-back
        car.AddForce(transform.forward * velocity * Time.deltaTime);

        //turn
        car.AddRelativeTorque(Vector3.up * turnStrength * Time.deltaTime);

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
                    car.AddForceAtPosition(thrustPosition.transform.up * hoverStrength, thrustPosition.transform.position);
                }
                else
                {
                    car.AddForceAtPosition(thrustPosition.transform.up * -hoverStrength, thrustPosition.transform.position);
                }
            }
        }
    }
}








//var Speed        : float = 0;//Don't touch this
//var MaxSpeed     : float = 10;//This is the maximum speed that the object will achieve
//var Acceleration : float = 10;//How fast will object reach a maximum speed
//var Deceleration : float = 10;//How fast will object reach a speed of 0

//function Update()
//{
//    if ((Input.GetKey("left")) && (Speed < MaxSpeed))
//        Speed = Speed - Acceleration  Time.deltaTime;
//else if ((Input.GetKey("right")) && (Speed > -MaxSpeed))
//        Speed = Speed + Acceleration  Time.deltaTime;
//else
//{
//        if (Speed > Deceleration  Time.deltaTime)
//        Speed = Speed - Deceleration  Time.deltaTime;
//    else if (Speed < -Deceleration  Time.deltaTime)
//        Speed = Speed + Deceleration  Time.deltaTime;
//    else
//        Speed = 0;
//    }

//    transform.position.x = transform.position.x + Speed * Time.deltaTime;
//}
