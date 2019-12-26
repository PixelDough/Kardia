﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{

    public Transform playerBody;
    public Transform playerHead;
    public BoxCollider playerBodyCollider;
    public BoxCollider playerCrawlCollider;

    public Cinemachine.CinemachineVirtualCamera mainVCam;
    public Cinemachine.CinemachineVirtualCamera leanVCam;

    public float walkSpeed = 5f;
    public float lookSpeed = 5f;
    public float gravity = -19.6f;
    public float jumpSpeed = 900;

    float jumpBufferTime;
    float jumpBuffer = 0.1f;
    bool canJumpBuffer = false;

    bool isGrounded = false;
    bool isCrawling = false;

    Vector3 rotation = Vector3.zero;

    Player player;
    Rigidbody rb;

    private Vector3 velocity = new Vector3();

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

        if (Time.time > jumpBufferTime)
        {
            canJumpBuffer = false;
        }
        else
        {
            canJumpBuffer = true;
        }

    }

    void DoGravity()
    {

        if (rb.velocity.y < 0)
            rb.AddForce(Vector3.up * gravity * 1.5f, ForceMode.Acceleration);
        else
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);

        //RaycastHit hit;
        //Vector3 skinDistance = (Vector3.up * 0.01f);

        //Ray[] rays = new Ray[4];
        //rays[0] = new Ray(transform.position + skinDistance + new Vector3(-playerBodyCollider.size.x / 2f, 0f, -playerBodyCollider.size.z / 2f), Vector3.down);
        //rays[1] = new Ray(transform.position + skinDistance + new Vector3(playerBodyCollider.size.x / 2f, 0f, -playerBodyCollider.size.z / 2f), Vector3.down);
        //rays[2] = new Ray(transform.position + skinDistance + new Vector3(-playerBodyCollider.size.x / 2f, 0f, playerBodyCollider.size.z / 2f), Vector3.down);
        //rays[3] = new Ray(transform.position + skinDistance + new Vector3(playerBodyCollider.size.x / 2f, 0f, playerBodyCollider.size.z / 2f), Vector3.down);

        //bool hitOne = false;

        //foreach (Ray r in rays)
        //{
        //    if (Physics.Raycast(r, 0.02f, LayerMask.GetMask("Wall")))
        //    {
        //        hitOne = true;
        //    }
        //}

        ////isGrounded = hitOne;
    }

    private void DoMovement()
    {

        if (player.GetButtonDown(RewiredConsts.Action.Crawl))
        {
            isCrawling = !isCrawling;
            RaycastHit[] hits = new RaycastHit[10];
            if (!isCrawling)
            {
                hits = rb.SweepTestAll(Vector3.up, 1f, QueryTriggerInteraction.Ignore);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.tag == "Wall")
                        isCrawling = true;
                }
            }
        }

        if (isCrawling)
        {
            playerBodyCollider.gameObject.SetActive(false);
            playerCrawlCollider.gameObject.SetActive(true);
            playerHead.localPosition = Vector3.MoveTowards(playerHead.localPosition, new Vector3(0, -0.5f, 0), 0.05f);
        }
        else
        {
            playerBodyCollider.gameObject.SetActive(true);
            playerCrawlCollider.gameObject.SetActive(false);
            playerHead.localPosition = Vector3.MoveTowards(playerHead.localPosition, new Vector3(0, 0.6f, 0), 0.05f);
        }

        // Neck movements
        //if (player.GetAxis(RewiredConsts.Action.Lean) != 0f)
        //{
        //    leanVCam.gameObject.SetActive(true);
        //    mainVCam.gameObject.SetActive(false);
        //}
        //else
        //{
        //    leanVCam.gameObject.SetActive(false);
        //    mainVCam.gameObject.SetActive(true);
        //}


        Vector3 input = Vector3.zero;
        input.x = player.GetAxis(RewiredConsts.Action.MoveX);
        input.z = player.GetAxis(RewiredConsts.Action.MoveZ);

        input = Vector3.ClampMagnitude(input, 1f);

        input = playerBody.transform.TransformDirection(input);

        input *= walkSpeed;

        float speedMultiplier = 1f;
        if (player.GetButton(RewiredConsts.Action.Sprint) && !isCrawling) speedMultiplier = 2f;

        Vector3 finalMovementVector = (input * speedMultiplier);

        DoGravity();

        if (player.GetButtonDown(RewiredConsts.Action.Jump) && canJumpBuffer)
        {
            Debug.Log("Jumped!");
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            //rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Acceleration);
            rb.AddForce(Vector3.up * CalculateJumpSpeed(1.25f, gravity), ForceMode.VelocityChange);

            jumpBufferTime -= 100f;
        }

        rb.velocity = Vector3.Lerp(rb.velocity, (finalMovementVector * Time.fixedDeltaTime) + Vector3.up * rb.velocity.y, 0.1f);
        //rb.velocity = (finalMovementVector * Time.fixedDeltaTime) + Vector3.up * rb.velocity.y;

    }

    private void CameraLook()
    {
        
        rotation.y += player.GetAxis(RewiredConsts.Action.LookHorizontal) * lookSpeed;
        rotation.x += player.GetAxis(RewiredConsts.Action.LookVertical) * lookSpeed;

        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        playerBody.transform.localRotation = Quaternion.Euler(0, rotation.y, 0);
        playerHead.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
    }

    // Calculate the initial velocity of a jump based off gravity and desired maximum height attained
    private float CalculateJumpSpeed(float jumpHeight, float gravity)
    {
        return Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(gravity));
    }


    void OnCollisionStay(Collision collisionInfo)
    {
        // Debug-draw all contact points and normals
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            if (contact.otherCollider.gameObject.CompareTag("Wall"))
            {
                Debug.Log("ON COLLISION STAY");
                if (contact.normal == Vector3.up)
                {
                    isGrounded = true;
                    jumpBufferTime = Time.time + jumpBuffer;
                }
            }
        }
    }

}
