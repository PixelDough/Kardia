using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class BlockData : ScriptableObject
{
    public Texture blockTextures;

    public int sideFace = 0;
    [PreviewField(), HideLabel]
    public Texture textureSideFace;

    public int topBottomFace = 0;
    [PreviewField(), HideLabel]
    public Texture textureTopBottomFace;

    public bool renderNeighborFaces = false;
    public bool isSolid = true;

    public new string name = "";

    public enum FaceType { Side, TopBottom };

    private void OnValidate()
    {

        //textureSideFace = GetTextureFromBlocks(sideFace);

    }

    private Texture GetTextureFromBlocks(int index)
    {
        float y = index / VoxelData.textureAtlasSizeInBlocks;
        float x = index - (y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.normalizedBlockTextureSize;
        y *= VoxelData.normalizedBlockTextureSize;

        y = 1f - y - VoxelData.normalizedBlockTextureSize;

        Texture2D texture = new Texture2D((int)VoxelData.normalizedBlockTextureSize, (int)VoxelData.normalizedBlockTextureSize);

        Graphics.CopyTexture(blockTextures, 0, 0, (int)x, (int)y, texture.width, texture.height, texture, 0, 0, 0, 0);

        return texture as Texture;
    }

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
