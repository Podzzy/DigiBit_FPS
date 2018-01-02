using UnityEngine;
using System.Collections;

/// <summary>
/// The component responsible for moving the player. 
/// Slightly modified version of this http://wiki.unity3d.com/index.php?title=RigidbodyFPSWalker
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterControls : MonoBehaviour
{
    public float speed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    private bool grounded = false;
    private Rigidbody rb;

    Vector3 targetVelocity;

    //The player model
    [SerializeField]
    private GameObject playerBody;

    //the health system script
    private HealthSystem hp;

    //can the player move?
    private bool canMove = true;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    void Start()
    {
        //Get the HP system and it's events, as we need logic based on that.
        //Eg we can't move if the player is dead
        hp = GetComponent<HealthSystem>();
        hp.OnPlayerDie += Hp_OnPlayerDie;
        hp.OnPlayerRespawn += Hp_OnPlayerRespawn;
    }

    /// <summary>
    /// Called from OnPlayerRespawn event from HealthSystem
    /// </summary>
    private void Hp_OnPlayerRespawn()
    {
        //the player can move again on respawn
        canMove = true;
    }

    /// <summary>
    /// Called from OnPlayerDie from the HealthSystem
    /// Force stop the player and make him unable to move
    /// </summary>
    /// <param name="attackerName"></param>
    private void Hp_OnPlayerDie(string attackerName)
    {
        targetVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        canMove = false;        
    }

    void FixedUpdate()
    {
        //Don't do anything if we can't move
        if (!canMove)
        {
            return;
        }
        if (grounded)
        {
            // Calculate how fast we should be moving
            targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            //use the playerbody's transform if it's defined
            if (playerBody)
            {
                targetVelocity = playerBody.transform.TransformDirection(targetVelocity);                
            }
            else
            {
                targetVelocity = transform.TransformDirection(targetVelocity);
            }
            targetVelocity *= speed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rb.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            rb.AddForce(velocityChange, ForceMode.VelocityChange);

            // Jump
            if (canJump && Input.GetButton("Jump"))
            {
                rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            }
        }

        // We apply gravity manually for more tuning control
        rb.AddForce(new Vector3(0, -gravity * rb.mass, 0));

        grounded = false;
    }

    void OnCollisionStay()
    {
        //horrible ground detection, use raycasts!
        grounded = true;
    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
}