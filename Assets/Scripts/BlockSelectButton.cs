using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSelectButton : MonoBehaviour
{
    
    public int blockID = 0;
    public VoxelData.VoxelTypes voxelType;
    public Image image = null;
    public Image hudImage = null;

    private CreativeCam creativeCam;

    private void Start()
    {
        creativeCam = FindObjectOfType<CreativeCam>();
    }

    public void SetCreativeIndex()
    {
        creativeCam.ChangeSelectedBlockID(blockID);
        creativeCam.voxelType = voxelType;
        hudImage.sprite = image.sprite;
    }

}
