using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(KinematicMover))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float pressedInputBuffering = .2f;
    [Min(0.1f)][SerializeField] float maxSpeed = 4;
    [Min(0.05f)][SerializeField] float timeToMaxSpeed = 0.1f;
    float acceleration;

    [Min(0.1f)][SerializeField] float maxJumpHeight = 4;
    [Min(0.1f)][SerializeField] float minJumpHeight = 1;
    [Min(0.05f)][SerializeField] float timeToMaxJumpHeight = 0.4f;
    float gravity;
    float jumpImpulse;
    float cancelledJumpCutoffPoint;

    Vector2 velocity = Vector2.zero;
    KinematicMover mover;
    float horizontalInput;
    bool JumpRequested => Time.time - jumpPressedTime <= pressedInputBuffering;
    bool jumpHeld;
    KinematicMover.MoveResult lastMoveResults;
    bool isJumping = false;
    float jumpPressedTime = Mathf.NegativeInfinity;
    void OnValidate()
    {
        CalculateDerivedMovementVariables();
    }

    void Awake()
    {
        mover = GetComponent<KinematicMover>();
        CalculateDerivedMovementVariables();
    }

    void Start()
    {
        lastMoveResults = mover.MoveAndSlide(new Vector2(0, gravity * Time.fixedDeltaTime));
    }

    void CalculateDerivedMovementVariables()
    {
        acceleration = maxSpeed / timeToMaxSpeed;
        gravity = -maxJumpHeight / (0.5f * (timeToMaxJumpHeight * timeToMaxJumpHeight));
        jumpImpulse = -(gravity * timeToMaxJumpHeight);
        cancelledJumpCutoffPoint = Mathf.Sqrt(jumpImpulse * jumpImpulse + 2 * gravity * minJumpHeight);
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
            jumpPressedTime = Time.time;
        jumpHeld = Input.GetButton("Jump");
    }

    void FixedUpdate()
    {
        HandleJumping();
        velocity.y += gravity * Time.fixedDeltaTime;
        HandleWalking();
        lastMoveResults = mover.MoveAndSlide(velocity * Time.fixedDeltaTime);
        AdjustVelocityAfterMoving();
    }

    private void AdjustVelocityAfterMoving()
    {
        if ((lastMoveResults.Grounded && velocity.y < 0) ||
            (lastMoveResults.HitCeiling && velocity.y > 0))
            velocity.y = 0;
        if (isJumping && velocity.y <= 0)
            isJumping = false;
    }

    private void HandleJumping()
    {
        if (JumpRequested && lastMoveResults.Grounded)
        {
            isJumping = true;
            velocity.y = jumpImpulse;
            jumpPressedTime = Mathf.NegativeInfinity;
        }

        if (isJumping && !jumpHeld && velocity.y <= cancelledJumpCutoffPoint)
        {
            isJumping = false;
            velocity.y = 0;
        }
    }

    private void HandleWalking()
    {
        velocity.x = Mathf.MoveTowards(velocity.x, maxSpeed * horizontalInput, acceleration * Time.fixedDeltaTime);
    }
}
