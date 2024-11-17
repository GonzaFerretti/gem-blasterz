﻿using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzler
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "General/GameConfig", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [Serializable]
        public struct GemDefinition
        {
            public PuzzlerBoard.GemType gemType;
            public GameObject prefab;
        }

        public GemDefinition GetGemDefinition(PuzzlerBoard.GemType gemType)
        {
            foreach (var gemDefinition in gemDefinitions)
            {
                if (gemDefinition.gemType.Equals(gemType))
                    return gemDefinition;
            }

            throw new Exception("This shouldn't happen");
        }

        [Header("Puzzler")]
        public List<GemDefinition> gemDefinitions;
        public int2 gridSize;
        public float turnTime;
        public int matchNumber = 3;
        public GameObject testBackground;
    }
}