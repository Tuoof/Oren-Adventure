using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class OldPlayerController : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private float horizontal;
    private PlayerInputAction playerInputAction;
    public float speed;
    public float jumpForce;
    private float highJump, ultraJump;

    private Rigidbody2D rb;

    private bool facingRight = true;

    private bool isGrounded, jump;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask groundLayer;

    private int extraJump;
    public int extraJumpValue;

    // private void Awake()
    // {
    //     rb = GetComponent<Rigidbody2D>();
    // }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsLocalPlayer)
        {
            extraJump = extraJumpValue;
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        // if (!IsLocalPlayer || !IsOwner) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        // horizontal = Input.GetAxisRaw("Horizontal");
        // inputY = Input.GetAxis("Vertical");

        // shoot = Input.GetButtonDown("Fire1");
        // shoot |= Input.GetButtonDown("Fire2");
        // #elif UNITY_IOS || UNITY_ANDROID		

        // 		horizontal = Input.GetAxis("Mouse X");
        // 		// inputY = Input.GetAxis("Mouse Y");
        // 		shoot = false;
        // 		if (Input.touchCount > 0)
        // 		{
        // 			horizontal = Input.touches[0].deltaPosition.x;
        // 			// inputY = Input.touches[0].deltaPosition.y;
        // 		}
        // 		for (int i = 0; i < Input.touchCount; i++)
        // 		{
        // 			Touch touch = Input.GetTouch(i);

        // 			// -- Tap: quick touch & release
        // 			// ------------------------------------------------
        // 			if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
        // 			{
        // 				shoot = true;
        // 			}
        // 		}
#endif
        animator.SetFloat("Speed", Mathf.Abs(horizontal * speed));
        Move();
        // isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (facingRight == false && horizontal > 0)
        {
            Flip();
        }
        else if (facingRight == true && horizontal < 0)
        {
            Flip();
        }
    }

    /*void OnCollisionEnter2D(Collision2D collision)
	{
		bool damagePlayer = false;

		// Collision with enemy
		EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
		if (enemy != null)
		{
			// Kill the enemy
			HealthScript enemyHealth = enemy.GetComponent<HealthScript>();
			if (enemyHealth != null) enemyHealth.Damage(enemyHealth.hp);

			damagePlayer = true;
		}

		// Damage the player
		if (damagePlayer)
		{
			_playerHealth.Damage(1);
		}
	}*/

    public void Move()
    {
        if (IsOwner)
        {
            rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed, rb.velocity.y);
        }
        else
        {
            Debug.Log("client access move");
        }
    }
    private bool IsGrounded()
    {
        extraJump = extraJumpValue;
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    public void Jump()
    {
        if (/*NetworkManager.Singleton.IsClient*/ IsHost)
        {
            if (extraJump == 0 && IsGrounded())
            {
                rb.velocity = Vector2.up * jumpForce;
            }
            else if (jump && extraJump > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                extraJump--;
            }
            else if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
        else
        {
            Debug.Log("client access jump");
            // JumpServerRpc();
        }

    }
    void Flip()
    {
        facingRight = !facingRight;

        transform.Rotate(0f, 180f, 0f);
    }
}