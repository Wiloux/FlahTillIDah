using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShakeTest : MonoBehaviour
{
    public GlideMovement moveController;
    public float shaking = .5f;
    public Vector3 originalPos;
    public Camera myCam;

    private void Start()
    {
        originalPos = transform.localPosition;
        
    }

    private void LateUpdate()
    {
        float modifiedShaking =  shaking * moveController.percentage;
        transform.localPosition = originalPos + new Vector3(Random.Range(-modifiedShaking, modifiedShaking), Random.Range(-modifiedShaking, modifiedShaking), 0);
        myCam = gameObject.GetComponent<Camera>();
        myCam.fieldOfView = 100-(moveController.rb.drag) * 10;
        Debug.Log(myCam.fieldOfView);
        myCam.fieldOfView = Mathf.Clamp(myCam.fieldOfView,60, 80);
    }
}
