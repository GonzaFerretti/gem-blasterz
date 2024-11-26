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
    private ShooterController player3Shooter;
    
    [SerializeField] 
    private ShooterController player4Shooter;

    [SerializeField] 
    private GameConfig gameConfig;

    [SerializeField] 
    private SoundManager sound;

    [SerializeField] 
    private int forceSeed = -1;

    [SerializeField] 
    private bool initialized;

    public static GameConfig GameConfig;
    public static SoundManager Sound;

    private static GeneralManager instance;
    public static bool GameStarted => instance != null && instance.initialized;
    
    private float boardTurnTimer = 0;
    private float p1TurnTimer = 0;
    private float p1HeldPressTimer = 0;
    private float p2HeldPressTimer = 0;
    private float p2TurnTimer = 0;
    
    private void Start()
    {
        instance = this;
        Sound = sound;
        GameConfig = gameConfig;
        uint seed = forceSeed > 0 ? (uint)forceSeed : (uint)Random.Range(1, uint.MaxValue);
        Debug.Log($"CurrentSeed: {seed}");

        int gamepadCount = 0;
        foreach (var inputDevice in InputSystem.devices)
        {
            if (inputDevice is Gamepad)
                ++gamepadCount;
        }
        
        if (gamepadCount < 3)
        {
            Debug.LogError($"Not enough input devices, currently we have {gamepadCount}, we need 3");
            return;
        }
        
        if (player1Board == null || player2Board == null || player3Shooter == null || player4Shooter == null)
        {
            Debug.LogError("Missing game components");
            return;
        }
        
        player1Board.Initialize(seed);
        player2Board.Initialize(seed);
        player3Shooter.Initialize();
        player4Shooter.Initialize();

        initialized = true;

        // Ensure both players have no devices initially
        // if (player1Board.PlayerInput.user.valid)
        //     player1Board.PlayerInput.user.UnpairDevices();
        //
        // if (player2Board.PlayerInput.user.valid)
        //     player2Board.PlayerInput.user.UnpairDevices();

        // Get all gamepads
        // var gamepads = Gamepad.all;
        //
        // // Assign keyboard to Player 1
        // InputUser.PerformPairingWithDevice(Keyboard.current, player1Board.PlayerInput.user);
        // player1Board.PlayerInput.SwitchCurrentControlScheme("Keyboard", Keyboard.current);
        //
        // // Assign the first gamepad to Player 2, if available
        // if (gamepads.Count > 0)
        // {
        //     InputUser.PerformPairingWithDevice(gamepads[0], player2Board.PlayerInput.user);
        //     player2Board.PlayerInput.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
        // }
        // else
        // {
        //     Debug.LogWarning("No gamepads available for Player 2!");
        // }
    }

    private void Update()
    {
        if (!GeneralManager.GameStarted)
            return;
        
        if (TryAdvanceTurn(ref p1HeldPressTimer, GameConfig.heldPressTurnTimeMultiplier))
        {
            player1Board.TryMoveHeld();
        }
        
        if (TryAdvanceTurn(ref p2HeldPressTimer, GameConfig.heldPressTurnTimeMultiplier))
        {
            player2Board.TryMoveHeld();
        }
        
        var p1Multiplier = player1Board.WantsFallFaster() ? GameConfig.fallFasterMultipler : 1;
        if (TryAdvanceTurn(ref p1TurnTimer, gameConfig.turnTime / p1Multiplier))
        {
            player1Board.UpdatePieces();
        }
        
        var p2Multiplier = player2Board.WantsFallFaster() ? GameConfig.fallFasterMultipler : 1;
        if (TryAdvanceTurn(ref p2TurnTimer, gameConfig.turnTime / p2Multiplier))
        {
            player2Board.UpdatePieces();
        }

        if (TryAdvanceTurn(ref boardTurnTimer, gameConfig.turnTime))
        {
            player1Board.UpdateBoard();
            player2Board.UpdateBoard();
        }
    }

    private bool TryAdvanceTurn(ref float turnTime, float totalTurnTime = 1)
    {
        turnTime += Time.deltaTime;
        if (turnTime >= totalTurnTime)
        {
            turnTime = 0;
            return true;
        }

        return false;
    }
}