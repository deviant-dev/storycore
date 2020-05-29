using UnityEditor;

namespace StoryCore {
    // NOTE: This CustomEditor allows the ScriptableObjects referenced in ChoiceManager to be expandable in the Inspector.
    [CustomEditor(typeof(ChoiceManager))]
    public class ChoiceManagerEditor : UnityEditor.Editor { }
}