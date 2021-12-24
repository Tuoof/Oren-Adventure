using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace oren_Network
{
    public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private float stiffness;
    private GameObject farBackground;
    private GameObject player;
    private float lastXPost;
    [SerializeField] private float minHeight, maxHeight;
    [SerializeField] private Camera _camera;

    // Start is called before the first frame update
    private void OnEnable()
    {
        Debug.Log("OnEnable called");
        // SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
    }
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // player = GameObject.FindGameObjectWithTag("Player");
        player = this.gameObject;
    }
    public override void OnNetworkSpawn()
    {
        lastXPost = transform.position.x;
        
        //cameraTarget = Player.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        FollowTarget();
    }
    private void FixedUpdate()
    {
        _camera = Camera.main;
        // farBackground = GameObject.FindGameObjectWithTag("FarBackground");
    }

    public void FollowTarget()
    {
        if (!IsOwner) { return; }

        _camera.transform.position = new Vector3(player.transform.position.x, Mathf.Clamp(player.transform.position.y, minHeight, maxHeight), _camera.transform.position.z);

        float amountToMoveX = _camera.transform.position.x - lastXPost;
        // farBackground.transform.position = farBackground.transform.position + new Vector3(amountToMoveX, 0f, 0f);

        lastXPost = transform.position.x;
    }
}
}