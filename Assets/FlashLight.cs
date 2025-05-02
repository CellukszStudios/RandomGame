using UnityEngine;
using Photon.Pun;

public class FlashLight : MonoBehaviourPunCallbacks
{
    private PhotonView view;

    public GameObject Flashlight;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void Update()
    {
        if (!view.IsMine) return;

        if (Input.GetKeyDown(KeyCode.F) && !Flashlight.active)
        {
            view.RPC("FlashOn", RpcTarget.AllBuffered);
        }else if (Input.GetKeyDown(KeyCode.F) && Flashlight.active)
        {
            view.RPC("FlashOff", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void FlashOn()
    {
        Flashlight.SetActive(true);
    }

    [PunRPC]
    public void FlashOff()
    {
        Flashlight.SetActive(false);
    }
}
