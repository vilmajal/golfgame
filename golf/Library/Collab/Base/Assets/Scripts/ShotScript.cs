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

    public float x;
    public float y;
    public float z;

    public Vector3 pos;
    public Vector3 ang = new Vector3(0.0001f, 0.0001f, 0f);
    public Vector3 compositeForce;
    //public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
      // Vector3 direction = new Vector3(x, y, z);
      velocity = new Vector3(0, 0, 0);


      //rb = GetComponent<Rigidbody>();
      //rb.AddForce(direction);
    }

    // Update is called once per frame
    void Update()
    {
      // Get all forces
      compositeForce = new Vector3(0,0,0);
      compositeForce += GravityForce();

      if(isTouching) {
        // Calc normal force
        float cosAngle = Vector3.Dot(GravityForce(), normal) / (GravityForce().magnitude * normal.magnitude);
        //float sinAngle = (float) Math.Sqrt(1 - cosAngle*cosAngle);
        //Debug.Log("I am alive and my name is " + cosAngle);
        compositeForce += normal * Vector3.Magnitude(cosAngle * GravityForce());

        //compositeForce = Vector3.Scale(compositeForce, new Vector3(1,0,1));
      }

      // Get acceleration
      Vector3 acceleration = compositeForce / mass;

      // Get veclocity
      velocity += acceleration * Time.deltaTime;

      // Get position
      transform.position += velocity * Time.deltaTime;
    }

    Vector3 GravityForce()
    {
      float g = -9.82f;

      Vector3 gravity = new Vector3(0, g * mass, 0);

      return gravity;
    }

    // Ball bounces on ground
    private void OnCollisionEnter(Collision collision)
    {
      normal = collision.contacts[0].normal;
      velocity = 0.5f * (velocity - 2 * (Vector3.Dot(velocity, normal)) * normal);
      isTouching = true;
    }

    // Ball rests on ground
    private void OnCollisionExit(Collision other)
    {
        isTouching = false;
    }

}
