using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string name;
        public Button button;
        public GameObject panel;
        public Image icon; // optional if you have an icon
        public Color normalColor = Color.gray;
        public Color activeColor = new Color(1f, 0.6f, 0f); // orange
    }

    public Tab[] tabs;

    void Start()
    {
        foreach (var tab in tabs)
        {
            string tabName = tab.name; // local copy
            tab.button.onClick.AddListener(() => ShowTab(tabName));
        }

        ShowTab(tabs[0].name); // default: first tab
    }

    public void ShowTab(string tabName)
    {
        foreach (var tab in tabs)
        {
            bool isActive = tab.name == tabName;
            tab.panel.SetActive(isActive);

            // Highlight button
            if (tab.icon != null)
                tab.icon.color = isActive ? tab.activeColor : tab.normalColor;
        }
    }
}
