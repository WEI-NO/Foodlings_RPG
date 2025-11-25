using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterObtainUIPage : BaseUIPage
{
    public List<CharacterData> currentList = new();

    public Transform content;
    public ObtainDisplay displayPrefab;

    public float delayPerDisplay = 0.75f;
    public float initialDelay = 1.5f;

    private Coroutine viewCoroutine;
    private bool spedUp = false;

    public void StartView(List<CharacterData> obtainedList)
    {
        currentList = new(obtainedList);
        ClearDisplay();
        SetActive(true);
        if (viewCoroutine != null)
        {
            StopCoroutine(viewCoroutine);
            spedUp = false;
        }
        viewCoroutine = StartCoroutine(ViewCoroutine());
    }

    public void SpeedUp()
    {
        spedUp = true;
    }

    private IEnumerator ViewCoroutine()
    {
        float localDelay = delayPerDisplay;
        bool expedited = false;
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < currentList.Count; i++)
        {
            var newDisplay = Instantiate(displayPrefab, content, false);
            newDisplay.AssignCharacter(currentList[i]);
            yield return new WaitForSeconds(localDelay / 2);

            if (spedUp && !expedited)
            {
                localDelay /= 5;
                expedited = true;
            }

            yield return new WaitForSeconds(localDelay / 2);
        }

        spedUp = false;
    }

    private void ClearDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
