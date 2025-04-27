using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviourPunCallbacks
{
    [Header("Enemy Variables")]
    public float Health = 100f;
    public float Radius = 40f;
    public float AimSpeed = 5f;
    public float StoppingDistance = 5f;

    [Header("Enemy Attributes")]
    private NavMeshAgent agent;
    private PhotonView view;
    public Animator animator;
    public GameObject[] Weapons;
    public LayerMask LootBox;
    public LayerMask EnemyLayer;

    [Header("Weapons")]
    public GameObject Weapon;
    bool hasWeapon = false;
    bool isReloading = false;
    private WeaponScript weapon_script;

    public GameObject EnemyPlayer;

    public enum states
    {
        Roam,
        Looting,
        Reloading,
        Attack
    }

    private states current_state;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();

        if (view.IsMine) current_state = states.Roam;
    }

    public Vector3 GetRandomPosAroundEnemy(float dist)
    {
        Vector3 pos = (EnemyPlayer.transform.right+EnemyPlayer.transform.forward) * UnityEngine.Random.Range(-dist, dist);

        if (Vector3.Distance(pos, EnemyPlayer.transform.position) >= 40)
            return pos;
        else 
            return GetRandomPosAroundEnemy(dist);
    }

    public GameObject GetRandomWeapon()
    {
        GameObject Weapon = Weapons[Weapons.Length-1];
        return Weapon;
    }

    public GameObject GetNearestLootBox()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, Radius, LootBox);
        if (cols.Length == 0) return null;

        GameObject NearestLootbox = cols[0].gameObject;

        foreach(Collider col in cols)
        {
            if (Vector3.Distance(transform.position, col.gameObject.transform.position) < Vector3.Distance(transform.position, NearestLootbox.transform.position))
                NearestLootbox = col.gameObject;
        }
        return NearestLootbox;
    }

    public GameObject GetNearestEnemy()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, Radius, EnemyLayer);
        if (cols.Length == 0) return null;

        GameObject NearestEnemy = cols[0].gameObject;

        foreach (Collider col in cols)
        {
            if (Vector3.Distance(transform.position, col.gameObject.transform.position) < Vector3.Distance(transform.position, NearestEnemy.transform.position))
                NearestEnemy = col.gameObject;
        }
        return NearestEnemy;
    }

    private void Update()
    {
        if (!view.IsMine) return;
        UpdateAnimations();

        StateManager();
        Behaviour();
    }

    void Behaviour()
    {
        if (weapon_script)
            isReloading = weapon_script.isReloading;

        if (!hasWeapon && GetNearestLootBox() && !EnemyPlayer)
            current_state = states.Looting;
        else if (hasWeapon && !EnemyPlayer)
            current_state = states.Roam;
        else if (hasWeapon && EnemyPlayer && !isReloading)
            current_state = states.Attack;
        else if (hasWeapon && EnemyPlayer && isReloading)
            current_state = states.Reloading;
    }

    void StateManager()
    {
        switch (current_state)
        {
            case states.Roam:
                Roam();
                return;
            case states.Attack:
                Attack();
                return;
            case states.Looting:
                Looting();
                return;
            case states.Reloading:
                Reloading();
                return;
        }
    }

    void Reloading()
    {
        if (!Weapon) return;

        if (EnemyPlayer)
        {
            Vector3 pos = GetRandomPosAroundEnemy(50);
            if (agent.velocity.magnitude <= 0.1)
                agent.SetDestination(pos);
        }
    }

    void Roam()
    {
        EnemyPlayer = GetNearestEnemy();

        if (agent.hasPath) return;

        float RandX = UnityEngine.Random.Range(-500, 500);
        float RandZ = UnityEngine.Random.Range(-500, 500);

        Vector3 RandPos = new Vector3(RandX, 4, RandZ);
        agent.SetDestination(RandPos);
    }

    void Attack()
    {
        if (!EnemyPlayer) return;

        if (Vector3.Distance(transform.position, EnemyPlayer.transform.position) <= StoppingDistance)
            agent.SetDestination(transform.position);
        else
            agent.SetDestination(EnemyPlayer.transform.position);

        Vector3 direction = new Vector3(EnemyPlayer.transform.position.x, EnemyPlayer.transform.localScale.y/2, EnemyPlayer.transform.position.z) - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, AimSpeed);
        }

        WeaponScript weapon_script = Weapon.GetComponent<WeaponScript>();
        weapon_script.Shoot();
    }

    void Looting()
    {
        GameObject LootBox = GetNearestLootBox();

        if (LootBox == null) 
        {
            current_state = states.Roam;
            return;
        }

        if (LootBox)
            agent.SetDestination(LootBox.transform.position);

        if (Vector3.Distance(transform.position, LootBox.transform.position) < 3)
        {
            hasWeapon = true;
            Weapon = GetRandomWeapon();
            view.RPC("EnableWeapon", RpcTarget.AllBuffered);
            view.RPC("OpenBoxRPCAnim", RpcTarget.All);
            LootBox.GetComponent<LootBox>().OpenBox();
            weapon_script = Weapon.GetComponent<WeaponScript>();
        }
    }

    [PunRPC]
    void OpenBoxRPCAnim()
    {
        animator.SetTrigger("OpenBox");
    }

    [PunRPC]
    void EnableWeapon()
    {
        Weapon.SetActive(true);
    }

    [PunRPC]
    void DisableWeapon()
    {
        Weapon.SetActive(false);
    }

    void UpdateAnimations()
    {
        animator.SetFloat("Blend", agent.velocity.magnitude);
        animator.SetBool("hasGun", hasWeapon);
    }
}
