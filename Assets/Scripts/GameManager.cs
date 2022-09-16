using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (!_instance) _instance = FindObjectOfType<GameManager>();
            _instance.Start();
            return _instance;
        }
        set => _instance = value;
    }

    public BlockManager blockManager;
    
    public bool isInEditMode = true;

    private void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        
        blockManager.Initialize();
    }
}