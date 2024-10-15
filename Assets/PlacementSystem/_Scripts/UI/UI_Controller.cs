using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Controller : MonoBehaviour
{
    private int clickYn = 0;
    Animator anim;

    public GameObject[] ChairPanel;
    public GameObject help;
    public bool helpYn;

    public void help_Panel()
    {
        helpYn = !helpYn;
        help.gameObject.SetActive(helpYn);
    }


    private void Start()
    {
        anim = GetComponent<Animator>();
        init();
    }

    public void init()
    {
        foreach (GameObject t in ChairPanel)
        {
            t.SetActive(false);
        }
        help.gameObject.SetActive(false);

    }

    public void deskPanel()
    {
        init();
        ChairPanel[1].SetActive(true);
    }

    public void wallPanel()
    {
        init();
        ChairPanel[2].SetActive(true);
    }

    public void ItPanel()
    {
        init();
        ChairPanel[3].SetActive(true);
    }

    public void EtcPanel()
    {
        init();
        ChairPanel[4].SetActive(true);
    }

    public void chairPanel()
    {
        init();
        ChairPanel[0].SetActive(true);
    }

    // 
    // 30  m204 ¹æ

    public void cleckMenu()
    {
        if (clickYn == 0) clickYn = 1;
        else if (clickYn == 1) clickYn = 2;
        else if (clickYn == 2) clickYn = 1;


        if (clickYn == 1)
        {
            anim.SetInteger("click", clickYn);
        }

        if (clickYn == 2)
        {
            init();
            anim.SetInteger("click", clickYn);
        }
    }

}
