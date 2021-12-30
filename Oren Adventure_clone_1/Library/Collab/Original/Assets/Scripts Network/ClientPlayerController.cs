using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

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

        public NetworkVariable<int> m_Lives = new NetworkVariable<int>();
        private SceneTransitionHandler.SceneStates m_CurrentSceneState;
        private bool m_HasGameStarted;
        private bool m_IsAlive = true;
        private GameObject m_MyBullet;
        private GameObject currentCheckpoint;
        public bool IsAlive => m_Lives.Value > 0;
        private ClientRpcParams m_OwnerRPCParams;

        private void Awake()
        {
            playerInputAction = new PlayerInputAction();
            playerInput = GetComponent<PlayerInput>();
            m_HasGameStarted = false;

            playerInputAction.Player.Movement.performed += ctx => setMovement(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovement();
            playerInputAction.Player.Jump.started += ctx => Jump();
            playerInputAction.Player.Jump.performed += ctx => Jump();
            playerInputAction.Player.Shoot.started += ctx => ShootServerRPC();
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
            if (m_CurrentSceneState == SceneTransitionHandler.SceneStates.Level1)
            {
                if (m_PlayerVisual != null) m_PlayerVisual.material.color = Color.green;
            }
            else
            {
                if (m_PlayerVisual != null) m_PlayerVisual.material.color = Color.black;
            }
        }

        public override void OnNetworkSpawn()
        {
            // if (!IsOwner) { return; }
            rb = GetComponent<Rigidbody2D>();

            m_Lives.OnValueChanged += OnLivesChanged;

            if (IsServer) m_OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

            if (!StageManager.Singleton)
                StageManager.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
            else
                SubscribeToDelegatesAndUpdateValues();

            if (IsServer) SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene += SceneTransitionHandler_clientLoadedScene;
            SceneTransitionHandler.sceneTransitionHandler.OnSceneStateChanged += SceneTransitionHandler_sceneStateChanged;

        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                m_Lives.OnValueChanged -= OnLivesChanged;
            }

            if (StageManager.Singleton)
            {
                StageManager.Singleton.isGameOver.OnValueChanged -= OnGameStartedChanged;
                StageManager.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
            }
        }
        private void SubscribeToDelegatesAndUpdateValues()
        {
            StageManager.Singleton.hasGameStarted.OnValueChanged += OnGameStartedChanged;
            StageManager.Singleton.isGameOver.OnValueChanged += OnGameStartedChanged;

            if (IsClient && IsOwner)
            {
                StageManager.Singleton.SetLives(m_Lives.Value);
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
            if (StageManager.Singleton != null) StageManager.Singleton.SetLives(m_Lives.Value);

            if (m_Lives.Value <= 0)
            {
                m_IsAlive = false;
            }
        }

        private void Update()
        {
            Debug.Log(SceneTransitionHandler.SceneStates.Level1);
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
            if (!IsOwner || !m_HasGameStarted) return;
            if (!m_IsAlive) return;

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

        [ServerRpc(RequireOwnership = false)]
        public void RespawnPlayerServerRpc(ClientRpcParams clientParams)
        {
            RespawnPlayerClientRpc();
            StageManager.Singleton.MovePlayerToCheckpoint();
        }
        [ClientRpc]
        public void RespawnPlayerClientRpc()
        {
            this.transform.position = StageManager.Singleton.currentCheckpoint.transform.position;
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
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }

        public void Jump()
        {
            if (!IsOwner) { return; }
            if (!m_IsAlive) { return; }

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
            if (!IsOwner) { return; }
            // if (!m_IsAlive) { return; }

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

        public void HitByEnemy()
        {
            Assert.IsTrue(IsServer, "HitByEnemy must be called server-side only!");
            if (!m_IsAlive) return;

            m_Lives.Value -= 1;

            if (m_Lives.Value <= 0)
            {
                // gameover!
                m_IsAlive = false;
                m_Lives.Value = 0;
                StageManager.Singleton.SetGameEnd(GameOverReason.Death);
                NotifyGameOverClientRpc(GameOverReason.Death, m_OwnerRPCParams);

                // Hide graphics of this player object server-side. Note we don't want to destroy the object as it
                // may stop the RPC's from reaching on the other side, as there is only one player controlled object
                if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    spriteRenderer.enabled = false;
            }
        }

        [ClientRpc]
        private void NotifyGameOverClientRpc(GameOverReason reason, ClientRpcParams clientParams)
        {
            NotifyGameOver(reason);
        }

        /// <summary>
        /// This should only be called locally, either through NotifyGameOverClientRpc or through the StageManager.BroadcastGameOverReason
        /// </summary>
        /// <param name="reason"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void NotifyGameOver(GameOverReason reason)
        {
            Assert.IsTrue(IsLocalPlayer);
            m_HasGameStarted = false;
            switch (reason)
            {
                case GameOverReason.Win:
                    StageManager.Singleton.DisplayGameOverText("You have win! \n");
                    break;
                case GameOverReason.TimeUp:
                    StageManager.Singleton.DisplayGameOverText("You have lost! \n The Time is up!");
                    break;
                case GameOverReason.Death:
                    StageManager.Singleton.DisplayGameOverText("You have lost! \n Your health was depleted!");
                    break;
                case GameOverReason.Max:
                    break;
                    // default:
                    //     throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }

    }
}
