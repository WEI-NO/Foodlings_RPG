using System.Collections.Generic;
using UnityEngine;

public class CharacterObtainViewer : MonoBehaviour
{
    private Animator anim;

    [Header("UI References")]
    public Transform content;
    public NewObtainedCharacterDisplay obtainDisplay;
    public GameObject confirmButton;

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
    public NewObtainedCharacterDisplay CreateDisplay(CharacterData data, bool seen)
    {
        if (data == null || obtainDisplay == null) return null;

        var display = Instantiate(obtainDisplay, content);

        display.Init(data, seen);
        return display;
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

    public void SetConfirmButton(bool state)
    {
        if (confirmButton == null) return;

        confirmButton.SetActive(state);
    }
}
