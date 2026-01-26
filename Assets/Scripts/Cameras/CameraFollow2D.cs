using System;
using UnityEngine;

namespace Cameras
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")] public Transform target;

        [Header("Smoothing")] public float smoothSpeed = 0.125f;

        [Header("Offset")] public Vector3 offset = new Vector3(0, 2, -10);

        private void LateUpdate()
        {
            if (target == null) return;
            
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}