using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviourPunCallbacks
{
    [Header("Enemy Variables")]
    public float Health = 100f;
    public float Radius = 40f;

    [Header("Enemy Attributes")]
    private NavMeshAgent agent;
    private PhotonView view;
    public Animator animator;
    public GameObject[] Weapons;
    public LayerMask LootBox;

    [Header("Weapons")]
    public GameObject Weapon;
    bool hasWeapon = false;

    private GameObject EnemyPlayer;
    private GameObject NearbyLootChest;

    public enum states
    {
        Roam,
        Looting,
        Attack
    }

    private states current_state;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();

        if (view.IsMine) current_state = states.Roam;
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

    private void Update()
    {
        if (!view.IsMine) return;
        UpdateAnimations();

        StateManager();
        Behaviour();
    }

    void Behaviour()
    {
        if (!hasWeapon && GetNearestLootBox() && !EnemyPlayer)
            current_state = states.Looting;
        else if (hasWeapon && !EnemyPlayer)
            current_state = states.Roam;
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
        }
    }

    void Roam()
    {
        if (agent.hasPath) return;

        float RandX = UnityEngine.Random.Range(-500, 500);
        float RandZ = UnityEngine.Random.Range(-500, 500);

        Vector3 RandPos = new Vector3(RandX, 4, RandZ);
        agent.SetDestination(RandPos);
    }

    void Attack()
    {

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
            animator.SetTrigger("OpenBox");
            LootBox.GetComponent<LootBox>().OpenBox();
        }
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
