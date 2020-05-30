﻿using UnityEngine;
using UnityEngine.Events;

namespace StoryCore {
    public class UnityEvents : MonoBehaviour {
        public UnityEvent OnEnableEvent;
        public UnityEvent OnDisableEvent;

        private void OnEnable() {
            OnEnableEvent.Invoke();
        }

        private void OnDisable() {
            OnDisableEvent.Invoke();
        }
    }
}