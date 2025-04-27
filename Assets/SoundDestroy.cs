using UnityEngine;
using Photon.Pun;
using System.Collections;

public class SoundDestroy : MonoBehaviourPunCallbacks
{
    private PhotonView view;
    public float time_delay = 1.0f;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine) StartCoroutine(DestroyDelay());
    }

    IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(time_delay);
        PhotonNetwork.Destroy(gameObject);
    }
}
