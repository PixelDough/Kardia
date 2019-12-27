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

    public Room[] rooms;

    [TableList()]
    public BlockType[] blockTypes;

    [TableList()]
    public EntitySpawnerType[] entitySpawnerTypes;

    public Chunk[,,] chunks = new Chunk[VoxelData.worldSizeInChunks.x, VoxelData.worldSizeInChunks.y, VoxelData.worldSizeInChunks.z];

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();


    private void Start()
    {
        for(int x = 0; x < VoxelData.worldSizeInChunks.x; x++)
        {
            for (int y = 0; y < VoxelData.worldSizeInChunks.y; y++)
            {
                for (int z = 0; z < VoxelData.worldSizeInChunks.z; z++)
                {
                    chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this);
                }
            }
        }
    }


    public Chunk GetChunkFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkSize.x);
        int y = Mathf.FloorToInt(pos.y / VoxelData.chunkSize.y);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkSize.z);
        return chunks[x, y, z];

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



