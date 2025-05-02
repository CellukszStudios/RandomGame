using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerHealthSystem : MonoBehaviourPunCallbacks
{
    [Header("PlayerHealth Variables")]
    public float health;
    public float maxHealth;

    [Header("Script Attributes")]
    private PhotonView view;
    private CharacterController characterController;
    public TMP_Text HealthUI;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        characterController = GetComponent<CharacterController>();
    }

    public void Damage(float damage)
    {
        view.RPC("RPC_Damage", RpcTarget.All, damage);
    }

    [PunRPC]
    public void RPC_Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
            Death();
    }

    public void Death()
    {
        characterController.enabled = false;
        transform.position = new Vector3(0f, 10f, 0f);
        characterController.enabled = true;
        health = maxHealth;
    }
}
