using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CreativeCam : MonoBehaviour
{
    public Cinemachine.CinemachineVirtualCamera cam;
    public Transform selectCube;
    public GameObject playerObject;

    public enum VoxelTools
    {
        Place,
        Paint,
        Box,
        Fill,
        Last
    }

    public VoxelTools tool = VoxelTools.Place;

    private int selectedBlockID = 1;

    World world;

    Player player;

    bool isMenuOpen = false;

    float flySpeed = 8f;
    float lookSpeed = 1f;

    Vector3 rotation = Vector3.zero;

    private void Start()
    {
        world = FindObjectOfType<World>();
        player = ReInput.players.GetPlayer(0);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePlayMode();
        }

        if (GameManager.Instance.isInEditMode)
        {
            if (player.GetButtonDown(RewiredConsts.Action.OpenMenu)) isMenuOpen = !isMenuOpen;

            if (!isMenuOpen)
            {
                CameraLook();
                DoMovement();
                DoInteraction();
            }
        }
        else
        {
            if (player.GetButtonDown(RewiredConsts.Action.OpenMenu)) TogglePlayMode();
        }
    }

    public void TogglePlayMode()
    {
        GameManager.Instance.isInEditMode = !GameManager.Instance.isInEditMode;

        if (GameManager.Instance.isInEditMode)
        {
            //world.GetChunkFromVector3(Vector3.zero).DoDespawners();
            world.DoAllDespawners();
            Camera.main.cullingMask |= LayerMask.GetMask("EditorUI");
            cam.gameObject.SetActive(true);
            isMenuOpen = false;
            playerObject.SetActive(false);
            playerObject.transform.localPosition = Vector3.zero;
            playerObject.transform.rotation = Quaternion.identity;
        }
        else
        {
            //world.GetChunkFromVector3(Vector3.zero).DoSpawners();
            world.DoAllSpawners();
            Camera.main.cullingMask &= ~LayerMask.GetMask("EditorUI");
            cam.gameObject.SetActive(false);
            isMenuOpen = false;
            playerObject.SetActive(true);
            playerObject.transform.localPosition = Vector3.zero;
            playerObject.transform.rotation = Quaternion.identity;
        }
    }

    private void DoInteraction()
    {
        if (Physics.Raycast(transform.position, cam.transform.forward, out var hit, 10))
        {
            selectCube.gameObject.SetActive(true);

            Vector3 blockHit;
            blockHit.x = Mathf.Floor(hit.point.x - hit.normal.x / 10);
            blockHit.y = Mathf.Floor(hit.point.y - hit.normal.y / 10);
            blockHit.z = Mathf.Floor(hit.point.z - hit.normal.z / 10);

            Vector3 blockPlacePosition = blockHit;
            blockPlacePosition.x += hit.normal.x;
            blockPlacePosition.y += hit.normal.y;
            blockPlacePosition.z += hit.normal.z;

            selectCube.position = blockHit;
        }
        else
        {
            selectCube.gameObject.SetActive(false);
        }
    }

    private void DoMovement()
    {
        Vector3 input;
        input.x = player.GetAxis(RewiredConsts.Action.FlyX) * flySpeed * Time.deltaTime;
        input.z = player.GetAxis(RewiredConsts.Action.FlyZ) * flySpeed * Time.deltaTime;
        input.y = player.GetAxis(RewiredConsts.Action.FlyY) * flySpeed * Time.deltaTime;

        float speedMultiplier = 1f;
        if (player.GetButton(RewiredConsts.Action.FlyFaster)) speedMultiplier = 2f;

        transform.Translate(input * speedMultiplier, cam.transform);
    }

    private void CameraLook()
    {
        rotation.y += player.GetAxis(RewiredConsts.Action.LookHorizontal) * lookSpeed;
        rotation.x += player.GetAxis(RewiredConsts.Action.LookVertical) * lookSpeed;

        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        cam.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
    }
}