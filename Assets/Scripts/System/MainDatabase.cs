using CustomLibrary.References;
using UnityEngine;

public class MainDatabase : MonoBehaviour
{
    public static MainDatabase Instance;

    public ItemDatabase itemDatabase;
    public UpgradeDatabase upgradeDatabase;
    public SpriteDatabase spriteDatabase;
    public LevelDatabase levelDatabase;


    private void Awake()
    {
        Initializer.SetInstance(this);
        levelDatabase.Init();
    }
}
