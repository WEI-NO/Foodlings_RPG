using CustomLibrary.References;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterObtainUIPage : BaseUIPage
{
    public static CharacterObtainUIPage Instance;

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
    private HashSet<string> unregisteredCharacters = new();

    [Header("Sequence Settings")]
    public float Delay_ObtainViewerStartAnimation; // Initial delay at the start
    public float Delay_NewObtainedStep;  // Delay between each character display.
    public float Delay_AfterCreatingDisplay_Default; // How long to wait for the NewObtainedCharacterDisplay animation
    public float Delay_AfterCreatingDisplay_Unseen; // How long to wait for the NewObtainedCharacterDisplay animation

    public List<CharacterData> debug_list_simulation;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.N)) 
        {
            StartView(debug_list_simulation);
        }
    }

    public void StartView(List<CharacterData> obtainedList)
    {
        foreach (var d in obtainedList)
        {
            if (!PlayerCollection.Instance.SeenInCatalog(d))
            {
                unregisteredCharacters.Add(d.id);
            }
        }

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
        obtainViewer.SetConfirmButton(false);

        // Delay until the full obtainViewer start animation is fully played
        yield return InterruptableDelay(Delay_ObtainViewerStartAnimation);

        CharacterData nextData = obtainViewer.Step();
        while (nextData != null)
        {
            // Spawn 1 Display Per Delay
            var display = obtainViewer.CreateDisplay(nextData, Seen(nextData));


            if (display == null)
            {
                Debug.LogError($"{gameObject.name} : failed to create a new display via obtainViewer.CreateDisplay(nextData);");
                continue;
            }

            // If unseen, start a different default animation
            if (!Seen(nextData))
            {
                display.RunAnimation_Seen();
                yield return InterruptableDelay(Delay_AfterCreatingDisplay_Unseen);
            } else
            {
                display.RunAnimation_Default();
                yield return InterruptableDelay(Delay_AfterCreatingDisplay_Default);
            }

            // If the first time unlocking this character
            if (!Seen(nextData))
            {
                seenCharacters.Add(nextData.id);
                unseenViewer.Init(nextData);

                yield return unseenViewer.StartView();

                display.RunAnimation_Continuation();
            }

            yield return InterruptableDelay(Delay_NewObtainedStep);
            nextData = obtainViewer.Step();
        }

        obtainViewer.SetConfirmButton(true);
    }

    public bool Seen(CharacterData data)
    {
        return !unregisteredCharacters.Contains(data.id) || seenCharacters.Contains(data.id);
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
        obtainViewer.SetConfirmButton(false);
        if (viewCoroutine != null)
        {
            StopCoroutine(viewCoroutine);
            viewCoroutine = null;
        }
    }

}
