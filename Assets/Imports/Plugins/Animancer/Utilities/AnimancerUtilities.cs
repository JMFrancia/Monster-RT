// Animancer // Copyright 2019 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Various extension methods and utilities.</summary>
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Adds the specified type of <see cref="AnimancerComponent"/>, links it to the 'animator', and returns it.
        /// </summary>
        public static T AddAnimancerComponent<T>(this Animator animator) where T : AnimancerComponent
        {
            var animancer = animator.gameObject.AddComponent<T>();
            animancer.Animator = animator;
            return animancer;
        }

        /// <summary>
        /// Adds an <see cref="AnimancerComponent"/>, links it to the 'animator', and returns it.
        /// </summary>
        public static AnimancerComponent AddAnimancerComponent(this Animator animator)
        {
            return animator.AddAnimancerComponent<AnimancerComponent>();
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Returns the <see cref="AnimancerComponent"/> on the same <see cref="GameObject"/> as the 'animator' if
        /// there is one. Otherwise this method adds a new one and returns it.
        /// </summary>
        public static AnimancerComponent GetOrAddAnimancerComponent(this Animator animator)
        {
            var animancer = animator.GetComponent<AnimancerComponent>();
            if (animancer != null)
                return animancer;
            else
                return animator.AddAnimancerComponent<AnimancerComponent>();
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Calculates all thresholds using the <see cref="Motion.averageSpeed"/> of each state.
        /// </summary>
#if !UNITY_EDITOR
        [Obsolete(AnimancerPlayable.ProOnlyMessage)]
#endif
        public static void CalculateThresholdsFromAverageVelocityXZ(this MixerState<Vector2> mixer)
        {
            mixer.ValidateThresholdCount();

            var count = mixer.States.Length;
            for (int i = 0; i < count; i++)
            {
                var state = mixer.States[i];
                if (state == null)
                    continue;

                var averageVelocity = state.AverageVelocity;
                mixer.SetThreshold(i, new Vector2(averageVelocity.x, averageVelocity.z));
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Marks the 'target' as dirty.</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional]
        /// If there are multiple components which inherit from <typeparamref name="T"/>, the first one is changed to
        /// the type of the second and any after the first are destroyed. This allows you to change the type without
        /// losing the values of any serialized fields they share.
        /// <para></para>
        /// The 'currentComponent' is used to determine which <see cref="GameObject"/> to examine and the base
        /// component type <typeparamref name="T"/>.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void IfMultiComponentThenChangeType<T>(T currentComponent) where T : MonoBehaviour
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // If there is already another instance of this component on the same object, delete this new instance and
            // change the original's type to match this one.
            var components = currentComponent.GetComponents<T>();
            if (components.Length > 1)
            {
                var oldComponent = components[0];
                var newComponent = components[1];

                if (oldComponent.GetType() != newComponent.GetType())
                {
                    // All we have to do is change the Script field to the new type and Unity will immediately deserialize
                    // the existing data as that type, so any fields shared between both types will keep their data.

                    using (var serializedObject = new UnityEditor.SerializedObject(oldComponent))
                    {
                        var scriptProperty = serializedObject.FindProperty("m_Script");
                        scriptProperty.objectReferenceValue = UnityEditor.MonoScript.FromMonoBehaviour(newComponent);
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                // Destroy all components other than the first (the oldest).
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    var i = 1;
                    for (; i < components.Length; i++)
                    {
                        Object.DestroyImmediate(components[i], true);
                    }
                };
            }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional]
        /// Plays the specified 'clip' if called in Edit Mode and optionally pauses it immediately.
        /// </summary>
        /// <remarks>
        /// Before Unity 2018.3, playing animations in Edit Mode didn't work properly.
        /// </remarks>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void EditModePlay(AnimancerComponent animancer, AnimationClip clip, bool pauseImmediately = true)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                animancer == null || clip == null)
                return;

            // Delay for a frame in case this was called at a bad time (such as during OnValidate).
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                    animancer == null || clip == null)
                    return;

                animancer.Play(clip);

                if (pauseImmediately)
                {
                    animancer.Evaluate();
                    animancer.Playable.PauseGraph();
                }
            };
#endif
        }

        /************************************************************************************************************************/
    }
}
