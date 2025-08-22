using UnityEngine;
using UnityEngine.UI;

public class MakeUI : MonoBehaviour
{
    [SerializeField]
    private GameObject skillButton;
    [SerializeField]
    private Sprite[] skillImages;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MakeSkillButton(GameObject skillButton)
    {
        GameObject instance = Instantiate(skillButton, transform.position, Quaternion.identity);
        Image[] image = instance.GetComponentsInChildren<Image>();

    }
}
