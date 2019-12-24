using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{

    public Transform playerBody;
    public Transform playerHead;
    public BoxCollider playerBodyCollider;
    public BoxCollider playerCrawlCollider;

    public float walkSpeed = 5f;
    public float lookSpeed = 5f;
    public float gravity = -19.6f;
    public float jumpSpeed = 900;

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

    }

    void DoGravity()
    {

        //RaycastHit hit = new RaycastHit();
        //Physics.BoxCast(transform.position, new Vector3(playerBodyCollider.size.x, 0.01f, playerBodyCollider.size.z)/2f, Vector3.down, out hit, Quaternion.identity, (Mathf.Abs(velocity.y) * Time.deltaTime) + 0.01f);
        //Debug.DrawLine(transform.position, transform.position + Vector3.down * ((Mathf.Abs(velocity.y) + 0.01f)), Color.red);
        //if (hit.collider != null)
        //{

        //    Debug.Log("Player is on ground! Distance of: " + hit.distance);
        //    transform.position += Vector3.down * hit.distance;
        //    velocity.y = 0;
        //    isGrounded = true;
        //    return;
        //}
        //else
        //{
        //    Debug.Log("Player is NOT on ground!");
        //    velocity.y += gravity;
        //    isGrounded = false;
        //}

        if (rb.velocity.y < 0)
            rb.AddForce(Vector3.up * gravity * 1.5f, ForceMode.Acceleration);
        else
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);

        RaycastHit hit;
        Vector3 skinDistance = (Vector3.up * 0.01f);
        Ray[] rays = new Ray[4];
        rays[0] = new Ray(transform.position + skinDistance + new Vector3(-playerBodyCollider.size.x / 2f, 0f, -playerBodyCollider.size.z / 2f), Vector3.down);
        rays[1] = new Ray(transform.position + skinDistance + new Vector3(playerBodyCollider.size.x / 2f, 0f, -playerBodyCollider.size.z / 2f), Vector3.down);
        rays[2] = new Ray(transform.position + skinDistance + new Vector3(-playerBodyCollider.size.x / 2f, 0f, playerBodyCollider.size.z / 2f), Vector3.down);
        rays[3] = new Ray(transform.position + skinDistance + new Vector3(playerBodyCollider.size.x / 2f, 0f, playerBodyCollider.size.z / 2f), Vector3.down);

        bool hitOne = false;

        foreach (Ray r in rays)
        {
            if (Physics.Raycast(r, 0.02f))
            {
                hitOne = true;
            }
        }

        isGrounded = hitOne;
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
            playerHead.localPosition = Vector3.MoveTowards(playerHead.localPosition, new Vector3(0, -0.3f, 0), 0.04f);
        }
        else
        {
            playerBodyCollider.gameObject.SetActive(true);
            playerCrawlCollider.gameObject.SetActive(false);
            playerHead.localPosition = Vector3.MoveTowards(playerHead.localPosition, new Vector3(0, 0.3f, 0), 0.04f);
        }

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

        if (player.GetButtonDown(RewiredConsts.Action.Jump) && isGrounded)
        {
            Debug.Log("Jumped!");
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Acceleration);
        }


        //finalMovementVector.y = velocity.y;

        // velocity = new Vector3(finalMovementVector.x, rb.velocity.y, finalMovementVector.z);

        //rb.velocity = new Vector3(finalMovementVector.x, rb.velocity.y + gravity, finalMovementVector.z) * Time.deltaTime;
        rb.velocity = (finalMovementVector * Time.deltaTime) + Vector3.up * rb.velocity.y;
    }

    private void CameraLook()
    {
        
        rotation.y += player.GetAxis(RewiredConsts.Action.LookHorizontal) * lookSpeed;
        rotation.x += player.GetAxis(RewiredConsts.Action.LookVertical) * lookSpeed;

        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        playerBody.transform.localRotation = Quaternion.Euler(0, rotation.y, 0);
        playerHead.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
    }

}
