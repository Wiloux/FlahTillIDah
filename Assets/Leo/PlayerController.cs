using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 12;
    public float drag = 6;

    public Rigidbody rb;
    private Vector3 rot;

    public float percentage;

    public GameObject SlashCollider;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rot = transform.eulerAngles;
    }
    public void Update()
    {
        //rotate the player
        //X
        rot.x += 20 * Input.GetAxis("Vertical") * Time.deltaTime;
        rot.x = Mathf.Clamp(rot.x, 0, 45);
        //Y
        rot.y += 20 * Input.GetAxis("Horizontal") * Time.deltaTime;
        //Z
        rot.z = -5 * Input.GetAxis("Horizontal");
        rot.z = Mathf.Clamp(rot.z, -5, 5);
        transform.rotation = Quaternion.Euler(rot);

        percentage = rot.x / 45;
        //Drag: fast = 4 and slow = 6
        float mod_drag = (percentage * -2) + 6;
        //Speed: Fast = 14 and slow = 12
        float mod_speed = percentage *(14-12) + 12;
        rb.drag = drag;
        Vector3 localV = transform.InverseTransformDirection(rb.velocity);
        localV.z = speed;
        rb.velocity = transform.TransformDirection(localV);
        if (Input.GetKeyDown(KeyCode.F))
        {

            StartCoroutine(SlashAttack(0.5f));
        }
        

    }

    public void SpeedSlash()
    {
        StartCoroutine(speedUp(2));
    }

    private IEnumerator SlashAttack(float waitTime)
    {
        
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            SlashCollider.GetComponent<Collider>().enabled = true;
            yield return new WaitForSeconds(waitTime);
            SlashCollider.GetComponent<Collider>().enabled = false;

            Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            yield return null;
        
        
    }

    public IEnumerator speedUp(float waitTime)
    {
        
        
            speed=speed*2;
            yield return new WaitForSeconds(waitTime);
            speed = speed/2;
            yield return null;
        

    }
}
