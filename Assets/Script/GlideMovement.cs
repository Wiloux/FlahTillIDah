using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMovement : MonoBehaviour
{
    public float speed = 12.5f;
    public float drag = 6f;

    public Rigidbody rb;

    private Vector3 myRotation;

    public float percentage;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        myRotation = transform.eulerAngles;
    }

    private void Update()
    {
        myRotation.x += 20 * Input.GetAxis("Vertical") * Time.deltaTime;
        myRotation.x = Mathf.Clamp(myRotation.x, -45, 45);

        myRotation.y += 20 * Input.GetAxis("Horizontal") * Time.deltaTime;

        myRotation.z = -5 * Input.GetAxis("Horizontal");
        myRotation.z = Mathf.Clamp(myRotation.z, -5, 5);

        transform.rotation = Quaternion.Euler(myRotation);

        percentage = myRotation.x / 45;

        float modifiedDrag = (percentage * -2) + 6;
        float modifiedSpeed = ((speed + 3.6f) - speed) + speed;
        
        rb.drag = modifiedDrag;
        Vector3 localV = transform.InverseTransformDirection(rb.velocity);
        localV.z = modifiedSpeed;
        rb.velocity = transform.TransformDirection(localV);

    }
}
