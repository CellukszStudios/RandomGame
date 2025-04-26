using Photon.Pun;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    float StartSpeed;
    public float speed = 12f;
    public float gravity = -9.81f;

    public Transform gndCheck;
    public float gndDistance = 0.4f;
    public LayerMask gndMask;

    public float jumpHeight = 3f;

    Vector3 velocity;
    bool isGrounded;

    [Header("Animations")]
    public Animator animator;

    //Networking
    private PhotonView view;

    bool isTwerking = false;

    void Start()
    {
        StartSpeed = speed;
        characterController = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!view.IsMine) return;

        isGrounded = Physics.CheckSphere(gndCheck.position, gndDistance, gndMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetFloat("x", z);
            animator.SetFloat("y", x);
            speed = 6;
        }else if (!Input.GetKey(KeyCode.LeftShift))
        {
            float x2 = x / 2;
            float z2 = z / 2;
            animator.SetFloat("x", z2);
            animator.SetFloat("y", x2);
            speed = StartSpeed;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isTwerking)
            {
                animator.SetBool("isTwerking", false);
                isTwerking = false;
            }
            else
            {
                animator.SetBool("isTwerking", true);
                isTwerking = true;
            }
        }

        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }
}
