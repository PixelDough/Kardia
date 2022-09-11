using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class BlockData : ScriptableObject
{
    [FormerlySerializedAs("name")] public string blockId = "";

    [PreviewField()]
    public Texture texture;

    public bool renderNeighborFaces = false;

    public Hash128 IdHash => Hash128.Compute(blockId);

    public virtual void AddTexturesToArray(ref Texture2DArray array)
    {
        
    }

    [Button]
    private void AddToBlockManager()
    {
        FindObjectOfType<BlockManager>().AddBlockData(this);
    }

    // private void Awake()
    // {
    //     GameManager.Instance.blockManager.AddBlockData(this);
    // }

    // public int GetTextureID(FaceType _faceType)
    // {
    //     switch (_faceType)
    //     {
    //         case FaceType.Side:
    //             return sideFace;
    //         case FaceType.TopBottom:
    //             return topBottomFace;
    //         default:
    //             Debug.Log("Error in GetTextureID; invalid face index");
    //             return 0;
    //     }
    // }
}
