using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCulling : MonoBehaviour
{
    public float availableDistance;
    private float _distance;
    private Light _lightcomponent;
    private Camera _player;

    private float _intensity = 1f;

    private void Start()
    {
        _lightcomponent = gameObject.GetComponent<Light>();
        _intensity = _lightcomponent.intensity;
        _player = Camera.main;

        _lightcomponent.intensity = 0f;
        _lightcomponent.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        _distance = Vector3.Distance(_player.transform.position, transform.position);

        if (_distance < availableDistance)
        {
            _lightcomponent.intensity = Mathf.Lerp(_lightcomponent.intensity, _intensity, 0.05f);
        }

        if (_distance > availableDistance)
        {
            _lightcomponent.intensity = Mathf.Lerp(_lightcomponent.intensity, 0f, 0.05f);
        }

        _lightcomponent.enabled = !Mathf.Approximately(_lightcomponent.intensity, 0);
    }
}