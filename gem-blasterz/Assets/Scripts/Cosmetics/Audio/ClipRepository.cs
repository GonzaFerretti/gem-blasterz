﻿using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sound/Sound Repository")]
public class ClipRepository : ScriptableObject
{
    [SerializeField] List<ClipConfig> sounds;

    public ClipConfig? GetClipConfig(string name)
    {
        foreach (ClipConfig clipConfig in sounds)
        {
            if (clipConfig.identifier == name)
            {
                return clipConfig;
            }
        }
        return null;
    }
}