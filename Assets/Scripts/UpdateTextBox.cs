using UnityEngine;
using UnityEngine.UI;

// Updates textbox size to match the scrollview's size
public class UpdateTextBox : MonoBehaviour
{
    LayoutElement layout;
    RectTransform rect;
    GameManager gm;
    float threshold = 10f;

    // Gets components
    void Start()
    {
        layout = GetComponent<LayoutElement>();
        rect = GetComponent<RectTransform>();
        gm = FindAnyObjectByType<GameManager>();
    }

    // Updates textbox size to match scroll view
    void Update()
    {
        layout.preferredWidth = gm.chatLog.GetComponent<RectTransform>().rect.width * 0.53f;

        if (layout.enabled && rect.rect.width < layout.preferredWidth - threshold)
        {
            layout.enabled = false;
        }
        else if (!layout.enabled && rect.rect.width > layout.preferredWidth + threshold)
        {
            layout.enabled = true;
        }
    }
}
