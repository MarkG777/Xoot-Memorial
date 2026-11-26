using UnityEngine;
using UnityEngine.Pool;

// Lanza murciélagos reutilizándolos con un pool (P4). Solo actúa mientras el jugador está en su zona.
public class BatSpawner : MonoBehaviour
{
    [SerializeField] private Bat batPrefab;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private float interval = 2.2f;
    [SerializeField] private float batSpeed = 4.5f;
    [SerializeField] private float batLifeTime = 5f;
    [SerializeField] private float direction = -1f;
    [SerializeField] private Vector2 heightRange = new Vector2(-1f, 1f);

    private ObjectPool<Bat> pool;
    private float timer;

    public bool Active { get; set; }

    private void Awake()
    {
        pool = new ObjectPool<Bat>(
            CreateBat, OnGetBat,
            OnReleaseBat, OnDestroyBat,
            true, 5, 15);
    }

    private void Update()
    {
        if (!Active || batPrefab == null) return;
        if (levelManager != null && !levelManager.IsPlaying) return;

        timer += Time.deltaTime;
        if (timer < interval) return;

        timer = 0f;
        Bat bat = pool.Get();
        Vector3 position = transform.position + Vector3.up * Random.Range(heightRange.x, heightRange.y);
        bat.Launch(position, direction, batSpeed, batLifeTime);
    }

    // Se llama al entrar a la zona para que el primer murciélago salga pronto
    public void Prime()
    {
        timer = interval * 0.6f;
    }

    private Bat CreateBat()
    {
        Bat bat = Instantiate(batPrefab, transform);
        bat.ConfigureRelease(pool.Release);
        return bat;
    }

    private void OnGetBat(Bat bat) => bat.gameObject.SetActive(true);

    private void OnReleaseBat(Bat bat) => bat.gameObject.SetActive(false);

    private void OnDestroyBat(Bat bat) => Destroy(bat.gameObject);
}
