using UnityEngine;
using Photon.Pun;
using TMPro;

public class CreateJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField connectInput;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(connectInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("OutdoorsScene");
    }
}
