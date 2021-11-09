using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControllerSInglePlayer : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    public Animator animator;
    private PlayerInputAction playerInputAction;
    private float horizontal;
    public float speed;
    public float jumpForce;
    private float highJump;
    private float ultraJump;

    private Rigidbody2D rb;

    private bool facingRight = true;

    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask groundLayer;

    private int extraJump;
    public int extraJumpValue;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputAction = new PlayerInputAction();
    }
    private void OnEnable()
    {
        playerInputAction.Enable();
    }
    private void OnDisable()
    {
        playerInputAction.Disable();
    }

    private void Start()
    {
        extraJump = extraJumpValue;
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(horizontal * speed));
        if (facingRight == false && horizontal > 0)
        {
            Flip();
        }
        else if (facingRight == true && horizontal < 0)
        {
            Flip();
        }
    }
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
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
}

