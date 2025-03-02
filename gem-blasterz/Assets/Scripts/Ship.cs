using System;
using Puzzler;
using Shooter;
using UnityEngine;

public class Ship : MonoBehaviour, IDamageReceiver
{
    [SerializeField]
    private MeshRenderer mainMesh;
    
    [SerializeField]
    private PuzzlerBoard puzzler;
    
    [SerializeField]
    private Team team;
    
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

    public bool CanDamage(Team team)
    {
        return team != this.team;
    }

    public void ReceiveDamage(float damage)
    {
        puzzler.CorruptNextPiece();
    }
}