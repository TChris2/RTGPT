using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Clips")]
    // Current enabled clip types
    [SerializeField] List<ClipType> validClipTypes;
    // Stores all clips
    private Dictionary<string, ClipInfo> clipDict = new Dictionary<string, ClipInfo>();
    // List of currently enabled clips
    [SerializeField] private List<ClipInfo> clipList;
    [Header("Main")]
    public TMP_InputField userInputField;
    // Textboxes instantiated in chat log
    [SerializeField] private GameObject[] Textboxes;
    public Transform chatLog;
    ScrollRect chatLogRect;
    [SerializeField] private TMP_Text thinkText;
    [SerializeField] private Animator rtAni;
    private AudioSource audioSource;
    [Header("Text Vars")]
    // Speed multiplier for text display
    [SerializeField] private float speed = 1;
    [SerializeField] private bool isAutoScrolling;
    [Header("Debug Menu")]
    // Debug selected clip
    [SerializeField] private ClipInfo debugClip;
    // Debug code input
    [SerializeField] private string debugMenuCode;
    [SerializeField] private CanvasGroup debugMenu;
    [SerializeField] private TMP_Dropdown debugClipDropdown;
    // Valid and invalid colors for debugClipDropdown
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;
    // Sliders
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider pitchSlider;
    [SerializeField] private Slider[] delaySliders = new Slider[2];
    // Clip type toggles
    [SerializeField] private Toggle[] clipTypeToggles = new Toggle[10];

    #region Main Function
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        chatLogRect = chatLog.GetComponentInParent<ScrollRect>();
        userInputField.onSubmit.AddListener(Submit);
        // Disables inputfield till after intro animation
        userInputField.gameObject.SetActive(false);
        thinkText.text = "";

        debugClipDropdown.onValueChanged.AddListener(SelectClip);
        
        // Updates scroll sensitivity based on build
        #if UNITY_WEBGL && !UNITY_EDITOR
            debugClipDropdown.GetComponentInChildren<ScrollRect>(true).scrollSensitivity = 2f;
        #else
            chatLogRect.scrollSensitivity = 6f;
        #endif

        volumeSlider.onValueChanged.AddListener(UpdateVolume);
        pitchSlider.onValueChanged.AddListener(UpdatePitch);

        foreach (Slider slider in delaySliders)
        {
            slider.onValueChanged.AddListener(value => UpdateLabel(slider, value));
        }

        // Sets sliders to their default values
        ResetSliders();
        
        // Adds valid clip types
        validClipTypes.Clear();
        foreach (ClipType type in Enum.GetValues(typeof(ClipType)))
            validClipTypes.Add(type);
        
        // Loads up all the clips
        ClipInfo[] clipInfoList = Resources.LoadAll<ClipInfo>("");
        foreach (ClipInfo clip in clipInfoList)
        {
            clipDict.Add(clip.clip.name, clip);
            debugClipDropdown.options.Add(new TMP_Dropdown.OptionData(clip.clip.name));
        }

        UpdateClipList();
        
        // Fills in dropdown
        debugClipDropdown.ClearOptions();
        debugClipDropdown.options.Add(new TMP_Dropdown.OptionData("None"));

        foreach (ClipInfo clip in clipList)
            debugClipDropdown.options.Add(new TMP_Dropdown.OptionData(clip.name));

        debugClipDropdown.value = 0;
        SelectClip(0);
        debugClipDropdown.RefreshShownValue();
    }

    // Updates current list of clips
    public void UpdateClipList()
    {
        clipList.Clear();

        foreach (KeyValuePair<string, ClipInfo> kvp in clipDict)
        {
            if (kvp.Value.clipTypes.Any(type => validClipTypes.Contains(type)))
                clipList.Add(kvp.Value);
        }

        clipList.Sort((a, b) => string.Compare(a.clip.name, b.clip.name, true));
    }

    void Update()
    {
        // Checks to see if the user has inputted the secret code for the debug menu
        if (InputFieldCheck())
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                debugMenuCode += "1";
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                debugMenuCode += "2";
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                debugMenuCode = "3";

            if (debugMenuCode == "312")
            {
                MenuOpenClose(debugMenu, true);
            }
        }
        // Closes the debug menu
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            MenuOpenClose(debugMenu, false);
            debugMenuCode = " ";
        }
    }

    // When the user submits a input
    void Submit(string text)
    {
        // Goes through as long as it is not empty or the list is empty
        if (text != "" && clipList.Count != 0)
        {
            // Disables the inputfield until a response is given
            userInputField.interactable = false;
            userInputField.text = "";
            userInputField.gameObject.SetActive(false);

            // Creates a textbox with the user's input
            GameObject userTextbox = Instantiate(Textboxes[0], chatLog);        
            TMP_Text userText = userTextbox.GetComponentInChildren<TMP_Text>();
            userText.text = text;

            // Updates chatlogs vertical position
            StartCoroutine(WidthCheck(userText));        
            
            // Gets a response
            StartCoroutine(GetResponse());
        }
    }

    // Updates chatlogs vertical position
    IEnumerator WidthCheck(TMP_Text text)
    {
        RectTransform rect = text.GetComponent<RectTransform>();
        LayoutElement layout = text.GetComponent<LayoutElement>();

        yield return null;

        if (rect.rect.width > layout.preferredWidth)
            layout.enabled = true;
        
        yield return null;
        yield return null;

        chatLogRect.verticalNormalizedPosition = 0f;
    }

    // Gets a response
    IEnumerator GetResponse()
    {
        int loop = 0;
        float minWait = delaySliders[0].value;
        float maxWait = delaySliders[0].value + delaySliders[1].value;

        // Waits for a random amt of time
        for (int i = 0; i < UnityEngine.Random.Range(minWait, maxWait); i++)
        {
            // Displays a thinking text display while looping
            switch (loop)
            {
                case 1:
                    thinkText.text = "RT Is Thinking .";
                    break;
                case 2:
                    thinkText.text = "RT Is Thinking . .";
                    break;
                case 3:
                    thinkText.text = "RT Is Thinking . . .";
                    break;
                default:
                    loop = 0;
                    thinkText.text = "RT Is Thinking";
                    break;
            }

            loop += 1;

            yield return new WaitForSeconds(.4f);
        }
        
        thinkText.text = "";
        ClipInfo clip;

        // Gets a random clip
        if (debugClip == null)
        {
            int randNum = UnityEngine.Random.Range(0, clipList.Count);
            Debug.Log($"{randNum} {clipList[randNum].clip.name}");
            clip = clipList[randNum];
        }
        // If a debug clip has been selected
        else
        {
            Debug.Log($"Debug clip {debugClip.clip.name} has been selected");
            clip = debugClip;
        }

        // Pops up text on screen
        StartCoroutine(TextPopIn(clip));

        // Sets and plays clip
        audioSource.clip = clip.clip;
        audioSource.Play();
    }

    // Pops up text on screen
    IEnumerator TextPopIn(ClipInfo clip)
    {
        // Creates a textbox for the response
        GameObject rtTextbox = Instantiate(Textboxes[1], chatLog);
        TMP_Text rtText = rtTextbox.GetComponentInChildren<TMP_Text>();
        RectTransform rect = rtText.GetComponent<RectTransform>();
        LayoutElement layout = rtText.GetComponent<LayoutElement>();
        float adjustedLength;
        float delay;
        
        rtText.maxVisibleCharacters = 0;
        rtText.text = clip.text;
        rtText.ForceMeshUpdate();

        isAutoScrolling = true;

        yield return null;

        if (!layout.enabled && rect.rect.width > layout.preferredWidth)
                layout.enabled = true;

        rtText.ForceMeshUpdate();

        chatLogRect.verticalNormalizedPosition = 0f;
        
        // Plays yapping animation
        rtAni.Play("Yapping");

        // Pops up text on screen
        for (int i = 0; i < rtText.textInfo.characterCount; i++)
        {
            // Reveals current character
            rtText.maxVisibleCharacters += 1;

            // Skips delay for spaces
            if (rtText.textInfo.characterInfo[i].character == ' ')
                continue;
            
            yield return null;
            yield return null;
            
            // Disables auto scroll after first chatLogRect position update
            if (isAutoScrolling)
            {
                chatLogRect.verticalNormalizedPosition = 0f;
                isAutoScrolling = false;
            }

            // Caculates the delay
            adjustedLength = clip.clip.length / Mathf.Abs(audioSource.pitch);
            delay = adjustedLength / clip.text.Length / speed;

            yield return new WaitForSeconds(delay);
        }

        // Returns to idle animation after yapping
        rtAni.Play("Idle");
        // Renables the inputfield
        userInputField.interactable = true;
        userInputField.gameObject.SetActive(true);
    }

    // Open and closes menus
    void MenuOpenClose(CanvasGroup menu, bool isOpen)
    {
        menu.alpha = isOpen ? 1 : 0;
        menu.interactable = isOpen;
        menu.blocksRaycasts = isOpen;
    }

    #endregion

    #region Debug

    // Checks if the person is currently using the inputfield
    bool InputFieldCheck()
    {
        if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null)
            return true;

        return EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != userInputField;
    }

    // Selects a clip from the debug dropdown
    void SelectClip(int value)
    {
        // If a clip is selected
        if (value != 0)
        {
            // Shows that a clip has been selected
            debugClipDropdown.GetComponent<Image>().color = validColor;
            debugClip = clipList[value - 1];
        }
        // If None is selected
        else 
        {
            // Shows that a clip has not been selected
            debugClipDropdown.GetComponent<Image>().color = invalidColor;
            debugClip = null;
        }
    }

    // Updates volume slider
    void UpdateVolume(float value) { audioSource.volume = value; }

    // Updates pitch slider
    void UpdatePitch(float value) { audioSource.pitch = value; }

    // Updates label for delay sliders
    void UpdateLabel(Slider slider, float value) { slider.GetComponentInChildren<TMP_Text>().text = $"{value}"; }

    // Resets sliders back to their default values
    public void ResetSliders()
    {
        volumeSlider.value = 1;
        pitchSlider.value = 1;
        delaySliders[0].value = 1;
        delaySliders[1].value = 15;
    }

    public void ClearChat()
    {
        foreach (Transform child in chatLog)
        {
            Destroy(child.gameObject);
        }
    }

    // Resets clip type filters
    public void ResetClipTypeFilters()
    {
        // Renables all cliptype toggles
        for (int i = 0; i < clipTypeToggles.Length; i++)
            clipTypeToggles[i].isOn = true;
    }

    // Updates clip type tied to that toggle
    public void UpdateClipType(int clipType)
    {
        if (clipTypeToggles[clipType].isOn)
            validClipTypes.Add((ClipType)clipType);
        else
            validClipTypes.Remove((ClipType)clipType);
    }
    
    #endregion
}