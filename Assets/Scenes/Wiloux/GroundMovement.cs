using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMovement : MonoBehaviour
{
    // Start is called before the first frame update

    //Assingables
    public Transform playerCam;
    public Transform orientation;

    //Other
    public Rigidbody rb;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    //Movement
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 1;
    public float jumpForce = 550f;

    //Input
    float x, y;
    public bool jumping, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    //Slope
    public float maxSlopeAngle = 35f;
    private Vector3 hitPointNormal;
    private Vector3 slopeMoveDirection;
    public float slopeSlidingSpeed;
    public float slopeAngle;
    RaycastHit slopeHit;

    public bool isOnSlope;

    public Transform groundSphere;
    public float sphereRadius;


    public float speed = 12.5f;
    public float drag = 6f;

    private Vector3 myRotation;

    public float percentage;

    public bool wantsGlinding;


    public float speedModifier = 20f;

    public Vector3 characterVelocity { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        myRotation = orientation.transform.eulerAngles;
    }


    private void FixedUpdate()
    {

        grounded = CheckIfGrounded();

        if (!wantsGlinding && grounded)
        {
            Movement();
            //Slope
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, Mathf.Infinity, whatIsGround))
            {
                hitPointNormal = slopeHit.normal;
                slopeAngle = Vector3.Angle(hitPointNormal, Vector3.up);
                isOnSlope = Vector3.Angle(hitPointNormal, Vector3.up) > maxSlopeAngle;

            }
            else
            {
                isOnSlope = false;
            }
        }
        else if(wantsGlinding && !grounded)
        {
            Glid();
        }
    }                                 

    private void Update()
    {
        MyInput();

        if(!wantsGlinding)
        Look();

        if (!wantsGlinding)
        {
            rb.drag = 0;
        }

        if (grounded)
        {
            wantsGlinding = false;
        }

        WallRunInput();
        CheckForWall();
        
        if (isWallRunning)
        {
            WallJump();
        }
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);
        if (Input.GetKeyDown(KeyCode.E) && !CheckIfGrounded()){
            wantsGlinding = !wantsGlinding;

            if(wantsGlinding)
                myRotation = orientation.transform.eulerAngles;
        };


        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        slopeMoveDirection = Vector3.ProjectOnPlane(orientation.transform.forward * y  * multiplier * multiplierV, slopeHit.normal);

        if (isOnSlope && grounded)
        {
            rb.AddForce(Time.deltaTime * slopeSlidingSpeed * slopeHit.normal.normalized);
        }
        else
        {

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
        }
    }

    private void Jump()
    {
        Debug.Log("Jump");
        if (grounded && readyToJump)
        {
            jumping = true;
            readyToJump = false;
            
            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
       
    }
    private void WallJump()
    {
        if (readyToJump && isWallRunning)
        {
            Debug.Log("Jump");
            rb.constraints = RigidbodyConstraints.None;
            jumping = true;
            readyToJump = false;


            //normal jump
            //if ((isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.Q)) && jumping)
            //{
            //    rb.AddForce(Vector2.up * jumpForce * 1.5f);
            //}

            //sidwards wallhop
            //if ((isWallRight || isWallLeft && Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.D)) && jumping) rb.AddForce(-orientation.up * jumpForce * 1f);
            if (isWallRight && Input.GetKey(KeyCode.Q)) 
            {
                rb.AddForce(-orientation.right * jumpForce * 1f);
                rb.AddForce(orientation.up * jumpForce * 0.25f);
            } 
            if (isWallLeft && Input.GetKey(KeyCode.D))
            {

                rb.AddForce(orientation.right * jumpForce * 1f);
                rb.AddForce(orientation.up * jumpForce * 0.25f);
            }

            //Always add forward force
            //rb.AddForce(orientation.forward * jumpForce * 1f);



            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void ResetJump()
    {
        readyToJump = true;
        jumping = false;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        if (!wantsGlinding)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.y, 0));
        }
    }
    private void Glid()
    {
        myRotation.x += 20 * Input.GetAxis("Vertical") * Time.deltaTime;
        myRotation.x = Mathf.Clamp(myRotation.x, -45, 45);

        myRotation.y += 20 * Input.GetAxis("Horizontal") * Time.deltaTime;

        myRotation.z = -5 * Input.GetAxis("Horizontal");
        myRotation.z = Mathf.Clamp(myRotation.z, -5, 5);

        orientation.transform.rotation = Quaternion.Euler(myRotation);
        transform.rotation = Quaternion.Euler(myRotation);

        percentage = myRotation.x / 45;

        float modifiedDrag = (percentage * -2) + 6;
        float modifiedSpeed = ((speed + speedModifier) - speed) + speed;

        rb.drag = modifiedDrag;
        Vector3 localV = transform.InverseTransformDirection(rb.velocity);
        localV.z = modifiedSpeed;
        rb.velocity = transform.TransformDirection(localV);

        playerCam.transform.localRotation = Quaternion.Euler(myRotation);
        orientation.transform.localRotation = Quaternion.Euler(localV);

    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
    }

    private bool CheckIfGrounded()
    {
        bool isGrounded = Physics.CheckSphere(groundSphere.position, sphereRadius, whatIsGround);


        return isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundSphere.position, sphereRadius);
    }

    private void StopGrounded()
    {
        grounded = false;
    }
    public void SpeedSlash()
    {
        StartCoroutine(speedUp(2));
    }
    private IEnumerator speedUp(float waitTime)
    {


        speed = speed * 2;
        yield return new WaitForSeconds(waitTime);
        speed = speed / 2;
        yield return null;


    }

    //Wallrunning
    public LayerMask whatIsWall;
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    public bool isWallRight, isWallLeft;
    public bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;

    private void WallRunInput()
    {
        //Wallrun
        if (Input.GetKey(KeyCode.D) && isWallRight) StartWallrun();
        if (Input.GetKey(KeyCode.Q) && isWallLeft) StartWallrun();
        if (readyToJump && isWallRunning && jumping)
        {
            WallJump();
        }
    }
    private void StartWallrun()
    {
        rb.useGravity = false;
        isWallRunning = true;
        rb.constraints = RigidbodyConstraints.FreezePositionY;

        if (rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);

            
            if (isWallRight)
                rb.AddForce(orientation.right * wallrunForce / 5 * Time.deltaTime);
            else
                rb.AddForce(-orientation.right * wallrunForce / 5 * Time.deltaTime);
        }
    }
    private void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
    }
    private void CheckForWall() 
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1f, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, whatIsWall);
        
        //leave wall run
        if (!isWallLeft && !isWallRight) StopWallRun();
        ////reset jump
        if (isWallLeft || isWallRight)
        {
            rb.constraints = RigidbodyConstraints.None;
            readyToJump = true;
        }
    }

}

