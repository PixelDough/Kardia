﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Chunk
{

    public ChunkCoord coord;

    bool isVoxelMapPopulated = false;
    private bool _isActive = false;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public GameObject chunkObject;
    Vector3 position;

    int vertexIndex = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();
    List<GameObject> spawners = new List<GameObject>();

    VoxelState[,,] voxelMap = new VoxelState[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

    World world;

    public Chunk(ChunkCoord _coord, World _world)
    {

        coord = _coord;
        world = _world;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        chunkObject.isStatic = true;

        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkSize.x, coord.y * VoxelData.chunkSize.y, coord.z * VoxelData.chunkSize.z);
        chunkObject.name = coord.x + ", " + coord.y + ", " + coord.z;
        position = chunkObject.transform.position;

        GenerateVoxelMap();
        


    }


    public void GenerateVoxelMap()
    {
        //for (int i = 0; i < chunkObject.transform.childCount; i++)
        //{
        //    Object.Destroy(chunkObject.transform.GetChild(i));
        //}

        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {
                    //voxelMap[x, y, z] = new VoxelState(Random.Range(1, world.blockTypes.Length));
                    if ((x == 0 || x == VoxelData.chunkSize.x - 1) || (y == 0 || y == VoxelData.chunkSize.y - 1) || (z == 0 || z == VoxelData.chunkSize.z - 1)) voxelMap[x, y, z] = new VoxelState(1);
                    else voxelMap[x, y, z] = new VoxelState(0);
                }
            }
        }

        CreateMeshData();
        CreateMesh();
    }


    public void CreateMeshData()
    {

        ClearMeshData();

        for (int y = 0; y < VoxelData.chunkSize.y; y++)
        {
            for (int x = 0; x < VoxelData.chunkSize.x; x++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
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

    public void EditVoxel(Vector3 pos, int newID, VoxelData.VoxelTypes _voxelType = VoxelData.VoxelTypes.Block)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newID;
        voxelMap[xCheck, yCheck, zCheck].voxelType = _voxelType;

        CreateMeshData();
        CreateMesh();

    }

    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.chunkSize.x - 1 || y < 0 || y > VoxelData.chunkSize.y - 1 || z < 0 || z > VoxelData.chunkSize.z - 1)
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

        foreach (GameObject s in spawners) Object.Destroy(s);
        spawners.Clear();

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

        // BLOCKS
        if (voxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.Block)
        {
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
                        AddTexture(world.blockTypes[blockID].textureTopBottomFace);
                    else
                        AddTexture(world.blockTypes[blockID].textureSideFace);

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

        if (voxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.EntitySpawner)
        {
            EntitySpawnerType entitySpawnerType = world.entitySpawnerTypes[voxelMap[x, y, z].id];

            if (true)
            {
                int diceRoll = Random.Range(1, 101);
                if (diceRoll <= entitySpawnerType.chanceToSpawnCheck)
                {
                    GameObject selectedObject = entitySpawnerType.prefabSpawnPool[Random.Range(0, entitySpawnerType.prefabSpawnPool.Length)];
                    spawners.Add(Object.Instantiate(selectedObject, pos, Quaternion.identity));
                    voxelMap[x, y, z].spawned = true;
                }
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

    void AddTexture(Sprite sprite)
    {
        //It's important to note that Rect is a value type because it is a struct, so this copies the Rect.  You don't want to change the original.

        float x = sprite.rect.x;
        float y = sprite.rect.y;
        float width = sprite.rect.width;
        float height = sprite.rect.height;

        x /= sprite.texture.width;
        width /= sprite.texture.width;
        y /= sprite.texture.height;
        height /= sprite.texture.height;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + height));
        uvs.Add(new Vector2(x + width, y));
        uvs.Add(new Vector2(x + width, y + height));
    }


    public void SaveChunk(string fileName)
    {
        VoxelMapData voxelMapData = new VoxelMapData(voxelMap);

        Debug.Log("Saving Chunk");

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(Application.dataPath + "/" + fileName + ".chunk");
        //bf.Serialize(file, voxelMapData);
        //file.Close();

        string json = JsonUtility.ToJson(voxelMapData);
        File.WriteAllText(Application.streamingAssetsPath + "/Rooms/" + fileName + ".chunk", json);
        Debug.Log("Saved Chunk as: " + fileName + ".chunk");
    }

    public void LoadChunk(string fileName)
    {
        Debug.Log("Loading Chunk");

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Open(Application.dataPath + "/" + fileName + ".chunk", FileMode.Open);
        //object voxelMapData = bf.Deserialize<VoxelMapData>(file);
        //voxelMapData.
        //file.Close();

        string json = File.ReadAllText(Application.streamingAssetsPath + "/Rooms/" + fileName + ".chunk");
        VoxelMapData voxelMapData = new VoxelMapData();
        voxelMapData.voxelMaps = JsonUtility.FromJson<VoxelMapData>(json).voxelMaps;

        voxelMap = voxelMapData.GetFullVoxelMap();
        

        CreateMeshData();
        CreateMesh();

        Debug.Log("Loaded Chunk: " + fileName + ".chunk");
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

        x = (int)posInt.x / VoxelData.chunkSize.x;
        y = (int)posInt.y / VoxelData.chunkSize.y;
        z = (int)posInt.z / VoxelData.chunkSize.z;

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

[System.Serializable]
public class VoxelMapData
{
    [SerializeField]
    public VoxelState[] voxelMaps = new VoxelState[VoxelData.chunkSize.x * VoxelData.chunkSize.y * VoxelData.chunkSize.z];

    public VoxelMapData(VoxelState[,,] voxelStates)
    {
        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {
                    int index = VoxelData.chunkSize.y * VoxelData.chunkSize.x * z + VoxelData.chunkSize.x * y + x;
                    voxelMaps[index] = voxelStates[x, y, z];
                }
            }
        }
    }

    public VoxelMapData()
    {
        voxelMaps[0] = new VoxelState(0);
    }

    public VoxelState[,,] GetFullVoxelMap()
    {

        VoxelState[,,] fullVoxelMap = new VoxelState[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {
                    int index = VoxelData.chunkSize.y * VoxelData.chunkSize.x * z + VoxelData.chunkSize.x * y + x;
                    fullVoxelMap[x, y, z] = voxelMaps[index];
                }
            }
        }

        return fullVoxelMap;
    }

}


[System.Serializable]
public class VoxelState
{
    public int id = 0;
    public VoxelData.VoxelTypes voxelType = VoxelData.VoxelTypes.Block;
    public bool spawned = false;

    public VoxelState(int _id)
    {
        id = _id;
    }

    public VoxelState(int _id, VoxelData.VoxelTypes _voxelType)
    {
        id = _id;
        voxelType = _voxelType;
    }
}