using System.Collections.Generic;
using UnityEngine;

namespace Puzzler
{
    public class PuzzlePreviewWidgetManager : MonoBehaviour
    {
        [SerializeField]
        private List<PuzzlePreviewWidget> previewWidgets;

        public void UpdatePreviews(List<PuzzlerBoard.PieceConfiguration> nextPieces)
        {
            for (int i = 1; i < nextPieces.Count; i++)
            {
                previewWidgets[i-1].UpdatePreview(nextPieces[i]);
            }
        }
    }
}