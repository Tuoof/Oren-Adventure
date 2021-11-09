using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float stiffness;
    public Transform cameraTarget;
    public Transform farBackground;
    public GameObject Player;
    private float lastXPost;

    // Start is called before the first frame update
    private void Start()
    {
        // Player = GameObject.Find("Player");
        lastXPost = transform.position.x;
        cameraTarget = Player.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        //FollowPlayer();
        FollowTarget();
    }

    private void FollowPlayer()
    {
        float t = Time.deltaTime * stiffness;
        transform.position = new Vector3(Mathf.Lerp(transform.position.x, cameraTarget.position.x, t), Mathf.Lerp(transform.position.y, cameraTarget.position.y, t), transform.position.z);
    }

    public void FollowTarget()
    {
        transform.position = new Vector3(cameraTarget.position.x, cameraTarget.position.y, transform.position.z);

        
        float amountToMoveX = transform.position.x - lastXPost;
        farBackground.position = farBackground.position + new Vector3(amountToMoveX, 0f, 0f);

        lastXPost = transform.position.x;
    }
}