using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float stiffness;
    public Transform cameraTarget;
    public GameObject Player;

    // Start is called before the first frame update
    private void Start()
    {
        Player = GameObject.Find("Player");
        cameraTarget = Player.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        Player = GameObject.Find("Player");
        cameraTarget = Player.transform;
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
    }
}