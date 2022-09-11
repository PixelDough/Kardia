using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BlockManager : MonoBehaviour
{
    [SerializeField]
    private List<BlockData> allBlockData = new List<BlockData>();

    private readonly Dictionary<Hash128, int> _blockReferences = new();

    [SerializeField] private Material material;
    private Texture2DArray _texture2DArray;

    private void Start()
    {
        _texture2DArray = new Texture2DArray(16, 16, 1024, TextureFormat.RGBA32, true, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat
        };
        
        for (int i = 0; i < allBlockData.Count; i++)
        {
            _blockReferences.Add(allBlockData[i].IdHash, i);
            
            Graphics.CopyTexture(allBlockData[i].texture, 0, _texture2DArray, i);
        }

        _texture2DArray.Apply(false, true);

        material.SetTexture("_MainTex", _texture2DArray);
    }
    
    public BlockData GetBlockData(Hash128 blockIdHash)
    {
        if (!_blockReferences.ContainsKey(blockIdHash)) return null;
        
        return allBlockData[_blockReferences[blockIdHash]];
    }

    public int GetBlockTextureIndexTest(Hash128 blockIdHash)
    {
        return _blockReferences[blockIdHash];
    }

    public void AddBlockData(BlockData blockData)
    {
        if (allBlockData.Contains(blockData)) return;
        allBlockData.Add(blockData);
    }
}
