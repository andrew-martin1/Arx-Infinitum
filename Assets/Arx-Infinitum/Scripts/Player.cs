using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* For getting the players inputs
*/
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{

    public float moveSpeed = 5;

    public Crosshair crosshair;

    PlayerController controller;
    GunController gunController;
    Camera viewCamera;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    //Awake method to prevent race condition with spawner Start()
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        viewCamera = Camera.main;
        gunController = GetComponent<GunController>();
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    // Update is called once per frame
    void Update()
    {
        ////////////////////
        //Movement input
        ////////////////////

        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); //GetAxisRaw is good!
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        ////////////////////
        //Look input
        ////////////////////

        //Draw a ray from the camera where the mouse is
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        //Create a plane where the ground is. Dont bother getting the plane from the game, too problematic.
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight); //First vector tells where the plane should point, second vector is a point the vector should intersect (for tilt)
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        { //If ray intersects with groundplane, return true and give out the distance the ray traveled.
            Vector3 point = ray.GetPoint(rayDistance); //Get the point where the ray intersected the plane
                                                       //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);

            crosshair.transform.position = point;
            crosshair.DetectTargets(ray);

            //print(new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)); //Check the dist between the player and the crosshair
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            { //If the crosshair is too close, stop aiming
                gunController.Aim(point);

            }
        }



        ////////////////////
        //Weapon input
        ////////////////////

        if (Input.GetMouseButton(0))
        { //if mouse button is held down
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        { //if mouse button is let go
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        ////////////////////
        //Menu input
        ////////////////////
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
        }

        //Kill the player if they fall off
        if (transform.position.y < -10)
        {
            TakeDamage(health);
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        PostProcessingManager.instance.PlayerHealthChanged(health, startingHealth);
    }
}
