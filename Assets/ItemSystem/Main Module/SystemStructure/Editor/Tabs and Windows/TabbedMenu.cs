using UnityEngine.UIElements;

public class TabbedMenu : VisualElement
{
    public TabbedMenu(string[] _TabNames, System.Action<string> _OnTabChanged)
    {
        var tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        foreach (var tabName in _TabNames)
        {
            var tabButton = new Button(() => _OnTabChanged?.Invoke(tabName)) { text = tabName };
            tabButton.style.flexGrow = 1;
            tabContainer.Add(tabButton);
        }
        Add(tabContainer);
    }
}