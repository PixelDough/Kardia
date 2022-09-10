using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class BlockData : ScriptableObject
{
    [DisplayAsString]
    public GUID guid;
    
    public new string name = "";

    [PreviewField()]
    public Texture texture;

    public bool renderNeighborFaces = false;
    
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (guid.Empty())
        {
            guid = GUID.Generate();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    public virtual void AddTexturesToArray(ref Texture2DArray array)
    {
        
    }

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
