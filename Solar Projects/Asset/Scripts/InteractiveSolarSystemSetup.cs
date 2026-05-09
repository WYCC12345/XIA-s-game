using UnityEngine;

public class InteractiveSolarSystemSetup : MonoBehaviour
{
    public float clickForgiveness = 0.35f;
    public float maxClickDragDistance = 12f;

    private readonly string[] planetNames =
    {
        "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune"
    };

    private Vector2 mouseDownPosition;

    private void Awake()
    {
        CameraController cameraController = Camera.main != null
            ? Camera.main.GetComponent<CameraController>()
            : FindObjectOfType<CameraController>();

        if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.gameObject.AddComponent<CameraController>();
        }

        if (cameraController != null)
        {
            cameraController.defaultZoom = 22f;
            cameraController.defaultRotationX = 0f;
            cameraController.defaultRotationY = 25f;
            cameraController.defaultTarget = new Vector3(8f, 0f, 0f);
        }

        PlanetInfoUI infoUI = FindObjectOfType<PlanetInfoUI>();
        if (infoUI == null)
        {
            infoUI = new GameObject("PlanetInfoUI").AddComponent<PlanetInfoUI>();
        }

        Transform sun = FindObject("Sun");
        if (sun != null)
        {
            AddSpin(sun.gameObject, 8f);
            ConfigureClickable(
                sun.gameObject,
                "太阳 Sun",
                "太阳是一颗会发光发热的恒星。它像太阳系的大灯泡，给行星带来光和温暖。",
                new Color(1f, 0.85f, 0.15f),
                6f,
                25f,
                cameraController,
                infoUI);
        }

        for (int i = 0; i < planetNames.Length; i++)
        {
            Transform planet = FindObject(planetNames[i]);
            if (planet == null)
            {
                continue;
            }

            AddSpin(planet.gameObject, GetSpinSpeed(planet.name));
            AddOrbit(planet.gameObject, sun, GetOrbitSpeed(planet.name), planet.position.magnitude, i * 37f);
        }

        ConfigurePlanetClickables(cameraController, infoUI);

        Transform earth = FindObject("Earth");
        if (earth != null)
        {
            Transform moon = EnsureMoon(earth);
            ConfigureClickable(
                moon.gameObject,
                "月亮 Moon",
                "月亮绕着地球旅行。晚上看到的亮光，其实是它反射太阳光。",
                new Color(1f, 0.95f, 0.55f),
                2.2f,
                32f,
                cameraController,
                infoUI);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
        }

        if (!Input.GetMouseButtonUp(0) || Camera.main == null)
        {
            return;
        }

        if (Vector2.Distance(mouseDownPosition, Input.mousePosition) > maxClickDragDistance)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.SphereCast(ray, clickForgiveness, out hit, 1000f))
        {
            return;
        }

        PlanetClickable clickable = hit.collider.GetComponentInParent<PlanetClickable>();
        if (clickable != null)
        {
            clickable.Select();
        }
    }

    private Transform EnsureMoon(Transform earth)
    {
        Transform moon = FindObject("Moon");
        if (moon == null)
        {
            GameObject moonGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moonGO.name = "Moon";
            moonGO.transform.localScale = Vector3.one * 0.16f;

            Material moonMaterial = Resources.Load<Material>("Moon");
            if (moonMaterial != null)
            {
                moonGO.GetComponent<Renderer>().sharedMaterial = moonMaterial;
            }
            else
            {
                moonGO.GetComponent<Renderer>().material.color = new Color(0.72f, 0.72f, 0.68f);
            }

            moon = moonGO.transform;
        }

        moon.position = earth.position + new Vector3(0.9f, 0f, 0f);
        AddSpin(moon.gameObject, 30f);

        PlanetOrbit orbit = AddOrbit(moon.gameObject, earth, 55f, 0.9f, 0f);
        orbit.sunCenter = earth;

        return moon;
    }

    private void ConfigurePlanetClickables(CameraController cameraController, PlanetInfoUI infoUI)
    {
        ConfigurePlanet(
            "Mercury",
            "水星 Mercury",
            "水星离太阳最近。它跑得很快，是太阳系里的小小短跑冠军。",
            new Color(0.75f, 0.78f, 0.82f),
            2.4f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Venus",
            "金星 Venus",
            "金星被厚厚的云包住，像穿了一件亮亮的外套。",
            new Color(1f, 0.75f, 0.25f),
            2.6f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Earth",
            "地球 Earth",
            "我们住在地球上！它有蓝色海洋、白色云朵，还有很多生命。",
            new Color(0.2f, 0.75f, 1f),
            3.5f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Mars",
            "火星 Mars",
            "火星看起来红红的，因为地面上有像铁锈一样的尘土。",
            new Color(1f, 0.35f, 0.15f),
            3f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Jupiter",
            "木星 Jupiter",
            "木星是最大的行星。它的大红斑是一场超级久的大风暴。",
            new Color(1f, 0.62f, 0.32f),
            4.6f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Saturn",
            "土星 Saturn",
            "土星有漂亮的光环，光环里有冰块和小石头在一起转圈。",
            new Color(1f, 0.86f, 0.45f),
            4.4f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Uranus",
            "天王星 Uranus",
            "天王星像是侧着滚动的蓝绿色冰球，转法很特别。",
            new Color(0.45f, 1f, 0.95f),
            4f,
            cameraController,
            infoUI);

        ConfigurePlanet(
            "Neptune",
            "海王星 Neptune",
            "海王星很远很蓝，上面有太阳系里非常强的风。",
            new Color(0.2f, 0.35f, 1f),
            4f,
            cameraController,
            infoUI);
    }

    private void ConfigurePlanet(
        string objectName,
        string displayName,
        string fact,
        Color glowColor,
        float focusZoom,
        CameraController cameraController,
        PlanetInfoUI infoUI)
    {
        Transform planet = FindObject(objectName);
        if (planet == null)
        {
            return;
        }

        ConfigureClickable(
            planet.gameObject,
            displayName,
            fact,
            glowColor,
            focusZoom,
            28f,
            cameraController,
            infoUI);
    }

    private void ConfigureClickable(
        GameObject target,
        string displayName,
        string fact,
        Color glowColor,
        float focusZoom,
        float focusPitch,
        CameraController cameraController,
        PlanetInfoUI infoUI)
    {
        PlanetClickable clickable = target.GetComponent<PlanetClickable>();
        if (clickable == null)
        {
            clickable = target.AddComponent<PlanetClickable>();
        }

        clickable.planetName = displayName;
        clickable.funFact = fact;
        clickable.glowColor = glowColor;
        clickable.focusZoom = focusZoom;
        clickable.focusPitch = focusPitch;
        clickable.highlightScale = 1.25f;
        clickable.highlightDuration = 0.55f;
        clickable.cameraController = cameraController;
        clickable.infoUI = infoUI;

        SphereCollider sphereCollider = target.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereCollider.radius = Mathf.Max(sphereCollider.radius, 0.9f);
        }
    }

    private void AddSpin(GameObject target, float speed)
    {
        PlanetSpin spin = target.GetComponent<PlanetSpin>();
        if (spin == null)
        {
            spin = target.AddComponent<PlanetSpin>();
        }

        spin.spinSpeed = speed;
    }

    private PlanetOrbit AddOrbit(GameObject target, Transform center, float speed, float radius, float startAngle)
    {
        PlanetOrbit orbit = target.GetComponent<PlanetOrbit>();
        if (orbit == null)
        {
            orbit = target.AddComponent<PlanetOrbit>();
        }

        orbit.sunCenter = center;
        orbit.orbitSpeed = speed;
        orbit.orbitRadius = radius;
        orbit.startAngle = startAngle;
        return orbit;
    }

    private Transform FindObject(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.transform : null;
    }

    private float GetOrbitSpeed(string planetName)
    {
        switch (planetName)
        {
            case "Mercury": return 18f;
            case "Venus": return 14f;
            case "Earth": return 11f;
            case "Mars": return 9f;
            case "Jupiter": return 5f;
            case "Saturn": return 4f;
            case "Uranus": return 3f;
            case "Neptune": return 2.5f;
            default: return 8f;
        }
    }

    private float GetSpinSpeed(string planetName)
    {
        switch (planetName)
        {
            case "Jupiter": return 36f;
            case "Saturn": return 28f;
            case "Uranus": return 20f;
            case "Neptune": return 22f;
            default: return 24f;
        }
    }
}
