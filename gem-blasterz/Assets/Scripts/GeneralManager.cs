using System;
using Puzzler;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
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
        
        // Ensure both players have no devices initially
        player1Board.PlayerInput.user.UnpairDevices();
        
        if (player2Board.PlayerInput.user.valid)
            player2Board.PlayerInput.user.UnpairDevices();

        // Get all gamepads
        var gamepads = Gamepad.all;

        // Assign keyboard to Player 1
        InputUser.PerformPairingWithDevice(Keyboard.current, player1Board.PlayerInput.user);
        player1Board.PlayerInput.SwitchCurrentControlScheme("Keyboard", Keyboard.current);

        // Assign the first gamepad to Player 2, if available
        if (gamepads.Count > 0)
        {
            InputUser.PerformPairingWithDevice(gamepads[0], player2Board.PlayerInput.user);
            player2Board.PlayerInput.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
        }
        else
        {
            Debug.LogWarning("No gamepads available for Player 2!");
        }
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