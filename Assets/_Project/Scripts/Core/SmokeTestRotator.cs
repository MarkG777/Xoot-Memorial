using UnityEngine;

// Script trivial para la prueba de humo del MCP: gira el objeto en Z.
public class SmokeTestRotator : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 45f;

    private void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
