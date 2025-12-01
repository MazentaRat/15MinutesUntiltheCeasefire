using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class PlayerStatus : MonoBehaviour
{
    [Header("Item & UI References")]
    public ItemPickup2D currentItem;
    public GameObject grabInfo;

    public Slider fullnessUI;
    public Slider temperatureUI;

    public AnimatorOverrideController suit;
    public AnimatorOverrideController graduation;
    public AnimatorOverrideController wedding;
    public Animator playerAnimator;

    [HideInInspector] public int hashHold = Animator.StringToHash("IsHold");
    [HideInInspector] public int hashDead = Animator.StringToHash("IsDead");
    [HideInInspector] public int currentWearID = -1;

    public bool isDead = false;
    public bool killedBySoldier = false;
    public GameObject blood;

    public GameObject deadScreen;
    public GameObject endScreen;
    public Volume deadVolume;
    WaitForSeconds ws02 = new WaitForSeconds(0.3f);
    float td = 0.5f;

    [Header("Color UI")]
    public Image fullnessColorImage;
    public Image temperatureColorImage;

    [Header("Fullness Settings")]
    [HideInInspector] public float fullness = 1f;
    float fullnessDecreaseDuration = 200f;

    [Header("Temperature Settings")]
    [HideInInspector] public float temperature = 1f;
    float temperatureDecreaseDuration = 200f;

    [HideInInspector] public float temperatureIncreaseRate = 0.1f;

    public bool isInBlanket = false;
    [HideInInspector] public int currentBlanketLevel = 0;

    private DayNightManager dayNightManager;

    private WaitForSeconds tick = new WaitForSeconds(0.5f);
    private bool nearFire = false;
    private string fireTag = "Fire";
    private bool routinesStarted = false;
    [HideInInspector] public bool hasEverPickedItem = false;

    private Color fullnessHigh = new Color(1f, 1f, 0f);
    private Color fullnessLow = new Color(200f / 255f, 0f, 0f);

    private Color temperatureHigh = new Color(1f, 0f, 0f);
    private Color temperatureLow = new Color(0f, 53f / 255f, 166f / 255f);

    public SpriteRenderer playerSR;
    public Color originalPlayerColor;

    private Coroutine fullnessWarningRoutine;
    private Coroutine temperatureWarningRoutine;
    private Coroutine fullnessHealRoutine;
    private Coroutine temperatureHealRoutine;

    private Color fullnessOriginalFillColor;
    private Color temperatureOriginalFillColor;

    private bool dangerFul;
    private bool dangerTem;

    public AudioSource background;

    private void Start()
    {
        fullness = 1f;
        temperature = 1f;

        fullnessUI.value = fullness;
        temperatureUI.value = temperature;

        playerAnimator = GetComponent<Animator>();
        dayNightManager = FindAnyObjectByType<DayNightManager>();

        UpdateFullnessColor();
        UpdateTemperatureColor();

        playerSR = GetComponent<SpriteRenderer>();
        if (playerSR != null)
            originalPlayerColor = playerSR.color;

        grabInfo.SetActive(false);
        endScreen.SetActive(false);
        fullnessUI.gameObject.SetActive(false);
        temperatureUI.gameObject.SetActive(false);
        blood.SetActive(false);
        deadScreen.SetActive(false);

        fullnessOriginalFillColor = fullnessUI.fillRect.GetComponent<Image>().color;
        temperatureOriginalFillColor = temperatureUI.fillRect.GetComponent<Image>().color;
    }

    public void StartRoutines()
    {
        if (routinesStarted) return;
        routinesStarted = true;

        if (isDead || killedBySoldier) return;

        fullnessUI.gameObject.SetActive(true);
        temperatureUI.gameObject.SetActive(true);

        StartCoroutine(FullnessRoutine());
        StartCoroutine(TemperatureRoutine());
    }

    private IEnumerator FullnessRoutine()
    {
        while (true)
        {
            yield return tick;

            bool isNight = dayNightManager != null && dayNightManager.IsNight;

            if (isNight && isInBlanket)
            {
                if (currentBlanketLevel == 2)
                {
                    fullnessUI.value = fullness;
                    UpdateFullnessColor();
                    StopFullnessWarning();
                    continue;
                }
                else if (currentBlanketLevel == 3)
                {
                    fullness += 0.05f;
                    fullness = Mathf.Clamp01(fullness);
                    fullnessUI.value = fullness;
                    UpdateFullnessColor();

                    StopFullnessWarning();
                    continue;
                }
            }

            float decreasePerTick = td / fullnessDecreaseDuration;
            float nightMultiplier = (isNight && !isInBlanket) ? 6f : 1f;

            fullness -= decreasePerTick * nightMultiplier;
            fullness = Mathf.Clamp01(fullness);

            fullnessUI.value = fullness;
            UpdateFullnessColor();

            dangerFul = (!isInBlanket && isNight) || fullness < 0.3f;

            if (isInBlanket)
            {
                StopFullnessWarning();
            }
            else
            {
                if (dangerFul)
                    StartFullnessWarning();
                else
                    StopFullnessWarning();
            }

            if (fullness <= 0f && !isDead)
            {
                isDead = true;
                deadScreen.SetActive(true);
                StartCoroutine(FadeInDeadVolume());
                yield break;
            }
        }
    }

    private IEnumerator TemperatureRoutine()
    {
        while (true)
        {
            yield return tick;

            bool isNight = dayNightManager != null && dayNightManager.IsNight;

            if (isNight && isInBlanket)
            {
                if (currentBlanketLevel == 2)
                {
                    temperatureUI.value = temperature;
                    UpdateTemperatureColor();
                    StopTemperatureWarning();
                    continue;
                }
                else if (currentBlanketLevel == 3)
                {
                    temperature += 0.01f;
                    temperature = Mathf.Clamp01(temperature);
                    temperatureUI.value = temperature;
                    UpdateTemperatureColor();

                    StopTemperatureWarning();
                    continue;
                }
            }

            if (nearFire)
            {
                temperature += temperatureIncreaseRate * td;

                if (temperatureHealRoutine == null)
                    TriggerTemperatureHealFlash();
            }
            else
            {
                float baseDecrease = td / temperatureDecreaseDuration;
                float nightMultiplier = (isNight && !isInBlanket) ? 6f : 1f;
                temperature -= baseDecrease * nightMultiplier;
            }

            temperature = Mathf.Clamp01(temperature);
            temperatureUI.value = temperature;
            UpdateTemperatureColor();

            dangerTem = (!isInBlanket && isNight) || temperature < 0.3f;

            if (isInBlanket)
            {
                StopTemperatureWarning();
            }
            else
            {
                if (dangerTem)
                    StartTemperatureWarning();
                else
                    StopTemperatureWarning();
            }

            if (temperature <= 0f && !isDead)
            {
                isDead = true;
                deadScreen.SetActive(true);
                StartCoroutine(FadeInDeadVolume());
                yield break;
            }
        }
    }

    private void StartFullnessWarning()
    {
        if (fullnessWarningRoutine != null) return;

        Image fill = fullnessUI.fillRect.GetComponent<Image>();

        Image[] imgs = { fill, fullnessColorImage };
        Color[] originals = { fullnessOriginalFillColor, fullnessColorImage.color };

        fullnessWarningRoutine = StartCoroutine(FlashWarning(imgs, originals));
    }

    private void StopFullnessWarning()
    {
        if (fullnessWarningRoutine == null) return;

        StopCoroutine(fullnessWarningRoutine);
        fullnessWarningRoutine = null;

        Image fill = fullnessUI.fillRect.GetComponent<Image>();
        fill.color = fullnessOriginalFillColor;

        fullnessColorImage.color = fullnessOriginalFillColor;
    }

    private void StartTemperatureWarning()
    {
        if (temperatureWarningRoutine != null) return;

        Image fill = temperatureUI.fillRect.GetComponent<Image>();

        Image[] imgs = { fill, temperatureColorImage };
        Color[] originals = { temperatureOriginalFillColor, temperatureColorImage.color };

        temperatureWarningRoutine = StartCoroutine(FlashWarning(imgs, originals));
    }

    private void StopTemperatureWarning()
    {
        if (temperatureWarningRoutine == null) return;

        StopCoroutine(temperatureWarningRoutine);
        temperatureWarningRoutine = null;

        Image fill = temperatureUI.fillRect.GetComponent<Image>();
        fill.color = temperatureOriginalFillColor;

        temperatureColorImage.color = temperatureOriginalFillColor;
    }

    IEnumerator FlashWarning(Image[] imgs, Color[] originalColors)
    {
        Color warnColor = Color.black;

        while (true)
        {
            for (int i = 0; i < imgs.Length; i++)
                imgs[i].color = warnColor;

            yield return ws02;

            for (int i = 0; i < imgs.Length; i++)
                imgs[i].color = originalColors[i];

            yield return ws02;
        }
    }

    IEnumerator FlashHeal(Image[] imgs, Color[] originalColors)
    {
        Color healColor = Color.white;
        float t = 0f;
        float duration = 0.3f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            for (int i = 0; i < imgs.Length; i++)
                imgs[i].color = Color.Lerp(healColor, originalColors[i], t);

            yield return null;
        }

        for (int i = 0; i < imgs.Length; i++)
            imgs[i].color = originalColors[i];
    }

    public void TriggerFullnessHealFlash()
    {
        if (fullnessHealRoutine != null)
            StopCoroutine(fullnessHealRoutine);

        Image fill = fullnessUI.fillRect.GetComponent<Image>();

        Image[] imgs = { fill, fullnessColorImage };
        Color[] originals = { fullnessOriginalFillColor, fullnessColorImage.color };

        fullnessHealRoutine = StartCoroutine(FlashHeal(imgs, originals));
    }

    public void TriggerTemperatureHealFlash()
    {
        if (temperatureHealRoutine != null)
            StopCoroutine(temperatureHealRoutine);

        Image fill = temperatureUI.fillRect.GetComponent<Image>();

        Image[] imgs = { fill, temperatureColorImage };
        Color[] originals = { temperatureOriginalFillColor, temperatureColorImage.color };

        temperatureHealRoutine = StartCoroutine(FlashHeal(imgs, originals));
    }

    private IEnumerator FadeInDeadVolume()
    {
        if (deadVolume == null)
            yield break;

        deadVolume.weight = 0f;
        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            deadVolume.weight = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        deadVolume.weight = 1f;
        Time.timeScale = 0f;
    }

    private void UpdateFullnessColor()
    {
        Color c = Color.Lerp(fullnessLow, fullnessHigh, fullness);

        fullnessUI.fillRect.GetComponent<Image>().color = c;
        if (fullnessColorImage != null)
            fullnessColorImage.color = c;
    }

    private void UpdateTemperatureColor()
    {
        Color c = Color.Lerp(temperatureLow, temperatureHigh, temperature);

        temperatureUI.fillRect.GetComponent<Image>().color = c;
        if (temperatureColorImage != null)
            temperatureColorImage.color = c;
    }
    public void Killed()
    {
        endScreen.SetActive(true);

        // --- 배경 오디오 모두 정지 ---
        if (background != null)
        {
            background.Stop();
        }

        Time.timeScale = 0f;   // 게임 시간 정지
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(fireTag))
        {
            nearFire = true;

            if (playerSR != null)
                playerSR.color = new Color(1f, 85f / 255f, 85f / 255f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(fireTag))
        {
            nearFire = false;

            if (playerSR != null)
                playerSR.color = originalPlayerColor;
        }
    }
}
