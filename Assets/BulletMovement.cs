using Photon.Pun;
using System.Collections;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    private PhotonView view;

    public float speed = 20f;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (view.IsMine)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.Destroy(gameObject);
    }
}
