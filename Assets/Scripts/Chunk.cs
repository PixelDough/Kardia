using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Chunk
{
    public ChunkCoord Coord;
    public ChunkVoxelPalette ChunkVoxelPalette;
    public VoxelState[,,] VoxelMap =
        new VoxelState[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

    private readonly MeshFilter _meshFilter;
    private readonly MeshCollider _meshCollider;
    private readonly GameObject _chunkObject;
    private readonly Vector3 _position;

    private int _vertexIndex;

    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();
    private readonly List<Vector3> _uvs = new();
    private readonly List<Vector3> _normals = new();
    private readonly List<Spawner> _spawners = new();

    private readonly Queue<Vector3> _lightBfsQueue = new();
    private readonly List<Color> _colors = new();
    private float[,,] _lightMap = new float[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];

    private readonly Queue<Vector3Int> _spawnersToUpdate = new();
    private readonly Queue<Vector3Int> _blocksToUpdate = new();

    private readonly World _world;

    public Chunk(ChunkCoord coord, World world)
    {
        Coord = coord;
        _world = world;

        _chunkObject = new GameObject();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        var meshRenderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshCollider = _chunkObject.AddComponent<MeshCollider>();
        _chunkObject.isStatic = false;
        _chunkObject.tag = "Wall";
        _chunkObject.layer = LayerMask.NameToLayer("Wall");

        meshRenderer.material = _world.zoneType.blockMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.On;
        //meshRenderer.receiveShadows = false;

        _chunkObject.transform.SetParent(this._world.transform);
        _chunkObject.transform.position = new Vector3(Coord.X * VoxelData.chunkSize.x, Coord.Y * VoxelData.chunkSize.y,
            Coord.Z * VoxelData.chunkSize.z);
        _chunkObject.name = Coord.X + ", " + Coord.Y + ", " + Coord.Z;
        _position = _chunkObject.transform.position;

        ChunkVoxelPalette = ChunkVoxelPalette.Create();

        GenerateVoxelMap();
    }

    public void GenerateVoxelMap()
    {
        ClearMeshData();
        
        ChunkVoxelPalette.palette.Add(new Hash128());
        ChunkVoxelPalette.palette.Add(Hash128.Compute("tomb.block"));
        ChunkVoxelPalette.palette.Add(Hash128.Compute("tomb.tile"));
        ChunkVoxelPalette.palette.Add(Hash128.Compute("tomb.sand"));

        for (var x = 0; x < VoxelData.chunkSize.x; x++)
        for (var y = 0; y < VoxelData.chunkSize.y; y++)
        for (var z = 0; z < VoxelData.chunkSize.z; z++)
        {
            _lightMap[x, y, z] = 0;
            VoxelMap[x, y, z] = new VoxelState();
            if (y == 0)
            {
                VoxelMap[x, y, z] = new VoxelState()
                {
                    index = Random.Range(1, 4)
                };
            }
        }

        CreateMeshData();
        CreateMesh();
    }

    public void CreateMeshData()
    {
        ClearMeshData();

        for (var x = 0; x < VoxelData.chunkSize.x; x++)
        for (var y = 0; y < VoxelData.chunkSize.y; y++)
        for (var z = 0; z < VoxelData.chunkSize.z; z++)
            UpdateMeshData(new Vector3(x, y, z));

        UpdateSpawners();

        UpdateBlocks();
    }

    private bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkSize.x - 1 || y < 0 || y > VoxelData.chunkSize.y - 1 || z < 0 ||
            z > VoxelData.chunkSize.z - 1)
            return false;
        return true;
    }

    public void DoSpawners()
    {
        foreach (var s in _spawners) s.DoSpawn();
    }

    public void DoDespawners()
    {
        foreach (var s in _spawners) s.DoDespawn();
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            triangles = _triangles.ToArray()
        };
        mesh.SetUVs(0, _uvs);
        
        mesh.colors = _colors.ToArray();

        mesh.normals = _normals.ToArray();

        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    private void ClearMeshData(bool clearSpawners = false)
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();
        _normals.Clear();
        _colors.Clear();
        _lightBfsQueue.Clear();
        _lightMap = new float[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];
        _spawnersToUpdate.Clear();
        _blocksToUpdate.Clear();
    }

    private bool CheckVoxel(Vector3 pos)
    {
        var x = Mathf.FloorToInt(pos.x);
        var y = Mathf.FloorToInt(pos.y);
        var z = Mathf.FloorToInt(pos.z);

        // If position is outside of this chunk...
        if (!IsVoxelInChunk(x, y, z)) return false;

        return true; //_world.blockTypes[VoxelMap[x, y, z].id].isSolid;
    }

    private void UpdateSpawners()
    {
        // while (_spawnersToUpdate.Count > 0)
        // {
        //     var spawnerCurrent = _spawnersToUpdate.Dequeue();
        //
        //     var entitySpawnerType =
        //         _world.entitySpawnerTypes[VoxelMap[spawnerCurrent.x, spawnerCurrent.y, spawnerCurrent.z].id];
        //
        //     if (entitySpawnerType.isLight)
        //     {
        //         _lightMap[spawnerCurrent.x, spawnerCurrent.y, spawnerCurrent.z] = 1;
        //         _lightBfsQueue.Enqueue(new Vector3(spawnerCurrent.x, spawnerCurrent.y, spawnerCurrent.z));
        //     }
        //
        //     var doCreate = true;
        //
        //     foreach (var s in _spawners.ToArray())
        //         if (Vector3.Distance(spawnerCurrent + _chunkObject.transform.position, s.position) <= 0.05f)
        //             doCreate = false;
        //
        //     if (doCreate)
        //         _spawners.Add(Object.Instantiate(entitySpawnerType.prefabSpawner,
        //                 spawnerCurrent + _chunkObject.transform.position, Quaternion.identity)
        //             .GetComponent<Spawner>());
        // }
    }

    private void UpdateBlocks()
    {
        while (_blocksToUpdate.Count > 0)
        {
            var blockCurrent = _blocksToUpdate.Dequeue();

            var blockID = ChunkVoxelPalette.palette[VoxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].index];
            var blockData = GameManager.Instance.blockManager.GetBlockData(blockID);
            // if (blockID == -1)
            // {
            //     blockID = VoxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].id;
            //     VoxelMap[blockCurrent.x, blockCurrent.y, blockCurrent.z].blockName = _world.blockTypes[blockID].name;
            // }

            for (var p = 0; p < 6; p++)
            {
                var newCheck = new Vector3Int((int)(blockCurrent.x + VoxelData.faceChecks[p].x),
                    (int)(blockCurrent.y + VoxelData.faceChecks[p].y),
                    (int)(blockCurrent.z + VoxelData.faceChecks[p].z));

                VoxelState neighbor = default;
                if (CheckVoxel(newCheck)) neighbor = VoxelMap[newCheck.x, newCheck.y, newCheck.z];

                var neighborBlockId = ChunkVoxelPalette.palette[neighbor.index];
                var neighborBlockData = GameManager.Instance.blockManager.GetBlockData(neighborBlockId);
                
                if (neighborBlockData == null ||
                    (neighborBlockData.renderNeighborFaces))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var vertPos = blockCurrent + VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                        var vertNorm = VoxelData.faceChecks[p];

                        _vertices.Add(vertPos);
                        _normals.Add(vertNorm);

                        float lightValue = 0;

                        _colors.Add(new Color(0, 0, 0, 0));
                    }

                    if (p == 2 || p == 3)
                        AddTexture(GameManager.Instance.blockManager.GetBlockTextureIndexTest(blockData.IdHash));
                    else
                        AddTexture(GameManager.Instance.blockManager.GetBlockTextureIndexTest(blockData.IdHash));

                    _triangles.Add(_vertexIndex);
                    _triangles.Add(_vertexIndex + 1);
                    _triangles.Add(_vertexIndex + 2);
                    _triangles.Add(_vertexIndex + 2);
                    _triangles.Add(_vertexIndex + 1);
                    _triangles.Add(_vertexIndex + 3);

                    _vertexIndex += 4;
                }
            }
        }
    }

    private void UpdateMeshData(Vector3 pos)
    {
        var x = Mathf.FloorToInt(pos.x);
        var y = Mathf.FloorToInt(pos.y);
        var z = Mathf.FloorToInt(pos.z);

        if (VoxelMap[x, y, z].index == 0) return;
        var blockID = ChunkVoxelPalette.palette[VoxelMap[x, y, z].index];

        // foreach (var s in _spawners.ToArray())
        //     if (Vector3.Distance(pos + _chunkObject.transform.position, s.position) <= 0.05f)
        //     {
        //         _spawners.Remove(s);
        //         Object.Destroy(s.gameObject);
        //     }

        // TODO: Move this into it's own function. Update spawners, then update lighting, then update voxel data.
        // if (VoxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.EntitySpawner)
        //     _spawnersToUpdate.Enqueue(new Vector3Int(x, y, z));

        // BLOCKS
        // if (VoxelMap[x, y, z].voxelType == VoxelData.VoxelTypes.Block) _blocksToUpdate.Enqueue(new Vector3Int(x, y, z));
        _blocksToUpdate.Enqueue(new Vector3Int(x, y, z));
    }

    private void AddTexture(int blockIdTest = 0)
    {
        var rand = Random.Range(0, 29);
        _uvs.Add(new Vector3(0, 0, blockIdTest));
        _uvs.Add(new Vector3(0, 1, blockIdTest));
        _uvs.Add(new Vector3(1, 0, blockIdTest));
        _uvs.Add(new Vector3(1, 1, blockIdTest));
    }

    public void SaveChunk(string fileName)
    {
        // ChunkData chunkData = new ChunkData(VoxelMap);
        //
        // Debug.Log("Saving Chunk");
        //
        // //BinaryFormatter bf = new BinaryFormatter();
        // //FileStream file = File.Create(Application.dataPath + "/" + fileName + ".chunk");
        // //bf.Serialize(file, voxelMapData);
        // //file.Close();
        //
        // var json = JsonUtility.ToJson(chunkData);
        // File.WriteAllText(Application.streamingAssetsPath + "/Rooms/" + fileName + ".chunk", json);
        // Debug.Log("Saved Chunk as: " + fileName + ".chunk");
    }

    public void LoadChunk(string fileName)
    {
        // Debug.Log("Loading Chunk");
        //
        // //BinaryFormatter bf = new BinaryFormatter();
        // //FileStream file = File.Open(Application.dataPath + "/" + fileName + ".chunk", FileMode.Open);
        // //object voxelMapData = bf.Deserialize<VoxelMapData>(file);
        // //voxelMapData.
        // //file.Close();
        //
        // var json = File.ReadAllText(Application.streamingAssetsPath + "/Rooms/" + fileName + ".chunk");
        // ChunkData chunkData = new ChunkData();
        // chunkData.voxelMaps = JsonUtility.FromJson<ChunkData>(json).voxelMaps;
        //
        // VoxelMap = chunkData.GetFullVoxelMap();
        //
        // var newVoxelStates =
        //     new VoxelState[VoxelData.chunkSize.x, VoxelData.chunkSize.y, VoxelData.chunkSize.z];
        //
        // for (var x = 0; x < VoxelData.chunkSize.x; x++)
        // for (var y = 0; y < VoxelData.chunkSize.y; y++)
        // for (var z = 0; z < VoxelData.chunkSize.z; z++)
        // {
        //     // Chunk rotation system
        //     var voxelPos = new Vector3(x, y, z);
        //     var pos = voxelPos;
        //     for (var i = 0; i < rotations; i++)
        //     {
        //         pos.z = VoxelData.chunkSize.x - 1 - voxelPos.x;
        //         pos.x = voxelPos.z;
        //
        //         voxelPos = pos;
        //     }
        //
        //     var vs = new VoxelState(0);
        //
        //     if (IsVoxelInChunk(x, y, z))
        //     {
        //         vs.blockName = VoxelMap[x, y, z].blockName;
        //         vs.id = VoxelMap[x, y, z].id;
        //         vs.voxelType = VoxelMap[x, y, z].voxelType;
        //     }
        //
        //     //Debug.Log(new Vector3(pos.z, pos.y, VoxelData.chunkSize.x - 1 - pos.x));
        //     try
        //     {
        //         // ROTATION
        //         newVoxelStates[(int)pos.z, (int)pos.y, VoxelData.chunkSize.x - 1 - (int)pos.x] = vs;
        //         //newVoxelStates[(int)pos.z, (int)pos.y, VoxelData.chunkSize.x - (int)pos.x] = vs;
        //     }
        //     catch
        //     {
        //         Debug.LogError(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z));
        //     }
        // }
        //
        // VoxelMap = newVoxelStates;
        //
        // CreateMeshData();
        // CreateMesh();
        //
        // Debug.Log("Loaded Chunk: " + fileName + ".chunk");
    }

    public int GetBlockArrayIndex(Vector3 pos)
    {
        return (int)(VoxelData.chunkSize.y * VoxelData.chunkSize.x * pos.z + VoxelData.chunkSize.x * pos.y + pos.x);
    }
}

public struct ChunkCoord
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public ChunkCoord(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public ChunkCoord(Vector3 pos)
    {
        var posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        X = posInt.x / VoxelData.chunkSize.x;
        Y = posInt.y / VoxelData.chunkSize.y;
        Z = posInt.z / VoxelData.chunkSize.z;
    }
}

[Serializable]
public struct ChunkVoxelPalette
{
    public List<Hash128> palette;

    public static ChunkVoxelPalette Create()
    {
        return new ChunkVoxelPalette()
        {
            palette = new List<Hash128>()
        };
    }
}

[Serializable]
public struct VoxelState
{
    public int index;

    public VoxelState(int index)
    {
        this.index = index;
    }
}