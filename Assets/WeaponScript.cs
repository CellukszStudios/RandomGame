using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WeaponScript : MonoBehaviourPunCallbacks
{
    [Header("Weapon Variables")]
    public float shoot_delay = 0.5f;
    public float current_ammo = 10;
    public float max_ammo = 10;
    public float reload_time;

    [Header("Weapon Attributes")]
    private PhotonView view;
    public GameObject MuzzleFlash;
    public GameObject ShotSound;
    public Transform muzzle_spawn;
    bool canShoot = true;
    public bool isReloading = false;

    [Header("Enemy Attributes")]
    public Animator anim;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (view.ViewID == 0)
        {
            PhotonNetwork.AllocateViewID(view);
        }
    }

    public void Shoot()
    {
        if (!canShoot)
        {
            return;
        }

        if (current_ammo == 0 && !isReloading)
        {
            StartCoroutine(Reload());
            return;
        }

        if (current_ammo == 0) return;

        canShoot = false;
        PhotonNetwork.Instantiate(MuzzleFlash.name, muzzle_spawn.position, muzzle_spawn.rotation);
        PhotonNetwork.Instantiate(ShotSound.name, muzzle_spawn.position, Quaternion.identity);
        current_ammo--;
        StartCoroutine(ShootDelay());
    }

    [PunRPC]
    public void StartReloadAnimation()
    {
        anim.SetTrigger("Reload");
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(shoot_delay);
        canShoot = true; 
    }

    IEnumerator Reload()
    {
        isReloading = true;
        view.RPC("StartReloadAnimation", RpcTarget.All);
        yield return new WaitForSeconds(reload_time);
        current_ammo = max_ammo;
        isReloading = false;
    }
}
