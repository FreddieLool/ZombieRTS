using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;
    private Vector3 originalPos;
    private float shakeTimer;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartShake();
        }

        // Timer to return to original position after shake
        if (shakeTimer > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void StartShake()
    {
        shakeTimer = shakeDuration;
    }
}
