using System;
using UnityEngine;

// Lleva la cuenta de las llaves (pistas) del nivel.
public class LevelProgress : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameEvent keyCollectedEvent;

    public int Keys { get; private set; }
    public int RequiredKeys => levelData != null ? levelData.RequiredKeys : 3;
    public bool HasAllKeys => Keys >= RequiredKeys;
    public LevelData Data => levelData;

    public event Action<int, int> KeysChanged;

    public void AddKey()
    {
        if (HasAllKeys) return;

        Keys++;
        KeysChanged?.Invoke(Keys, RequiredKeys);

        if (keyCollectedEvent != null) keyCollectedEvent.Raise();
    }
}
