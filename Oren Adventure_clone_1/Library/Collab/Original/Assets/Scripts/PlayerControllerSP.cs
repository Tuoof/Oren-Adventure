using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

namespace oren_Advent
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerControllerSP : MonoBehaviour
    {
        [SerializeField]private StageManagerSP stageManager;
        private PlayerInput playerInput;
        public Animator animator;
        private PlayerInputAction playerInputAction;
        private Rigidbody2D rb;
        private SpriteRenderer m_PlayerVisual;

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

        private SceneTransitionHandler.SceneStates m_CurrentSceneState;
        private GameObject m_MyBullet;
        private GameObject currentCheckpoint;
        public bool IsAlive => PlayerHealthSP.singleton.currentHealth > 0;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInputAction = new PlayerInputAction();
            
            rb = GetComponent<Rigidbody2D>();

            playerInputAction.Player.Movement.performed += ctx => setMovement(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovement();
            playerInputAction.Player.Jump.started += ctx => Jump();
            playerInputAction.Player.Jump.performed += ctx => Jump();
            playerInputAction.Player.Shoot.started += ctx => Shoot();
        }
        private void Start()
        {
            extraJump = extraJumpValue;
            m_PlayerVisual = GetComponent<SpriteRenderer>();
            if (m_PlayerVisual != null) m_PlayerVisual.material.color = Color.black;
        }

        private void OnEnable()
        {
            playerInputAction.Enable();
        }
        private void OnDisable()
        {
            playerInputAction.Disable();
        }

        private void Update()
        {
            HitByEnemy();
            InGameUpdate();
        }
        private void InGameUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

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

            if (isGrounded == true)
            {
                extraJump = extraJumpValue;
            }
        }

        public void RespawnPlayer()
        {
            this.transform.position = StageManagerSP.Singleton.currentCheckpoint.transform.position;
        }

        private void setMovement(float movement)
        {

            horizontal = movement;
        }

        private void ResetMovement()
        {
            horizontal = Vector2.zero.x;
        }

        public void Move()
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
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
        void Flip()
        {
            facingRight = !facingRight;

            transform.Rotate(0f, 180f, 0f);
        }

        public void Shoot()
        {
            m_MyBullet = Instantiate(Bullet, firePoint.position, firePoint.rotation);
            // m_MyBullet.GetComponent<Bullet>().owner = this;
            var hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);
            SpiderEnemySP spiderEnemy = hitInfo.transform.GetComponent<SpiderEnemySP>();
            if (spiderEnemy != null)
            {
                spiderEnemy.TakeDamage(Damage);
            }
        }

        public void HitByEnemy()
        {
            if (IsAlive == false)
            {
                StageManagerSP.Singleton.SetGameEnd(GameOverReason.Death);

                if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    spriteRenderer.enabled = false;
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag=="Winbox")
            {
                stageManager.winMenu.gameObject.SetActive(true);
                Destroy(this.gameObject);
            }
        }
        /// <summary>
        /// This should only be called locally, either through NotifyGameOverClientRpc or through the StageManager.BroadcastGameOverReason
        /// </summary>
        /// <param name="reason"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void NotifyGameOver(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.Win:
                    StageManagerSP.Singleton.DisplayGameOverText("You have win! \n");
                    break;
                case GameOverReason.TimeUp:
                    StageManagerSP.Singleton.DisplayGameOverText("You have lost! \n The Time is up!");
                    break;
                case GameOverReason.Death:
                    StageManagerSP.Singleton.DisplayGameOverText("You have lost! \n Your health was depleted!");
                    break;
                case GameOverReason.Max:
                    break;
                    // default:
                    //     throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }

    }
}
