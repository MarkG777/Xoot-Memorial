using System;
using UnityEngine;

// Lágrima de La Llorona: primero brilla un aviso en el suelo y después cae. Regresa a su pool.
public class TearDrop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer dropRenderer;
    [SerializeField] private SpriteRenderer warningMark;
    [SerializeField] private Collider2D dropCollider;

    private Action<TearDrop> releaseAction;
    private float floorY;
    private float warningTime;
    private float fallSpeed;
    private float age;
    private bool released;

    public void ConfigureRelease(Action<TearDrop> action)
    {
        releaseAction = action;
    }

    public void Launch(float x, float startY, float floorHeight, float warning, float speed)
    {
        transform.position = new Vector3(x, startY, 0f);
        floorY = floorHeight;
        warningTime = warning;
        fallSpeed = speed;
        age = 0f;
        released = false;
        SetFalling(false);
        PlaceWarning();
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (age < warningTime)
        {
            PlaceWarning();
            warningMark.enabled = Mathf.FloorToInt(age * 12f) % 2 == 0;
            return;
        }

        if (!dropRenderer.enabled) SetFalling(true);

        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        PlaceWarning();
        warningMark.enabled = true;

        if (transform.position.y <= floorY + 0.25f) Release();
    }

    private void SetFalling(bool falling)
    {
        dropRenderer.enabled = falling;
        dropCollider.enabled = falling;
    }

    // El aviso se queda pegado al suelo mientras la lágrima baja
    private void PlaceWarning()
    {
        warningMark.transform.position = new Vector3(transform.position.x, floorY + 0.1f, 0f);
    }

    private void Release()
    {
        if (released) return;

        released = true;
        releaseAction?.Invoke(this);
    }
}
