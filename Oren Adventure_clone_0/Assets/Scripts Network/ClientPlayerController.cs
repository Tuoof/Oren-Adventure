using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace oren_Network
{
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

        private NetworkVariable<int> m_Lives = new NetworkVariable<int>(3);
        private SceneTransitionHandler.SceneStates m_CurrentSceneState;
        private bool m_HasGameStarted;
        private bool m_IsAlive = true;
        private GameObject m_MyBullet;
        public bool IsAlive => m_Lives.Value > 0;
        private ClientRpcParams m_OwnerRPCParams;

        private void Awake()
        {
            playerInputAction = new PlayerInputAction();
            playerInput = GetComponent<PlayerInput>();
            m_HasGameStarted = false;
        }
        private void Start()
        {
            extraJump = extraJumpValue;
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
            // if (!IsOwner) { return; }
            rb = GetComponent<Rigidbody2D>();
            playerInputAction.Player.Movement.performed += ctx => setMovement(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovement();
            playerInputAction.Player.Jump.started += ctx => JumpServerRpc();
            playerInputAction.Player.Shoot.started += ctx => ShootServerRPC();

            m_Lives.OnValueChanged += OnLivesChanged;

            if (IsServer) m_OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

            if (!InvadersGame.Singleton)
                InvadersGame.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
            else
                SubscribeToDelegatesAndUpdateValues();

            if (IsServer) SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene += SceneTransitionHandler_clientLoadedScene;
            SceneTransitionHandler.sceneTransitionHandler.OnSceneStateChanged += SceneTransitionHandler_sceneStateChanged;

        }
        private void Update()
        {
            InGameUpdate();
            // if (!IsOwner) { return; }
            Debug.Log(SceneTransitionHandler.SceneStates.Level1);
            switch (m_CurrentSceneState)
            {
                case SceneTransitionHandler.SceneStates.Level1:
                    {
                        // InGameUpdate();
                        break;
                    }
            }
        }
        private void InGameUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

            animator.SetFloat("Speed", Mathf.Abs(horizontal * speed));
            Move();
            JumpServerRpc();

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
            // m_MyBullet.GetComponent<Bullet>().owner = this;
            m_MyBullet.GetComponent<NetworkObject>().Spawn();
            var hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);
            SpiderEnemy spiderEnemy = hitInfo.transform.GetComponent<SpiderEnemy>();
            if (spiderEnemy != null)
            {
                spiderEnemy.TakeDamage(Damage);
            }
        }

        private void SceneTransitionHandler_clientLoadedScene(ulong clientId)
        {
            SceneStateChangedClientRpc(m_CurrentSceneState);
        }

        [ClientRpc]
        private void SceneStateChangedClientRpc(SceneTransitionHandler.SceneStates state)
        {
            if (!IsServer) SceneTransitionHandler.sceneTransitionHandler.SetSceneState(state);
        }

        private void SceneTransitionHandler_sceneStateChanged(SceneTransitionHandler.SceneStates newState)
        {
            m_CurrentSceneState = newState;
        }
        private void SubscribeToDelegatesAndUpdateValues()
        {
            InvadersGame.Singleton.hasGameStarted.OnValueChanged += OnGameStartedChanged;
            InvadersGame.Singleton.isGameOver.OnValueChanged += OnGameStartedChanged;

            if (IsClient && IsOwner)
            {
                InvadersGame.Singleton.SetLives(m_Lives.Value);
            }
        }
        private void OnGameStartedChanged(bool previousValue, bool newValue)
        {
            m_HasGameStarted = newValue;
        }
        private void OnLivesChanged(int previousAmount, int currentAmount)
        {
            // Hide graphics client side upon death
            if (currentAmount <= 0 && IsClient && TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                spriteRenderer.enabled = false;

            if (!IsOwner) return;
            Debug.LogFormat("Lives {0} ", currentAmount);
            if (InvadersGame.Singleton != null) InvadersGame.Singleton.SetLives(m_Lives.Value);

            if (m_Lives.Value <= 0)
            {
                m_IsAlive = false;
            }
        }
    }
}
