using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosingTimer : MonoBehaviour
{
    public float closeTime = 2.0f;

    private float timer;
    private GameObject panelToClose;
    // Start is called before the first frame update
    void Start()
    {
        panelToClose = this.gameObject;
        timer = closeTime;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        panelToClose = this.gameObject;
        timer = closeTime;
    }
}
