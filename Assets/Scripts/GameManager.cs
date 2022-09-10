using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (!_instance) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
        set => _instance = value;
    }

    public bool isInEditMode = true;

    private void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }
}