﻿using StandardAssets.Characters.Common;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
    /// <summary>
    /// Allows an animation state to change the OpenCharacterController's capsule height and offset.
    /// </summary>
    public class ScaleCharacterColliderState : StateMachineBehaviour
    {
        // COMMENT TODO
        enum NormalizedMode
        {
            Once,
            Loop,
            PingPong
        }

        // COMMENT TODO
        const float k_MinHeightScale = 0.0f;

        // COMMENT TODO
        const float k_MaxHeightScale = 2.0f;

        // If true, use the current state's normalizeTime value (ideally, not looped animations),
        // otherwise, we will use our own time.
        [SerializeField]
        bool m_UseNormalizedTime = true;

        // We are using curve only for normalized time from the animation,
        // but for looping animations it is useless (looping collider size...).
        [SerializeField]
        AnimationCurve m_Curve = new AnimationCurve()
        {
            keys = new[]
            {
                new Keyframe(0, 0),
                new Keyframe(1, 1),
            }
        };

        // COMMENT TODO
        [SerializeField]
        NormalizedMode m_AnimationMode;

        // How many seconds it will take to change the height.
        [SerializeField]
        float m_Duration = 1.0f;

        // Adjusted character scale.
        [SerializeField]
        float m_HeightScale = 1.0f;

        // Scale character's collider and offset relative to character's height. 0.5 is center.
        [SerializeField, Range(0.0f, 1.0f)]
        float m_HeightOrigin = 0.5f;

        // if false, we will not restore collider on exit
        // (allows another state behavior to use the last state of the collider)
        [SerializeField]
        bool m_ResetOnExit = true;

        // Using own time as normalized time seems to be looping.
        float m_Time;

        // COMMENT TODO
        float m_CurrentScale;

        // COMMENT TODO
        float m_EntryScale;

        // COMMENT TODO
        float m_EntryOffset;

        // COMMENT TODO
        ControllerAdapter m_Adapter;

        // COMMENT TODO
        OpenCharacterController m_Controller;


        /// <summary>
        /// OnStateEnter is called on any state inside this state machine
        /// </summary>
        /// <param name="animator">Animator to be used</param>
        /// <param name="stateInfo">Info about state</param>
        /// <param name="layerIndex">Index of layer</param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_Adapter == null)
            {
                var characterBrain = animator.GetComponentInChildren<CharacterBrain>();
                m_Adapter = characterBrain != null
                    ? characterBrain.controllerAdapter
                    : null;
                if (m_Adapter == null)
                {
                    return;
                }

                m_Controller = m_Adapter.characterController;
                if (m_Controller == null)
                {
                    return;
                }
            }

            if (m_Controller == null)
            {
                return;
            }

            // if another state does not reset the collider on exit, we can blend collider from it's existing state:
            m_CurrentScale = m_EntryScale = m_Controller.GetHeight() / m_Controller.defaultHeight;
            m_EntryOffset = m_Controller.GetCenter().y;
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_Controller == null)
            {
                return;
            }

            if (m_UseNormalizedTime)
            {
                switch (m_AnimationMode)
                {
                    default:
                        m_Time = stateInfo.normalizedTime;
                        break;
                    case NormalizedMode.Loop:
                        m_Time = Mathf.Repeat(stateInfo.normalizedTime, 1.0f);
                        break;
                    case NormalizedMode.PingPong:
                        m_Time = Mathf.PingPong(stateInfo.normalizedTime, 1.0f);
                        break;
                }

                m_CurrentScale = m_Curve.Evaluate(m_Time);
            }
            else
            {
                m_Time = m_Duration <= 0.0f ? 1.0f : Mathf.Clamp01(m_Time + (Time.deltaTime * (1.0f / m_Duration)));

                m_CurrentScale = Mathf.Lerp(m_EntryScale, m_HeightScale, m_Time);
            }

            var height = m_Controller.ValidateHeight(m_CurrentScale * m_Controller.defaultHeight);

            var offset = m_UseNormalizedTime ? Mathf.Lerp(m_EntryOffset, Center(), 1.0f - m_Curve.Evaluate(m_Time)) : Mathf.Lerp(m_EntryOffset, Center(), 1.0f - m_Time);

            m_Controller.SetHeightAndCenter(height, new Vector3(0.0f, offset, 0.0f), true, false);
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            HandleStateExit();
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            HandleStateExit();
        }

        // COMMENT TODO
        void HandleStateExit()
        {
            if (m_Controller == null)
            {
                return;
            }

            if (m_ResetOnExit)
            {
                m_Controller.ResetHeightAndCenter(true, false);
            }

            m_Time = 0.0f;
        }

        // COMMENT TODO
        void OnValidate()
        {
            m_HeightScale = Mathf.Clamp(m_HeightScale, k_MinHeightScale, k_MaxHeightScale);
            m_Duration = Mathf.Max(0.0f, m_Duration);
        }

        // COMMENT TODO
        float Center()
        {
            var charHeight = m_Controller.defaultHeight;

            // collider is centered on character:
            var center = (m_CurrentScale * charHeight) * 0.5f;
            var offset = center * (m_HeightOrigin - 0.5f);
            return center + offset;
        }
    }
}
