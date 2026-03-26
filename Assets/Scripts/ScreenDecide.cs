using UnityEngine;

// Decides what screen layout to use
public class ScreenDecide : MonoBehaviour
{
    public Transform[] screenLayouts;
    void Start()
    {
        bool isLandscape = Screen.width > Screen.height;
        
        screenLayouts[0].gameObject.SetActive(isLandscape);
        screenLayouts[1].gameObject.SetActive(!isLandscape);
    }
}
