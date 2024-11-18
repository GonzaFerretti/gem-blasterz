using Puzzler;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Cosmetics.Audio
{
    public class ComboSounds : MonoBehaviour
    {
        public static void PlayComboSound(OnPuzzlerMatch.CurrentCombo currentCombo)
        {
            var clip = GeneralManager.Sound.ClipRepository.GetClipConfig("comboSound1").Value;
            //clip.pitch = GeneralManager.GameConfig.comboPitchCurve.Evaluate(currentCombo.comboCount);
            clip.pitch = GetPitch(currentCombo.comboCount);
            GeneralManager.Sound.Play(clip);
        }

        public static float GetPitch(int noteCount)
        {
            float note = 0f;
            switch (noteCount)
            {
                case 1:
                    note = 0;
                    break;
                case 2:
                    note = 4;
                    break;
                case 3:
                    note = 7;
                    break;
                default:
                    break;
            }
            return Mathf.Pow(2, (note) / 12.0f);
        }

    }

    
}