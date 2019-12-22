using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{

    public bool startActive = false;
    public bool activateMouseOnActive = true;

    private void Awake()
    {

        this.gameObject.SetActive(startActive);

    }

    public void ToggleActive()
    {
        ToggleActive(!this.gameObject.activeSelf);
    }
    public void ToggleActive(bool activeState)
    {
        this.gameObject.SetActive(activeState);

        if (activateMouseOnActive)
        {
            if (activeState) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;
        }

    }


}
