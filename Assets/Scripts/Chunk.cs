using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

    public ChunkCoord coord;

    bool isVoxelMapPopulated = false;
    private bool _isActive = false;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    GameObject chunkObject;
    Vector3 position;

    int vertexIndex = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();

    VoxelState[,,] voxelMap = new VoxelState[VoxelData.chunkSize, VoxelData.chunkSize, VoxelData.chunkSize];

    World world;

    public Chunk(ChunkCoord _coord, World _world)
    {

        coord = _coord;
        world = _world;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkSize, coord.y * VoxelData.chunkSize, coord.z * VoxelData.chunkSize);
        chunkObject.name = coord.x + ", " + coord.y + ", " + coord.z;
        position = chunkObject.transform.position;

        GenerateVoxelMap();
        CreateMeshData();
        CreateMesh();


    }


    void GenerateVoxelMap()
    {
        for (int x = 0; x < VoxelData.chunkSize; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize; z++)
                {
                    //voxelMap[x, y, z] = new VoxelState(Random.Range(1, world.blockTypes.Length));
                    if ((x == 0 || x == VoxelData.chunkSize - 1) || (y == 0 || y == VoxelData.chunkSize - 1) || (z == 0 || z == VoxelData.chunkSize - 1)) voxelMap[x, y, z] = new VoxelState(1);
                    else voxelMap[x, y, z] = new VoxelState(0);
                }
            }
        }
    }


    public void CreateMeshData()
    {

        for (int y = 0; y < VoxelData.chunkSize; y++)
        {
            for (int x = 0; x < VoxelData.chunkSize; x++)
            {
                for (int z = 0; z < VoxelData.chunkSize; z++)
                {

                    UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

    }


    public bool isActive
    {

        get { return _isActive; }
        set
        {

            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);

        }

    }

    public bool isEditable
    {

        get
        {

            if (!isVoxelMapPopulated)
                return false;
            else
                return true;

        }

    }

    public void EditVoxel(Vector3 pos, byte newID)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newID;



    }

    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.chunkSize - 1 || y < 0 || y > VoxelData.chunkSize - 1 || z < 0 || z > VoxelData.chunkSize - 1)
            return false;
        else return true;

    }

    //public void UpdateChunk()
    //{

    //    ClearMeshData();

    //    for (int y = 0; y < VoxelData.chunkSize; y++)
    //    {
    //        for (int x = 0; x < VoxelData.chunkSize; x++)
    //        {
    //            for (int z = 0; z < VoxelData.chunkSize; z++)
    //            {

    //                if (world.blockTypes[voxelMap[x, y, z].id].isSolid)
    //                    UpdateMeshData(new Vector3(x, y, z));

    //            }
    //        }
    //    }

    //    world.chunksToDraw.Enqueue(this);
    //}


    public void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

    }


    void ClearMeshData()
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();

    }


    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // If position is outside of this chunk...
        if (!IsVoxelInChunk(x, y, z))
            return false;

        return world.blockTypes[voxelMap[x, y, z].id].isSolid;

    }


    void UpdateMeshData(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        int blockID = voxelMap[x, y, z].id;

        //bool isTransparent = world.blockTypes[blockID].renderNeighborFaces;

        for (int p = 0; p < 6; p++)
        {
            Vector3Int newCheck = new Vector3Int((int)(pos.x + VoxelData.faceChecks[p].x), (int)(pos.y + VoxelData.faceChecks[p].y), (int)(pos.z + VoxelData.faceChecks[p].z));

            VoxelState neighbor = null;
            if (CheckVoxel(newCheck))
                neighbor = voxelMap[newCheck.x, newCheck.y, newCheck.z];

            if ((neighbor == null || world.blockTypes[neighbor.id].renderNeighborFaces) && world.blockTypes[blockID].isSolid)
            {

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                for (int i = 0; i < 4; i++)
                    normals.Add(VoxelData.faceChecks[p]);

                if (p == 2 || p == 3)
                    AddTexture(world.blockTypes[blockID].GetTextureID(BlockType.FaceType.TopBottom));
                else
                    AddTexture(world.blockTypes[blockID].GetTextureID(BlockType.FaceType.Side));

                //float lightLevel = neighbor.globalLightPercent;

                //colors.Add(new Color(0, 0, 0, lightLevel));
                //colors.Add(new Color(0, 0, 0, lightLevel));
                //colors.Add(new Color(0, 0, 0, lightLevel));
                //colors.Add(new Color(0, 0, 0, lightLevel));


                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                
                //else
                //{
                //    transparentTriangles.Add(vertexIndex);
                //    transparentTriangles.Add(vertexIndex + 1);
                //    transparentTriangles.Add(vertexIndex + 2);
                //    transparentTriangles.Add(vertexIndex + 2);
                //    transparentTriangles.Add(vertexIndex + 1);
                //    transparentTriangles.Add(vertexIndex + 3);
                //}

                vertexIndex += 4;

            }
        }

    }

    public VoxelState GetVoxelFromMap(Vector3 pos)
    {

        pos -= position;

        return voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

    }

    void AddTexture(int textureID)
    {

        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.normalizedBlockTextureSize;
        y *= VoxelData.normalizedBlockTextureSize;

        y = 1f - y - VoxelData.normalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.normalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y + VoxelData.normalizedBlockTextureSize));
    }

}

public class ChunkCoord
{

    public int x;
    public int y;
    public int z;

    public ChunkCoord()
    {

        x = 0;
        y = 0;
        z = 0;

    }

    public ChunkCoord(int _x, int _y, int _z)
    {

        x = _x;
        y = _y;
        z = _z;

    }

    public ChunkCoord(Vector3 pos)
    {

        Vector3 posInt = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        x = (int)posInt.x / VoxelData.chunkSize;
        y = (int)posInt.y / VoxelData.chunkSize;
        z = (int)posInt.z / VoxelData.chunkSize;

    }

    public bool Equals(ChunkCoord other)
    {

        if (other == null)
            return false;
        else if (other.x == x && other.y == y && other.z == z)
            return true;
        else
            return false;

    }

}


public class VoxelState
{
    public int id;
    
    public VoxelState()
    {
        id = 0;
    }

    public VoxelState(int _id)
    {
        id = _id;
    }
}