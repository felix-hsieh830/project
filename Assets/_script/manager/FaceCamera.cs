using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 dirToCamera = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-dirToCamera);
        }
    }
}