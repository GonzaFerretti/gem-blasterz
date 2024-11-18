using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Random = Unity.Mathematics.Random;
using Vector3 = UnityEngine.Vector3;

namespace Puzzler
{
    public class PuzzlerBoard : MonoBehaviour
    {
        private Random rng;

        [SerializeField]
        private Transform nextPiecePreview;
        
        [SerializeField]
        private PlayerInput playerInput;

        public PlayerInput PlayerInput => playerInput;
        
        [SerializeField]
        private Grid grid;

        private List<Piece> pieces = new();
        private GridSlot[,] logicalGrid;

        [SerializeField]
        private List<PieceConfiguration> nextPieces = new();

        private Piece activePiece;
        public Piece ActivePiece => activePiece;
        
        [SerializeField]
        private List<Gem> currentGems;

        [Serializable]
        public struct GridSlot
        {
            public Gem gem;
        }
        
        [Serializable]
        public struct GemType : IEquatable<GemType>
        {
            public string name;
            public bool isScrap;
            
            public bool Equals(GemType other)
            {
                return name == other.name;
            }

            public override bool Equals(object obj)
            {
                return obj is GemType other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (name != null ? name.GetHashCode() : 0);
            }

            public override string ToString()
            {
                return name;
            }
        }

        [Serializable]
        public class Gem
        {
            public GemType type;
            public GameObject model;
            public Vector3 lastGridPos;
            public bool inPiece;
            public bool blocked;
        }
        
        [Serializable]
        public class Piece
        {
            public List<Gem> gems;
        }

        [Serializable]
        public struct PieceConfiguration
        {
            public List<GemType> gemTypes;
            public int turns;
        }

        private struct Match
        {
            public List<Gem> matchedGems;
            public int scrapCount;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) CorruptNextPiece();
        }

        public void UpdateCurrentPreview()
        {
            foreach (var childTransform in nextPiecePreview.GetComponentInChildren<Transform>())
            {
                if ((childTransform as Transform).gameObject != null)
                {
                    Destroy((childTransform as Transform).gameObject);
                }
            }

            var firstNextConfig = nextPieces[0];
            var prefab0 = CreateGemPrefab(firstNextConfig.gemTypes[0], nextPiecePreview.position + Vector3.Scale(grid.cellSize, new Vector3(1f,0f, 0f)), $"Preview: {firstNextConfig.gemTypes[0].name}", nextPiecePreview);
            var prefab1 = CreateGemPrefab(firstNextConfig.gemTypes[1], nextPiecePreview.position + Vector3.Scale(grid.cellSize, new Vector3(0f,1f, 0f)), $"Preview: {firstNextConfig.gemTypes[1].name}", nextPiecePreview);
            var prefab2 = CreateGemPrefab(firstNextConfig.gemTypes[2], nextPiecePreview.position + Vector3.Scale(grid.cellSize, new Vector3(1f,1f, 0f)), $"Preview: {firstNextConfig.gemTypes[2].name}", nextPiecePreview);

            var center = nextPiecePreview.position + grid.cellSize / 2;
            prefab0.transform.position = RotateAround(prefab0.transform.position, center, firstNextConfig.turns);
            prefab1.transform.position = RotateAround(prefab1.transform.position, center, firstNextConfig.turns);
            prefab2.transform.position = RotateAround(prefab2.transform.position, center, firstNextConfig.turns);
        }

        public void Initialize(uint seed)
        {
            logicalGrid = new GridSlot[GeneralManager.GameConfig.gridSize.x, GeneralManager.GameConfig.gridSize.y];
            rng = new Random(seed);

            for (int i = 0; i < 4; i++) 
                nextPieces.Add(GeneratePieceConfiguriation());

            UpdateCurrentPreview();

            foreach (var inputAction in playerInput.actions)
            {
                if (inputAction.name == "Move")
                {
                    inputAction.performed += OnMoveRequested;
                }
                else if (inputAction.name == "Rotate")
                {
                    inputAction.performed += OnRotateRequested;
                }
            }
        }

        public bool WantsFallFaster()
        {
            foreach (var inputAction in playerInput.actions)
            {
                if (inputAction.name == "FallFaster")
                {
                    return inputAction.IsPressed();
                }
            }

            return false;
        }

        private void OnRotateRequested(InputAction.CallbackContext obj)
        {
            var dir = obj.ReadValue<float>();

            if (activePiece == null) return;
            if (dir == 0) return;
            if (!CanRotatePiece(activePiece)) return;
            
            RotatePiece(activePiece, 1, (int)math.sign(dir) == 1);
        }

        private void OnMoveRequested(InputAction.CallbackContext obj)
        {
            var dir = obj.ReadValue<float>();

            if (activePiece == null) return;

            MoveActivePiece(dir);
        }

        private void MoveActivePiece(float dir)
        {
            if (dir == 0) return;
            var offset = Vector3.right * math.sign(dir);
            foreach (var gem in activePiece.gems)
            {
                if (AnyCollision(gem, offset))
                    return;
            }

            foreach (var gem in activePiece.gems) 
                NudgeGem(gem, offset);
        }

        public bool CanSpawnPiece(int2 pos)
        {
            if (logicalGrid[pos.x, pos.y].gem != null)
                return false;
            
            if (logicalGrid[pos.x + 1, pos.y].gem != null)
                return false;
            
            if (logicalGrid[pos.x, pos.y + 1].gem != null)
                return false;
            
            if (logicalGrid[pos.x + 1, pos.y + 1].gem != null)
                return false;

            return true;
        }

        public Piece GeneratePieceAtPos(int2 pos, PieceConfiguration config)
        {
            var newPiece = GeneratePiece(new Vector3(pos.x, pos.y), config);
            pieces.Add(newPiece);
            return newPiece;
        }

        private struct WeightedGem
        {
            public float weight;
            public GemType type;
        }

        public void UpdateBoard()
        {
            EnsureActivePiece();

            var blockedAny = false;
            currentGems = currentGems.OrderBy(g => g.lastGridPos.y).ToList();
            foreach (var gem in currentGems)
            {
                if (!gem.inPiece)
                {
                    if (!gem.blocked)
                    {
                        if (TryFindFallTargetPos(gem, out var targetPos))
                        {
                            var initialPos = gem.lastGridPos;
                            MoveGem(gem, targetPos);
                            if (AnyCollision(initialPos, Vector3.up) && (int)initialPos.y + 1 < GeneralManager.GameConfig.gridSize.y &&  logicalGrid[(int)initialPos.x, (int)initialPos.y + 1].gem != null) 
                                logicalGrid[(int)initialPos.x, (int)initialPos.y + 1].gem.blocked = false;
                        }
                        
                        gem.blocked = true;
                        blockedAny = true;
                    }
                }
            }

            var anyMoving = false;
            foreach (var gem in currentGems)
            {
                if (!gem.blocked && !gem.inPiece)
                {
                    anyMoving = true;
                    break;
                }
            }
            if (!anyMoving && !blockedAny)
                ClearMatchingGems();
            LogWrongPos();
        }

        public void TryMoveHeld()
        {
            var moveHoldAction = playerInput.actions.FindAction("MoveHold");
            if (activePiece != null && moveHoldAction.phase == InputActionPhase.Performed)
                MoveActivePiece(moveHoldAction.ReadValue<float>());
        }

        private void EnsureActivePiece()
        {
            var anyMoving = false;
            foreach (var gem in currentGems)
            {
                if (!gem.blocked && !gem.inPiece)
                {
                    anyMoving = true;
                    break;
                }
            }

            var validMatches = new List<Match>();
            FindMatches(validMatches);
            if (activePiece == null && !anyMoving && validMatches.Count == 0)
            {
                var couldSpawn = false;
                var initialPos = new int2(0, GeneralManager.GameConfig.gridSize.y - 2);
                for (int i = 0; i < GeneralManager.GameConfig.gridSize.x / 2; i++)
                {
                    if (CanSpawnPiece(initialPos))
                    {
                        var newPiece = nextPieces[0];
                        nextPieces.RemoveAt(0);
                        activePiece = GeneratePieceAtPos(initialPos, newPiece);
                        nextPieces.Add(GeneratePieceConfiguriation());
                        UpdateCurrentPreview();
                        couldSpawn = true;
                        break;
                    }
                    initialPos.x += 2;
                }

                if (!couldSpawn) 
                    Debug.Log($"Player from {gameObject.name} lost.");
            }
        }

        private bool TryFindFallTargetPos(Gem gem, out Vector3 targetPos)
        {
            targetPos = default;
            if (gem.lastGridPos.y == 0)
                return false;

            var possibleTarget = gem.lastGridPos;
            bool foundFallTarget = false;
            while (!AnyCollision(possibleTarget, Vector3.down))
            {
                possibleTarget += Vector3.down;
                targetPos = possibleTarget;
                foundFallTarget = true;
            }

            return foundFallTarget;
        }

        public void UpdatePieces()
        {
            var brokenPieces = new List<Piece>();
            foreach (var piece in pieces)
            {
                bool impacted = false;
                foreach (var gem in piece.gems)
                {
                    if (AnyCollision(gem, Vector3.down))
                    {
                        brokenPieces.Add(piece);
                        impacted = true;
                        break;
                    }
                }

                if (impacted) continue;
                    
                foreach (var gem in piece.gems)
                {
                    NudgeGem(gem, Vector3.down);
                }
            }
            
            foreach (var brokenPiece in brokenPieces)
            {
                pieces.Remove(brokenPiece);
                if (brokenPiece == activePiece)
                    activePiece = null;
                foreach (var gem in brokenPiece.gems)
                {
                    gem.inPiece = false;
                }
            }
        }

        private void LogWrongPos()
        {
            foreach (var currentGem in currentGems)
            {
                if (logicalGrid[(int)currentGem.lastGridPos.x, (int)currentGem.lastGridPos.y].gem != currentGem)
                {
                    int2? foundPos = null;
                    var rows = GeneralManager.GameConfig.gridSize.x;
                    var cols = GeneralManager.GameConfig.gridSize.y;
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            if (logicalGrid[i, j].gem == currentGem)
                            {
                                foundPos = new int2(i, j);
                            }
                        }
                    }

                    var location = foundPos.HasValue ? $"{foundPos.Value}" : "nowhere";
                    Debug.LogError($"{currentGem.type} should be in {currentGem.lastGridPos} but is in {location}");
                }
            }
        }

        private void ClearMatchingGems()
        {
            var validMatches = new List<Match>();
            FindMatches(validMatches);
            foreach (var validMatch in validMatches)
            {
                foreach (var matchedGem in validMatch.matchedGems)
                {
                    if (matchedGem != null)
                        DestroyGem(matchedGem);
                }
            }
        }

        void FindMatches(List<Match> validMatches)
        {
            var rows = GeneralManager.GameConfig.gridSize.x;
            var cols = GeneralManager.GameConfig.gridSize.y;
            bool[,] visited = new bool[rows, cols];
            var directions = new (int x, int y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) }; // Right, Down, Left, Up
            var stack = new Stack<(int x, int y)>();

            // Start searching from each unvisited slot
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (visited[i, j] || logicalGrid[i, j].gem == null || logicalGrid[i, j].gem.type.isScrap) continue;
                    // Get the gem type dynamically for the current cell
                    var gemType = logicalGrid[i, j].gem.type;
                    var matches = new Match() { matchedGems = new List<Gem>() };
                    stack.Push((i, j));

                    while (stack.Count > 0)
                    {
                        var (x, y) = stack.Pop();
                        if (x < 0 || x >= rows || y < 0 || y >= cols)
                            continue; // Out of bounds

                        if (visited[x, y])
                            continue; // Already visited

                        if (IsCellEmpty(x, y))
                            continue; // No gem in this cell

                        var gridSlot = logicalGrid[x, y];
                        if (gridSlot.gem.inPiece)
                            continue; // Gem is part of a piece, skip it

                        if (!gridSlot.gem.blocked)
                            continue; // Gem is not blocked, skip it

                        if (!gridSlot.gem.type.isScrap && !gridSlot.gem.type.Equals(gemType))
                            continue; // Gem type does not match

                        visited[x, y] = !gridSlot.gem.type.isScrap; // Scrap can be visited multiple times
                        matches.matchedGems.Add(gridSlot.gem);

                        if (gridSlot.gem.type.isScrap)
                        {
                            matches.scrapCount++;
                            continue;
                        }

                        // Push neighboring cells to the stack
                        foreach (var dir in directions)
                        {
                            stack.Push((x + dir.x, y + dir.y));
                        }
                    }

                    // Only add the match if it has the required number of gems
                    if (matches.matchedGems.Count - matches.scrapCount >= GeneralManager.GameConfig.matchNumber)
                    {
                        validMatches.Add(matches);
                    }
                }
            }
        }

        private bool IsCellEmpty(int x, int y)
        {
            return logicalGrid[x, y].gem == null;
        }

        void DestroyGem(Gem gem)
        {
            var gridX = (int)gem.lastGridPos.x;
            var gridY = (int)gem.lastGridPos.y;

            if (AnyCollision(gem, Vector3.up) && logicalGrid[gridX, gridY + 1].gem != null) 
                logicalGrid[gridX, gridY + 1].gem.blocked = false;
            
            logicalGrid[gridX, gridY].gem = null;
            currentGems.Remove(gem);
            Destroy(gem.model);
        }

        private bool AnyCollision(Gem gem, Vector3 offset, bool ignoreOnPiece = true)
        {
            return AnyCollision(gem.lastGridPos, offset, ignoreOnPiece);
        }
        private bool AnyCollision(Vector3 sourcePos, Vector3 offset, bool ignoreOnPiece = true)
        {
            var targetPos = sourcePos + offset;
            
            if (targetPos.y < 0 || targetPos.x < 0 || targetPos.y >= GeneralManager.GameConfig.gridSize.y ||  targetPos.x >= GeneralManager.GameConfig.gridSize.x)
            {
                return true;
            }
            
            foreach (var currentGem in currentGems)
            {
                if (ignoreOnPiece && currentGem.inPiece) continue;
                if (currentGem.lastGridPos == targetPos)
                {
                    return true;
                }
            }

            return false;
        }
        
        private Piece GeneratePiece(Vector3 initialPosition, PieceConfiguration pieceConfig)
        {
            var newPiece = new Piece(){ gems = new()};
            
            newPiece.gems.Add(CreateGem(pieceConfig.gemTypes[0], initialPosition + new Vector3(1f,0f, 0f), inPiece: true));
            newPiece.gems.Add(CreateGem(pieceConfig.gemTypes[1], initialPosition + new Vector3(0f,1f, 0f), inPiece: true));
            newPiece.gems.Add(CreateGem(pieceConfig.gemTypes[2], initialPosition + new Vector3(1f,1f, 0f), inPiece: true));

            RotatePiece(newPiece, pieceConfig.turns);
            return newPiece;
        }

        public void CorruptNextPiece()
        {
            var scrap = GeneralManager.GameConfig.GetGemDefinition("Scrap").gemType;
            for (var index = 0; index < nextPieces.Count; index++)
            {
                var configuration = nextPieces[index];
                var availableIndices = new List<int>();
                for (var j = 0; j < configuration.gemTypes.Count; j++)
                {
                    var gemType = configuration.gemTypes[j];
                    if (!gemType.isScrap) availableIndices.Add(j);
                }

                if (availableIndices.Count > 0)
                {
                    var indexToCurse = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                    configuration.gemTypes[indexToCurse] = scrap;
                    nextPieces[index] = configuration;
                    UpdateCurrentPreview();
                    return;
                }
            }
        }

        private PieceConfiguration GeneratePieceConfiguriation()
        {
            var weightedGemTypes = new List<WeightedGem>();
            
            // Populate with definition with equal weights initially
            foreach (var definition in GeneralManager.GameConfig.gemDefinitions)
            {
                if (!definition.gemType.isScrap)
                    weightedGemTypes.Add(new WeightedGem(){ type = definition.gemType, weight = 1f});
            }

            var pieceConfiguration = new PieceConfiguration() { gemTypes = new() };
            pieceConfiguration.gemTypes.Add(RollNextPiece(weightedGemTypes));
            pieceConfiguration.gemTypes.Add(RollNextPiece(weightedGemTypes));
            pieceConfiguration.gemTypes.Add(RollNextPiece(weightedGemTypes));
            pieceConfiguration.turns = rng.NextInt(4);
            return pieceConfiguration;
        }

        private bool CanRotatePiece(Piece piece)
        {
            int minX = int.MinValue;
            int minY = int.MinValue;
            int maxX = int.MaxValue;
            int maxY = int.MaxValue;

            foreach (var gem in piece.gems)
            {
                minX = math.max((int)gem.lastGridPos.x, minX);
                minY = math.max((int)gem.lastGridPos.y, minY);
                maxX = math.min((int)gem.lastGridPos.x, maxX);
                maxY = math.min((int)gem.lastGridPos.y, maxY);
            }

            return IsCellEmpty(minX, minY) || IsCellEmpty(minX, maxY) || IsCellEmpty(maxX, minY) || IsCellEmpty(maxX, maxY);
        }

        private void RotatePiece(Piece piece, int turns = 1, bool clockWise = true)
        {
            float minX = float.NegativeInfinity;
            float minY = float.NegativeInfinity;
            float maxX = float.PositiveInfinity;
            float maxY = float.PositiveInfinity;

            foreach (var gem in piece.gems)
            {
                minX = math.max(gem.lastGridPos.x, minX);
                minY = math.max(gem.lastGridPos.y, minY);
                maxX = math.min(gem.lastGridPos.x, maxX);
                maxY = math.min(gem.lastGridPos.y, maxY);
            }

            var center = new Vector3((minX + maxX)/2f, (minY + maxY)/2f);
            for (int i = 0; i < turns; i++)
            {
                foreach (var gem in piece.gems)
                {
                    var newPos = RotateAround(gem.lastGridPos, center, clockWise);
                    MoveGem(gem, newPos);
                }
            }
        }
        
        private Vector3 RotateAround(Vector3 pos, Vector3 center, int turns, bool clockWise = true)
        {
            var lastPos = pos;
            for (int i = 0; i < turns; i++)
            {
                lastPos = RotateAround(lastPos, center, clockWise);
            }

            return lastPos;
        }
        
        private Vector3 RotateAround(Vector3 pos, Vector3 center, bool clockWise = true)
        {
            var relativePos = pos - center;
            relativePos = clockWise ? new Vector3(relativePos.y, -relativePos.x) : new Vector3(-relativePos.y, relativePos.x);
            return relativePos + center;
        }
        
        private void NudgeGem(Gem gem, Vector3 offset)
        {
            MoveGem(gem, gem.lastGridPos + offset);
        }

        private void MoveGem(Gem gem, Vector3 gridPos)
        {
            var prevGridPos = gem.lastGridPos;
            gem.lastGridPos = gridPos;
            gem.model.transform.position = grid.LocalToWorld(gridPos);
            
            // May not be us if we moved multiple pieces in a single turn!
            if (logicalGrid[(int)prevGridPos.x, (int)prevGridPos.y].gem == gem) 
                logicalGrid[(int)prevGridPos.x, (int)prevGridPos.y].gem = null;
            
            logicalGrid[(int)gridPos.x, (int)gridPos.y].gem = gem;
            gem.model.name = $"{gem.type.name}: ({gridPos.x}, {gridPos.y})";
        }
        
        private GameObject CreateGemPrefab(GemType gemType, Vector3 worldPos, string name, Transform parentTransform)
        {
            var gemDefinition = GeneralManager.GameConfig.GetGemDefinition(gemType);
            var model = Instantiate(gemDefinition.prefab, worldPos, Quaternion.identity);
            model.transform.SetParent(parentTransform);
            model.name = name;
            return model;
        }
        
        private Gem CreateGem(GemType gemType, Vector3 gridPos, bool inPiece = false)
        {
            var worldPos = grid.LocalToWorld(gridPos);
            var model = CreateGemPrefab(gemType, worldPos, $"{gemType.name}: ({gridPos.x}, {gridPos.y})", transform);
           
            var gem = new Gem()
            {
                type = gemType, 
                model = model, 
                lastGridPos = gridPos,
                inPiece = inPiece
            };
            
            currentGems.Add(gem);
            logicalGrid[(int)gridPos.x, (int)gridPos.y].gem = gem;
            return gem;
        }
        
        private GemType RollNextPiece(List<WeightedGem> gemTypes)
        {
            var totalWeight = 0f;
            foreach (var weightedGem in gemTypes) 
                totalWeight += weightedGem.weight;

            var accumulatedWeight = 0f;
            var rolled = rng.NextFloat(totalWeight);
            for (var index = 0; index < gemTypes.Count; index++)
            {
                var weightedGem = gemTypes[index];
                accumulatedWeight += weightedGem.weight;
                if (rolled <= accumulatedWeight)
                {
                    weightedGem.weight /= 2f;
                    gemTypes[index] = weightedGem;
                    return weightedGem.type;
                }
            }

            throw new Exception("No gem rolled, how?");
        }
    }
}
