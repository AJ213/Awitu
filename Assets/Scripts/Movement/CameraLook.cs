using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public static float MouseSensitivityX = 100f;
    public static float MouseSensitivityY = 100f;
    [SerializeField] Transform playerTranform = default;
    [SerializeField] Transform weaponTranform = default;

    float xRotation = 0;
    void Update()
    {
        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X") * MouseSensitivityX * Time.deltaTime, Input.GetAxis("Mouse Y") * MouseSensitivityY * Time.deltaTime);
        
        xRotation -= mouse.y;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        this.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        weaponTranform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerTranform.Rotate(Vector3.up * mouse.x);
    }
}
