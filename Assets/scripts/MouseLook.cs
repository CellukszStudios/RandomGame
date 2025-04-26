using UnityEngine;
using Photon.Pun;

public class MouseLook : MonoBehaviourPunCallbacks
{
    public float mouseSens = 1f;

    public Transform playerBody;

    float xRot = 0;

    [Header("Networking")]
    private PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Networking
        if (!view.IsMine) Destroy(gameObject);
    }

    void Update()
    {
        if (!view.IsMine) return;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSens;
        float mouseX = Input.GetAxis("Mouse X") * mouseSens;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
