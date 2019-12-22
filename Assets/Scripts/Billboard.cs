using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{

    [SerializeField] private bool doVerticalRotation = false;

    Quaternion rotation;
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void OnRenderObject()
    {

        //if (Application.isPlaying)
        //    transform.LookAt(2 * (transform.position + Camera.main.transform.forward * 0) - Camera.main.transform.position);
        //else
        //    transform.LookAt(2 * (transform.position + UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward * 0) - UnityEditor.SceneView.lastActiveSceneView.camera.transform.position);


        //if (Application.isPlaying)
        //    transform.rotation = Camera.main.transform.rotation;
        //else
        //    transform.rotation = UnityEditor.SceneView.lastActiveSceneView.camera.transform.rotation;

        if (cam)
        {

            if (!Application.isPlaying)
                cam = UnityEditor.SceneView.lastActiveSceneView.camera;

            Vector3 point = cam.WorldToScreenPoint(transform.position);


            Ray ray = cam.ScreenPointToRay(point);

            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

            transform.rotation = Quaternion.LookRotation(ray.direction, cam.transform.up);

        }

        //if (!doVerticalRotation)
        //    transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0f);

    }

}
