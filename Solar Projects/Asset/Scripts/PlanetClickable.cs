using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlanetClickable : MonoBehaviour
{
    public string planetName = "星球";
    [TextArea(2, 5)]
    public string funFact = "这是一个很特别的星球！";
    public Color glowColor = Color.cyan;
    public float focusZoom = 6f;
    public float focusPitch = 28f;
    public float highlightScale = 1.2f;
    public float highlightDuration = 0.5f;
    public AudioClip clickSound;
    public CameraController cameraController;
    public PlanetInfoUI infoUI;

    private Renderer planetRenderer;
    private Material planetMaterial;
    private Color originalColor;
    private Color originalEmission;
    private bool hasEmissionProperty;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private bool isFlashing;

    private void Start()
    {
        planetRenderer = GetComponent<Renderer>();
        if (planetRenderer != null)
        {
            planetMaterial = planetRenderer.material;
            originalColor = planetMaterial.color;
            hasEmissionProperty = planetMaterial.HasProperty("_EmissionColor");
            if (hasEmissionProperty)
            {
                originalEmission = planetMaterial.GetColor("_EmissionColor");
            }
        }

        originalScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (infoUI == null)
        {
            infoUI = PlanetInfoUI.Instance;
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
    }

    private void OnMouseDown()
    {
        Select();
    }

    public void Select()
    {
        if (cameraController != null)
        {
            cameraController.FocusOn(transform, focusZoom, focusPitch);
        }

        if (infoUI != null)
        {
            infoUI.ShowInfo(planetName, funFact);
        }

        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        StartCoroutine(FlashSelection());
    }

    private IEnumerator FlashSelection()
    {
        if (isFlashing)
        {
            yield break;
        }

        isFlashing = true;

        if (planetMaterial != null)
        {
            planetMaterial.color = glowColor;
            if (hasEmissionProperty)
            {
                planetMaterial.EnableKeyword("_EMISSION");
                planetMaterial.SetColor("_EmissionColor", glowColor * 1.5f);
            }
        }

        transform.localScale = originalScale * highlightScale;

        yield return new WaitForSeconds(highlightDuration);

        transform.localScale = originalScale;

        if (planetMaterial != null)
        {
            planetMaterial.color = originalColor;
            if (hasEmissionProperty)
            {
                planetMaterial.SetColor("_EmissionColor", originalEmission);
            }
        }

        isFlashing = false;
    }
}
