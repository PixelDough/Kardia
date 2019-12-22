using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CreativeMenu : MenuPanel
{

    World world;
    public GridLayoutGroup blockGroup;
    public BlockSelectButton blockButton;

    public List<string> roomFiles;

    private void Start()
    {
        world = FindObjectOfType<World>();

        BlockSelectButton currentButton = blockButton;

        for (int i = 1; i < world.blockTypes.Length; i++)
        {
            currentButton.transform.SetAsLastSibling();

            currentButton.image.sprite = world.blockTypes[i].textureSideFace;
            currentButton.blockID = i;

            if (i < world.blockTypes.Length - 1)
                currentButton = Instantiate(blockButton.gameObject, blockGroup.transform).GetComponent<BlockSelectButton>();
        }

        UpdateFileNames();

    }


    public void UpdateFileNames()
    {
        roomFiles = new List<string>(Directory.GetFiles(Application.streamingAssetsPath + "/Rooms", "*.chunk"));

        foreach(string s in roomFiles)
        {

            Debug.Log(roomFiles.IndexOf(s) + ": " + Path.GetFileName(s));
        }
    }



}
