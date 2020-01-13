using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

public class Chunk
{

    public ChunkCoord coord;
    public bool isBorderChunk;

    bool isVoxelMapPopulated = false;
    private bool _isActive = false;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public GameObject chunkObject;
    Vector3 position;

    int rotations = 0;

    int vertexIndex = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();
    List<Spawner> spawners = new List<Spawner>();

    Queue<Vector3> lightBfsQueue = new Queue<Vector3>();
    List<Color> colors = new List<Color>();
    float[,,] lightMap = new float[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

    VoxelState[,,] voxelMap = new VoxelState[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

    Queue<Vector3Int> spawnersToUpdate = new Queue<Vector3Int>();
    Queue<Vector3Int> blocksToUpdate = new Queue<Vector3Int>();

    World world;

    public Chunk(ChunkCoord _coord, World _world, bool _isBorderChunk = false, int _rotations = 0)
    {

        coord = _coord;
        world = _world;
        isBorderChunk = _isBorderChunk;
        rotations = _rotations;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        chunkObject.isStatic = true;
        chunkObject.tag = "Wall";
        chunkObject.layer = LayerMask.NameToLayer("Wall");

        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkSize.x, coord.y * VoxelData.chunkSize.y, coord.z * VoxelData.chunkSize.z);
        chunkObject.name = coord.x + ", " + coord.y + ", " + coord.z;
        position = chunkObject.transform.position;

        GenerateVoxelMap();

        if (!isBorderChunk)
        {
            string[] chunkFiles = Directory.GetFiles(Application.streamingAssetsPath + "/Rooms", "tomb_*.chunk");
            string selectedChunkFile = Path.GetFileNameWithoutExtension(chunkFiles[Random.Range(0, chunkFiles.Length)]);
            LoadChunk(selectedChunkFile);
        }


    }


    public void GenerateVoxelMap()
    {
        //for (int i = 0; i < chunkObject.transform.childCount; i++)
        //{
        //    Object.Destroy(chunkObject.transform.GetChild(i));
        //}

        ClearMeshData();

        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {
                    lightMap[x, y, z] = 0;

                    if (!isBorderChunk)
                    {
                        //voxelMap[x, y, z] = new VoxelState(Random.Range(1, world.blockTypes.Length));
                        if ((x == 0 || x == VoxelData.chunkSize.x - 1) || (y == 0 || y == VoxelData.chunkSize.y - 1) || (z == 0 || z == VoxelData.chunkSize.z - 1)) voxelMap[x, y, z] = new VoxelState(1, "tomb:block");
                        else voxelMap[x, y, z] = new VoxelState(0, "air");
                    }
                    else
                    {
                        voxelMap[x, y, z] = new VoxelState(10, "barrier");
                    }
                }
            }
        }

        CreateMeshData();
        CreateMesh();
    }


    public void CreateMeshData()
    {

        ClearMeshData();

        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {
                    UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

        UpdateSpawners();

        while (lightBfsQueue.Count > 0)
        {
            Vector3 v = lightBfsQueue.Peek();

            //for (int p = 0; p < 6; p++)
            //{
            //    Vector3 currentVoxel = new Vector3Int((int)(v.x + VoxelData.faceChecks[p].x), (int)(v.y + VoxelData.faceChecks[p].y), (int)(v.z + VoxelData.faceChecks[p].z));
            //    Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

            //    if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
            //    {
            //        Debug.Log(GetVoxelFromMap(neighbor));
            //        if (!world.blockTypes[GetVoxelFromMap(neighbor).id].renderNeighborFaces)
            //        {
            //            lightMap[neighbor.x, neighbor.y, neighbor.z] = 0f;
            //        }
            //        else
            //        {

            //            if (lightMap[neighbor.x, neighbor.y, neighbor.z] < lightMap[(int)v.x, (int)v.y, (int)v.z] - VoxelData.lightFalloff)
            //            {
            //                lightMap[neighbor.x, neighbor.y, neighbor.z] = lightMap[(int)v.x, (int)v.y, (int)v.z] - VoxelData.lightFalloff;

            //                //if (lightMap[neighbor.x, neighbor.y, neighbor.z] > VoxelData.lightFalloff)
            //                lightBfsQueue.Enqueue(neighbor);
            //            }
            //        }
                    
            //    }
            //}

            lightBfsQueue.Dequeue();
        }

        UpdateBlocks();
        

        //while (lightBfsQueue.Count > 0)
        //{
        //    //    Vector3 currentLightBlock = lightBfsQueue.Dequeue();

        //    //    for (int p = 0; p < 6; p++)
        //    //    {
        //    //        Vector3Int newCheckPos = new Vector3Int((int)(currentLightBlock.x + VoxelData.faceChecks[p].x), (int)(currentLightBlock.y + VoxelData.faceChecks[p].y), (int)(currentLightBlock.z + VoxelData.faceChecks[p].z));

        //    //        VoxelState neighbor = null;
        //    //        if (CheckVoxel(newCheckPos))
        //    //            neighbor = voxelMap[newCheckPos.x, newCheckPos.y, newCheckPos.z];

        //    //        if (neighbor != null)
        //    //        {

        //    //            if (lightMap[newCheckPos.x, newCheckPos.y, newCheckPos.z] < lightMap[(int)currentLightBlock.x, (int)currentLightBlock.y, (int)currentLightBlock.z] - 0.1)
        //    //            {
        //    //                Debug.Log(colors.Count);
        //    //                //lightMap[newCheckPos.x, newCheckPos.y, newCheckPos.z] = lightMap[(int)currentLightBlock.x, (int)currentLightBlock.y, (int)currentLightBlock.z] - 0.1;
        //    //                for (int i = 0; i < 4; i ++)
        //    //                {
        //    //                    int id = VoxelData.chunkSize.y * VoxelData.chunkSize.x * newCheckPos.z + VoxelData.chunkSize.x * newCheckPos.y + newCheckPos.x;
        //    //                    id *= 24;
        //    //                    id += p;
        //    //                    id += i;
        //    //                    colors[id] = new Color(1, 1, 1, 1);
        //    //                }
        //    //            }

        //    //        }

        //    //    }
        //    //}

        //    Vector3 v = lightBfsQueue.Dequeue();

        //    for (int p = 0; p < 6; p++)
        //    {
        //        Vector3 currentVoxel = v + VoxelData.faceChecks[p];
        //        Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

        //        if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
        //        {
        //            if (lightMap[neighbor.x, neighbor.y, neighbor.z] < lightMap[(int)v.x, (int)v.y, (int)v.z] - VoxelData.lightFalloff)
        //            {
        //                lightMap[neighbor.x, neighbor.y, neighbor.z] = lightMap[(int)v.x, (int)v.y, (int)v.z] - VoxelData.lightFalloff;

        //                if (lightMap[neighbor.x, neighbor.y, neighbor.z] > VoxelData.lightFalloff)
        //                    lightBfsQueue.Enqueue(neighbor);
        //            }
        //        }
        //    }
        //}

        //for (int y = 0; y < VoxelData.chunkSize.y; y++)
        //{
        //    for (int x = 0; x < VoxelData.chunkSize.x; x++)
        //    {
        //        for (int z = 0; z < VoxelData.chunkSize.z; z++)
        //        {
        //            // Set color of block faces based on lighting of neighboring blocks.

        //            int blockID = voxelMap[x, y, z].id;

        //            // BLOCKS
        //            if (voxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.Block)
        //            {
        //                for (int p = 0; p < 6; p++)
        //                {
        //                    Vector3Int newCheck = new Vector3Int((int)(x + VoxelData.faceChecks[p].x), (int)(y + VoxelData.faceChecks[p].y), (int)(z + VoxelData.faceChecks[p].z));

        //                    VoxelState neighbor = null;
        //                    if (CheckVoxel(newCheck))
        //                        neighbor = voxelMap[newCheck.x, newCheck.y, newCheck.z];

        //                    if (neighbor != null)
        //                    {
        //                        int blockArrayID = GetBlockArrayIndex(newCheck);
        //                        int id2 = (blockArrayID * 24) + p;
        //                        Debug.Log(colors.Count + " " + id2);
        //                        colors[id2 + 0] = (new Color(0, 0, 0, lightMap[newCheck.x, newCheck.y, newCheck.z]));
        //                        colors[id2 + 1] = (new Color(0, 0, 0, lightMap[newCheck.x, newCheck.y, newCheck.z]));
        //                        colors[id2 + 2] = (new Color(0, 0, 0, lightMap[newCheck.x, newCheck.y, newCheck.z]));
        //                        colors[id2 + 3] = (new Color(0, 0, 0, lightMap[newCheck.x, newCheck.y, newCheck.z]));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
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
        voxelMap[xCheck, yCheck, zCheck].blockName = world.blockNames[newID];
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

    public void DoSpawners()
    {
        foreach (Spawner s in spawners)
        {
            s.DoSpawn();
        }
    }

    public void DoDespawners()
    {
        foreach (Spawner s in spawners)
        {
            s.DoDespawn();
        }
    }


    public void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        //Debug.LogError(vertices.Count + " " + colors.Count);
        mesh.colors = colors.ToArray();

        mesh.normals = normals.ToArray();
        //mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

    }


    void ClearMeshData(bool clearSpawners = false)
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();
        colors.Clear();
        lightBfsQueue.Clear();
        lightMap = new float[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];
        spawnersToUpdate.Clear();
        blocksToUpdate.Clear();
        //foreach (Spawner s in spawners)
        //{
        //    Object.Destroy(s.gameObject);
        //}
        //spawners.Clear();


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


    private void UpdateSpawners()
    {
        while (spawnersToUpdate.Count > 0)
        {
            Vector3Int spawnerCurrent = spawnersToUpdate.Dequeue();

            EntitySpawnerType entitySpawnerType = world.entitySpawnerTypes[voxelMap[spawnerCurrent.x, spawnerCurrent.y, spawnerCurrent.z].id];

            if (entitySpawnerType.isLight)
            {
                lightMap[spawnerCurrent.x, spawnerCurrent.y, spawnerCurrent.z] = 1;
                lightBfsQueue.Enqueue(new Vector3(spawnerCurrent.x, spawnerCurrent.y, spawnerCurrent.z));
            }



            bool doCreate = true;

            foreach (Spawner s in spawners.ToArray())
            {
                if (Vector3.Distance(spawnerCurrent + chunkObject.transform.position, s.position) <= 0.05f)
                {
                    doCreate = false;
                }
            }

            if (doCreate)
                spawners.Add(Object.Instantiate(entitySpawnerType.prefabSpawner, spawnerCurrent + chunkObject.transform.position, Quaternion.identity).GetComponent<Spawner>());
            //voxelMap[x, y, z].spawned = true;
        }
    }


    private void UpdateBlocks()
    {
        while (blocksToUpdate.Count > 0)
        {
            Vector3Int blockCurrent = blocksToUpdate.Dequeue();

            string blockName = voxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].blockName;
            int blockID = voxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].id;

            blockID = world.GetBlockIndex(blockName);

            if (blockID == -1)
            {
                blockID = voxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].id;
                voxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].blockName = world.blockTypes[blockID].name;
            }

            for (int p = 0; p < 6; p++)
            {

                Vector3Int newCheck = new Vector3Int((int)(blockCurrent.x + VoxelData.faceChecks[p].x), (int)(blockCurrent.y + VoxelData.faceChecks[p].y), (int)(blockCurrent.z + VoxelData.faceChecks[p].z));

                VoxelState neighbor = null;
                if (CheckVoxel(newCheck))
                    neighbor = voxelMap[newCheck.x, newCheck.y, newCheck.z];

                if ((neighbor == null || (world.blockTypes[neighbor.id].renderNeighborFaces && world.blockTypes[neighbor.id].name != world.blockTypes[blockID].name) || neighbor.voxelType != VoxelData.VoxelTypes.Block) && world.blockTypes[blockID].isSolid)
                {


                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 vertPos = blockCurrent + VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                        Vector3 vertNorm = VoxelData.faceChecks[p];

                        //int otherVert = vertices.IndexOf(vertPos);
                        //if (otherVert != -1)
                        //{
                        //    vertNorm = (vertNorm + normals[otherVert]) / 2f;
                        //}

                        vertices.Add(vertPos);
                        normals.Add(vertNorm);

                        float lightValue = 0;

                        //if (IsVoxelInChunk(newCheck.x, newCheck.y, newCheck.z))
                        //{
                        //    lightValue = lightMap[newCheck.x, newCheck.y, newCheck.z];

                        //    for (int _x = -1; _x <= 1; _x += 2)
                        //    {
                        //        for (int _y = -1; _y <= 1; _y += 2)
                        //        {
                        //            if (newCheck.x + _x > 0 && newCheck.x + _x < VoxelData.chunkSize.x && newCheck.z + _y > 0 && newCheck.z + _y < VoxelData.chunkSize.z && world.blockTypes[GetVoxelFromMap(new Vector3(newCheck.x + _x, newCheck.y, newCheck.z + _y)).id].renderNeighborFaces)
                        //                lightValue = (lightValue + lightMap[newCheck.x + _x, newCheck.y, newCheck.z + _y]) / 2f;
                        //        }
                        //    }

                        //    colors.Add(new Color(0, 0, 0, lightValue));
                        //}
                        //else
                            colors.Add(new Color(0, 0, 0, 0));
                    }

                    if (p == 2 || p == 3)
                        AddTexture(world.blockTypes[blockID].textureTopBottomFace);
                    else
                        AddTexture(world.blockTypes[blockID].textureSideFace);


                    //if (IsVoxelInChunk(newCheck.x, newCheck.y, newCheck.z))
                    //{
                    //    float lightValue = lightMap[newCheck.x, newCheck.y, newCheck.z];
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //    colors.Add(new Color(1, 1, 1, lightValue));

                    //}
                    //else
                    //{
                    //    float lightValue = 1;
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //    colors.Add(new Color(1, 1, 1, lightValue));
                    //}



                    //colors.Add(new Color(0, 0, 0, 0.5f));
                    //colors.Add(new Color(0, 0, 0, 0.5f));
                    //colors.Add(new Color(0, 0, 0, 0.5f));
                    //colors.Add(new Color(0, 0, 0, 0.5f));


                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);

                    vertexIndex += 4;

                }


            }
        }
    }


    void UpdateMeshData(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //Debug.LogError(voxelMap[x, y, z] != null);

        string blockName = voxelMap[x, y, z].blockName;
        //Debug.Log(blockName);

        int blockID = voxelMap[x, y, z].id;

        blockID = world.GetBlockIndex(blockName);

        if (blockID == -1)
        {
            blockID = voxelMap[x, y, z].id;
            voxelMap[x, y, z].blockName = world.blockTypes[blockID].name;
        }

        foreach (Spawner s in spawners.ToArray())
        {
            if (Vector3.Distance(pos + chunkObject.transform.position, s.position) <= 0.05f)
            {
                spawners.Remove(s);
                Object.Destroy(s.gameObject);

            }

        }

        // TODO: Move this into it's own function. Update spawners, then update lighting, then update voxel data.
        if (voxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.EntitySpawner)
        {

            spawnersToUpdate.Enqueue(new Vector3Int(x, y, z));

            

        }


        // BLOCKS
        if (voxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.Block)
        {

            blocksToUpdate.Enqueue(new Vector3Int(x, y, z));

            
        }

        // DELETE THIS

        
        

    }

    public VoxelState GetVoxelFromMap(Vector3 pos)
    {

        pos -= position;

        if (pos.x < 0 || pos.x > VoxelData.chunkSize.x) return null;
        if (pos.y < 0 || pos.y > VoxelData.chunkSize.y) return null;
        if (pos.z < 0 || pos.z > VoxelData.chunkSize.z) return null;

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


        VoxelState[,,] newVoxelStates = new VoxelState[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

        for (int x = 0; x < VoxelData.chunkSize.x; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize.y; y++)
            {
                for (int z = 0; z < VoxelData.chunkSize.z; z++)
                {

                    // Chunk rotation system
                    Vector3 voxelPos = new Vector3(x, y, z);
                    Vector3 pos = voxelPos;
                    for (int i = 0; i < rotations; i++)
                    {
                        pos.z = VoxelData.chunkSize.x - 1 - voxelPos.x;
                        pos.x = voxelPos.z;

                        voxelPos = pos;
                    }

                    VoxelState vs = new VoxelState(0);

                    if (IsVoxelInChunk(x, y, z))
                    {
                        vs.blockName = voxelMap[x, y, z].blockName;
                        vs.id = voxelMap[x, y, z].id;
                        vs.voxelType = voxelMap[x, y, z].voxelType;
                    }

                    try
                    {
                        newVoxelStates[(int)pos.x, (int)pos.y, (int)pos.z] = vs;
                    }
                    catch
                    {
                        Debug.LogError(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z));
                    }
                }
            }
        }

        voxelMap = newVoxelStates;

        CreateMeshData();
        CreateMesh();

        Debug.Log("Loaded Chunk: " + fileName + ".chunk");
    }

    public int GetBlockArrayIndex(Vector3 pos)
    {
        return (int)(VoxelData.chunkSize.y * VoxelData.chunkSize.x * pos.z + VoxelData.chunkSize.x * pos.y + pos.x);
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
        
        //voxelMaps[0] = new VoxelState(0, "");
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
    public string blockName = "air";
    public VoxelData.VoxelTypes voxelType = VoxelData.VoxelTypes.Block;
    //public bool spawned = false;

    public VoxelState(int _id)
    {
        id = _id;
    }

    public VoxelState(int _id, string _name)
    {
        id = _id;
        blockName = _name;
    }

    public VoxelState(int _id, VoxelData.VoxelTypes _voxelType)
    {
        id = _id;
        voxelType = _voxelType;
    }

}