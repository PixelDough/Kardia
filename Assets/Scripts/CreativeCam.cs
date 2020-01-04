using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CreativeCam : MonoBehaviour
{

    public Cinemachine.CinemachineVirtualCamera cam;
    public Transform selectCube;
    public MenuPanel menu;
    public TMPro.TextMeshProUGUI toolText;
    public VoxelData.VoxelTypes voxelType = VoxelData.VoxelTypes.Block;
    

    public enum VoxelTools
    {
        Place,
        Paint,
        Box,
        Fill,
        Last
    }
    public VoxelTools tool = VoxelTools.Paint;

    private int selectedBlockID = 1;

    World world;

    Player player;

    bool isMenuOpen = false;

    float flySpeed = 8f;
    float lookSpeed = 150f;

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

        
        if (menu.gameObject.activeSelf != isMenuOpen)
            menu.ToggleActive(isMenuOpen);

        if (GameManager.Instance.isInEditMode)
        {
            if (player.GetButtonDown(RewiredConsts.Action.OpenMenu)) isMenuOpen = !isMenuOpen;

            if (!isMenuOpen)
            {
                CameraLook();
                DoMovement();
                DoInteraction();
            }

            if (player.GetButtonDown(RewiredConsts.Action.Next)) tool = (VoxelTools)Mathf.Repeat((int)tool + 1, (int)VoxelTools.Last);
            if (player.GetButtonDown(RewiredConsts.Action.Previous)) tool = (VoxelTools)Mathf.Repeat((int)tool - 1, (int)VoxelTools.Last);
            toolText.text = System.Enum.GetName(typeof(VoxelTools), tool);
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
        }
        else
        {
            //world.GetChunkFromVector3(Vector3.zero).DoSpawners();
            world.DoAllSpawners();
            Camera.main.cullingMask &= ~LayerMask.GetMask("EditorUI");
            cam.gameObject.SetActive(false);
            isMenuOpen = false;
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

            Vector3 blockPlacePosition = blockHit;
            blockPlacePosition.x += hit.normal.x;
            blockPlacePosition.y += hit.normal.y;
            blockPlacePosition.z += hit.normal.z;

            bool placeCheck = player.GetButtonDown(RewiredConsts.Action.Place);

            if (tool == VoxelTools.Paint || tool == VoxelTools.Fill)
            {
                blockPlacePosition = blockHit;
                if (tool == VoxelTools.Paint)
                    placeCheck = player.GetButton(RewiredConsts.Action.Place);
            }

            selectCube.position = blockHit;

            if (player.GetButtonDown(RewiredConsts.Action.Destroy))
            {
                Debug.Log("Destroyed Block");
                world.GetChunkFromVector3(blockHit).EditVoxel(blockHit, 0);
            }

            if (placeCheck)
            {
                if (tool != VoxelTools.Fill)
                    world.GetChunkFromVector3(blockPlacePosition).EditVoxel(blockPlacePosition, selectedBlockID, voxelType);
                else
                {
                    Chunk currentChunk = world.GetChunkFromVector3(blockPlacePosition);

                    int blockID = currentChunk.GetVoxelFromMap(blockPlacePosition).id;

                    StartCoroutine(ReplaceBlocks(currentChunk, blockID, selectedBlockID));
                }
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                
                world.GetChunkFromVector3(blockPlacePosition).EditVoxel(blockPlacePosition, 0, VoxelData.VoxelTypes.EntitySpawner);
            }
        }
        else
        {
            selectCube.gameObject.SetActive(false);
        }
    }

    private IEnumerator ReplaceBlocks(Chunk chunk, int sourceBlock, int destinationBlock)
    {
        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {
                    
                    if (chunk.GetVoxelFromMap(new Vector3(x, y, z)).id == sourceBlock)
                    {
                        yield return null;
                        chunk.EditVoxel(new Vector3(x, y, z), destinationBlock, voxelType);
                    }
                }
            }
        }

        yield return null;
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
        selectedBlockID = Mathf.Clamp(_id, 0, world.blockTypes.Length - 1);

        return 0;
    }
}
