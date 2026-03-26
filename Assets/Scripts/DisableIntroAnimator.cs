using UnityEngine;

// Disables intro animator and enables inputfield from an animation event
public class DisableIntroAnimator : MonoBehaviour
{
    public void DisableIntro()
    {
        GetComponent<Animator>().enabled = false;
        FindAnyObjectByType<GameManager>().userInputField.gameObject.SetActive(true);
    }
}
