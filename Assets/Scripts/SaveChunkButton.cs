using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveChunkButton : MonoBehaviour
{

    public TMPro.TextMeshProUGUI fileNameText;

    World world;

    private void Start()
    {
        world = FindObjectOfType<World>();
    }


    public void SaveChunkClicked()
    {
        world.chunks[0, 0, 0].SaveChunk(fileNameText.text);
    }

    public void LoadChunkClicked()
    {
        world.chunks[0, 0, 0].LoadChunk(fileNameText.text);
    }

    public void ResetChunkClicked()
    {
        world.chunks[0, 0, 0].GenerateVoxelMap();
    }

}
