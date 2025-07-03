using System;
using AutumnForest;
using UnityEngine;

public class PitchedAudioMonoBehaviour : MonoBehaviour
{
    [SerializeField] private PitchedAudio pitchedAudio;

    private void Reset()
    {
        if (TryGetComponent(out AudioSource source))
        {
            pitchedAudio = new PitchedAudio(source);
            
            pitchedAudio.minPitch = 0.8f;
            pitchedAudio.maxPitch = 1.2f;
        }
    }

    public void Play()
    {
        pitchedAudio.Play();
    }

    public void Stop()
    {
        pitchedAudio.Stop();
    }

    public void Pause()
    {
        pitchedAudio.Pause();
    }
}
