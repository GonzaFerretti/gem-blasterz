using System;
using Puzzler;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GeneralManager : MonoBehaviour
{
    [SerializeField] 
    private PuzzlerBoard player1Board;
    
    [SerializeField] 
    private PuzzlerBoard player2Board;

    [SerializeField] 
    private GameConfig gameConfig;

    [SerializeField] 
    private int forceSeed = -1;

    public static GameConfig GameConfig;
    private int lastTurn = -1;
    
    private void Start()
    {
        GameConfig = gameConfig;
        uint seed = forceSeed > 0 ? (uint)forceSeed : (uint)Random.Range(1, uint.MaxValue);
        Debug.Log($"CurrentSeed: {seed}");
        player1Board.Initialize(seed);
        player2Board.Initialize(seed);
    }

    private void Update()
    {
        var currentTurn = Mathf.FloorToInt(Time.time / gameConfig.turnTime);
        if (currentTurn <= lastTurn)
            return;

        lastTurn = currentTurn;

        // if (currentTurn % 4 == 0)
        // {
        //     player1Board.Test_GeneratePieceWave();
        //     player2Board.Test_GeneratePieceWave();
        // }
        
        player1Board.UpdateBoard();
        player2Board.UpdateBoard();
    }
}