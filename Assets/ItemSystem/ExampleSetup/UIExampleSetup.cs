using ItemSystem.MainModule;
using ItemSystem.SubModules;
using UnityEngine;

public class UIExampleSetup : MonoBehaviour
{
    private void Start()
    {
        ItemEventHandler.Instance.OnHyperlinkClickLeft += (_LinkID) =>
        {
            string text = UIToolTipRegistry.Instance.RetrieveTooltip(_LinkID);
            string HyperlinkText = UIToolTipRegistry.Instance.RetrieveTooltipHyperlinkText(_LinkID);
            UIFactory.CreateNewUIWindow($"{_LinkID}", Input.mousePosition, new Vector2(0.75f, 0.75f), _LinkID, text, Color.white, Color.blue, Color.red, 36);
        };
    }
}