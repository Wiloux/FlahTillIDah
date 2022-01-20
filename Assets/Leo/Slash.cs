using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    public PlayerController pc;


    private void OnTriggerEnter(Collider other)
    {
        
    

        if (other.CompareTag("Destructible"))
        {
            Debug.Log("Slash");
            pc.SpeedSlash();
            Destroy(other.gameObject);
            
        }


    }

}
