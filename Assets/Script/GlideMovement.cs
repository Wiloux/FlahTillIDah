using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMovement : MonoBehaviour
{
    public enum movementState{ running, gliding };

    [Header("Physics")]
    public Rigidbody rb;
    [Space(10)]

    [Header("State")]
    public movementState moveState;
    [Space(10)]

    [Header("Glide")]
    public float glideSpeed = 12.5f;
    public float drag = 6f;
    private Vector3 myRotation;
    public float percentage;
    [Space(10)]

    [Header("Run")]
    public float runSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        myRotation = transform.eulerAngles;
    }

    private void Update()
    {
        if(moveState == movementState.gliding) { 
        myRotation.x += 20 * Input.GetAxis("Vertical") * Time.deltaTime;
        myRotation.x = Mathf.Clamp(myRotation.x, -45, 45);

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
        if(moveState == movementState.running)
        {
           
        }
    }
}
