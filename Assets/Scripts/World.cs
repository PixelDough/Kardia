using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using Sirenix.OdinInspector;


public class World : MonoBehaviour
{

    public Transform player;
    public Vector3 spawn;

    public GameObject climbZonePrefab;

    public Material material;
    public Material materialTransparent;

    public Room[,,] rooms = new Room[VoxelData.worldSizeInChunks.x, VoxelData.worldSizeInChunks.y, VoxelData.worldSizeInChunks.z];

    [TableList()]
    public BlockType[] blockTypes;

    [HideInInspector] public List<string> blockNames = new List<string>();

    [TableList()]
    public EntitySpawnerType[] entitySpawnerTypes;

    public Chunk[,,] chunks = new Chunk[VoxelData.worldSizeInChunks.x, VoxelData.worldSizeInChunks.y, VoxelData.worldSizeInChunks.z];

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();


    private void Start()
    {

        foreach(BlockType bt in blockTypes)
        {
            blockNames.Add(bt.name);
        }

        RenderSettings.fogColor = material.GetColor("_FogColor");


        // Populate room map
        for (int x = 0; x < VoxelData.worldSizeInChunks.x; x++)
        {
            for (int y = 0; y < VoxelData.worldSizeInChunks.y; y++)
            {
                for (int z = 0; z < VoxelData.worldSizeInChunks.z; z++)
                {
                    rooms[x, y, z] = new Room(Room.Openings.Front & Room.Openings.Up, 0, 0);
                    rooms[x, y, z] = PickRoomToSpawn(rooms[x, y, z]);
                    chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this, (VoxelData.worldSizeInChunks.x > 1 && VoxelData.worldSizeInChunks.y > 1 && VoxelData.worldSizeInChunks.z > 1) && (x == 0 || x == VoxelData.worldSizeInChunks.x-1 || y == 0 || y == VoxelData.worldSizeInChunks.y-1 || z == 0 || z == VoxelData.worldSizeInChunks.z-1));
                }
            }
        }
    }


    public Room PickRoomToSpawn(Room room)
    {
        string roomType = "";
        roomType += ((room.openings & Room.Openings.Front) == Room.Openings.Front ? "f" : "");
        roomType += ((room.openings & Room.Openings.Right) == Room.Openings.Front ? "r" : "");
        roomType += ((room.openings & Room.Openings.Back) == Room.Openings.Front ? "b" : "");
        roomType += ((room.openings & Room.Openings.Left) == Room.Openings.Front ? "l" : "");
        roomType += ((room.openings & Room.Openings.Up) == Room.Openings.Front ? "u" : "");
        roomType += ((room.openings & Room.Openings.Down) == Room.Openings.Front ? "d" : "");

        int roomToSpawn = 0;
        int rotation = 0;
        bool hasUp = false;
        bool hasDown = false;

        if (roomType.Contains("u"))
        {
            hasUp = true;
            roomType.Remove(roomType.IndexOf("u"));
        }
        if (roomType.Contains("d"))
        {
            hasDown = true;
            roomType.Remove(roomType.IndexOf("d"));
        }
        
        switch (roomType)
        {
            case "f":
                roomToSpawn = 1;
                rotation = 0;
                break;
            case "r":
                roomToSpawn = 1;
                rotation = 1;
                break;
            case "b":
                roomToSpawn = 1;
                rotation = 2;
                break;
            case "l":
                roomToSpawn = 1;
                rotation = 3;
                break;

            case "fr":
                roomToSpawn = 2;
                rotation = 0;
                break;
            case "rb":
                roomToSpawn = 2;
                rotation = 1;
                break;
            case "bl":
                roomToSpawn = 2;
                rotation = 2;
                break;
            case "lf":
                roomToSpawn = 2;
                rotation = 3;
                break;

            case "fb":
                roomToSpawn = 3;
                rotation = Random.Range(0,2) * 2;
                break;
            case "rl":
                roomToSpawn = 3;
                rotation = Random.Range(0, 2) * 2;
                break;

            case "frb":
                roomToSpawn = 4;
                rotation = 0;
                break;
            case "rbl":
                roomToSpawn = 4;
                rotation = 1;
                break;
            case "blf":
                roomToSpawn = 4;
                rotation = 2;
                break;
            case "lfr":
                roomToSpawn = 4;
                rotation = 3;
                break;

            case "frbl":
                roomToSpawn = 5;
                rotation = Random.Range(0, 4);
                break;
        }

        if (hasUp) roomToSpawn += 5;
        if (hasDown) roomToSpawn += 10;

        room.roomTypeID = roomToSpawn;
        room.rotations = rotation;

        return room;
    }


    public int GetBlockIndex(string value)
    {
        for (int index = 0; index < blockNames.Count; index++)
        {
            if (blockNames[index] == value)
            {
                return index;
            }
        }
        return -1;
    }


    public Chunk GetChunkFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkSize.x);
        int y = Mathf.FloorToInt(pos.y / VoxelData.chunkSize.y);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkSize.z);
        return chunks[x, y, z];

    }

    public void DoAllSpawners()
    {
        foreach(Chunk c in chunks)
        {
            c.DoSpawners();
        }
    }

    public void DoAllDespawners()
    {
        foreach (Chunk c in chunks)
        {
            c.DoDespawners();
        }
    }

}


[System.Serializable]
public class Room
{

    public Openings openings;
    public int roomTypeID;
    public int rotations;

    public Room(Openings _openings, int _roomTypeID, int _rotations)
    {
        // EXAMPLE: How to add masks to a flag
        openings |= Openings.Front | Openings.Left;

        // EXAMPLE: How to remove masks from a flag
        openings &= ~Openings.Front & ~Openings.Left;

        roomTypeID = _roomTypeID;
        rotations = _rotations;
    }

    [System.Flags]
    public enum Openings { 
        Front = 1 << 1, 
        Back = 1 << 2, 
        Up = 1 << 3, 
        Down = 1 << 4, 
        Left = 1 << 5, 
        Right = 1 << 6 
    };

}


[System.Serializable]
public class BlockType
{

    [PreviewField(Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Center)]
    [VerticalGroup("SideFaces")]
    public Sprite textureSideFace;

    [PreviewField(Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Center)]
    [VerticalGroup("TopBottomFaces")]
    public Sprite textureTopBottomFace;

    public bool renderNeighborFaces = false;
    public bool isSolid = true;

    public string name = "";

    public enum FaceType { Side, TopBottom };

    public int GetTextureID(FaceType _faceType)
    {
        switch (_faceType)
        {
            case FaceType.Side:
                return 0;
            case FaceType.TopBottom:
                return 0;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }

    }

}



