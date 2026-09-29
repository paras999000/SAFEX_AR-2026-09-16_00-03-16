using UnityEngine;

public class EmergencyLightFlicker : MonoBehaviour
{
    public Light targetLight;

    private float timer;
    private float nextChange;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        nextChange =
            Random.Range(0.08f, 0.3f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextChange)
        {
            timer = 0f;

            targetLight.intensity =
                Random.Range(0.25f, 1.0f);

            nextChange =
                Random.Range(0.08f, 0.35f);
        }
    }
}