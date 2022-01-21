using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    public GroundMovement gm;


    private void OnTriggerStay(Collider other)
    {
        
    

        if (other.CompareTag("Destructible"))
        {
            Debug.Log("Slash");
            gm.SpeedSlash();
            Destroy(other.gameObject);
            
        }


    }

}
