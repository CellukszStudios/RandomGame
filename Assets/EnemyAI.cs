using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EnemyAI : MonoBehaviourPunCallbacks
{
    [Header("Team System")]
    public bool isLeader = false;
    public bool isRecruited = false;
    public int MaxTeamSize = 10;
    public List<GameObject> TeamMembers = new List<GameObject>();

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
    public LayerMask FriendlyLayer;

    [Header("Weapons")]
    public GameObject Weapon;
    bool hasWeapon = false;
    bool isReloading = false;
    private WeaponScript weapon_script;

    public GameObject EnemyPlayer;
    public GameObject Boss;
    public RecruiteList recruite_list;
    public Rigidbody[] rbs;
    bool isDead = false;

    public enum states
    {
        Roam,
        Looting,
        Reloading,
        Attack
    }

    private states current_state;

    private void Awake()
    {
        GameObject recruit_list_obj = GameObject.Find("RecruitableEnemyList");
        recruite_list = recruit_list_obj.GetComponent<RecruiteList>();
        if (!isLeader)
            recruite_list.RecruitList.Add(gameObject);
    }

    private void Start()
    {
        view = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = true;
        }

        if (view.IsMine)
        {
            current_state = states.Roam;
            StartTeamChecking();
        }
    }

    public void StartTeamChecking()
    {
        StartCoroutine(CheckTeam());
    }

    public Vector3 GetRandomPosAroundEnemy(float dist)
    {
        Vector3 pos = (EnemyPlayer.transform.right + EnemyPlayer.transform.forward) * UnityEngine.Random.Range(-dist, dist);

        if (Vector3.Distance(pos, EnemyPlayer.transform.position) >= 40)
            return pos;
        else
            return GetRandomPosAroundEnemy(dist);
    }

    public GameObject GetRandomWeapon()
    {
        GameObject Weapon = Weapons[Weapons.Length - 1];
        return Weapon;
    }

    public GameObject GetNearestLootBox()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, Radius, LootBox);
        if (cols.Length == 0) return null;

        GameObject NearestLootbox = cols[0].gameObject;

        foreach (Collider col in cols)
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
        if (isDead)
            return;

        UpdateAnimations();
        StateManager();
        Behaviour();
    }

    void Behaviour()
    {
        if (isLeader && TeamMembers.Count < 10) Recruit();
        if (agent.hasPath && Vector3.Distance(transform.position,agent.destination) < 3) MoveToRandpos();
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

    void MoveToRandpos()
    {
        float RandX = UnityEngine.Random.Range(-2000, 2000);
        float RandZ = UnityEngine.Random.Range(-2000, 2000);

        Vector3 RandPos = new Vector3(RandX, 4, RandZ);
        agent.SetDestination(RandPos);
        if (isLeader && TeamMembers.Count > 0)
        {
            foreach (GameObject member in TeamMembers)
            {
                if (member)
                {
                    Vector3 RandRandPos = new Vector3(RandPos.x + UnityEngine.Random.Range(-15, 15), RandPos.y, RandPos.z + UnityEngine.Random.Range(-15, 15));
                    member.GetComponent<EnemyAI>().agent.SetDestination(RandRandPos);
                }
            }
        }
    }

    void Roam()
    {
        EnemyPlayer = GetNearestEnemy();
        if (isRecruited)
        {
            agent.SetDestination(Boss.GetComponent<EnemyAI>().agent.destination);
            return;
        }

        if (isLeader && EnemyPlayer && TeamMembers.Count > 0)
        {
            foreach (GameObject member in TeamMembers)
            {
                if (member)
                    member.GetComponent<EnemyAI>().EnemyPlayer = EnemyPlayer;
            }
        }

        if (agent.hasPath) return;
        MoveToRandpos();
    }

    void Recruit()
    {
        foreach (GameObject member in recruite_list.RecruitList)
        {
            if (TeamMembers.Count < MaxTeamSize)
            {
                if (!member.GetComponent<EnemyAI>().isRecruited)
                {
                    TeamMembers.Add(member);
                    member.GetComponent<EnemyAI>().isRecruited = true;
                    member.GetComponent<EnemyAI>().Boss = gameObject;
                }
            }
        }
    }

    void Attack()
    {
        if (!EnemyPlayer) return;

        if (Vector3.Distance(transform.position, EnemyPlayer.transform.position) <= StoppingDistance)
            agent.SetDestination(transform.position);
        else
            agent.SetDestination(EnemyPlayer.transform.position);

        Vector3 direction = new Vector3(EnemyPlayer.transform.position.x, EnemyPlayer.transform.position.y-EnemyPlayer.transform.localScale.y/2, EnemyPlayer.transform.position.z) - transform.position;
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
            int index = Weapons.ToList().IndexOf(Weapon);
            view.RPC("EnableWeapon", RpcTarget.AllBuffered, index);
            view.RPC("OpenBoxRPCAnim", RpcTarget.All);
            LootBox.GetComponent<LootBox>().OpenBox();
            weapon_script = Weapon.GetComponent<WeaponScript>();
            agent.SetDestination(transform.position);
            MoveToRandpos();
        }
    }

    public void Damage(float amount)
    {
        view.RPC("RPC_Damage", RpcTarget.AllBuffered, amount);
    }

    [PunRPC]
    public void RPC_Damage(float amount) 
    { 
        Health -= amount;
        if (Health <= 0)
            Death();
    }

    public void Death()
    {
        isDead = true;
        animator.enabled = false;
        Destroy(agent);
        Destroy(GetComponent<CapsuleCollider>());
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
        }
    }

    [PunRPC]
    void OpenBoxRPCAnim()
    {
        animator.SetTrigger("OpenBox");
    }

    [PunRPC]
    void EnableWeapon(int index)
    {
        Weapon = Weapons[index];
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

    IEnumerator CheckTeam()
    {
        yield return new WaitForSeconds(10);
        foreach (GameObject member in TeamMembers)
        {
            if (!member)
                TeamMembers.Remove(member);
        }
        StartTeamChecking();
    }
}
