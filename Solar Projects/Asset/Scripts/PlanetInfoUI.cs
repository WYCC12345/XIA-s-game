using UnityEngine;
using UnityEngine.UI;

public class PlanetInfoUI : MonoBehaviour
{
    public static PlanetInfoUI Instance;

    public Text infoText;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    public Color textColor = Color.white;
    public string defaultText = "点一个星球，探索它的故事！按空格或点按钮回到太空视角。";

    private AudioSource audioSource;
    private Button backButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (infoText == null)
        {
            CreateInfoUI();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        ResetInfo();
    }

    public void ShowInfo(string title, string fact)
    {
        if (infoText == null)
        {
            CreateInfoUI();
        }

        infoText.text = $"<b>{title}</b>\n{fact}\n\n按空格或点按钮返回主视角";
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }
    }

    public void ResetInfo()
    {
        if (infoText == null)
        {
            CreateInfoUI();
        }

        infoText.text = defaultText;
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        audioSource.PlayOneShot(clip);
    }

    private void CreateInfoUI()
    {
        GameObject canvasGO = new GameObject("PlanetInfoCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("InfoPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = backgroundColor;

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.02f, 0.02f);
        panelRect.anchorMax = new Vector2(0.52f, 0.22f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("InfoText");
        textGO.transform.SetParent(panelGO.transform, false);

        infoText = textGO.AddComponent<Text>();
        infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        infoText.fontSize = 24;
        infoText.color = textColor;
        infoText.alignment = TextAnchor.UpperLeft;
        infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = infoText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 46f);
        textRect.offsetMax = new Vector2(-12f, -12f);

        GameObject buttonGO = new GameObject("BackButton");
        buttonGO.transform.SetParent(panelGO.transform, false);
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.45f, 0.95f, 0.95f);
        backButton = buttonGO.AddComponent<Button>();
        backButton.onClick.AddListener(ReturnToMainView);

        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.zero;
        buttonRect.pivot = Vector2.zero;
        buttonRect.anchoredPosition = new Vector2(12f, 10f);
        buttonRect.sizeDelta = new Vector2(110f, 30f);

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(buttonGO.transform, false);
        Text label = labelGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "返回";
        label.fontSize = 18;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private void ReturnToMainView()
    {
        FindObjectOfType<CameraController>()?.ResetView();
        ResetInfo();
    }
}
