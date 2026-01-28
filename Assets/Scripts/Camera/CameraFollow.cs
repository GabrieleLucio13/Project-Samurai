/* using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Player
    [SerializeField] private float distance = 7f;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;

    private float _yaw;
    private float _pitch;

    private void FreeCamera()
    {
        _yaw += Input.GetAxis("Mouse X") * sensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position = target.position + rotation * Vector3.back * distance;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    }
*/
