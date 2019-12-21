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

    public Material material;

    public Room[] rooms;

    [TableList()]
    public BlockType[] blockTypes;

    Chunk[,,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();


    private void Start()
    {
        for(int x = 0; x < VoxelData.worldSizeInChunks; x++)
        {
            for (int y = 0; y < VoxelData.worldSizeInChunks; y++)
            {
                for (int z = 0; z < VoxelData.worldSizeInChunks; z++)
                {
                    chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this);
                }
            }
        }
    }

}

[System.Serializable]
public class Room
{

    public Openings openings;

    public Room()
    {
        // EXAMPLE: How to add masks to a flag
        openings |= Openings.Front | Openings.Left;

        // EXAMPLE: How to remove masks from a flag
        openings &= ~Openings.Front & ~Openings.Left;
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

    public int sideFace = 0;
    public int topBottomFace = 0;

    public bool renderNeighborFaces = false;
    public bool isSolid = true;

    public enum FaceType { Side, TopBottom };

    public int GetTextureID(FaceType _faceType)
    {
        switch (_faceType)
        {
            case FaceType.Side:
                return sideFace;
            case FaceType.TopBottom:
                return topBottomFace;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }

    }

}
