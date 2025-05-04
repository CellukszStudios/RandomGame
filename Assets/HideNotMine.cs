using UnityEngine;
using Photon.Pun;

public class HideNotMine : MonoBehaviourPunCallbacks
{
    private PhotonView view;
    public SkinnedMeshRenderer SkinnedMeshRenderer;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
            SkinnedMeshRenderer.enabled = false;
    }
}
