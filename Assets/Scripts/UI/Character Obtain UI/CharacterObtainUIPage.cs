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

    public CharacterObtainViewer obtainViewer;
    public UnseenCharacterUI unseenViewer;

    public GameObject confirmButton;

    private HashSet<string> seenCharacters = new();

    [Header("Sequence Settings")]
    public float Delay_ObtainViewerStartAnimation;
    public float Delay_NewObtainedStep;
    public float Delay_UnseenObtainedInitial;

    public List<CharacterData> debug_list_simulation;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) 
        {
            StartView(debug_list_simulation);
        }
    }

    public void StartView(List<CharacterData> obtainedList)
    {
        currentList = new(obtainedList);
        SetActive(true);
        seenCharacters.Clear();
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
        // Initialize obtain and unseen viewer
        obtainViewer.Init(currentList);
        obtainViewer.StartAnimation();
        // Delay until the full obtainViewer start animation is fully played
        yield return InterruptableDelay(Delay_ObtainViewerStartAnimation);

        CharacterData nextData = obtainViewer.Step();
        while (nextData != null)
        {
            // Spawn 1 Display Per Delay
            obtainViewer.CreateDisplay(nextData);

            yield return new WaitForSeconds(Delay_UnseenObtainedInitial);
            // If the first time unlocking this character
            if (!PlayerCollection.Instance.SeenInCatalog(nextData) && !seenCharacters.Contains(nextData.id))
            {
                seenCharacters.Add(nextData.id);
                unseenViewer.Init(nextData);

                yield return unseenViewer.StartView();
            }
            yield return InterruptableDelay(Delay_NewObtainedStep);
            nextData = obtainViewer.Step();
        }

        // obtainViewer.EndAnimation();
    }

    private IEnumerator InterruptableDelay(float seconds)
    {
        float t = 0;
        while (t < seconds)
        {
            if (spedUp)
            {
                yield return null;
            }
            t  += Time.deltaTime;
            yield return null;
        }
    }

    public void EndAnimation()
    {
        obtainViewer.EndAnimation();
        if (viewCoroutine != null)
        {
            StopCoroutine(viewCoroutine);
            viewCoroutine = null;
        }
    }

}
