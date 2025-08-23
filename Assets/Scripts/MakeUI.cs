using System;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MakeUI : MonoBehaviour
{
    [SerializeField]
    private GameObject skillButton;
    [SerializeField]
    private Transform Canvas;
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Sprite[] skillImages;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MakeSkillButton(new int[] { 2, 4, 3 }, 3);
        MakeHealthSlier();
        Hit(20);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MakeSkillButton(int[] skillKind, int filledCount) // 인벤토리 창 생성
    {
        int count = 2;
        int[] result = new int[8]; 
        Array.Copy(skillKind, result, skillKind.Length);
        GameObject instance = Instantiate(skillButton, Canvas);
        instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 90);
        Transform array = instance.transform.GetChild(2);
        Debug.Log(array);   
        Image[] image = instance.GetComponentsInChildren<Image>();
        foreach (int num in result)
        { 
            image[count].sprite = skillImages[num];
            count++;    
        }
    }
    void MakeHealthSlier()
    {
        Slider instance = Instantiate(healthSlider, Canvas);
        instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(-520, 500);
        healthSlider.maxValue = 100;
        healthSlider.value = 100;
    }

    void Hit(int damage)
    {
        healthSlider.value -= damage;
    }
}
