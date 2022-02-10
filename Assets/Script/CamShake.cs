using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamShake : MonoBehaviour
{
    public GroundMovement moveController;
    public float shaking = .5f;
    public Vector3 originalPos;
    public Camera myCam;
    public CinemachineFreeLook cmLook;

    private void Start()
    {
        originalPos = transform.localPosition;

    }

    private void LateUpdate()
    {
        if (moveController.grounded == false)
        {
            float modifiedShaking = shaking * moveController.percentage;
            transform.localPosition = originalPos + new Vector3(Random.Range(-modifiedShaking, modifiedShaking), Random.Range(-modifiedShaking, modifiedShaking), 0);
            //cmLook = gameObject.GetComponent<Camera>();
            cmLook.m_Lens.FieldOfView = moveController.myRotation.x + 80;
            cmLook.m_Lens.FieldOfView = Mathf.Clamp(myCam.fieldOfView, 60, 80);
        }
    }
}
