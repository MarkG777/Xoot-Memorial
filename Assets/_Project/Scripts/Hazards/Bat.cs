using System;
using UnityEngine;

// Murciélago que manda el jefe: vuela en línea con un vaivén y regresa al pool (como en la P4).
public class Bat : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float waveAmplitude = 0.6f;
    [SerializeField] private float waveFrequency = 4f;

    private Action<Bat> releaseAction;
    private Vector3 basePosition;
    private float direction = -1f;
    private float speed = 4f;
    private float lifeTime = 6f;
    private float age;
    private bool released;

    public void ConfigureRelease(Action<Bat> action)
    {
        releaseAction = action;
    }

    public void Launch(Vector3 position, float flightDirection, float flightSpeed, float duration)
    {
        basePosition = position;
        transform.position = position;
        direction = Mathf.Sign(flightDirection);
        speed = flightSpeed;
        lifeTime = duration;
        age = 0f;
        released = false;
        spriteRenderer.flipX = direction > 0f;
    }

    private void Update()
    {
        age += Time.deltaTime;
        basePosition += Vector3.right * (direction * speed * Time.deltaTime);
        transform.position = basePosition + Vector3.up * (Mathf.Sin(age * waveFrequency) * waveAmplitude);

        if (age >= lifeTime) Release();
    }

    public void Release()
    {
        if (released) return;

        released = true;
        releaseAction?.Invoke(this);
    }
}
