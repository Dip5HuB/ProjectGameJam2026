using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script ini berfungsi untuk memberikan efek visual interaktif pada tombol UI (seperti hover, scale, dan fade in/out).
/// Menggunakan event dari Unity EventSystems (IPointerEnter, IPointerExit, IPointerClick).
/// </summary>
public class MenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual Settings")]
    [Tooltip("Objek visual tambahan yang muncul saat di-hover (misalnya: BG_play)")]
    [SerializeField] private GameObject highlightGraphic;

    [Tooltip("Apakah ingin menggunakan animasi transisi fade (memudar)? Jika ya, disarankan menggunakan CanvasGroup.")]
    [SerializeField] private bool useFade = true;

    [Tooltip("Durasi transisi memudar (fade in/out) dalam detik")]
    [SerializeField] private float fadeDuration = 0.2f;

    [Tooltip("Apakah tombol akan membesar sedikit ketika disorot mouse?")]
    [SerializeField] private bool useScale = true;

    [Tooltip("Durasi transisi perubahan ukuran tombol")]
    [SerializeField] private float scaleDuration = 0.15f;

    [Tooltip("Ukuran target tombol saat mouse menyorot (hover)")]
    [SerializeField] private Vector3 hoveredScale = new Vector3(1.08f, 1.08f, 1.08f);

    [Header("Audio Settings")]
    [Tooltip("Komponen AudioSource untuk memutar efek suara tombol")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Suara saat mouse menyorot tombol")]
    [SerializeField] private AudioClip hoverSound;

    [Tooltip("Suara saat tombol diklik")]
    [SerializeField] private AudioClip clickSound;

    // Status apakah tombol ini sedang terpilih (khusus untuk sistem tab menu)
    [HideInInspector] public bool isSelected = false;

    // Menyimpan ukuran asli tombol agar bisa dikembalikan saat mouse keluar
    private Vector3 originalScale;

    // Komponen CanvasGroup yang digunakan untuk memudarkan objek highlight secara halus
    private CanvasGroup highlightCanvasGroup;

    // Menyimpan referensi coroutine yang sedang berjalan agar tidak saling bertabrakan
    private Coroutine activeFadeRoutine;
    private Coroutine activeScaleRoutine;

    private void Start()
    {
        // Menyimpan ukuran default tombol saat game dimulai
        originalScale = transform.localScale;

        // Setup awal untuk objek highlight (misal: BG_play)
        if (highlightGraphic != null)
        {
            if (useFade)
            {
                // Dapatkan CanvasGroup, jika belum ada tambahkan secara dinamis
                highlightCanvasGroup = highlightGraphic.GetComponent<CanvasGroup>();
                if (highlightCanvasGroup == null)
                {
                    highlightCanvasGroup = highlightGraphic.AddComponent<CanvasGroup>();
                }

                // Set alpha awal ke 0 (tidak terlihat) dan pastikan objeknya aktif
                highlightCanvasGroup.alpha = 0f;
                highlightGraphic.SetActive(true);
            }
            else
            {
                // Jika tidak menggunakan efek memudar, matikan objek di awal
                highlightGraphic.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Dipanggil secara otomatis oleh EventSystem ketika mouse menyorot tombol (Hover Enter).
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Jika sudah terpilih, abaikan hover visual tambahan
        if (isSelected) return;

        // 1. Jalankan animasi memudar untuk highlight jika digunakan
        if (highlightGraphic != null)
        {
            if (useFade && highlightCanvasGroup != null)
            {
                StartFade(1f); // Memudarkan ke terlihat penuh (alpha 1)
            }
            else
            {
                highlightGraphic.SetActive(true); // Langsung aktifkan objek jika tidak menggunakan fade
            }
        }

        // 2. Jalankan animasi perubahan ukuran tombol (scaling up)
        if (useScale)
        {
            StartScale(hoveredScale);
        }

        // 3. Putar suara hover jika komponen dan audio sudah di-assign
        PlaySound(hoverSound);
    }

    /// <summary>
    /// Dipanggil secara otomatis oleh EventSystem ketika mouse keluar dari tombol (Hover Exit).
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Jika tombol dalam status terpilih (misal tab aktif), jangan hilangkan sorotan visualnya
        if (isSelected) return;

        // 1. Jalankan animasi memudar untuk highlight jika digunakan
        if (highlightGraphic != null)
        {
            if (useFade && highlightCanvasGroup != null)
            {
                StartFade(0f); // Memudarkan ke transparan (alpha 0)
            }
            else
            {
                highlightGraphic.SetActive(false); // Langsung nonaktifkan objek jika tidak menggunakan fade
            }
        }

        // 2. Kembalikan ukuran tombol ke ukuran asli (scaling down)
        if (useScale)
        {
            StartScale(originalScale);
        }
    }

    /// <summary>
    /// Menyetel status terpilih (Selected) pada tombol secara manual (biasanya digunakan untuk Tab Menu).
    /// </summary>
    /// <param name="state">True jika terpilih, False jika tidak terpilih</param>
    public void SetSelectedState(bool state)
    {
        isSelected = state;

        if (isSelected)
        {
            // Tampilkan highlight secara halus/instan
            if (highlightGraphic != null)
            {
                if (useFade && highlightCanvasGroup != null)
                {
                    StartFade(1f);
                }
                else
                {
                    highlightGraphic.SetActive(true);
                }
            }

            // Perbesar ukuran tombol ke target
            if (useScale)
            {
                StartScale(hoveredScale);
            }
        }
        else
        {
            // Matikan highlight
            if (highlightGraphic != null)
            {
                if (useFade && highlightCanvasGroup != null)
                {
                    StartFade(0f);
                }
                else
                {
                    highlightGraphic.SetActive(false);
                }
            }

            // Kembalikan ukuran tombol ke normal
            if (useScale)
            {
                // Jika game belum dimulai, set localScale manual (menghindari error coroutine di Start)
                if (originalScale == Vector3.zero)
                {
                    originalScale = transform.localScale;
                }
                StartScale(originalScale);
            }
        }
    }

    /// <summary>
    /// Dipanggil secara otomatis oleh EventSystem ketika tombol diklik.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Putar suara klik jika komponen dan audio sudah di-assign
        PlaySound(clickSound);

        // Jika ingin mematikan efek hover saat diklik, kita bisa menyetel ulang ke keadaan normal
        // Namun biasanya saat diklik, kita biarkan transisi ke panel berikutnya terjadi secara alami.
    }

    /// <summary>
    /// Fungsi pembantu untuk memulai Coroutine transisi fade.
    /// </summary>
    private void StartFade(float targetAlpha)
    {
        if (activeFadeRoutine != null)
        {
            StopCoroutine(activeFadeRoutine);
        }
        activeFadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    /// <summary>
    /// Fungsi pembantu untuk memulai Coroutine transisi ukuran.
    /// </summary>
    private void StartScale(Vector3 targetScale)
    {
        if (activeScaleRoutine != null)
        {
            StopCoroutine(activeScaleRoutine);
        }
        activeScaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    /// <summary>
    /// Coroutine untuk mengubah alpha CanvasGroup secara mulus (Linear Interpolation).
    /// </summary>
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = highlightCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Gunakan Lerp untuk menghitung nilai alpha di antara startAlpha dan targetAlpha berdasarkan waktu
            highlightCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        highlightCanvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Coroutine untuk mengubah ukuran (localScale) objek secara mulus (Linear Interpolation).
    /// </summary>
    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            // Gunakan Lerp untuk menghitung ukuran di antara startScale dan targetScale berdasarkan waktu
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleDuration);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    /// <summary>
    /// Fungsi pembantu untuk memutar efek suara tombol.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // mainkan sekali (tidak memotong suara yang sedang dimainkan jika suara lain dipicu)
            audioSource.PlayOneShot(clip);
        }
    }
}
