using Puzzler;
using Unity.Mathematics;
using UnityEngine;

namespace Cosmetics.Audio
{
    public class ComboSounds : MonoBehaviour
    {
        public static void PlayComboSound(OnPuzzlerMatch.CurrentCombo currentCombo)
        {
            var clip = GeneralManager.Sound.ClipRepository.GetClipConfig("comboSound1").Value;
            clip.pitch = GeneralManager.GameConfig.comboPitchCurve.Evaluate(currentCombo.comboCount);
            GeneralManager.Sound.Play(clip);
        }
    }
}