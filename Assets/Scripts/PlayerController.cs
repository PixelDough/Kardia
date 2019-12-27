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

    [Header("Climb Positions")]
    public Transform climbPositionHead;
    public Transform climbPositionFeet;

    [Header("Others")]

    public Cinemachine.CinemachineVirtualCamera mainVCam;
    public Cinemachine.CinemachineVirtualCamera leanVCam;

    public float walkSpeed = 5f;
    public float lookSpeed = 5f;
    public float gravity = -19.6f;
    public float jumpSpeed = 900;

    World world;

    float jumpBufferTime;
    float jumpBuffer = 0.1f;
    bool canJumpBuffer = false;

    bool isGrounded = false;
    bool isCrawling = false;

    bool canStartClimb = true;
    bool climbStarted = false;
    bool climbWallAboveHead = false;
    bool climbWallAtFeet = false;
    bool isClimbing = false;
    float climbEndY = float.MinValue;

    Vector3 rotation = Vector3.zero;

    Player player;
    Rigidbody rb;

    private Vector3 velocity = new Vector3();

    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
        rb = GetComponent<Rigidbody>();

        world = FindObjectOfType<World>();

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

    }

    private void ToggleCrouch()
    {
        ToggleCrouch(!isCrawling);
    }


    private void ToggleCrouch(bool toggleState)
    {
        isCrawling = toggleState;
        if (!isCrawling)
        {
            RaycastHit[] hits = new RaycastHit[10];
            hits = rb.SweepTestAll(Vector3.up, 1f, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.tag == "Wall")
                    isCrawling = true;
            }
        }
    }


    private void DoMovement()
    {

        if (player.GetButtonDown(RewiredConsts.Action.Crawl))
        {
            ToggleCrouch();
        }

        if (player.GetButtonDown(RewiredConsts.Action.Sprint) && isCrawling)
        {
            ToggleCrouch(false);
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

        //DoClimbing();

        //if (rb.velocity.y < -0.1f && player.GetButton(RewiredConsts.Action.Jump))
        //{
        //    // Check variables to see if we can start a climb.
        //    if (canStartClimb)
        //    {
        //        if (transform.position.y < climbEndY)
        //        {
        //            Debug.Log("Climb Started!");
        //            isClimbing = true;
        //        }
        //    }
        //}

        //if (isClimbing)
        //{
        //    rb.velocity = new Vector3(0f, 0f, 0f);
        //    rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);

        //    //if (!climbWallAtFeet)
        //    //{
        //    //    isClimbing = false;
        //    //}
        //}


        if (!isClimbing) DoGravity();

        if (player.GetButtonDown(RewiredConsts.Action.Jump) && canJumpBuffer)
        {
            Debug.Log("Jumped!");
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            //rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Acceleration);
            rb.AddForce(Vector3.up * CalculateJumpSpeed(1.25f, gravity), ForceMode.VelocityChange);

            jumpBufferTime -= 100f;
        }

        if (!isClimbing)
            rb.velocity = Vector3.Lerp(rb.velocity, (finalMovementVector * Time.fixedDeltaTime) + Vector3.up * rb.velocity.y, 0.1f);
        //rb.velocity = (finalMovementVector * Time.fixedDeltaTime) + Vector3.up * rb.velocity.y;

    }

    private void DoClimbing()
    {
        if ((rb.velocity.y < -0.1f || isClimbing) && canStartClimb)
        {
            Vector3 direction = NearestWorldAxis(climbPositionHead.forward);

            Ray faceLeft = new Ray(climbPositionHead.position + (-climbPositionHead.right * playerBodyCollider.size.x / 2), direction);
            Ray faceRight = new Ray(climbPositionHead.position + (climbPositionHead.right * playerBodyCollider.size.x / 2), direction);
            Ray feetLeft = new Ray(climbPositionFeet.position + (-climbPositionFeet.right * playerBodyCollider.size.x / 2), direction);
            Ray feetRight = new Ray(climbPositionFeet.position + (climbPositionFeet.right * playerBodyCollider.size.x / 2), direction);

            Debug.DrawRay(faceLeft.origin, faceLeft.direction * 0.2f, Color.green);
            Debug.DrawRay(faceRight.origin, faceRight.direction * 0.2f, Color.green);
            Debug.DrawRay(feetLeft.origin, feetLeft.direction * 0.2f, Color.green);
            Debug.DrawRay(feetRight.origin, feetRight.direction * 0.2f, Color.green);

            if (Physics.Raycast(feetLeft, 0.2f, LayerMask.GetMask("Wall")) || Physics.Raycast(feetRight, 0.2f, LayerMask.GetMask("Wall")))
            {
                if (!Physics.Raycast(faceLeft, 0.2f, LayerMask.GetMask("Wall")) && !Physics.Raycast(faceRight, 0.2f, LayerMask.GetMask("Wall")))
                {
                    if (player.GetButton(RewiredConsts.Action.Jump))
                    {
                        isClimbing = true;
                        rb.velocity = new Vector3(0f, 0f, 0f);
                        rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
                        //rb.AddForce(playerBody.forward * 5f, ForceMode.VelocityChange);
                    }
                }
            }
            else
            {
                isClimbing = false;
                canStartClimb = false;
            }
        }
    }

    private void DoClimbing(Vector3 wallNormal)
    {

    }

    private static Vector3 NearestWorldAxis(Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);

        if (Mathf.Abs(v.x) < Mathf.Abs(v.y))
        {
            v.x = 0;
            if (Mathf.Abs(v.y) < Mathf.Abs(v.z))
                v.y = 0;
            else
                v.z = 0;
        }
        else
        {
            v.y = 0;
            if (Mathf.Abs(v.x) < Mathf.Abs(v.z))
                v.x = 0;
            else
                v.z = 0;
        }
        return v;
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

        climbWallAboveHead = false;
        climbWallAtFeet = false;

        // Debug-draw all contact points and normals
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            if (contact.otherCollider.gameObject.CompareTag("Wall"))
            {
                // Check grounded
                if (contact.normal == Vector3.up)
                {
                    isGrounded = true;
                    climbStarted = false;
                    climbWallAboveHead = false;
                    climbWallAtFeet = false;
                    climbEndY = float.MinValue;
                    jumpBufferTime = Time.time + jumpBuffer;
                }

                

                //Climbing wall
                if (!climbStarted && !isClimbing)
                {
                    if (contact.normal == -NearestWorldAxis(playerBody.forward)) // We are looking at the wall that we are hitting.
                    {
                        if (contact.point.y >= climbPositionHead.position.y) // One of the contacts is above the climb position, meaning we cannot climb it.
                        {
                            Debug.LogError("WALL TOO HIGH TO CLIMB");
                            climbWallAboveHead = true;
                        }
                        else
                        {
                            climbWallAtFeet = true;
                            climbEndY = Mathf.Max(climbEndY, contact.point.y + 0.1f);
                        }
                    }
                }

            }
        }

        if (!climbStarted && !isClimbing)
        {
            Debug.Log(climbWallAboveHead + " - " + climbWallAtFeet);
            if (rb.velocity.y < -0.1f && player.GetButton(RewiredConsts.Action.Jump) && !climbWallAboveHead && climbWallAtFeet)
            {
                StartCoroutine(ClimbUpWall());
            }
        }

    }


    IEnumerator ClimbUpWall()
    {
        climbStarted = true;

        while (transform.position.y < climbEndY)
        {
            isClimbing = true;
            climbStarted = true;
            rb.velocity = new Vector3(0f, 0f, 0f);
            rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
            rb.AddForce(playerBody.forward * 3f, ForceMode.VelocityChange);
            yield return null;
        }

        isClimbing = false;
        yield return null;
    }

}
