using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteAnimationClip
{
    public string clipName;
    public Sprite[] frames;
    public float frameRate = 12f;
    public bool loop = true;
}

[CreateAssetMenu(fileName = "NewAnimationData", menuName = "Character/Character Animation Data")]
public class CharacterAnimationData : ScriptableObject
{
    public List<SpriteAnimationClip> clips = new List<SpriteAnimationClip>();
}
