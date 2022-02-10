using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Material red, green;
    public Renderer rend;
    public bool isCurrent;
    public bool isVisible;
    public GroundMovement player;
    private void Start()
    {
        player = FindObjectOfType<GroundMovement>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isVisible = false;
            Debug.Log("Player hit target");
            player.DestroyCrystal(gameObject);
            Destroy(gameObject);
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
