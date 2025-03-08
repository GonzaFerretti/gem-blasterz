using System;
using System.Collections;
using Puzzler;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Random = UnityEngine.Random;

public class GeneralManager : MonoBehaviour
{
    private enum TestMode
    {
        None,
        OnlyPuzzlers,
        OnlyShooters
    }
    
    [SerializeField] 
    private PuzzlerBoard player1Board;
    
    [SerializeField] 
    private PuzzlerBoard player2Board;
    
    [SerializeField] 
    private ShooterController player3Shooter;
    
    [SerializeField] 
    private ShooterController player4Shooter;

    [SerializeField] private Camera puzzlerP1, previewP1, puzzlerP2, previewP2;

    [SerializeField] private bool enableCameras;

    [SerializeField] 
    private GameConfig gameConfig;

    [SerializeField] 
    private SoundManager sound;

    [SerializeField] 
    private int forceSeed = -1;

    [SerializeField] 
    private bool initialized;

    [SerializeField] 
    private TestMode testMode;
    
    [SerializeField]
    private Material bgMaterial;

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
        bgMaterial.SetInt("_ShouldScroll", 1);
        uint seed = forceSeed > 0 ? (uint)forceSeed : (uint)Random.Range(1, uint.MaxValue);
        Debug.Log($"CurrentSeed: {seed}");
        
        if (testMode == TestMode.None && !CheckGamepadCount()) return;

        if (player1Board == null || player2Board == null || player3Shooter == null || player4Shooter == null)
        {
            Debug.LogError("Missing game components");
            return;
        }

        player1Board.Initialize(seed);
        player2Board.Initialize(seed);
        
        if (testMode != TestMode.OnlyShooters)
        {
            player1Board.InitializeInput();
            player2Board.InitializeInput();
        }

        if (testMode != TestMode.OnlyPuzzlers)
        {
            player3Shooter.InitializeInput();
            player4Shooter.InitializeInput();
        }

        previewP1.enabled = false;
        previewP2.enabled = false;
        puzzlerP1.enabled = false;
        puzzlerP2.enabled = false;

        player1Board.OnPreviewUpdated += () =>
        {
            if (enableCameras) return;
            StartCoroutine(Test(previewP1));
        };
        player2Board.OnPreviewUpdated += () =>
        {
            if (enableCameras) return;
            StartCoroutine(Test(previewP2));
        };

        initialized = true;
    }

    public IEnumerator Test(Camera cam)
    {
        yield return new WaitForEndOfFrame();
        cam.Render();
    } 

    private void OnDestroy()
    {
        bgMaterial.SetInt("_ShouldScroll", 0);
    }

    private static bool CheckGamepadCount()
    {
        int gamepadCount = 0;
        foreach (var inputDevice in InputSystem.devices)
        {
            if (inputDevice is Gamepad)
                ++gamepadCount;
        }
        
        if (gamepadCount < 3)
        {
            Debug.LogError($"Not enough input devices, currently we have {gamepadCount}, we need 3");
            return false;
        }

        return true;
    }

    private void Update()
    {
        previewP1.enabled = enableCameras;
        previewP2.enabled = enableCameras;

        if (Time.frameCount % 2 == 0)
        {
            puzzlerP1.Render();
        }
        else
        {
            puzzlerP2.Render();
        }
        
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