using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMovement : MonoBehaviour
{
    public enum movementState { running, gliding };

    [Header("Physics")]
    public Rigidbody rb;
    [Space(10)]

    [Header("State")]
    public movementState moveState;
    [Space(10)]

    [Header("Glide")]
    public float glideSpeed = 12.5f;
    public float drag = 6f;
    public Vector3 myRotation;
    public float percentage;
    [Space(10)]

    [Header("Run")]
    public float runSpeed;
    [Space(10)]

    [Header("Dash")]
    public bool dashing;
    public float maxDashDistance = 5.0f;
    private float dashDistance = 5.0f;
    public float dashDuration = 0.15f;
    public Transform dashTarget;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        myRotation = transform.eulerAngles;
    }

    private void Update()
    {
        if (moveState == movementState.gliding)
        {
            myRotation.x += 20 * Input.GetAxis("Vertical") * Time.deltaTime;
            myRotation.x = Mathf.Clamp(myRotation.x, -20, 45);

            myRotation.y += 20 * Input.GetAxis("Horizontal") * Time.deltaTime;

            myRotation.z = -5 * Input.GetAxis("Horizontal");
            myRotation.z = Mathf.Clamp(myRotation.z, -10, 10);

            transform.rotation = Quaternion.Euler(myRotation);

            percentage = myRotation.x / 45;

            float modifiedDrag = (percentage * -2) + 6;
            float modifiedSpeed = ((glideSpeed + 3.6f) - glideSpeed) + glideSpeed;

            rb.drag = modifiedDrag;
            Vector3 localV = transform.InverseTransformDirection(rb.velocity);
            localV.z = modifiedSpeed;
            rb.velocity = transform.TransformDirection(localV);
        }
        if (moveState == movementState.running)
        {
            rb.drag = 0;
            transform.rotation = Quaternion.identity;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashDistance = Vector3.Distance(dashTarget.position, transform.position);
            if (dashDistance <= maxDashDistance) { 
                Dash();
            }
        }
    }

    private void Movement()
    {
       
    }

    void Dash()
    {
        
        var direction = (dashTarget.position - this.transform.position);

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
}
