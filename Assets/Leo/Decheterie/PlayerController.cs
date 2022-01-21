using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    public GameObject SlashCollider;

    public GroundMovement gm;
    public void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {

            StartCoroutine(SlashAttack(0.5f));
        }
        

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

    
}
