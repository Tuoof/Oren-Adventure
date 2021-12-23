using UnityEngine;
using UnityEngine.InputSystem;

namespace oren_Advent
{
    public class PlayerControllerSP : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        public Animator animator;
        private PlayerInputAction playerInputAction;
        public Rigidbody2D rb;

        // Movement and Jump variable
        private float horizontal;
        public float speed, jumpForce;
        private float highJump, ultraJump;
        private int extraJump;
        public int extraJumpValue;
        private bool facingRight = true;

        // Shooting Variable
        public GameObject Bullet;
        public GameObject firePoint;
        public int Damage = 10;

        // Check if player in the ground variable
        public bool isGrounded;
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
            playerInputAction.Player.Movement.performed += ctx => setMovement(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovement();
            playerInputAction.Player.Jump.started += ctx => Jump();
            playerInputAction.Player.Shoot.started += ctx => Shoot();
        }
        private void Update()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

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

            if (isGrounded == true)
            {
                extraJump = extraJumpValue;
            }
        }
        private void setMovement(float movement)
        {
            horizontal = movement;
        }

        private void ResetMovement()
        {
            horizontal = Vector2.zero.x;
        }
        public void Move(InputAction.CallbackContext context)
        {
            horizontal = context.ReadValue<Vector2>().x;
        }

        public void Jump()
        {
            if (extraJump > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                extraJump--;
            }
            else if (extraJump == 0 && isGrounded)
            {
                rb.velocity = Vector2.up * jumpForce;                
            }
            else if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        public void Shoot()
        {

            Instantiate(Bullet, firePoint.transform.position, firePoint.transform.rotation);
            RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.transform.position, firePoint.transform.right);
            SpiderEnemySP spiderEnemySP = hitInfo.transform.GetComponent<SpiderEnemySP>();
            if (spiderEnemySP != null)
            {
                spiderEnemySP.TakeDamage(Damage);
            }

        }

        void Flip()
        {
            facingRight = !facingRight;

            transform.Rotate(0f, 180f, 0f);
        }
    }
}


