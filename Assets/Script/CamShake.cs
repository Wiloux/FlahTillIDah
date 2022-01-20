using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    public GlideMovement glideController;
    public float shaking = .5f;
    public Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        float modifiedShaking =  shaking * glideController.percentage;
        transform.localPosition = originalPos + new Vector3(Random.Range(-modifiedShaking, modifiedShaking), Random.Range(-modifiedShaking, modifiedShaking), 0);
    }
}
