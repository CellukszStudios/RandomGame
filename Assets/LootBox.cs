using Photon.Pun;
using UnityEngine;

public class LootBox : MonoBehaviour
{
    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void OpenBox()
    {
        if (!view.IsMine) return;

        view.RPC("OpenBoxRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void OpenBoxRPC()
    {
        Destroy(gameObject);
    }
}
