﻿using CoreUtils;
using UnityEngine;
using VRTK;

namespace StoryCore {
    public class StoryCoreSettings : Singleton<StoryCoreSettings> {

        #region Global Settings

        private static string kUseDebugInInkKey = "StoryCore-UseDebugInInk";
        private static string kForceSimulationKey = "StoryCore-ForceSimulationMode";
        private const string kEnableLoggingKey = "StoryCore-LoggingEnabled";
        private const string kDebugJumpKey = "StoryCore-DebugJump";

        private static int s_UseDebugInInk = -1;
        private static int s_ForceSimulationMode = -1;
        private static int s_EnableLogging = -1;
        private static string s_DebugJump;

        public static bool UseDebugInInk {
            get {
                if (s_UseDebugInInk < 0) {
                    s_UseDebugInInk = PlayerPrefs.GetInt(kUseDebugInInkKey, 0);
                }

                return s_UseDebugInInk > 0;
            }
            set {
                if (s_UseDebugInInk >= 0 && s_UseDebugInInk > 0 == value) {
                    return;
                }

                s_UseDebugInInk = value ? 1 : 0;
                PlayerPrefs.SetInt(kUseDebugInInkKey, s_UseDebugInInk);
            }
        }

        public static bool ForceSimulationMode {
            get {
                if (s_ForceSimulationMode < 0) {
                    s_ForceSimulationMode = PlayerPrefs.GetInt(kForceSimulationKey, 0);
                }

                return s_ForceSimulationMode > 0;
            }
            set {
                if (s_ForceSimulationMode >= 0 && s_ForceSimulationMode > 0 == value) {
                    return;
                }

                s_ForceSimulationMode = value ? 1 : 0;
                PlayerPrefs.SetInt(kForceSimulationKey, s_ForceSimulationMode);
            }
        }

        public static bool EnableLogging {
            get {
                if (s_EnableLogging < 0) {
                    s_EnableLogging = PlayerPrefs.GetInt(kEnableLoggingKey, 1);
                }

                return s_EnableLogging > 0;
            }
            set {
                if (s_EnableLogging == (value ? 1 : 0)) {
                    return;
                }

                s_EnableLogging = value ? 1 : 0;
                PlayerPrefs.SetInt(kEnableLoggingKey, s_EnableLogging);
            }
        }

        public static string DebugJump {
            get => s_DebugJump ?? (s_DebugJump = PlayerPrefs.GetString(kDebugJumpKey, ""));
            set {
                if (s_DebugJump == value) {
                    return;
                }

                s_DebugJump = value;
                PlayerPrefs.SetString(kDebugJumpKey, s_DebugJump);
            }
        }

        #endregion

        [SerializeField] private VRTK_SDKManager m_Manager;
        [SerializeField] private StoryTeller m_StoryTeller;

#if UNITY_EDITOR
        public VRTK_SDKManager Manager {
            get => m_Manager;
            set => m_Manager = value;
        }

        public StoryTeller StoryTeller {
            get => m_StoryTeller;
            set => m_StoryTeller = value;
        }

        private void Awake() {
            if (!Application.isEditor) {
                return;
            }

            CheckForceSimulationMode();
        }

        private void CheckForceSimulationMode() {
            if (!ForceSimulationMode) {
                return;
            }
            Manager.forceSimulatorInEditor = true;
        }

        private void Reset() {
            Manager = FindObjectOfType<VRTK_SDKManager>();
            StoryTeller = FindObjectOfType<StoryTeller>();
        }
#endif
    }
}