using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class SummonButton : MonoBehaviour
{
    [Header("Button Properties")]
    [SerializeField] private Sprite enabledSprite;
    [SerializeField] private Sprite disabledSprite;
    [SerializeField] private Image buttonSprite;
    [SerializeField] private Image troopIcon;
    public Animator anim;
    public Button button;
    public TextMeshProUGUI costText;
    public Color AffordableColor;
    public Color UnffordableColor;


    [Header("Summon Settings")]
    public CharacterData assignedUnit;
    private CharacterData lastAssignedUnit;

    [Header("Cooldown")]
    public float cooldownTimer = 0.0f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        UpdateDisplay();
    }

    private void Update()
    {
        if (assignedUnit != lastAssignedUnit)
        {
            UpdateDisplay();
        }

        if (assignedUnit != null)
        {
            if (GameMatchManager.Instance.HasMoney(assignedUnit.summonCost))
            {
                costText.color = AffordableColor;
            } else
            {
                costText.color = UnffordableColor;
            }
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                button.interactable = true;
            }
        }
    }

    public void AssignUnit(CharacterInstance unit)
    {

        assignedUnit = unit == null ? null : unit.baseData;
    }

    private void UpdateDisplay()
    {
        bool state = assignedUnit != null;

        buttonSprite.sprite = state ? enabledSprite : disabledSprite;

        troopIcon.sprite = state ? assignedUnit.unitSprite : null;
        troopIcon.gameObject.SetActive(state);

        if (state)
        {
            costText.text = $"${assignedUnit.summonCost}";
        } else
        {
            costText.text = "";
        }

        lastAssignedUnit = assignedUnit;
    }

    public void SummonUnit()
    {
        if (assignedUnit == null) return;

        if (cooldownTimer > 0) return;

        if (!GameMatchManager.Instance.UseMoney(assignedUnit.summonCost))
        {
            return;
        }

        var spawnedActor = Instantiate(assignedUnit.unitPrefab);
        if (spawnedActor == null) return;


        spawnedActor.transform.position = MapController.Instance.spawnedPlayerBase.GetSpawnPoint();
        var entity = spawnedActor.GetComponent<CharacterEntity>();
        entity.SetTeam(Team.Friendly);
        CharacterContainer.Instance.RegisterUnit(entity);

        var cooldown = entity.characterData.cooldownSec;
        button.interactable = false;
        anim.SetFloat("CooldownMultiplier", 1.0f / cooldown);
        cooldownTimer = cooldown;
    }
}
