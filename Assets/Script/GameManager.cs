using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject firstPersPlayer;
    public GameObject thirdPersPlayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            thirdPersPlayer.SetActive(false);
            firstPersPlayer.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            firstPersPlayer.SetActive(false);
            thirdPersPlayer.SetActive(true);
        }
    }
}
