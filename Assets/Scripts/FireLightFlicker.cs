using UnityEngine;

public class FireLightFlicker : MonoBehaviour
{
    public Light targetLight;

    private float timer;
    private float nextChange;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        nextChange = 0.1f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextChange)
        {
            timer = 0f;

            targetLight.intensity =
                Random.Range(1.5f, 3.5f);

            nextChange =
                Random.Range(0.05f, 0.2f);
        }
    }
}
