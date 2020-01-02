using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCulling : MonoBehaviour
{

    public float availableDistance;
    private float Distance;
    private Light Lightcomponent;
    private Camera Player;

    private float intensity = 1f;
    ///------------------------------
    void Start()
    {
        Lightcomponent = gameObject.GetComponent<Light>();
        intensity = Lightcomponent.intensity;
        Player = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {


        Distance = Vector3.Distance(Player.transform.position, transform.position);


        if (Distance < availableDistance)
        {
            Lightcomponent.intensity = Mathf.Lerp(Lightcomponent.intensity, intensity, 0.05f);
        }
        if (Distance > availableDistance)
        {
            Lightcomponent.intensity = Mathf.Lerp(Lightcomponent.intensity, 0f, 0.05f);
        }

        Lightcomponent.enabled = !Mathf.Approximately(Lightcomponent.intensity, 0);


    }

}