using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;

public class CustomAnimationLibrary : SerializedMonoBehaviour
{
    //[AssetList]
    [SerializeField]
    public List<AnimationClip> animationClips;

    public static CustomAnimationLibrary instance { get; private set; }

    public Dictionary<AnimationClip, Dictionary<string, AnimationCurve>> animationCurves;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else 
            Destroy(this);
    }

    private void Start()
    {
        if(animationCurves == null) {
            Debug.Log("Animation curves not set!");
            return;
        }
        foreach (AnimationClip clip in animationCurves.Keys)
        {
            foreach (string property in animationCurves[clip].Keys)
            {
                Debug.Log(string.Format("{0}, {1}: {2}", clip.name, property, animationCurves[clip][property]));
            }
        }
    }

    public Keyframe[] GetKeyframes(AnimationClip clip, string property) {
        if (!animationCurves.ContainsKey(clip)) {
            Debug.LogError(string.Format("CustomAnimationLibrary does not contain animation clip {0}. Be sure to add it in inspect in edito mode.", clip.name));
            return null;
        } else if (!animationCurves[clip].ContainsKey(property)) {
            Debug.LogError(string.Format("Custom animation clip {0} does not contain attribute {1}.", clip.name, property));
            return null;
        }
        return animationCurves[clip][property].keys;
    }

    public string[] GetPropertyNames(AnimationClip clip)
    {
        if (!animationCurves.ContainsKey(clip))
        {
            Debug.LogError(string.Format("CustomAnimationLibrary does not contain animation clip {0}. Be sure to add it in inspect in edito mode.", clip.name));
            return null;
        }
        return animationCurves[clip].Keys.ToArray();
    }
}
