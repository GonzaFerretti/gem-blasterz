using UnityEngine;
using UnityEngine.UI;

namespace Puzzler
{
    public class PuzzlePreviewWidget : MonoBehaviour
    {
        public Image topLeftGem;
        public Image topRightGem;
        public Image bottomLeftGem;
        public Image bottomRightGem;

        public void UpdatePreview(PuzzlerBoard.PieceConfiguration pieceConfiguration)
        {
            topLeftGem.gameObject.SetActive(false);
            topRightGem.gameObject.SetActive(false);
            bottomLeftGem.gameObject.SetActive(false);
            bottomRightGem.gameObject.SetActive(false);

            for (var index = 0; index < pieceConfiguration.gemTypes.Count; index++)
            {
                var gemType = pieceConfiguration.gemTypes[index];
                var pos = new Vector3(1f, 0f, 0f);
                if (index == 1) pos = new Vector3(0f, 1f, 0f);
                else if (index == 2) pos = new Vector3(1f, 1f, 0f);
                pos = PuzzlerBoard.RotateAround(pos, new Vector3(0.5f, 0.5f, 0), pieceConfiguration.turns);
                Image imageToSet = null;
                if (pos == Vector3.zero)
                {
                    imageToSet = bottomLeftGem;
                }
                else if (pos == new Vector3(1f, 0f, 0f))
                {
                    imageToSet = bottomRightGem;
                }
                else if (pos == new Vector3(0f, 1f, 0f))
                {
                    imageToSet = topLeftGem;
                }
                else if (pos == new Vector3(1f, 1f, 0f))
                {
                    imageToSet = topRightGem;
                }

                imageToSet.gameObject.SetActive(true);
                imageToSet.color = GeneralManager.GameConfig.GetGemDefinition(gemType).previewColor;
            }
        }
    }
}