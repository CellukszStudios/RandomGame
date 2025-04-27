using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MuzzleDespwaner : MonoBehaviourPunCallbacks
{
    private PhotonView view;
    public float despawnTime = 1f;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
            StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawnTime);
        PhotonNetwork.Destroy(gameObject);
    }
}
