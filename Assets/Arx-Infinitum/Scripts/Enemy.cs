using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public ParticleSystem deathEffect;
    public static event System.Action OnDeathStatic;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float myCollisionRadius; //how thicc this boi is (to keep him from walkin in the player)
    float targetCollisionRadius; //how thicc his target is

    bool hasTarget;

    private void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        //DONT assume the player exists in the scene: this enemy may spawn after the player has died!
        if (GameObject.FindGameObjectWithTag("Player") != null) //Set up chasing code
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();

        //DONT assume the player exists in the scene: this enemy may spawn after the player has died!
        if (hasTarget) //Set up chasing code
        {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        //Set the death effect material
        ParticleSystem.MainModule main = deathEffect.main;
        main.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originalColor = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            //Make a particle effect and destroy it afterwards
            if (OnDeathStatic != null)
            {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);

    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                //CALCULATING DISTANCE BETWEEN TARGETS THE RIGHT WAY
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude; //Square distance so we dont have to calculate a square root uneccesarily
                                                                                            //Square the attack distance to match with the squared distance to target
                                                                                            //Add in the length of both our collision radii to lengthen the attack distance. Equivelant of subtracting it from the variable.
                                                                                            //This makes us calculate the distance between the target from the edges of our colliders rather than the centers of our game objects.
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position; //Point of start/end for animation
        Vector3 dirToTarget = (target.position - transform.position).normalized; //Direction of the target
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius); //Target position minus the length of how thicc we are (in the right direction)

        float percent = 0; //Percent of the animation complete
        float attackSpeed = 3;

        skinMaterial.color = Color.red;

        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            percent += Time.deltaTime * attackSpeed;

            if (percent >= .5 && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            //Parabola equasion with percent as X. This makes interpolation (y) start at 0 and go to 1 and then back to 0.
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            //Vector3.Lerp provides a Vector3 value between two Vector3s, where the third paramter defines the percent of the way from point A to B
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }
        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized; //Direction of the target
                Vector3 targetPos = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2); //Target position minus the length of how thicc we are (in the right direction)
                if (!dead)
                { //Make sure code doesn't run after the enemy was killed. Important for coroutines to make sure the object state is still desirable.
                    pathfinder.SetDestination(targetPos);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}

