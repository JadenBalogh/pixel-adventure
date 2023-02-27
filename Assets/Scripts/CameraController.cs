using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followTime = 0.1f;
    [SerializeField] private float shakeDecay = 1f;

    private Vector2 currVel;
    private float shakeForce = 0f;

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPos = Vector2.SmoothDamp(transform.position, target.position, ref currVel, followTime);
        transform.position = targetPos + Vector3.forward * transform.position.z;
    }

    private void Update()
    {
        transform.position += (Vector3)Random.insideUnitCircle * shakeForce * Time.deltaTime;

        shakeForce = Mathf.Max(shakeForce - shakeDecay * Time.deltaTime, 0f);
    }

    public static void CameraShake(float force)
    {
        Camera.main.GetComponent<CameraController>().shakeForce += force;
    }
}
