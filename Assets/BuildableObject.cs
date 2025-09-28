using UnityEngine;
using Photon.Pun;

public class BuildableObject : MonoBehaviourPunCallbacks
{
    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void DestroySelf()
    {
        view.RPC("RPC_DestroySelf", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_DestroySelf()
    {
        Destroy(gameObject);
    }
}
