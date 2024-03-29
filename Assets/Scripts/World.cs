﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class World : MonoBehaviour
{
    public ZoneType zoneType;

    [TableList()] public BlockType[] blockTypes;

    [HideInInspector] public List<string> blockNames = new();

    [TableList()] public EntitySpawnerType[] entitySpawnerTypes;

    public readonly Chunk[,,] Chunks = new Chunk[VoxelData.worldSizeInChunks.x, VoxelData.worldSizeInChunks.y,
        VoxelData.worldSizeInChunks.z];

    private void Start()
    {
        RenderSettings.fogColor = zoneType.fogColor;

        // Populate room map
        for (int x = 0; x < VoxelData.worldSizeInChunks.x; x++)
        {
            for (int y = 0; y < VoxelData.worldSizeInChunks.y; y++)
            {
                for (int z = 0; z < VoxelData.worldSizeInChunks.z; z++)
                {
                    //rooms[x, y, z] = new Room(Room.Openings.Right | Room.Openings.Left, 0, Random.Range(0, 4));
                    //rooms[x, y, z] = PickRoomToSpawn(rooms[x, y, z]);
                    //chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this,
                    //    (VoxelData.worldSizeInChunks.x > 1 && VoxelData.worldSizeInChunks.y > 1 &&
                    //     VoxelData.worldSizeInChunks.z > 1) && (x == 0 || x == VoxelData.worldSizeInChunks.x - 1 ||
                    //                                            y == 0 || y == VoxelData.worldSizeInChunks.y - 1 ||
                    //                                            z == 0 || z == VoxelData.worldSizeInChunks.z - 1),
                    //    rooms[x, y, z].rotations);

                    Chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this);
                }
            }
        }
    }

    public Room PickRoomToSpawn(Room room)
    {
        string roomType = "";
        if (room.openings.HasFlag(Room.Openings.Front)) roomType += "f";
        if (room.openings.HasFlag(Room.Openings.Right)) roomType += "r";
        if (room.openings.HasFlag(Room.Openings.Back)) roomType += "b";
        if (room.openings.HasFlag(Room.Openings.Left)) roomType += "l";
        if (room.openings.HasFlag(Room.Openings.Up)) roomType += "u";
        if (room.openings.HasFlag(Room.Openings.Down)) roomType += "d";

        int roomToSpawn = 0;
        int rotation = 0;
        bool hasUp = false;
        bool hasDown = false;

        if (roomType.Contains("u"))
        {
            hasUp = true;
            rotation = Random.Range(0,
                4); // Set random rotation for vertical tunnels, gets overridden if there are any horizontally planar entrances.
            roomType.Remove(roomType.IndexOf("u", StringComparison.Ordinal));
        }

        if (roomType.Contains("d"))
        {
            hasDown = true;
            rotation = Random.Range(0,
                4); // Set random rotation for vertical tunnels, gets overridden if there are any horizontally planar entrances.
            roomType.Remove(roomType.IndexOf("d", StringComparison.Ordinal));
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
                rotation = Random.Range(0, 2) * 2;
                break;
            case "rl":
                roomToSpawn = 3;
                rotation = (Random.Range(0, 2) * 2) + 1;
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
        return Chunks[x, y, z];
    }

    public bool IsChunkInWorld(ChunkCoord chunkCoord)
    {
        if (chunkCoord.X < 0 && chunkCoord.X > VoxelData.worldSizeInChunks.x) return false;
        if (chunkCoord.Y < 0 && chunkCoord.Y > VoxelData.worldSizeInChunks.y) return false;
        if (chunkCoord.Z < 0 && chunkCoord.Z > VoxelData.worldSizeInChunks.z) return false;

        return true;
    }

    public bool IsChunkInWorld(Vector3 chunkCoordVector3)
    {
        return IsChunkInWorld(new ChunkCoord(chunkCoordVector3));
    }

    public void DoAllSpawners()
    {
        foreach (Chunk c in Chunks)
        {
            c.DoSpawners();
        }
    }

    public void DoAllDespawners()
    {
        foreach (Chunk c in Chunks)
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
        //// EXAMPLE: How to add masks to a flag
        //openings |= Openings.Front | Openings.Left;

        //// EXAMPLE: How to remove masks from a flag
        //openings &= ~Openings.Front & ~Openings.Left;

        openings = _openings;
        roomTypeID = _roomTypeID;
        rotations = _rotations;
    }

    [System.Flags]
    public enum Openings
    {
        Front = 1 << 1,
        Back = 1 << 2,
        Up = 1 << 3,
        Down = 1 << 4,
        Left = 1 << 5,
        Right = 1 << 6,
        Count
    };
}


[System.Serializable]
public class BlockType
{
    [PreviewField(Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Center)] [VerticalGroup("SideFaces")]
    public Sprite textureSideFace;

    [PreviewField(Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Center)] [VerticalGroup("TopBottomFaces")]
    public Sprite textureTopBottomFace;

    public bool renderNeighborFaces = false;
    public bool isSolid = true;

    public string name = "";

    public enum FaceType
    {
        Side,
        TopBottom
    };

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