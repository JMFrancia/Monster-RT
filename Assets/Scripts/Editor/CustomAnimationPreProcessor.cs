using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public static class CustomAnimationPreProcessor
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        PreprocessCustomAnimationLibrary();
    }

    static void PreprocessCustomAnimationLibrary()
    {
        CustomAnimationLibrary library = CustomAnimationLibrary.instance;

        Dictionary<AnimationClip, Dictionary<string, AnimationCurve>> animationCurves = new Dictionary<AnimationClip, Dictionary<string, AnimationCurve>>();
        List<AnimationClip> animationClips = library.animationClips;

        //For each clip in the library
        foreach (AnimationClip clip in animationClips)
        {
            animationCurves[clip] = new Dictionary<string, AnimationCurve>();
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
            //For each curve binding of the clip
            foreach (EditorCurveBinding binding in curveBindings)
            {
                //Set animationCurves such that the curve can be searched by clip and property name
                animationCurves[clip][binding.propertyName] = AnimationUtility.GetEditorCurve(clip, binding);
            }
        }
        library.animationCurves = animationCurves;
     }
}
