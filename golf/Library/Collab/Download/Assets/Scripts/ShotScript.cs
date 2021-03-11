using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
    public Vector3 velocity;
    public float mass;
    public bool isTouching = false;
    public Vector3 normal;
    public float contactPointY;
    public Vector3 gravity;
    public float x;
    public float y;
    public float z;
    public float timeSinceLastCollision = 0;

    public bool bounce = false;

    public Vector3 pos;
    public Vector3 ang = new Vector3(0.0001f, 0.0001f, 0f);
    public Vector3 compositeForce;
    //public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
      // Vector3 direction = new Vector3(x, y, z);
      velocity = new Vector3(0, 0, 0);
      gravity = new Vector3(0, -9.82f * mass, 0);
      //rb = GetComponent<Rigidbody>();
      //rb.AddForce(direction);
    }

    // Update is called once per frame
    void Update()
    {
      timeSinceLastCollision += Time.deltaTime;

      // Get all forces
      compositeForce = new Vector3(0,0,0);
      compositeForce += gravity;

      if(isTouching) {
        // Calc normal force
        float cosAngle = Vector3.Dot(gravity, normal) / (gravity.magnitude * normal.magnitude);
        //float sinAngle = (float) Math.Sqrt(1 - cosAngle*cosAngle);
        compositeForce += normal * Vector3.Magnitude(cosAngle * gravity);
        //Debug.Log("I am alive and my name is " + cosAngle);

        //compositeForce = Vector3.Scale(compositeForce, new Vector3(1,0,1));

        //Dont allow to go through floor

      }

      // Get acceleration
      Vector3 acceleration = compositeForce / mass;

      // Get veclocity
      velocity += acceleration * Time.deltaTime;

      // Fix, so velocity is aligned with plane
      if(isTouching && !bounce) {
        velocity = Vector3.Normalize(compositeForce) * Vector3.Magnitude(velocity);
      }

      // Get position
      transform.position += velocity * Time.deltaTime;

      /*if (isRolling && transform.position.y < contactPointY) {
          transform.position = new Vector3(transform.position.x, contactPointY, transform.position.z);
       }*/
    }

    // Ball bounces on ground
    private void OnCollisionEnter(Collision collision)
    {
      normal = collision.contacts[0].normal;
      contactPointY = collision.contacts[0].point.y;

      if (timeSinceLastCollision > 0.3) {
        velocity = 0.5f * (velocity - 2 * (Vector3.Dot(velocity, normal)) * normal);
        bounce = true;
      }
      isTouching = true;
      //Debug.Log("touch");

      timeSinceLastCollision = 0;
    }

    // Ball rests on ground
    private void OnCollisionExit(Collision other)
    {
        isTouching = false;
        bounce = false;
        //Debug.Log("Untouch");
    }

}
