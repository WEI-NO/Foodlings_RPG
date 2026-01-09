using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tower : BaseEntity
{
    [Header("Spawn Position")]
    public Transform spawnPoint;
    public Vector2 yOffsetRange;
    public int yOffsetSteps = 4;

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

        healthText.text = $"{CurrentHealth:F0} / {MaxHealth}";
        healthFillBar.fillAmount = healthPercentage;
    }

    public Vector2 GetSpawnPoint()
    {
        //int step = Random.Range(0, yOffsetSteps);
        //float range = yOffsetRange.y - yOffsetRange.x;
        //float stepAmount = range / yOffsetSteps;

        //float lowerBound = spawnPoint.position.y + yOffsetRange.x;
        //float yPosition = lowerBound + stepAmount * step;


        //return new Vector2(spawnPoint.position.x, yPosition);
        float randomY = spawnPoint.position.y + Random.Range(yOffsetRange.x, yOffsetRange.y);
        return new Vector2(GetXPosition(), randomY);
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
