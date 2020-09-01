using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        this.transform.LookAt(this.transform.position + cam.forward);
    }
}
