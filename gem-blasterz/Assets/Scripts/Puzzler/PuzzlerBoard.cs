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
        private PlayerInput playerInput;
        
        [SerializeField]
        private Grid grid;

        private List<Piece> pieces = new();
        private GridSlot[,] logicalGrid;

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
            public GameObject inPieceBG;
            public Vector3 lastGridPos;
            public bool inPiece;
            public bool blocked;
        }
        
        [Serializable]
        public class Piece
        {
            public List<Gem> gems;
        }

        private struct Match
        {
            public List<Gem> matchedGems;
        }

        public void Initialize(uint seed)
        {
            logicalGrid = new GridSlot[GeneralManager.GameConfig.gridSize.x, GeneralManager.GameConfig.gridSize.y];
            rng = new Random(seed);

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

        public void Test_GeneratePieceWave()
        {
            var initialPos = new int2(0, GeneralManager.GameConfig.gridSize.y - 2);
            for (int i = 0; i < GeneralManager.GameConfig.gridSize.x / 2; i++)
            {
                if (CanSpawnPiece(initialPos))
                    GeneratePieceAtPos(initialPos);
                initialPos.x += 2;
            }
            LogWrongPos();
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

        public Piece GeneratePieceAtPos(int2 pos)
        {
            var newPiece = GeneratePiece(new Vector3(pos.x, pos.y));
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
            if (activePiece == null)
            {
                var couldSpawn = false;
                var initialPos = new int2(0, GeneralManager.GameConfig.gridSize.y - 2);
                for (int i = 0; i < GeneralManager.GameConfig.gridSize.x / 2; i++)
                {
                    if (CanSpawnPiece(initialPos))
                    {
                        activePiece = GeneratePieceAtPos(initialPos);
                        couldSpawn = true;
                        break;
                    }
                    initialPos.x += 2;
                }

                if (!couldSpawn) 
                    Debug.Log($"Player from {gameObject.name} lost.");
            }
            
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
                    Destroy(gem.inPieceBG);
                }
            }
            
            foreach (var gem in currentGems)
            {
                if (!gem.inPiece)
                {
                    if (!AnyCollision(gem, Vector3.down))
                    {
                        gem.blocked = false;
                        NudgeGem(gem, Vector3.down);
                    }
                    else
                        gem.blocked = true;
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
            if (!anyMoving)
                ClearMatchingGems();
            LogWrongPos();
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
                    if (!visited[i, j] && logicalGrid[i, j].gem != null)
                    {
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

                            if (logicalGrid[x, y].gem.inPiece)
                                continue; // Gem is part of a piece, skip it

                            if (!logicalGrid[x, y].gem.blocked)
                                continue; // Gem is not blocked, skip it

                            if (!logicalGrid[x, y].gem.type.Equals(gemType))
                                continue; // Gem type does not match

                            visited[x, y] = true;
                            matches.matchedGems.Add(logicalGrid[x, y].gem);

                            // Push neighboring cells to the stack
                            foreach (var dir in directions)
                            {
                                stack.Push((x + dir.x, y + dir.y));
                            }
                        }

                        // Only add the match if it has the required number of gems
                        if (matches.matchedGems.Count >= GeneralManager.GameConfig.matchNumber)
                        {
                            validMatches.Add(matches);
                        }
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
            var targetPos = gem.lastGridPos + offset;
            
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
        
        private Piece GeneratePiece(Vector3 initialPosition)
        {
            var newPiece = new Piece(){ gems = new()};
            var weightedGemTypes = new List<WeightedGem>();
            
            // Populate with definition with equal weights initially
            foreach (var definition in GeneralManager.GameConfig.gemDefinitions) 
                weightedGemTypes.Add(new WeightedGem(){ type = definition.gemType, weight = 1f});

            // Roll three times to get L shape
            newPiece.gems.Add(CreateGem(RollNextPiece(weightedGemTypes), initialPosition + new Vector3(1f,0f, 0f), inPiece: true));
            newPiece.gems.Add(CreateGem(RollNextPiece(weightedGemTypes), initialPosition + new Vector3(0f,1f, 0f), inPiece: true));
            newPiece.gems.Add(CreateGem(RollNextPiece(weightedGemTypes), initialPosition + new Vector3(1f,1f, 0f), inPiece: true));

            // Assign random rotation
            var turns = rng.NextInt(4);
            RotatePiece(newPiece, turns);
            return newPiece;
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
                    var relativePos = gem.lastGridPos - center;
                    relativePos = clockWise ? new Vector3(relativePos.y, -relativePos.x) : new Vector3(-relativePos.y, relativePos.x);
                    var newPos = relativePos + center;
                    MoveGem(gem, newPos);
                }
            }
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

        private Gem CreateGem(GemType gemType, Vector3 gridPos, bool inPiece = false)
        {
            var gemDefinition = GeneralManager.GameConfig.GetGemDefinition(gemType);
            var model = Instantiate(gemDefinition.prefab, grid.LocalToWorld(gridPos), Quaternion.identity);
            model.transform.SetParent(transform);
            model.name = $"{gemType.name}: ({gridPos.x}, {gridPos.y})";
            var gem = new Gem() { type = gemType, model = model, lastGridPos = gridPos };
            gem.inPiece = inPiece;
            var bg = Instantiate(GeneralManager.GameConfig.testBackground, gem.model.transform.position, Quaternion.identity);
            bg.transform.SetParent(gem.model.transform);
            gem.inPieceBG = bg;
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

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
