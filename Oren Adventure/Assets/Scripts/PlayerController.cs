using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float horizontal;
    private PlayerInputAction playerInputAction;
    public float speed;
    public float jumpForce;
    private float highJump;
    private float ultraJump;

    public Animator animator;
    private Rigidbody2D rb;

    private bool facingRight = true;

    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask groundLayer;

    private int extraJump;
    public int extraJumpValue;

    private Camera _camera;
    private float lastXPost;
    public GameObject farBackground;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        playerInput = GetComponent<PlayerInput>();

        //playerInputAction.Player.Enable();
        // playerInputAction.Player.Jump.performed += Jump;
        // playerInputAction.Player.Movement.performed += Move;
    }

    private void OnEnable()
    {
        playerInputAction.Enable();
        playerInputAction.Player.Jump.performed += Jump;
        // playerInputAction.Player.Movement. += Move;
    }
    private void OnDisable()
    {
        playerInputAction.Disable();
        playerInputAction.Player.Jump.performed -= Jump;
        // playerInputAction.Player.Movement.performed -= Move;
    }
    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            Move();            
        }
    }

    public void Start()
    {
        if (IsLocalPlayer)
        {
            extraJump = extraJumpValue;
            rb = GetComponent<Rigidbody2D>();
            lastXPost = _camera.transform.position.x;
        }
    }
    private void Update()
    {
        if (IsLocalPlayer)
        {
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

            FollowTarget();
        }
    }

    public void Move()
    {
        horizontal = playerInputAction.Player.Movement.ReadValue<Vector2>().x;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

    }
    private bool IsGrounded()
    {
        extraJump = extraJumpValue;
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && extraJump == 0 && IsGrounded())
        {
            rb.velocity = Vector2.up * jumpForce;
        }
        if (context.performed && extraJump > 0)
        {
            rb.velocity = Vector2.up * jumpForce;
            extraJump--;
        }
        else if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    public void FollowTarget()
    {
        if (IsOwner)
        {
            _camera.transform.position = new Vector3(rb.position.x, rb.position.y, _camera.transform.position.z);

            float amountToMoveX = _camera.transform.position.x - lastXPost;
            farBackground.transform.position = farBackground.transform.position + new Vector3(amountToMoveX, 0f, 0f);

            lastXPost = transform.position.x;
        }
    }
}
