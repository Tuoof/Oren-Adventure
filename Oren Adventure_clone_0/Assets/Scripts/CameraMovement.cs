using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float stiffness;
    public Transform player;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        float t = Time.deltaTime * stiffness;
        transform.position = new Vector3(Mathf.Lerp(transform.position.x, player.position.x, t), Mathf.Lerp(transform.position.y, player.position.y, t), transform.position.z);
    }

    public void TeleportTo(Vector2 _transform)
    {
        transform.position = new Vector3(_transform.x, _transform.y, transform.position.z);
    }
}