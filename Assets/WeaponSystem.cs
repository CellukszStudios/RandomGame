using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.Animations.Rigging;

public class WeaponSystem : MonoBehaviourPunCallbacks
{
    [Header("Player Weapon")]
    public GameObject Weapon;

    [Header("Script Variables")]
    public GameObject[] Weapons;
    private PhotonView view;
    public Animator anim;
    public Animator local_anim;
    private PlayerWeaponScript weapon_script;
    public Transform Camera;
    public LayerMask PlayerLayer;
    [Header("AnimationRigging")]
    public Transform LeftHandIK;
    public TwoBoneIKConstraint LeftHandConstraint;

    [Header("WeaponChars")]
    public GameObject WeaponCharAK;

    bool canShoot = true;
    bool isReloading = false;
    bool canReload = true;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!view.IsMine) return;

        UseWeapon();
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (Weapon.active)
            LeftHandIK.position = weapon_script.hand_target.position;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            local_anim.SetFloat("x", z);
            local_anim.SetFloat("y", x);
        }
        else if (!Input.GetKey(KeyCode.LeftShift))
        {
            float x2 = x / 2;
            float z2 = z / 2;
            local_anim.SetFloat("x", z2);
            local_anim.SetFloat("y", x2);
        }
    }

    void TurnOnWeapon()
    {
        Weapon.SetActive(true);
        anim.SetBool("holdingWeapon", true);
        local_anim.SetBool("holdingWeapon", true);
        LeftHandConstraint.weight = 1;
        view.RPC("RPC_TurnOnWeapon", RpcTarget.OthersBuffered);
    }

    void ReloadWeapon()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            isReloading = true;
            StartCoroutine(Reload());
            view.RPC("RPC_ReloadAnimation", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_ReloadAnimation()
    {
        anim.SetTrigger("reload");
    }

    void TurnOffWeapon()
    {
        Weapon.SetActive(false);
        anim.SetBool("holdingWeapon", false);
        local_anim.SetBool("holdingWeapon", false);
        view.RPC("RPC_TurnOffWeapon", RpcTarget.OthersBuffered);
        LeftHandConstraint.weight = 0;
        LeftHandIK.position = Vector3.zero;
    }

    void EquipWeapon()
    {
        if (Input.GetKeyDown("1") && Weapon.active)
        {
            TurnOffWeapon();
        }
        else if (Input.GetKeyDown("1") && !Weapon.active)
        {
            TurnOnWeapon();
        }
    }

    private void UseWeapon()
    {
        if (!Weapon) return;
        weapon_script = Weapon.GetComponent<PlayerWeaponScript>();

        EquipWeapon();
        Shoot();
        ReloadWeapon();
    }

    private void Shoot()
    {
        if (Input.GetMouseButton(0) && Weapon && Weapon.active && weapon_script.currentAmmo > 0 && canShoot)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.position, Camera.forward, out hit, weapon_script.shootDistance, ~PlayerLayer))
            {
                Quaternion rotation = Quaternion.LookRotation(hit.normal);
                PhotonNetwork.Instantiate(weapon_script.HitEffect.name, hit.point, rotation);
                PhotonNetwork.Instantiate(weapon_script.HitEffectSound.name, hit.point, Quaternion.identity);
                if (hit.collider.gameObject.tag == "Enemy")
                {
                    hit.collider.gameObject.GetComponent<EnemyAI>().Damage(weapon_script.damage);
                }
            }
            ShootEffects();
            local_anim.SetTrigger("Fire");
            canShoot = false;
            weapon_script.currentAmmo--;
            StartCoroutine(ShootDelay());
        }
    }

    void ShootEffects()
    {
        GameObject muzFlash = PhotonNetwork.Instantiate(weapon_script.MuzzleFlashObj.name, weapon_script.spawn.position, weapon_script.spawn.rotation);
        muzFlash.transform.SetParent(weapon_script.spawn, true);
        muzFlash.transform.forward = weapon_script.spawn.forward;

        GameObject sound = PhotonNetwork.Instantiate(weapon_script.ShootSound.name, transform.position, Quaternion.identity);
        muzFlash.transform.SetParent(transform, true);
        view.RPC("FireAnimation", RpcTarget.All);
    }

    [PunRPC]
    public void FireAnimation()
    {
        anim.SetTrigger("Fire");
    }

    [PunRPC]
    public void RPC_TurnOnWeapon()
    {
        WeaponCharAK.SetActive(true);
    }

    [PunRPC]
    public void RPC_TurnOffWeapon()
    {
        WeaponCharAK.SetActive(false);
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(weapon_script.shootDelay);
        canShoot = true;
    }

    IEnumerator Reload()
    {
        LeftHandConstraint.weight = 0;
        local_anim.SetTrigger("reload");
        canShoot = false;
        yield return new WaitForSeconds(weapon_script.reloadTime);
        isReloading = false;
        canShoot = true;
        LeftHandConstraint.weight = 1;
        weapon_script.currentAmmo = weapon_script.maxAmmo;
    }
}