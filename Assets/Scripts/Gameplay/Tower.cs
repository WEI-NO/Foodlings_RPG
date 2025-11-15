using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tower : BaseEntity
{
    [Header("Spawn Position")]
    public Transform spawnPoint;
    public Vector2 yOffsetRange;

    [Header("UI Display")]
    public TextMeshProUGUI healthText;
    public Image healthFillBar;
    public Team team;

    private void Start()
    {
        ResetHealth();
    }

    private void Update()
    {
        var healthPercentage = CurrentHealth / MaxHealth;

        healthText.text = $"{CurrentHealth} / {MaxHealth}";
        healthFillBar.fillAmount = healthPercentage;
    }

    public Vector2 GetSpawnPoint()
    {
        float randomYOffset = Random.Range(yOffsetRange.x, yOffsetRange.y);
        return (Vector2)spawnPoint.position + new Vector2(0, randomYOffset);
    }

    public float GetXPosition()
    {
        return spawnPoint.position.x;
    }

    protected override void OnDeath()
    {
        if (team == Team.Friendly)
        {
            GameMatchManager.Instance.SetGameResult(false);
        } else
        {
            GameMatchManager.Instance.SetGameResult(true);
        }
    }
}
