using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;

public class DayNightManager : MonoBehaviour
{
    [Header("Total Duration Settings")]
    float dayDuration = 900f;
    int totalDays = 2;

    [Header("2D Global Light (URP)")]
    public Light2D globalLight;

    [Header("Light Colors")]
    public Color dayColor = new Color(1, 0.95f, 0.8f);
    public Color eveningColor = new Color(1, 0.6f, 0.4f);
    public Color nightColor = new Color(0.1f, 0.2f, 0.4f);
    public Color dawnColor = new Color(0.25f, 0.3f, 0.5f);
    public Color morningColor = new Color(1f, 0.8f, 0.6f);

    [Header("Intensity")]
    public float dayIntensity = 1f;
    public float eveningIntensity = 0.6f;
    public float nightIntensity = 0.2f;
    public float dawnIntensity = 0.3f;
    public float morningIntensity = 0.8f;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI noticeText;

    [Header("Localization Keys")]
    public LocalizedString sleepNoticeKey;
    public LocalizedString wakeupNoticeKey;
    public LocalizedString BlanketMessageKey;

    [Header("Events")]
    public UnityEvent OnCycleCompleted;

    public bool IsNight { get; private set; }
    private bool isStarted = false;

    private float timer = 0f;
    private float singleDayDuration;
    private WaitForSeconds delay = new WaitForSeconds(0.1f);
    WaitForSeconds ws10 = new WaitForSeconds(3f);
    WaitForSeconds ws04 = new WaitForSeconds(0.04f);

    private bool nightMessageShown = false;
    private bool morningMessageShown = false;

    private Coroutine messageRoutine = null;

    private DayEventManager eventManager;

    private bool lightFrozen = false;
    private float frozenIntensity = 0.8f;
    PlayerStatus ps;

    private void Start()
    {
        eventManager = FindAnyObjectByType<DayEventManager>();
        ps = FindAnyObjectByType<PlayerStatus>();
        timeText.gameObject.SetActive(false);
    }

    public void StartCycle()
    {
        if (isStarted) return;   // 첫 시작만 허용
        isStarted = true;

        ResetCycle();
    }

    public void ShowBlanketMessage()
    {
        ShowLocalizedMessage(BlanketMessageKey);
    }
    public void ResetCycle()
    {
        StopAllCoroutines();

        timer = 0f;
        singleDayDuration = dayDuration / totalDays;

        if (globalLight != null)
        {
            globalLight.color = dayColor;
            globalLight.intensity = dayIntensity;
        }

        timeText.gameObject.SetActive(true);
        nightMessageShown = false;
        morningMessageShown = false;

        noticeText.text = "";
        UpdateTimeText(dayDuration);

        eventManager?.ResetEvents();

        StartCoroutine(DayNightCycle());
    }

    // 외부에서 호출: 조명 고정
    public void FreezeLight()
    {
        lightFrozen = true;

        if (globalLight != null)
            globalLight.intensity = frozenIntensity;
    }

    // 외부에서 호출: 조명 해제
    public void UnfreezeLight()
    {
        lightFrozen = false;
    }

    private IEnumerator DayNightCycle()
    {
        while (timer < dayDuration)
        {
            timer += 0.1f;

            float remaining = Mathf.Max(0, dayDuration - timer);
            UpdateTimeText(remaining);

            float dayProgress = (timer % singleDayDuration) / singleDayDuration;

            // === 밤 판정: 전체의 3% ===
            bool isNowNight = dayProgress >= 0.48f && dayProgress < 0.51f;
            bool isNowMorning = dayProgress >= 0.75f && dayProgress < 1f;

            // === 밤 메시지 ===
            if (isNowNight && !nightMessageShown)
            {
                IsNight = true;
                nightMessageShown = true;
                morningMessageShown = false;

                ShowLocalizedMessage(sleepNoticeKey);
            }

            // === 아침 메시지 ===
            if (isNowMorning && !morningMessageShown)
            {
                IsNight = false;
                morningMessageShown = true;
                nightMessageShown = false;

                ShowLocalizedMessage(wakeupNoticeKey);
            }

            // === 빛 전환도 기존 퍼센트가 아니라 밤 구간을 강제로 적용 ===
            if (isNowNight)
            {
                ApplyLightTransition(0.50f, true);
            }
            else
            {
                ApplyLightTransition(dayProgress, false);
            }

            // ----------------------------------------------
            // 여기서 DayEventManager로 진행률 넘겨줌
            // 0~1 로 환산한 전체 진행률
            // ----------------------------------------------
            float totalProgress = timer / dayDuration;
            eventManager?.CheckProgress(totalProgress);

            yield return delay;
        }

        noticeText.text = "";

        // 하루가 끝났으므로 타임 UI 숨기기
        if (timeText != null)
            timeText.gameObject.SetActive(false);
        ps.fullnessUI.gameObject.SetActive(false);
        ps.temperatureUI.gameObject.SetActive(false);
        ps.enabled = false;

        OnCycleCompleted?.Invoke();
    }

    private void ApplyLightTransition(float p, bool isNowNight = false)
    {
        if (lightFrozen)
        {
            globalLight.intensity = frozenIntensity;
            return;
        }

        // =============== 밤(3%) 강제 적용 ===============
        if (isNowNight)
        {
            globalLight.color = nightColor;
            globalLight.intensity = nightIntensity;
            return;
        }

        // 낮 → 저녁 (0.00 ~ 0.35)
        if (p < 0.35f)
            BlendLight(dayColor, eveningColor, dayIntensity, eveningIntensity, p / 0.35f);

        // 저녁 → 밤 전환 (0.35 ~ 0.48)
        else if (p < 0.48f)
            BlendLight(eveningColor, nightColor, eveningIntensity, nightIntensity,
                (p - 0.35f) / (0.48f - 0.35f));

        // 밤 → 새벽 (0.51 ~ 0.65)
        else if (p < 0.65f)
            BlendLight(nightColor, dawnColor, nightIntensity, dawnIntensity,
                (p - 0.51f) / (0.65f - 0.51f));

        // 새벽 → 낮 (0.65 ~ 1.00)
        else
            BlendLight(dawnColor, dayColor, dawnIntensity, dayIntensity,
                        (p - 0.65f) / (1f - 0.65f));
    }

    private void BlendLight(Color c1, Color c2, float i1, float i2, float t)
    {
        globalLight.color = Color.Lerp(c1, c2, t);
        globalLight.intensity = Mathf.Lerp(i1, i2, t);
    }

    private void UpdateTimeText(float remainingTime)
    {
        if (timeText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void ShowLocalizedMessage(LocalizedString localizedString)
    {
        StopCoroutine("ShowMessageRoutine");
        StartCoroutine("ShowMessageRoutine", localizedString);
    }

    private IEnumerator ShowMessageRoutine(LocalizedString localizedString)
    {
        noticeText.text = "";

        string fullMsg = localizedString.GetLocalizedString();

        for (int i = 0; i < fullMsg.Length; i++)
        {
            noticeText.text = fullMsg.Substring(0, i + 1);
            yield return ws04;
        }

        yield return ws10;
        noticeText.text = " ";
        yield return null;
        noticeText.text = "";

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(noticeText.rectTransform);
    }
}
