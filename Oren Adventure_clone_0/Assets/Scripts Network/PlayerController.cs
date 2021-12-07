using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace oren_Network
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
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

        private NetworkVariable<int> m_Lives = new NetworkVariable<int>(3);
        private SceneTransitionHandler.SceneStates m_CurrentSceneState;
        private bool m_HasGameStarted;
        private bool m_IsAlive = true;
        private GameObject m_MyBullet;
        public bool IsAlive => m_Lives.Value > 0;

        private void Awake()
        {
            playerInputAction = new PlayerInputAction();
            playerInput = GetComponent<PlayerInput>();
            m_HasGameStarted = false;
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
            extraJump = extraJumpValue;
            rb = GetComponent<Rigidbody2D>();
            playerInputAction.Player.Movement.performed += ctx => setMovementServerRpc(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovementServerRpc();
            playerInputAction.Player.Jump.started += ctx => JumpServerRpc();
            playerInputAction.Player.Shoot.started += ctx => ShootServerRPC();
        }
        private void Update()
        {
            // if (!IsOwner) { return; }
            InGameUpdate();

            switch (m_CurrentSceneState)
            {
                case SceneTransitionHandler.SceneStates.Level1:
                    {
                        InGameUpdate();
                        break;
                    }
            }           
        }

        private void InGameUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

            animator.SetFloat("Speed", Mathf.Abs(horizontal * speed));
            MoveServerRpc();

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

        [ServerRpc]
        private void setMovementServerRpc(float movement)
        {
            // if (!IsOwner) { return; }
            horizontal = movement;
        }
        [ServerRpc]
        private void ResetMovementServerRpc()
        {
            // if (!IsOwner) { return; }
            horizontal = Vector2.zero.x;
        }
        [ServerRpc]
        public void MoveServerRpc()
        {
            if (!IsServer) { return; }

            // horizontal = playerInputAction.Player.Movement.ReadValue<Vector2>().x;
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

            // MoveClientRpc();
        }

        [ClientRpc]
        public void MoveClientRpc(ServerRpcParams rpcParams = default)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }

        [ServerRpc]
        public void JumpServerRpc()
        {
            if (!IsServer) { return; }

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

            // JumpClientRpc();
        }

        [ClientRpc]
        public void JumpClientRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsOwner) { return; }
            
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

        [ServerRpc]
        public void ShootServerRPC()
        {
            if (!IsServer) { return; }

            m_MyBullet = Instantiate(Bullet, firePoint.position, firePoint.rotation);
            m_MyBullet.GetComponent<Bullet>().owner = this;
            m_MyBullet.GetComponent<NetworkObject>().Spawn();
            var hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);
            SpiderEnemy spiderEnemy = hitInfo.transform.GetComponent<SpiderEnemy>();
            if (spiderEnemy != null)
            {
                spiderEnemy.TakeDamage(Damage);
            }
        }
    }
}
