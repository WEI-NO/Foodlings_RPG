using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Region/Level List")]
public class RegionLevelList : ScriptableObject
{
    public List<Level> levels;
}
