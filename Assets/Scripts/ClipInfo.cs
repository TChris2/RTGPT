using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clip Info
[CreateAssetMenu(fileName = "ClipInfo", menuName = "Scriptable Objects/ClipInfo")]
public class ClipInfo : ScriptableObject
{
    public AudioClip clip;
    // Text of the clip
    public string text;
    // Type of clip
    public List<ClipType> clipTypes;
}

public enum ClipType
{
    SFW,
    NSFW,
    Noises,
    Toad,
    HurtSFX,
    Youtube,
    Backseating,
    Brainrot,
    Cursed,
    DriftKing
}