using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Material red, green;
    public Renderer rend;
    public bool isCurrent;
    public bool isVisible;
    private void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit target");
        }
    }

    public void MakeActive(bool shouldBeActive)
    {
        if (shouldBeActive)
        {
            rend.material = green;
        }
        else
        {
            rend.material = red;
        }
    }

    public void OnBecameInvisible()
    {
        isVisible = false;
    }

    public void OnBecameVisible()
    {
        isVisible = true;
    }


}
