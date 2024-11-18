using System;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer mainMesh;
    private static readonly int percentageGradientAlpha = Shader.PropertyToID("_StepTester");
    private Material percentageGradientMaterial;

    public void Awake()
    {
        Material[] materials = mainMesh.materials;
        percentageGradientMaterial = Material.Instantiate(materials[1]);
        materials[1] = percentageGradientMaterial;
        mainMesh.materials = materials;
    }

    public void UpdateSideFillPercentage(float percentage)
    {
        percentageGradientMaterial.SetFloat(percentageGradientAlpha, percentage);
    }
}