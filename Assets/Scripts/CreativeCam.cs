using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CreativeCam : MonoBehaviour
{

    public Camera cam;
    public Transform selectCube;
    public MenuPanel menu;
    public GameObject roomLightPrefab;

    private int selectedBlockID = 1;

    World world;

    Player player;

    bool isMenuOpen = false;

    float flySpeed = 8f;
    float lookSpeed = 75f;

    Vector3 rotation = Vector3.zero;

    private void Start()
    {
        world = FindObjectOfType<World>();
        player = ReInput.players.GetPlayer(0);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {

        if (player.GetButtonDown(RewiredConsts.Action.OpenMenu)) isMenuOpen = !isMenuOpen;
        menu.ToggleActive(isMenuOpen);

        if (!isMenuOpen)
        {
            CameraLook();
            DoMovement();
            DoInteraction();
        }
        else
        {

        }



    }

    private void DoInteraction()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, cam.transform.forward, out hit, 10))
        {
            selectCube.gameObject.SetActive(true);

            Vector3 blockHit;
            blockHit.x = Mathf.Floor(hit.point.x - hit.normal.x / 10);
            blockHit.y = Mathf.Floor(hit.point.y - hit.normal.y / 10);
            blockHit.z = Mathf.Floor(hit.point.z - hit.normal.z / 10);

            selectCube.position = blockHit;

            if (player.GetButtonDown(RewiredConsts.Action.Destroy))
            {
                Debug.Log("Destroyed Block");
                world.GetChunkFromVector3(blockHit).EditVoxel(blockHit, 0);
            }

            if (player.GetButtonDown(RewiredConsts.Action.Place))
            {
                Vector3 blockPlacePosition = blockHit;
                blockPlacePosition.x += hit.normal.x;
                blockPlacePosition.y += hit.normal.y;
                blockPlacePosition.z += hit.normal.z;

                world.GetChunkFromVector3(blockPlacePosition).EditVoxel(blockPlacePosition, selectedBlockID);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Vector3 blockPlacePosition = blockHit;
                blockPlacePosition.x += hit.normal.x;
                blockPlacePosition.y += hit.normal.y;
                blockPlacePosition.z += hit.normal.z;

                //Instantiate(roomLightPrefab, blockPlacePosition, Quaternion.identity, world.GetChunkFromVector3(blockPlacePosition).chunkObject.transform);
                world.GetChunkFromVector3(blockPlacePosition).EditVoxel(blockPlacePosition, 0, VoxelData.VoxelTypes.EntitySpawner);
            }
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
        rotation.y += player.GetAxis(RewiredConsts.Action.LookHorizontal) * Time.deltaTime * lookSpeed;
        rotation.x += player.GetAxis(RewiredConsts.Action.LookVertical) * Time.deltaTime * lookSpeed;

        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        cam.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
    }

    public int ChangeSelectedBlockID(int _id)
    {
        selectedBlockID = Mathf.Clamp(_id, 1, world.blockTypes.Length - 1);

        return 0;
    }
}
