﻿using UnityEngine;
using Unity.Netcode;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private float stiffness;
    public GameObject farBackground;
    // private Rigidbody2D rb;
    public GameObject Player;
    private float lastXPost;
    [SerializeField] private float minHeight, maxHeight;
    [SerializeField ]private Camera _camera;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        lastXPost = transform.position.x;
        _camera = Camera.main;
        // rb = GetComponent<Rigidbody2D>();
        farBackground = GameObject.FindGameObjectWithTag("FarBackground");
        Player = GameObject.FindGameObjectWithTag("Player");
        //cameraTarget = Player.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        FollowTarget();
    }

    // private void FollowPlayer()
    // {
    //     float t = Time.deltaTime * stiffness;
    //     transform.position = new Vector3(Mathf.Lerp(transform.position.x, _camera.transform.position.x, t), Mathf.Lerp(transform.position.y, _camera.transform.position.y, t), transform.position.z);
    // }

    public void FollowTarget()
    {
        if (!IsOwner) { return; }

        _camera.transform.position = new Vector3(Player.transform.position.x, Mathf.Clamp(Player.transform.position.y, minHeight, maxHeight), _camera.transform.position.z);

        float amountToMoveX = _camera.transform.position.x - lastXPost;
        farBackground.transform.position = farBackground.transform.position + new Vector3(amountToMoveX, 0f, 0f);

        lastXPost = transform.position.x;
    }
}