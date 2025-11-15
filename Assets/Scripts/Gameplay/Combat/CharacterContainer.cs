using CustomLibrary.References;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterContainer : MonoBehaviour
{
    public static CharacterContainer Instance;

    private List<List<CharacterEntity>> AllCharacters = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            AllCharacters.Add(new());
        }
    }

    private void LateUpdate()
    {
        GetTeam(Team.Friendly).Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x));
        GetTeam(Team.Hostile).Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }

    public CharacterEntity GetFrontMost(Team team)
    {
        var t = GetTeam(team);

        if (t.Count > 0)
        {
            return t[0];
        }

        return null;
    }

    public void RegisterUnit(CharacterEntity entity)
    {
        var list = GetTeam(entity.team);
        list.Add(entity);
        entity.OnEntityDead += (u) =>
        {
            if (u is CharacterEntity ce)
            {
                UnregisterUnit(ce);
            }
        };
    }

    public void UnregisterUnit(CharacterEntity entity)
    {
        var list = GetTeam(entity.team);
        if (list == null) return;

        list.Remove(entity);
    }

    private List<CharacterEntity> GetTeam(Team team)
    {
        var teamIndex = team.ToInt();
        if (teamIndex < 0 || teamIndex >= 2) return null;

        return AllCharacters[teamIndex];
    }
}