using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]

public class ClientPlayerController : NetworkBehaviour
{
    // public NetworkVariable<float> horizontal = new NetworkVariable<float>();
    private PlayerInput playerInput;
    public Animator animator;
    private PlayerInputAction playerInputAction;
    private Rigidbody2D rb;

    // Movement and Jump variable
    private float horizontal;
    public float speed, jumpForce;
    private float highJump, ultraJump;
    private int extraJump;
    public int extraJumpValue;
    private bool facingRight = true;

    // Shooting Variable
    public GameObject Bullet;
    public Transform firePoint;
    public int Damage = 10;

    // Check if player in the ground variable
    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask groundLayer;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        playerInput = GetComponent<PlayerInput>();
    }
    private void Start()
    {
        extraJump = extraJumpValue;
        rb = GetComponent<Rigidbody2D>();
        playerInputAction.Player.Movement.performed += ctx => setMovement(ctx.ReadValue<Vector2>().x);
        playerInputAction.Player.Movement.canceled += ctx => ResetMovement();
        playerInputAction.Player.Jump.started += ctx => Jump();
        playerInputAction.Player.Shoot.started += ctx => Shoot();
    }

    private void OnEnable()
    {
        playerInputAction.Enable();
    }
    private void OnDisable()
    {
        playerInputAction.Disable();
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        
    }
    private void Update()
    {
        // if (!IsOwner) { return; }

        animator.SetFloat("Speed", Mathf.Abs(horizontal * speed));
        Move();

        if (facingRight == false && horizontal > 0)
        {
            Flip();
        }
        else if (facingRight == true && horizontal < 0)
        {
            Flip();
        }
        rb.constraints = RigidbodyConstraints2D.FreezePositionY;
    }

    private void setMovement(float movement)
    {
        if (!IsOwner) { return; }
        horizontal = movement;
    }

    private void ResetMovement()
    {
        if (!IsOwner) { return; }
        horizontal = Vector2.zero.x;
    }

    public void Move()
    {
        if (!IsOwner) { return; }

        // horizontal = playerInputAction.Player.Movement.ReadValue<Vector2>().x;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);


    }


    private bool IsGrounded()
    {
        extraJump = extraJumpValue;
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    public void Jump()
    {
        if (!IsOwner) { return; }

        if (extraJump == 0 && IsGrounded())
        {
            rb.velocity = Vector2.up * jumpForce;
        }
        else if (extraJump > 0)
        {
            rb.velocity = Vector2.up * jumpForce;
            extraJump--;
        }
        else if (rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

    }

    void Flip()
    {
        facingRight = !facingRight;

        transform.Rotate(0f, 180f, 0f);
    }

    public void Shoot()
    {
        if (!IsOwner) { return; }

        Instantiate(Bullet, firePoint.position, firePoint.rotation);
        RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);
        SpiderEnemy spiderEnemy = hitInfo.transform.GetComponent<SpiderEnemy>();
        if (spiderEnemy != null)
        {
            spiderEnemy.TakeDamage(Damage);
        }
    }
}
