﻿using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;


public class ArenaData : MonoBehaviour
{
    [SerializeField] public GameObject[] snappingObjects;
    public static ArenaData instance;
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public GameObject GETId(string name)
    {
        foreach (var point in snappingObjects)
        {
            if (point.name.Equals(name))
            {
                return point;
            }
        }

        if (int.TryParse(name, out int id)) {
            return GameManager.players[id].gameObject;
        }

        return null;
    }
}