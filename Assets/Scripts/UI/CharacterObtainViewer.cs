using System.Collections.Generic;
using UnityEngine;

public class CharacterObtainViewer : MonoBehaviour
{
    private Animator anim;

    [Header("UI References")]
    public Transform content;
    public NewObtainedCharacterDisplay obtainDisplay;

    private List<CharacterData> InternalList;
    private int stepIndex = 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Init(List<CharacterData> obtainedList)
    {
        InternalList = new(obtainedList);
        stepIndex = 0;
        ClearDisplay();
    }

    public void StartAnimation()
    {
        if (anim)
        {
            anim.SetTrigger("Start");
        }
    }

    public void EndAnimation()
    {
        if (anim)
        {
            anim.SetTrigger("End");
        }
    }
    public void CreateDisplay(CharacterData data)
    {
        if (data == null || obtainDisplay == null) return;

        var display = Instantiate(obtainDisplay, content);

        display.Init(data);
    }

    public CharacterData Step()
    {
        if (stepIndex >= InternalList.Count)
        {
            return null;
        }

        return InternalList[stepIndex++];
    }
    private void ClearDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
