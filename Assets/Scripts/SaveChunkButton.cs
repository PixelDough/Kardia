﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

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
        string path = Path.GetFileNameWithoutExtension(EditorUtility.SaveFilePanel("Load Room File", Application.streamingAssetsPath + "/Rooms", "biome_name", "chunk"));
        if (path.Length != 0) 
            world.Chunks[0, 0, 0].SaveChunk(path);
    }

    public void LoadChunkClicked()
    {
        string path = Path.GetFileNameWithoutExtension(EditorUtility.OpenFilePanel("Load Room File", Application.streamingAssetsPath + "/Rooms", "*chunk"));
        if (path.Length != 0)
            world.Chunks[0, 0, 0].LoadChunk(path);
    }

    public void ResetChunkClicked()
    {
        world.Chunks[0, 0, 0].GenerateVoxelMap();
    }

}
