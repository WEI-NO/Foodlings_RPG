using CustomLibrary.References;
using UnityEngine;

public class PlayerGachaInfo : MonoBehaviour
{
    public static PlayerGachaInfo Instance;

    public static int InitialOvenSlots = 1;
    public static int MaximumOvenSlots = 5;

    public int currentOvenSlot = 1;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }



}

public class Oven
{
    public GachaRequest request;

    public Oven()
    {
        request = new GachaRequest(1, 1, 1);
    }
}
