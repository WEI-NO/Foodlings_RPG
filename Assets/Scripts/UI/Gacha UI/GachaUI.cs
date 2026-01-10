using UnityEngine;

public class GachaUI : BaseUIPage
{
    
    public CharacterObtainUIPage obtainUI;

    public Transform content;
    public OvenDisplay display;

    public SnapHorizontalScroll snapScroller;

    protected override void OnContentEnabled()
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        for (int i = 0; i < PlayerGachaInfo.Instance.Ovens.Count; i++)
        {
            var dis = Instantiate(display, content);
            dis.Initialize(i);
        }

        //snapScroller.SetupPages();
    }

}
