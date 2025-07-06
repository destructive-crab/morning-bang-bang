using MohDIed.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace MothDIed.Audio
{
    [CreateAssetMenu(fileName = nameof(AudioSystemConfig), menuName = "MothDIed/Audio/Audio System Config")]
    public sealed class AudioSystemConfig : ScriptableObject
    {
        public AudioContainerPreset GlobalPreset;
        public AudioMixer MasterMixer;
    }
}