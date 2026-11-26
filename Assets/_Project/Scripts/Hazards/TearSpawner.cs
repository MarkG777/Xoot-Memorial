using UnityEngine;
using UnityEngine.Pool;

// Hace llover lágrimas en un tramo, reutilizándolas con un pool (P4). A veces apunta al jugador.
public class TearSpawner : MonoBehaviour
{
    [SerializeField] private TearDrop tearPrefab;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Transform player;

    [Header("Zona (posiciones en el mundo)")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float startY = 8f;
    [SerializeField] private float floorY;

    [Header("Ritmo")]
    [SerializeField] private float interval = 1f;
    [SerializeField] private float warningTime = 0.7f;
    [SerializeField] private float fallSpeed = 7f;
    [SerializeField, Range(0f, 1f)] private float aimAtPlayerChance = 0.4f;

    [Header("Aviso sonoro: suena del lado de donde viene")]
    [SerializeField] private AudioSource cueSource;
    [SerializeField] private AudioClip cueClip;

    private ObjectPool<TearDrop> pool;
    private float timer;

    public bool Active { get; set; }

    private void Awake()
    {
        pool = new ObjectPool<TearDrop>(
            CreateTear, OnGetTear,
            OnReleaseTear, OnDestroyTear,
            true, 6, 20);
    }

    private void Update()
    {
        if (!Active || tearPrefab == null) return;
        if (levelManager != null && !levelManager.IsPlaying) return;

        timer += Time.deltaTime;
        if (timer < interval) return;

        timer = 0f;
        float x = Random.Range(minX, maxX);
        if (player != null && Random.value < aimAtPlayerChance)
            x = Mathf.Clamp(player.position.x + Random.Range(-0.6f, 0.6f), minX, maxX);

        pool.Get().Launch(x, startY, floorY, warningTime, fallSpeed);
        PlayCue(x);
    }

    public void Prime()
    {
        timer = interval * 0.5f;
    }

    private void PlayCue(float x)
    {
        if (cueSource == null || cueClip == null || player == null) return;

        cueSource.panStereo = Mathf.Clamp((x - player.position.x) / 4f, -1f, 1f);
        cueSource.PlayOneShot(cueClip, 0.28f);
    }

    private TearDrop CreateTear()
    {
        TearDrop tear = Instantiate(tearPrefab, transform);
        tear.ConfigureRelease(pool.Release);
        return tear;
    }

    private void OnGetTear(TearDrop tear) => tear.gameObject.SetActive(true);

    private void OnReleaseTear(TearDrop tear) => tear.gameObject.SetActive(false);

    private void OnDestroyTear(TearDrop tear) => Destroy(tear.gameObject);
}
