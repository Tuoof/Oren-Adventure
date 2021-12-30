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
        public NetworkVariable<bool> playerRespawned { get; } = new NetworkVariable<bool>(false);
        private PlayerInput playerInput;
        // private PlayerHealth playerHealth;
        public Animator animator;
        private PlayerInputAction playerInputAction;
        private Rigidbody2D rb;
        private GameObject ChildImage;

        // Movement and Jump variable
        private float horizontal = 0;
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
        private bool m_HasGameStarted;
        private bool m_HasPlayerRespawned;
        private GameObject m_MyBullet;
        private GameObject currentCheckpoint;
        // public bool IsAlive => playerHealth.currentHealth.Value > 0;
        public bool IsAlive = true;
        private ClientRpcParams m_OwnerRPCParams;

        private void Awake()
        {
            // playerHealth = this.gameObject.GetComponent<PlayerHealth>();
            playerInputAction = new PlayerInputAction();
            playerInput = GetComponent<PlayerInput>();
            ChildImage = transform.GetChild(0).gameObject;
            m_HasGameStarted = false;
            m_HasPlayerRespawned = false;

            playerInputAction.Player.Movement.performed += ctx => setMovement(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovement();
            playerInputAction.Player.Jump.started += ctx => Jump();
            playerInputAction.Player.Jump.performed += ctx => Jump();
            playerInputAction.Player.Shoot.started += ctx => ShootServerRPC();
        }
        private void Start()
        {
            extraJump = extraJumpValue;
            if (ChildImage.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.material.color = Color.white;
            }
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
            if (!IsServer) SceneTransitionHandler.singleton.SetSceneState(state);
        }

        private void SceneTransitionHandler_sceneStateChanged(SceneTransitionHandler.SceneStates newState)
        {
            m_CurrentSceneState = newState;
            // if (m_CurrentSceneState == SceneTransitionHandler.SceneStates.Level1)
            // {
            //     if (ChildImage.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            //     {
            //         spriteRenderer.material.color = Color.white;
            //     }
            // }
            // else
            // {
            //     if (ChildImage.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            //     {
            //         spriteRenderer.material.color = Color.black;
            //     }
            // }
        }

        public override void OnNetworkSpawn()
        {
            // if (!IsOwner) { return; }
            rb = GetComponent<Rigidbody2D>();

            if (IsServer) m_OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

            if (!StageManager.Singleton)
                StageManager.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
            else
                SubscribeToDelegatesAndUpdateValues();

            if (IsServer) SceneTransitionHandler.singleton.OnClientLoadedScene += SceneTransitionHandler_clientLoadedScene;
            SceneTransitionHandler.singleton.OnSceneStateChanged += SceneTransitionHandler_sceneStateChanged;

        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (StageManager.Singleton)
            {
                StageManager.Singleton.isGameOver.OnValueChanged -= OnGameStartedChanged;
                StageManager.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
                playerRespawned.OnValueChanged -= OnPlayerRespawned;
            }
        }
        private void SubscribeToDelegatesAndUpdateValues()
        {
            StageManager.Singleton.hasGameStarted.OnValueChanged += OnGameStartedChanged;
            StageManager.Singleton.isGameOver.OnValueChanged += OnGameStartedChanged;
            playerRespawned.OnValueChanged += OnPlayerRespawned;
        }
        private void OnGameStartedChanged(bool previousValue, bool newValue)
        {
            m_HasGameStarted = newValue;

            if (!m_HasGameStarted)
            {
                if (ChildImage.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                {
                    spriteRenderer.enabled = false;
                }
            }
        }
        private void OnPlayerRespawned(bool previousValue, bool newValue)
        {
            m_HasPlayerRespawned = newValue;

            if(m_HasPlayerRespawned)
            {
                RespawnPlayer();
            }
        }

        private void Update()
        {
            Debug.Log(SceneTransitionHandler.SceneStates.Level1);
            switch (m_CurrentSceneState)
            {
                case SceneTransitionHandler.SceneStates.Level1:
                    {
                        // HitByEnemy();
                        StageTimeUp();
                        InGameUpdate();
                        PlayerWinMultiplayerStage();
                        break;
                    }
            }
        }
        private void InGameUpdate()
        {
            if (!IsOwner || !m_HasGameStarted) return;
            // if (!PlayerHealth.Singleton.m_IsAlive) return;

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

            animator.SetFloat("Speed", Mathf.Abs(horizontal * speed));
            Move();

            if (facingRight == false && horizontal > 0) { Flip(); }
            else if (facingRight == true && horizontal < 0) { Flip(); }

            if (isGrounded == true) { extraJump = extraJumpValue; }
        }
        public void RespawnPlayer()
        {
            this.transform.position = StageManager.Singleton.currentCheckpoint.transform.position;
        }

        private void setMovement(float movement)
        {
            if (!IsOwner || !m_HasGameStarted) return;
            horizontal = movement;
        }

        private void ResetMovement()
        {
            if (!IsOwner || !m_HasGameStarted) return;
            horizontal = Vector2.zero.x;
        }

        public void Move()
        {
            if (!IsOwner || !m_HasGameStarted) return;
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }

        public void Jump()
        {
            if (!IsOwner || !m_HasGameStarted) return;
            // if (!PlayerHealth.Singleton.m_IsAlive) { return; }

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
            // if (!IsLocalPlayer || !m_HasGameStarted) return;

            m_MyBullet = Instantiate(Bullet, firePoint.position, firePoint.rotation);
            m_MyBullet.GetComponent<NetworkObject>().Spawn();

            var hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);

            Enemy Enemy = hitInfo.transform.GetComponent<Enemy>();
            if (Enemy != null)
            {
                Enemy.TakeDamage(Damage);
            }
        }


        // public void HitByEnemy()
        // {
        //     if (playerHealth.currentHealth.Value <= 0)
        //     {
        //         // gameover!
        //         playerHealth.m_IsAlive = false;
        //         playerHealth.currentHealth.Value = 0;
        //         StageManager.Singleton.SetGameEndServerRpc(GameOverReason.Death);
        //         NotifyGameOverClientRpc(GameOverReason.Death, m_OwnerRPCParams);

        //         if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        //         {
        //             spriteRenderer.enabled = false;
        //         }
        //     }
        // }


        public void StageTimeUp()
        {
            if (StageManager.Singleton.m_TimeRemaining <= 0)
            {
                // Gameover!
                StageManager.Singleton.SetGameEndServerRpc(GameOverReason.TimeUp);
                NotifyGameOverClientRpc(GameOverReason.TimeUp, m_OwnerRPCParams);
            }
        }
        public void PlayerWinMultiplayerStage()
        {
            if (StageManager.Singleton.Winstate)
            {
                // Gameover!
                StageManager.Singleton.SetGameEndServerRpc(GameOverReason.Win);
                NotifyGameOverClientRpc(GameOverReason.Win, m_OwnerRPCParams);
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
                    StageManager.Singleton.DisplayWinGameOver();
                    break;
                case GameOverReason.TimeUp:
                    StageManager.Singleton.DisplayLoseGameOver();
                    break;
                case GameOverReason.Death:
                    StageManager.Singleton.DisplayLoseGameOver();
                    break;
                case GameOverReason.Max:
                    break;
                    // default:
                    //     throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }

    }
}
