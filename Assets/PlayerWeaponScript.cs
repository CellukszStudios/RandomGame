using UnityEngine;
using Photon.Pun;

public class PlayerWeaponScript : MonoBehaviourPunCallbacks
{
    [Header("Weapon Attributes")]
    public float shootDelay = 0.1f;
    public float reloadTime = 2f;
    public int maxAmmo = 30;
    public int currentAmmo = 30;
    public int shootDistance = 3000;
    public int damage = 15;

    [Header("Script Variables")]
    public GameObject ShootSound;
    public GameObject MuzzleFlashObj;
    public GameObject HitEffect;
    public GameObject HitEffectSound;
    public Transform spawn;
    public Transform hand_target;

    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }
}
