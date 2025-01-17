using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.Commands {
    [CreateAssetMenu(menuName = "Commands/Command Wait")]
    public class CommandWaitHandler : CommandHandler {
        [SerializeField] private float m_DefaultWait = 1;

        public override DelaySequence Run(ScriptCommandInfo info) {
            float duration = m_DefaultWait;

            if (info.Params.Length > 0) {
                if (float.TryParse(info.Params[0], out float paramDuration)) {
                    duration = paramDuration;
                } else {
                    Debug.LogWarningFormat(this, "Couldn't convert {0} to a duration. (command = {1})", info.Params[0], info);
                }
            }

            return Delay.For(duration, this);
        }
    }
}