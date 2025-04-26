using UnityEngine;
using Photon.Pun;

public class SpawnPlayer : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;

    private void Start()
    {
        PhotonNetwork.Instantiate(PlayerPrefab.name, transform.position, Quaternion.identity);
    }
}
