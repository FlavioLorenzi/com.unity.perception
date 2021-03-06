﻿using System;
using System.Collections.Generic;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Scenarios;

namespace UnityEngine.Perception.Randomization.Randomizers
{
    /// <summary>
    /// Derive Randomizer to implement systems that randomize GameObjects and/or simulation properties.
    /// </summary>
    /// <remark>
    /// Known issue:
    /// https://issuetracker.unity3d.com/issues/serializereference-non-serialized-initialized-fields-lose-their-values-when-entering-play-mode
    /// </remark>
    [Serializable]
    public abstract class Randomizer
    {
        bool m_PreviouslyEnabled;

        [HideInInspector, SerializeField] internal bool collapsed;

        /// <summary>
        /// Enabled Randomizers are updated, disabled Randomizers are not.
        /// </summary>
        [field: SerializeField, HideInInspector] public bool enabled { get; set; } = true;

        /// <summary>
        /// Returns the scenario containing this Randomizer
        /// </summary>
        public ScenarioBase scenario => ScenarioBase.activeScenario;

        /// <summary>
        /// Retrieves the RandomizerTagManager of the scenario containing this Randomizer
        /// </summary>
        public RandomizerTagManager tagManager => RandomizerTagManager.singleton;

        internal IEnumerable<Parameter> parameters
        {
            get
            {
                var fields = GetType().GetFields();
                foreach (var field in fields)
                {
                    if (!field.IsPublic || !field.FieldType.IsSubclassOf(typeof(Parameter)))
                        continue;
                    var parameter = (Parameter)field.GetValue(this);
                    if (parameter == null)
                    {
                        parameter = (Parameter)Activator.CreateInstance(field.FieldType);
                        field.SetValue(this, parameter);
                    }
                    yield return parameter;
                }
            }
        }

        /// <summary>
        /// OnCreate is called when the Randomizer is added or loaded to a scenario
        /// </summary>
        protected virtual void OnCreate() { }

        /// <summary>
        /// OnIterationStart is called at the start of a new scenario iteration
        /// </summary>
        protected virtual void OnIterationStart() { }

        /// <summary>
        /// OnIterationEnd is called the after a scenario iteration has completed
        /// </summary>
        protected virtual void OnIterationEnd() { }

        /// <summary>
        /// OnScenarioComplete is called the after the entire scenario has completed
        /// </summary>
        protected virtual void OnScenarioComplete() { }

        /// <summary>
        /// OnStartRunning is called on the first frame a Randomizer is enabled
        /// </summary>
        protected virtual void OnStartRunning() { }

        /// <summary>
        /// OnStartRunning is called on the first frame a disabled Randomizer is updated
        /// </summary>
        protected virtual void OnStopRunning() { }

        /// <summary>
        /// OnUpdate is executed every frame for enabled Randomizers
        /// </summary>
        protected virtual void OnUpdate() { }

        internal virtual void Create()
        {
            OnCreate();
        }

        internal virtual void IterationStart()
        {
            OnIterationStart();
        }

        internal virtual void IterationEnd()
        {
            OnIterationEnd();
        }

        internal virtual void ScenarioComplete()
        {
            OnScenarioComplete();
        }

        internal void Update()
        {
            if (enabled)
            {
                if (!m_PreviouslyEnabled)
                {
                    m_PreviouslyEnabled = true;
                    OnStartRunning();
                }
                OnUpdate();
            }
            else if (m_PreviouslyEnabled)
            {
                m_PreviouslyEnabled = false;
                OnStopRunning();
            }
        }
    }
}
