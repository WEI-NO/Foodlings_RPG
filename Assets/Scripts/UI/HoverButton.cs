using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // A reference to the normal button (keeps OnClick functionality)
    private Button button;

    public string DisplayName;
    public TextMeshProUGUI text;

    [Header("Hover Events")]
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (text != null)
        {
            text.text = DisplayName;
            text.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Triggered when pointer enters the button
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke();

        if (text != null)
        {
            text.gameObject.SetActive(true);
        }

    }

    /// <summary>
    /// Triggered when pointer exits the button
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();

        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
    }
}
