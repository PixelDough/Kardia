using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{

    public Transform playerBody;
    public Transform playerHead;
    public MeshCollider playerBodyCollider;

    public float walkSpeed = 5f;
    public float lookSpeed = 5f;
    public float gravity = -19.6f;

    Vector3 rotation = Vector3.zero;

    Player player;
    Rigidbody rb;

    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        CameraLook();
        
    }

    private void FixedUpdate()
    {
        
        DoMovement();
    }

    private void DoMovement()
    {
        Vector3 input = Vector3.zero;
        input.x = player.GetAxis(RewiredConsts.Action.MoveX);
        input.z = player.GetAxis(RewiredConsts.Action.MoveZ);

        input = Vector3.ClampMagnitude(input, 1f);

        input = rb.transform.TransformDirection(input);

        input *= walkSpeed;

        float speedMultiplier = 1f;
        if (player.GetButton(RewiredConsts.Action.Sprint)) speedMultiplier = 2f;

        Vector3 finalMovementVector = (input * speedMultiplier);


        rb.velocity = new Vector3(finalMovementVector.x, rb.velocity.y + gravity, finalMovementVector.z) * Time.deltaTime;
    }

    private void CameraLook()
    {
        
        rotation.y += player.GetAxis(RewiredConsts.Action.LookHorizontal) * lookSpeed * Time.deltaTime;
        rotation.x += player.GetAxis(RewiredConsts.Action.LookVertical) * lookSpeed * Time.deltaTime;

        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        transform.localRotation = Quaternion.Euler(0, rotation.y, 0);
        playerHead.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
    }

}
