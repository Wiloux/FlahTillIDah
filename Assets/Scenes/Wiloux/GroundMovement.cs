using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMovement : MonoBehaviour
{
    // Start is called before the first frame update

    //Assingables
    [Header("Assignables")]
    public Transform playerCam;
    public Transform orientation;
    [Space(10)]

    [Header("Physics")]
    //Other
    public Rigidbody rb;
    [Space(10)]

    [Header("Rotation and Look")]
    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;
    [Space(10)]

    [Header("Movement")]
    public Vector3 movementDir;
    //Movement
    public float moveSpeed = 4500;
    //public float[] maxSpeed;    
    public float maxSpeed;
    public bool justLeapt;
    public bool grounded;
    public LayerMask whatIsGround;
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;

    //QTE Accélération
    //public int speedThreshold;
    //public float maxTimeToSpeedBoost;
    //float timeSpeedBoostRemaining;
    //public GameObject speedBoostPrompt;
    [Space(10)]

    [Header("Crouch & Slide")]
    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    [Space(10)]

    [Header("Jump")]
    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 1;
    public float jumpForce = 550f;
    [Space(10)]

    [Header("Input")]
    //Input
    float horizontalInput, verticalInput;
    public bool jumping, sprinting, crouching;
    [Space(10)]

    [Header("Sliding")]
    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;
    [Space(10)]

    [Header("Slope")]
    //Slope
    public float maxSlopeAngle = 35f;
    private Vector3 hitPointNormal;
    private Vector3 slopeMoveDirection;
    public float slopeSlidingSpeed;
    public float slopeAngle;
    RaycastHit slopeHit;

    public bool isOnSlope;
    [Space(10)]

    [Header("Dash")]
    public bool dashing;
    public float maxDashDistance = 5.0f;
    private float dashDistance = 5.0f;
    public float dashDuration = 0.15f;
    public Target[] dashTargets;
    public Target currentTarget;
    Target lastTarget;
    [Space(10)]

    [Header("Glide")]
    public Camera myCam;
    public float glideSpeed = 12.5f;
    public float drag = 6f;
    public Vector3 myRotation;
    public float percentage;
    public float quickSpeed;
    public float inputTest;
    [Space(10)]

    public Transform groundSphere;
    public float sphereRadius;

    public bool wantsGlinding;


    public float speedModifier = 20f;

    public Vector3 characterVelocity { get; set; }




    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        dashTargets = FindObjectsOfType<Target>();
        currentTarget = GetClosestEnemy(dashTargets);
        if (currentTarget != null)
        {
            currentTarget.MakeActive(true);
            lastTarget = currentTarget;
        }
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
            if (justLeapt)
            {
                justLeapt = false;
                //timeSpeedBoostRemaining = maxTimeToSpeedBoost;
            }
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
        else if (wantsGlinding && !grounded)
        {
            Glid();
        }

        //QTE ACCELERATION
        //if (timeSpeedBoostRemaining > 0)
        //{
        //    speedBoostPrompt.SetActive(true);
        //    timeSpeedBoostRemaining -= Time.deltaTime;
        //    if (Input.GetKeyDown(KeyCode.F))
        //    {
        //        if (speedThreshold < maxSpeed.Length - 1)
        //        {
        //            speedThreshold++;
        //            speedUp(3);
        //        }
        //        timeSpeedBoostRemaining = 0;

        //    }
        //}
        //else
        //{
        //    speedBoostPrompt.SetActive(false);
        //}
    }

    public Transform directionTransform;

    private void Update()
    {
        MyInput();

        //ROTATE PLAYER WITH DIRECTION

        directionTransform.position =(transform.position)+ orientation.transform.forward;

        Vector3 lookAtPos = directionTransform.position - orientation.transform.position;

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            lookAtPos.y = 0; // do not rotate the player around x
            Quaternion newRotation = Quaternion.LookRotation(lookAtPos, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 8);

        } // Quaternion newRotation = Quaternion.LookRotation(lookAtPos, transform.up);
        //transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 8);



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
        if (isWallRunning && iswalljumping)
        {
            WallJump();
        }
        if (isWallRunning)
        {
            SideJump();
        }

        currentTarget = GetClosestEnemy(dashTargets);
        if (currentTarget != lastTarget)
        {
            if (lastTarget != null)
            {
                lastTarget.MakeActive(false);
            }
            if (currentTarget != null)
            {
                currentTarget.MakeActive(true);
                lastTarget = currentTarget;
            }
            else
            {
                lastTarget = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentTarget != null)
        {
            Dash();
        }

    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        movementDir = new Vector3(horizontalInput, 0, verticalInput);
        movementDir.Normalize();

        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);
        if (Input.GetKeyDown(KeyCode.E) && !CheckIfGrounded())
        {
            justLeapt = false;
            wantsGlinding = !wantsGlinding;

            if (wantsGlinding)
                myRotation = orientation.transform.eulerAngles;
        };


        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();

       

    }

    #region Movement

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
        CounterMovement(horizontalInput, verticalInput, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        //float maxSpeed = this.maxSpeed[speedThreshold];     
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (horizontalInput > 0 && xMag > maxSpeed) horizontalInput = 0;
        if (horizontalInput < 0 && xMag < -maxSpeed) horizontalInput = 0;
        if (verticalInput > 0 && yMag > maxSpeed) verticalInput = 0;
        if (verticalInput < 0 && yMag < -maxSpeed) verticalInput = 0;

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

        slopeMoveDirection = Vector3.ProjectOnPlane(orientation.transform.forward * verticalInput * multiplier * multiplierV, slopeHit.normal);

        if (isOnSlope && grounded)
        {
            rb.AddForce(Time.deltaTime * slopeSlidingSpeed * slopeHit.normal.normalized);
        }
        else
        {

            //Apply forces to move player
            rb.AddForce(orientation.transform.forward * verticalInput * moveSpeed * Time.deltaTime * multiplier * multiplierV);
            rb.AddForce(orientation.transform.right * horizontalInput * moveSpeed * Time.deltaTime * multiplier);
        }
    }
    private float desiredX;
    //private void Look()
    //{
    //    float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
    //    float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

    //    //Find current look rotation
    //    Vector3 rot = playerCam.transform.localRotation.eulerAngles;
    //    desiredX = rot.y + mouseX;

    //    //Rotate, and also make sure we dont over- or under-rotate.
    //    xRotation -= mouseY;
    //    xRotation = Mathf.Clamp(xRotation, -90f, 90f);

    //    //Perform the rotations
    //    playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
    //    orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

    //    if (!wantsGlinding)
    //    {
    //        transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.y, 0));
    //    }
    //}
    private void Glid()
    {
        myRotation.x += 20 * Input.GetAxis("Vertical") * Time.deltaTime;
        myRotation.x = Mathf.Clamp(myRotation.x, -20, 45);

        myRotation.y += 40 * Input.GetAxis("Horizontal") * Time.deltaTime;

        myRotation.z = -5 * Input.GetAxis("Horizontal");
        myRotation.z = Mathf.Clamp(myRotation.z, -30, 30);

        transform.rotation = Quaternion.Euler(myRotation);

        percentage = myRotation.x / 45;

        float modifiedDrag = (percentage * -2) + 6;
        float modifiedSpeed = ((glideSpeed + 3.6f) - glideSpeed) + glideSpeed;

        rb.drag = modifiedDrag;
        Vector3 localV = transform.InverseTransformDirection(rb.velocity);
        localV.z = modifiedSpeed + 20 * Input.GetAxis("Vertical") * Time.deltaTime;
        //Debug.Log(localV.z);
        rb.velocity = transform.TransformDirection(localV);


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


        glideSpeed = glideSpeed * 2;
        yield return new WaitForSeconds(waitTime);
        glideSpeed = glideSpeed / 2;
        yield return null;


    }
    #endregion

    #region Jump
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

            //    rb.constraints = RigidbodyConstraints.None;
            jumping = true;
            readyToJump = false;


            //normal jump
            //if ((isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.Q)) && jumping)
            //{
            //    rb.AddForce(Vector2.up * jumpForce * 1.5f);
            //}

            //sidwards wallhop
            //if ((isWallRight || isWallLeft && Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.D)) && jumping) rb.AddForce(-orientation.up * jumpForce * 1f);
            //if (isWallRight && Input.GetKey(KeyCode.A))
            //{
            //    rb.AddForce(-orientation.right * jumpForce * 1f);
            //    rb.AddForce(orientation.up * jumpForce * 0.25f);
            //    justLeapt = true;
            //    timeSpeedBoostRemaining = 0;
            //}
            //if (isWallLeft && Input.GetKey(KeyCode.E))
            //{

            //    rb.AddForce(orientation.right * jumpForce * 1f);
            //    rb.AddForce(orientation.up * jumpForce * 0.25f);
            //    justLeapt = true;
            //    timeSpeedBoostRemaining = 0;
            //}

            if (isWallRight && jumping)
            {
                rb.AddForce(-orientation.right * jumpForce * 0.25f);
                rb.AddForce(orientation.up * jumpForce * 0.25f);
                justLeapt = true;
                //timeSpeedBoostRemaining = 0;
            }
            if (isWallLeft && jumping)
            {

                rb.AddForce(orientation.right * jumpForce * 0.25f);
                rb.AddForce(orientation.up * jumpForce * 0.25f);
                justLeapt = true;
                //timeSpeedBoostRemaining = 0;
            }
            //Always add forward force
            //rb.AddForce(orientation.forward * jumpForce * 1f);


            iswalljumping = false;
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void SideJump()
    {
        if (readyToJump && isWallRunning)
        {

            // rb.constraints = RigidbodyConstraints.None;
            jumping = true;
            readyToJump = false;


            if (isWallRight && Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-orientation.right * jumpForce * 1f);
                rb.AddForce(orientation.up * jumpForce * 0.25f);
                justLeapt = true;
                //timeSpeedBoostRemaining = 0;
            }
            if (isWallLeft && Input.GetKey(KeyCode.E))
            {

                rb.AddForce(orientation.right * jumpForce * 1f);
                rb.AddForce(orientation.up * jumpForce * 0.25f);
                justLeapt = true;
                //timeSpeedBoostRemaining = 0;
            }


            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void ResetJump()
    {
        readyToJump = true;
        jumping = false;
    }
    #endregion

    #region WallRun
    //Wallrunning
    //Variable
    public LayerMask whatIsWall;
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    public bool isWallRight, isWallLeft;
    public bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    public bool iswalljumping = false;
    private void WallRunInput()
    {
        //Wallrun
        if (isWallRight || isWallLeft)
        {
            wantsGlinding = false;
            StartWallrun();
        }

        if (isWallRight && Input.GetKey(KeyCode.Space))
        {
            iswalljumping = true;
            //WallJump();
        }
        if (isWallLeft && Input.GetKey(KeyCode.Space))
        {
            iswalljumping = true;
            //WallJump();
        }
    }
    private void StartWallrun()
    {
        //QTE ACCELERATION
        //if (speedThreshold < maxSpeed.Length - 1)
        //{
        //    timeSpeedBoostRemaining = maxTimeToSpeedBoost;
        //    Debug.Log("Can speed boost");
        //}
        rb.useGravity = false;
        isWallRunning = true;
        //rb.constraints = RigidbodyConstraints.FreezePositionY;

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
        // rb.constraints = RigidbodyConstraints.None;
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
            //   rb.constraints = RigidbodyConstraints.None;
            readyToJump = true;
        }
    }
    #endregion

    #region Dash
    void Dash()
    {

        var direction = (currentTarget.transform.position - this.transform.position);

        // Making sure we have a reasonable vector here
        if (direction.magnitude >= 0.1f)
        {
            // Don't exceed the target, you might not want this
            this.StartCoroutine(this.DashRoutine(direction.normalized));
        }
    }

    IEnumerator DashRoutine(Vector3 direction)
    {
        // Account for some edge cases   
        if (this.dashDistance <= 0.001f)
            yield break;

        if (this.dashDuration <= 0.001f)
        {
            this.transform.position += direction * this.dashDistance;
            yield break;
        }

        // Update our state
        this.dashing = true;
        var elapsed = 0f;
        var start = this.transform.position;
        var target = this.transform.position + this.dashDistance * direction;

        // There are a few different ways to do this, but I've always preferred
        // Lerp for things that have a fixed duration as the interpolant is clear
        while (elapsed < this.dashDuration)
        {
            var iterTarget = Vector3.Lerp(start, target, elapsed / this.dashDuration);
            this.transform.position = iterTarget;

            yield return null;
            elapsed += Time.deltaTime;
        }

        // Snap there when we finish then update our state
        this.transform.position = target;
        this.dashing = false;
    }

    Target GetClosestEnemy(Target[] target)
    {
        Target bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Target potentialTarget in target)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        dashDistance = Vector3.Distance(bestTarget.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0)));

        if (dashDistance <= maxDashDistance && bestTarget.isVisible == true)
        {
            return bestTarget;
        }
        else
        {
            return null;
        }

    }
    #endregion
}

