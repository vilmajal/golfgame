using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotScript : MonoBehaviour
{
	public float mass;
	public float diameter;
	public Vector3 velocity;
	public Vector3 gravityForce;

	public Vector3 frictionForce;
  public Vector3 normalForce;

  public float puttAcceleration; // (deceleration)
	public Vector3 compositeForce;
	public Dictionary<Rigidbody, CollisionHolder> myMap;
  public float runTime = 0;
  public int nShots = 0;

	// Air stuff
	public float airDensity;
	public Vector3 windVelocity;
	public Vector3 airResistanceForce;
	public float dynamicViscosityAir;
	public double re; // Reynolds number
	public float sphereDrag;
	public Vector3 relativeVelocityBallToAir;

  public Slider strengthSlider;
  public Slider directionSlider;
  public Text score;
  public GameObject arrow;

  public class CollisionHolder {
    public Collision collision_;
    public float timeSinceLastCollision_;
    public bool bouncing_;
    public bool isTouching_;
    public Vector3 normal_;

    public CollisionHolder(Collision collision_, float timeSinceLastCollision_, bool bouncing_, bool isTouching_, Vector3 normal_) {
      this.collision_ = collision_;
      this.timeSinceLastCollision_ = timeSinceLastCollision_;
      this.bouncing_ = bouncing_;
      this.isTouching_ = isTouching_;
      this.normal_ = normal_;
	  }
	}

	void Start() // Start is called before the first frame update
	{
	  mass = 0.04593f; // 45.93 grams
	  diameter = 0.04167f; // 4,167 centimters
	  puttAcceleration = -0.4560f; // The deceleration from rolling friction from the project specification.
	  //transform.localScale = new Vector3(diameter, diameter, diameter);

	  windVelocity = new Vector3(0f, 0f, 0f); // At 101.325 kPa and 15 degrees c.
	  airDensity = 1.225f;
	  dynamicViscosityAir = 17.89f * 0.000001f; // At 15 degrees c.

	  velocity = new Vector3(0f, 0f, 0f);
	  gravityForce = new Vector3(0, -9.82f * mass, 0);
	  myMap = new Dictionary<Rigidbody, CollisionHolder>();

    strengthSlider =  GameObject.FindGameObjectWithTag("slider").GetComponent<Slider>();
    directionSlider = GameObject.FindGameObjectWithTag("slider2").GetComponent<Slider>();
    score = GameObject.FindGameObjectWithTag("score").GetComponent<Text>();
    //arrow = GameObject.FindGameObjectWithTag("arrow").GetComponent<GameObject>();
  }

  void Update() {
    runTime += Time.deltaTime;
    score.text = "Shots: " + nShots;

    Vector3 velocityVector = new Vector3(0f, 0f, 1f);
    GameObject.FindGameObjectWithTag("arrow").transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0, directionSlider.value, 0) * velocityVector);
  }

	void FixedUpdate() // Update is called once per frame
	{
	  // Reset composite force each tick, begin with gravity which always applies.
	  compositeForce = gravityForce;

	  // Calculate wind resistance
	  relativeVelocityBallToAir = -1f * (windVelocity - velocity).magnitude * Vector3.Normalize(velocity-windVelocity);
    if((windVelocity - velocity).magnitude == 0) {
        sphereDrag = 0; // For when reynolds number causes division by zero
    } else {
        re = (double)(airDensity * (windVelocity - velocity).magnitude * diameter) / dynamicViscosityAir;
        sphereDrag = (float)((24.0 / re) + ((2.6 * (re / 5.0)) / (1.0 + Math.Pow(re / 5.0, 1.52))) + ((0.411 * Math.Pow(re / 263000.0, -7.94)) / (1.0 + Math.Pow(re / 263000.0, -8))) + ((0.25 * (re / 1000000.0)) / (1.0 + (re / 1000000.0))));
    }
    float crossSectionalArea = (float) Math.Pow((double) (diameter / 2), (double) 2) * 3.14159265358979f;
	  airResistanceForce = 0.5f * airDensity * relativeVelocityBallToAir * sphereDrag * crossSectionalArea;
	  compositeForce += airResistanceForce;


    /* Handle "rolling" (as opposed to bouncing) behavior here. */
	  foreach( KeyValuePair<Rigidbody, CollisionHolder> kvp in myMap ) { // Iterate over surfaces collided with and handle rolling on top of those applicable.
      kvp.Value.timeSinceLastCollision_ += Time.fixedDeltaTime;

      //kvp.Value.bouncing_ = false; //test
      if (kvp.Value.timeSinceLastCollision_ > 0.02) {
        kvp.Value.bouncing_ = false; // Fix for edge case when ball should bounce but does not have velocity enough to trigger a collision exit and remove tag, making the ball fall through floor.
      }
      if(kvp.Value.isTouching_ && !kvp.Value.bouncing_) { // If its touching and not bouncing we say its "rolling"
        if (Vector3.Dot(Vector3.Normalize(velocity), kvp.Value.normal_) < 0.5f) { // Check if the velocity is pointing toward or against the rigidbody
          velocity -= Vector3.Project(velocity, kvp.Value.normal_); // Remove the velocity component paralell and opposite-facing compared to the normal. This is to prevent ball from having small residual velocity from bouncing and going through floor.
        }

        normalForce = Vector3.Project(compositeForce, kvp.Value.normal_);
        compositeForce -= normalForce;

        frictionForce = Vector3.Normalize(Vector3.ProjectOnPlane(velocity, kvp.Value.normal_)) * mass * puttAcceleration; // Apply rolling friction pointing against the velocity projected upon the plane.
        compositeForce += frictionForce;
      }

	  }



	  /* Derive and apply acceleration, velocity, and position. */
	  Vector3 acceleration = compositeForce / mass; // Get acceleration
    if (runTime > 1f) { // Put a small pause before manipulating position to avoid startup jerkiness.
      velocity += acceleration * Time.fixedDeltaTime; // The velocity survives each tick, add incremental velocity.
      transform.position += velocity * Time.fixedDeltaTime; // Set position, e.g., move ball.
    }
	}


	 void OnCollisionEnter(Collision collision)
	{
	  Vector3 normalTest = collision.contacts[0].normal;
	  bool bouncingTest = false;
	  bool touchingTest = true;
	  Debug.Log("touch " + collision.gameObject + normalTest);

	  /* Handle "bouncing" (as opposed to rolling) behavior here. */

      if (!myMap.ContainsKey(collision.rigidbody) || myMap[collision.rigidbody].timeSinceLastCollision_ > 0.35) { // This works sort of like a timer, so that when bounces become shorter than 0.35s, the ball will not be rolling anymore.


        Vector3 vNormProj = Vector3.Project(velocity, normalTest); // Calculate the velocity projected onto the normal
        velocity -= vNormProj; // Set velocity to the vector rejection of the velocity onto the normal.
        velocity -= 0.6f * vNormProj; // "Reflect" the velocity projected upon the normal to make a "bounce"
        bouncingTest = true;

        // if (Vector3.Dot(Vector3.Normalize(velocity), normalTest) < 0.5f) { // Check if the velocity is pointing toward or against the rigidbody
        //   velocity -= Vector3.Project(velocity, normalTest); // Remove the velocity component paralell and opposite-facing compared to the normal. This is to prevent ball from having small residual velocity from bouncing and going through floor.
        // }
      }


	  myMap[collision.rigidbody] = new CollisionHolder(collision, 0f, bouncingTest, touchingTest, normalTest);
	}

	private void OnCollisionExit(Collision collision)
	{
	  myMap[collision.rigidbody].isTouching_ = false;
	  myMap[collision.rigidbody].bouncing_ = false;
	  Debug.Log("Untouch" + collision.gameObject);
	}

  public void OnButtonPress() {
    Vector3 velocityVector = new Vector3(0f, 0f, 1f);
    velocityVector = Quaternion.Euler(0, directionSlider.value, 0) * velocityVector;
    velocityVector *= strengthSlider.value;
    velocity += velocityVector;
    nShots++;

  }

  public void OnButtonPressTwo() {
      velocity = new Vector3(0f, 0f, 0f);
      transform.position = new Vector3(0f, 1f, 0f);
  }
}
