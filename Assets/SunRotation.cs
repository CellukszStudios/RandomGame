using UnityEngine;
using Photon.Pun;

public class SunRotation : MonoBehaviourPunCallbacks
{
    private PhotonView view;
    public float sunRotSpeed = 0.025f;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            transform.Rotate(sunRotSpeed, 0f, 0f);
    }
}
