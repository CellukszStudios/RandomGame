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

    public states current_state;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!view.IsMine) return;
        UpdateAnimations();

        StateManager();

    }

    void StateManager()
    {
        switch (current_state)
        {
            case states.Roam:
                Roam();
                return;
            case states.Attack:
                Roam();
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

    }

    void UpdateAnimations()
    {
        if (agent.hasPath)
            animator.SetFloat("Blend", 1);
        else
            animator.SetFloat("Blend", 0);
    }
}
