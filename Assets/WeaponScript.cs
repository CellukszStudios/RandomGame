using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WeaponScript : MonoBehaviourPunCallbacks
{
    [Header("Weapon Variables")]
    public float shoot_delay = 0.5f;
    public float current_ammo = 10;
    public float max_ammo = 10;
    public float distance = 5000f;
    public float damage = 10f;
    public float reload_time;

    [Header("Weapon Attributes")]
    private PhotonView view;
    public GameObject MuzzleFlash;
    public GameObject BulletImpactEffect;
    public GameObject ShotSound;
    public GameObject ImpactSound;
    public Transform muzzle_spawn;
    public Transform PlayerBody;
    bool canShoot = true;
    public bool isReloading = false;
    public LayerMask PlayerLayer;

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
            return;

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
        RaycastHit hit;
        if (Physics.Raycast(PlayerBody.position+transform.up * 1, PlayerBody.forward, out hit, distance))
        {
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            PhotonNetwork.Instantiate(BulletImpactEffect.name, hit.point, rotation);
            if (hit.collider.gameObject.tag == "Player")
            {
                PlayerHealthSystem health_system = hit.collider.gameObject.GetComponent<PlayerHealthSystem>();
                health_system.Damage(damage);
            }
        }
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
