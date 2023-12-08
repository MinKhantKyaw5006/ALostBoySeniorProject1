using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

public class HeartController : MonoBehaviour
{
    PlayerController player;

    private GameObject[] heartContainers;
    private Image[] heartFills;
    public Transform heartsParent;
    public GameObject heartContainerPrefeb;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.Instance;
        heartContainers = new GameObject[PlayerController.Instance.maxHealth];
        heartFills = new Image[PlayerController.Instance.maxHealth];

        PlayerController.Instance.onHealthChangedCallBack += UpdateHeartHUD;
        InstantiateHeartContainers();
        UpdateHeartHUD();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetHeartContainer()
    {
        for (int i  = 0; i < heartContainers.Length; i++)
        {
            if(i < PlayerController.Instance.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);    
            }
        }
    }

    void SetFilledHeart()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < PlayerController.Instance.maxHealth)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < PlayerController.Instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefeb);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();

        }
    }

    void UpdateHeartHUD()
    {
        SetHeartContainer();
        SetFilledHeart();
    }
}
