using System;
using Puzzler;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer mainMesh;
    
    [SerializeField]
    private PuzzlerBoard puzzler;
    
    private static readonly int percentageGradientAlpha = Shader.PropertyToID("_StepTester");
    private Material percentageGradientMaterial;

    public void Awake()
    {
        Material[] materials = mainMesh.materials;
        percentageGradientMaterial = Material.Instantiate(materials[1]);
        materials[1] = percentageGradientMaterial;
        mainMesh.materials = materials;
    }

    public void ReceiveDamage()
    {
        puzzler.CorruptNextPiece();
    }

    public void UpdateSideFillPercentage(float percentage)
    {
        percentageGradientMaterial.SetFloat(percentageGradientAlpha, percentage);
    }
}