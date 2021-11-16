using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControllerSP : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
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

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Instantiate(Bullet, firePoint.position, firePoint.rotation);
            RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);
            SpiderEnemy spiderEnemy = hitInfo.transform.GetComponent<SpiderEnemy>();
            if (spiderEnemy !=null)
            {
                spiderEnemy.TakeDamage(Damage);
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        transform.Rotate(0f, 180f, 0f);
    }
}

