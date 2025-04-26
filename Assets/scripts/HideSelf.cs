using UnityEngine;
using Photon.Pun;

public class HideSelf : MonoBehaviourPunCallbacks
{
    public SkinnedMeshRenderer render;
    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();

        if (view.IsMine) render.enabled = false;
    }
}
