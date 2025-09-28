using Photon.Pun;
using UnityEngine;

public class BuildingSystem : MonoBehaviourPunCallbacks
{
    [Header("Building Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Player Stuff")]
    public Transform playerCamera;
    public Transform playerBody;
    public float maxBuildDistance = 5f;

    [Header("BuildingSystem values")]
    private int buildingIndex = 0;
    public LayerMask GND;
    public GameObject BuldingMenu;

    [Header("Network Stuff")]
    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void SetBuildingIndex()
    {
        if (Input.GetKeyDown("1"))
        {
            buildingIndex = 1; // Wall
        }
        if (Input.GetKeyDown("2"))
        {
            buildingIndex = 2; // Floor
        }
    }

    private void DestroyBuilding()
    {
        if (BuldingMenu.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxBuildDistance))
            {
                if (hit.collider.CompareTag("Buildable"))
                {
                    hit.collider.gameObject.GetComponent<BuildableObject>().DestroySelf();
                }
            }
        }
    }

    private void Build()
    {
        if (!BuldingMenu.activeSelf) return;

        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxBuildDistance, GND))
            {
                Vector3 spawnPos = hit.point;

                switch (buildingIndex)
                {
                    case 1:
                        PhotonNetwork.Instantiate(wallPrefab.name, spawnPos, playerBody.transform.rotation);
                        break;
                    case 2:
                        PhotonNetwork.Instantiate(floorPrefab.name, spawnPos, playerBody.transform.rotation);
                        break;
                }
            }
        }
    }

    private void OpenCloseBuildMenu()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            BuldingMenu.SetActive(!BuldingMenu.activeSelf);
        }
    }

    private void Update()
    {
        if (!view.IsMine) return;

        Build();
        SetBuildingIndex();
        OpenCloseBuildMenu();
        DestroyBuilding();
    }
}
