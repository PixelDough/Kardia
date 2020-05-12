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
    public SphereCollider playerClimbCollider;

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
    bool isClimbing = false;


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

        Vector3 finalMovementVector = Vector3.zero;

        finalMovementVector = (input * speedMultiplier);

        //if (player.GetButton(RewiredConsts.Action.Jump))
            DoClimbing();

        if (isClimbing)
        {
            if (player.GetButton(RewiredConsts.Action.Jump))
            {
                isClimbing = false;
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * CalculateJumpSpeed(1.25f, gravity), ForceMode.VelocityChange);
            }
        }

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
        // Setup variables
        List<Ray> outRays = new List<Ray>();
        List<Ray> downRays = new List<Ray>();
        Vector3 dirForward = SnapTo(playerBody.forward);
        Vector3 dirRight = SnapTo(playerBody.right);
        Vector3 dirUp = SnapTo(playerBody.up);
        float skinSize = 0.0001f;

        // Create Rays
        if (rb.velocity.y < -0.1f && !isClimbing)
        {
            for (int i = 0; i <= 0; i+=1)
            {
                Vector3 pos = playerBodyCollider.transform.position;
                if (isCrawling) pos = playerCrawlCollider.transform.position;
                outRays.Add(new Ray(pos + (Vector3.up * 0.3f) + (dirRight * ((playerBodyCollider.size.x - skinSize) / 2f) * i), dirForward));
                downRays.Add(new Ray(outRays[outRays.Count-1].GetPoint(playerBodyCollider.size.x/2), -dirUp));
            }
        }

        // Perform raycasts
        foreach(Ray r in outRays)
        {
            RaycastHit hit;
            Physics.Raycast(r, out hit, 1f, LayerMask.GetMask("Wall"), QueryTriggerInteraction.Ignore);
            Debug.DrawRay(r.origin, r.direction, hit.transform != null ? Color.red : Color.green);
            if (hit.transform) return;
        }
        foreach (Ray r in downRays)
        {
            RaycastHit hit;
            Physics.Raycast(r, out hit, 0.1f, LayerMask.GetMask("Wall"), QueryTriggerInteraction.Ignore);
            if (hit.transform)
            {
                if (!Physics.Raycast(hit.point, Vector3.up, playerBodyCollider.size.y, LayerMask.GetMask("Wall")))
                {
                    isClimbing = true;
                    rb.isKinematic = true;
                }
            }
            Debug.DrawRay(r.origin, r.direction, hit.transform != null ? Color.red : Color.green);
        }
    }


    Vector3 SnapTo(Vector3 _direction)
    {
        float _x = Mathf.Abs(_direction.x);
        float _y = Mathf.Abs(_direction.y);
        float _z = Mathf.Abs(_direction.z);

        if (_x > _y && _x > _z)
        {
            _x = Mathf.Sign(_direction.x);
            _y = 0;
            _z = 0;
        }
        else if (_y > _x && _y > _z)
        {
            _y = Mathf.Sign(_direction.y);
            _x = 0;
            _z = 0;
        } 
        else if (_z > _y && _z > _x)
        {
            _z = Mathf.Sign(_direction.z);
            _y = 0;
            _x = 0;
        }

        return new Vector3(_x, _y, _z);
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
                // Check grounded
                if (contact.normal == Vector3.up)
                {
                    isGrounded = true;
                    jumpBufferTime = Time.time + jumpBuffer;
                }

            }
        }

    }

}
