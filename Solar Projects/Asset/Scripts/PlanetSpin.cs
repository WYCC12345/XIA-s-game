using UnityEngine;

public class PlanetSpin : MonoBehaviour
{
    public float spinSpeed = 20f;

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}