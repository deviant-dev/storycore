using System;
using CoreUtils.Editor;
using StoryCore.Utils;
using UnityEditor;

namespace CoreUtils.GameEvents {
    [CustomEditor(typeof(OnGameEventSetAnimator))]
    public class OnGameEventSetAnimatorEditor : Editor<OnGameEventSetAnimator> {
        public override void OnInspectorGUI() {
            switch (Target.Type) {
                case OnGameEventSetAnimator.ParamType.Bool:
                    DrawPropertiesExcluding(serializedObject, "m_IntValue", "m_FloatValue");
                    break;
                case OnGameEventSetAnimator.ParamType.Int:
                    DrawPropertiesExcluding(serializedObject, "m_BoolValue", "m_FloatValue");
                    break;
                case OnGameEventSetAnimator.ParamType.Float:
                    DrawPropertiesExcluding(serializedObject, "m_BoolValue", "m_IntValue");
                    break;
                case OnGameEventSetAnimator.ParamType.Trigger:
                    DrawPropertiesExcluding(serializedObject, "m_BoolValue", "m_IntValue", "m_FloatValue");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}