using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Projectile : MonoBehaviour
{
    public Gradient tracerGradient;

    TrailRenderer tracer;

    float speed = 10;
    float damage = 1;

    float lifetime = 3;
    //Adds a little extra to our collision raycasts so that we account for if the enemy is moving towards the projectile to prevent raycasting from inside the enemy
    float skinWidth = .1f;

    public LayerMask collisionMask; //Determine which layers the projectile can collide with

    void Start()
    {
        GetComponent<TrailRenderer>().colorGradient = tracerGradient;

        Destroy(gameObject, lifetime); //Destroy bullets after lifetime passes

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask); //Array of all the colliders our projectile intersects with at spawn
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0], transform.position);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime; //How far it has moved since the last frame
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance); //Move
    }

    /**
    * Check if the bullet hits anything
    */
    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward); //A ray infront of the bullet to calculate if it will hit anything
        RaycastHit hit;

        //QueryTriggerInteraction determines if the raycast should also check trigger objects
        //We're checking for triggers since we've set the enemies to triggers
        //We add skinWidth to the length of the raycast to make sure we account for if an enemy is moving towards the projectile.
        //This prevents the issue with raycasts that start from inside an object not intersecting with the object
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        //print(c.gameObject.name);
        GameObject.Destroy(gameObject);
    }
}

