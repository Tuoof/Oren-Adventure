using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Assertions;

namespace oren_Network
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        public NetworkVariable<float> horizontal = new NetworkVariable<float>();
        // public NetworkVariableString playerName = new NetworkVariable<string>();
        private ClientRpcParams m_OwnerRPCParams;
        private PlayerInput playerInput;
        public Animator animator;
        private PlayerInputAction playerInputAction;
        private Rigidbody2D rb;

        // Movement and Jump variable
        // private float horizontal;
        public float speed, jumpForce;
        private float highJump, ultraJump;
        private int extraJump;
        public int extraJumpValue;
        private bool facingRight = true;

        // Shooting Variable
        public GameObject Bullet;
        public Transform firePoint;
        private GameObject currentCheckpoint;
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

            playerInputAction.Player.Movement.performed += ctx => setMovementServerRpc(ctx.ReadValue<Vector2>().x);
            playerInputAction.Player.Movement.canceled += ctx => ResetMovementServerRpc();
            playerInputAction.Player.Jump.started += ctx => JumpServerRpc();
            playerInputAction.Player.Shoot.started += ctx => ShootServerRPC();
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
            if (m_CurrentSceneState == SceneTransitionHandler.SceneStates.Level1)
            {
                // if (m_PlayerVisual != null) m_PlayerVisual.material.color = Color.green;
            }
            else
            {
                // if (m_PlayerVisual != null) m_PlayerVisual.material.color = Color.black;
            }
        }
        public override void OnNetworkSpawn()
        {
            extraJump = extraJumpValue;
            rb = GetComponent<Rigidbody2D>();

            m_Lives.OnValueChanged += OnLivesChanged;

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
            if (IsClient)
            {
                m_Lives.OnValueChanged -= OnLivesChanged;
            }

            if (InvadersGame.Singleton)
            {
                InvadersGame.Singleton.isGameOver.OnValueChanged -= OnGameStartedChanged;
                InvadersGame.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
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
        private void FixedUpdate()
        {
            currentCheckpoint = GameObject.FindGameObjectWithTag("Checkpoint");
            // if (!IsOwner) { return; }

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

            animator.SetFloat("Speed", Mathf.Abs(horizontal.Value * speed));
            MoveServerRpc();

            if (facingRight == false && horizontal.Value > 0)
            {
                Flip();
            }
            else if (facingRight == true && horizontal.Value < 0)
            {
                Flip();
            }

            if (isGrounded == true)
            {
                extraJump = extraJumpValue;
            }
        }
        // public void RespawnPlayer()
        // {
        //     StageManager.Singleton.MovePlayerToCheckpoint();
        // }

        [ServerRpc]
        private void setMovementServerRpc(float movement)
        {
            // if (!IsOwner) { return; }
            horizontal.Value = movement;
        }
        [ServerRpc]
        private void ResetMovementServerRpc()
        {
            // if (!IsOwner) { return; }
            horizontal.Value = Vector2.zero.x;
        }
        [ServerRpc]
        public void MoveServerRpc()
        {
            if (!IsServer) { return; }

            rb.velocity = new Vector2(horizontal.Value * speed, rb.velocity.y);
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
            m_MyBullet.GetComponent<Bullet>().owner = this;
            m_MyBullet.GetComponent<NetworkObject>().Spawn();
            var hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);
            Enemy spiderEnemy = hitInfo.transform.GetComponent<Enemy>();
            if (spiderEnemy != null)
            {
                spiderEnemy.TakeDamage(Damage);
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
