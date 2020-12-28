using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeout : MonoBehaviour
{

    public Vector3 startScale;
    public Vector3 endScale;

    public float lifetime;
    private float currentLifetime;

    void Start()
    {
        currentLifetime = lifetime;
        if (startScale != Vector3.zero)
            transform.localScale = startScale;
    }

    void Update()
    {
        if (startScale != Vector3.zero)
        {
            transform.localScale = endScale + (startScale - endScale) * (currentLifetime / lifetime);
        }
        if(currentLifetime <= 0)
        {
            Destroy(gameObject);
        }
        currentLifetime -= Time.deltaTime;

    }
    public void SetBulletScale(float dist)
    {
        startScale = new Vector3(startScale.x, 1, dist);
        endScale = new Vector3(endScale.x, 1, dist);
    }
}
