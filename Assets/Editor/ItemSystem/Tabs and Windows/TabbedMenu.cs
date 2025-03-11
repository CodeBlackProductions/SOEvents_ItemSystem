using UnityEngine.UIElements;

public class TabbedMenu : VisualElement
{
    public TabbedMenu(string[] tabNames, System.Action<string> onTabChanged)
    {
        var tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        foreach (var tabName in tabNames)
        {
            var tabButton = new Button(() => onTabChanged(tabName)) { text = tabName };
            tabButton.style.flexGrow = 1;
            tabContainer.Add(tabButton);
        }
        Add(tabContainer);
    }
}