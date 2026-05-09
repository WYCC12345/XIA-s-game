using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    public Transform sunCenter;
    public float orbitSpeed = 10f;
    public float orbitRadius = 5f;
    public float startAngle = 0f;

    private float currentAngle;

    void Start()
    {
        currentAngle = startAngle;
        UpdatePosition();
    }

    void Update()
    {
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;
        UpdatePosition();
    }

    void UpdatePosition()
    {
        float rad = currentAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * orbitRadius;
        float z = Mathf.Sin(rad) * orbitRadius;
        Vector3 center = sunCenter != null ? sunCenter.position : Vector3.zero;
        transform.position = center + new Vector3(x, 0, z);
    }
}
