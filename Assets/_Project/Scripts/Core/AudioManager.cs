using System;
using System.Collections.Generic;
using UnityEngine;

// RQF-10: reproduce música y efectos. Cada efecto está ligado a un evento (ScriptableObject),
// así que ningún otro script necesita llamar al audio directamente.
public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class EventSound
    {
        public GameEvent gameEvent;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.8f;
    }

    [SerializeField] private AudioSource effectsSource;
    [SerializeField] private EventSound[] sounds;

    [Header("Salto")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.35f;

    private readonly List<Action> handlers = new List<Action>();

    private void OnEnable()
    {
        handlers.Clear();
        foreach (EventSound sound in sounds)
        {
            EventSound current = sound;
            Action handler = () => Play(current.clip, current.volume);
            handlers.Add(handler);
            if (current.gameEvent != null) current.gameEvent.Raised += handler;
        }

        if (player != null) player.Jumped += PlayJump;
    }

    private void OnDisable()
    {
        for (int i = 0; i < handlers.Count && i < sounds.Length; i++)
        {
            if (sounds[i].gameEvent != null) sounds[i].gameEvent.Raised -= handlers[i];
        }

        handlers.Clear();
        if (player != null) player.Jumped -= PlayJump;
    }

    private void PlayJump() => Play(jumpClip, jumpVolume);

    private void Play(AudioClip clip, float volume)
    {
        if (effectsSource != null && clip != null) effectsSource.PlayOneShot(clip, volume);
    }
}
