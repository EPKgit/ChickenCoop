using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Targeting
{
    public class RuntimeAbilityTargetingData
    {
        public Affiliation Affiliation { get => _targetingData.affiliation; }
        public Targeting.TargetType TargetType { get => _targetingData.targetType; }
        public float BaseRange { get => _targetingData.range; }
        public OutOfRangeHandlingType OutOfRangeHandlingType { get => _targetingData.outOfRangeHandlingType; }
        public Vector3 PreviewScale { get => _targetingData.previewScale; }
        public GameObject PreviewPrefab { get => _targetingData.rangePreviewPrefab; }
        public GameObject SecondaryPreviewPrefab { get => _targetingData.secondaryPreviewPrefab; }
        
        
        /// <summary>
        /// The base data that we get from the ability data xml
        /// </summary>
        private AbilityTargetingData _targetingData;

        public float Range { get => rangeOverride < 0 ? _targetingData.range : rangeOverride; }
        public Vector2 inputPoint { get; private set; } = Vector2.negativeInfinity;
        public Vector2 inputDirectionNormalized { get; private set; }
        public float inputMagnitude { get; private set; }
        /// <summary>
        /// Rotation in counterclockwise degrees from 2d up
        /// </summary>
        public float inputRotationZ { get; private set; }
        public ITargetable inputTarget { get; private set; }

        private GameObject preview;
        private GameObject previewSecondary;

        private float rangeOverride = -1;
        private float previewScaleMultiplier = 1;

        public RuntimeAbilityTargetingData(AbilityTargetingData targetingData)
        {
            _targetingData = targetingData;
            ResetInput();

            if(targetingData.rangePreviewPrefab == null)
            {
                switch(targetingData.targetType) 
                {
                    case TargetType.LINE_TARGETED:
                    case TargetType.GROUND_TARGETED:
                    case TargetType.ENTITY_TARGETED:
                    default:
                        targetingData.rangePreviewPrefab = InGameUIManager.instance.rangeIndicatorPrefab;
                        break;
                }
            }
            if(targetingData.secondaryPreviewPrefab == null)
            {
                switch(targetingData.targetType)
                {
                    case TargetType.LINE_TARGETED:
                        targetingData.secondaryPreviewPrefab = InGameUIManager.instance.arrowIndicatorPrefab;
                        break;
                    case TargetType.GROUND_TARGETED:
                        targetingData.secondaryPreviewPrefab = InGameUIManager.instance.circleIndicatorPrefab; 
                        break;
                    case TargetType.ENTITY_TARGETED:
                        targetingData.secondaryPreviewPrefab = InGameUIManager.instance.crosshairIndicatorPrefab;
                        break;
                    default:
                        targetingData.secondaryPreviewPrefab = null;
                        break;
                }
            }
        }

        public bool IsInputSet()
        {
            return (TargetType == TargetType.ENTITY_TARGETED && inputTarget != null) ||
                   (inputPoint != Vector2.negativeInfinity);
        }
        public void ResetInput()
        {
            inputPoint = Vector2.negativeInfinity;
            inputTarget = null;
        }        

        public void Preview(Ability usedAbility, GameObject user)
        {
            switch(TargetType)
            {
                case TargetType.LINE_TARGETED:
                case TargetType.GROUND_TARGETED:
                case TargetType.ENTITY_TARGETED:
                {
                    preview = usedAbility.GameObjectManipulation(PreviewPrefab, true);
                } break;
            }

            switch (TargetType)
            {
                case TargetType.LINE_TARGETED:
                {
                    previewSecondary = usedAbility.GameObjectManipulation(SecondaryPreviewPrefab, true);
                    PreviewLine(usedAbility, user);
                } break;
                case TargetType.ENTITY_TARGETED:
                {
                    previewSecondary = usedAbility.GameObjectManipulation(SecondaryPreviewPrefab, true);
                    PreviewTargeted(usedAbility, user);
                } break;
                case TargetType.GROUND_TARGETED:
                {
                    previewSecondary = usedAbility.GameObjectManipulation(SecondaryPreviewPrefab, true);
                    PreviewGround(usedAbility, user);
                } break;
                // case TargetType.CUSTOM_TARGETING:
                // {

                // } break;
            }
        }

        public void PreviewUpdate(Ability usedAbility, GameObject user)
        {
            if (TargetType != TargetType.NONE)
            {
                preview.transform.localScale = new Vector3(Range, Range, 1);
                preview.transform.position = user.transform.position;
            }
            switch (TargetType)
            {
                case TargetType.LINE_TARGETED:
                    {
                        PreviewLine(usedAbility, user);
                    }
                    break;
                case TargetType.ENTITY_TARGETED:
                    {
                        PreviewTargeted(usedAbility, user);
                    }
                    break;
                case TargetType.GROUND_TARGETED:
                    {
                        PreviewGround(usedAbility, user);
                    }
                    break;
                    // case TargetType.CUSTOM_TARGETING:
                    // {

                    // }
                    // break;
            }
        }

        public void Cleanup(Ability usedAbility, GameObject user)
        {
            if (preview)
            {
                usedAbility.GameObjectManipulation(preview, false);
                preview = null;
            }
            if (previewSecondary)
            {
                usedAbility.GameObjectManipulation(previewSecondary, false);
                previewSecondary = null;
            }
        }

        public void SetRangeOverride(float f)
        {
            rangeOverride = f;
        }

        public void ResetRangeOverride()
        {
            rangeOverride = -1;
        }
        
        public void SetPreviewScaleMultiplier(float f)
        {
            previewScaleMultiplier = f;
        }

        public void ResetPreviewScaleMultiplier()
        {
            previewScaleMultiplier = 1;
        }

        void PreviewLine(Ability usedAbility, GameObject user)
        {
            previewSecondary.transform.position = user.transform.position;
            Vector3 direction = user.GetComponent<PlayerInput>().aimPoint - (Vector2)user.transform.position;
            bool right = Vector3.Dot(Vector3.right, direction) > 0;
            if (!right)
            {
                previewSecondary.transform.rotation = Quaternion.Euler(0, 0, Vector3.Angle(Vector3.up, direction));
            }
            else
            {
                previewSecondary.transform.rotation = Quaternion.Euler(0, 0, 360 - Vector3.Angle(Vector3.up, direction));
            }
            previewSecondary.transform.localScale = new Vector3(PreviewScale.x, Range, 1) * previewScaleMultiplier;
        }

        void PreviewGround(Ability usedAbility, GameObject user)
        {
            Vector2 aimPoint = user.GetComponent<PlayerInput>().aimPoint;
            switch (usedAbility.targetingData.OutOfRangeHandlingType)
            {
                case OutOfRangeHandlingType.NONE:
                    previewSecondary.transform.position = aimPoint;
                    break;
                case OutOfRangeHandlingType.CANCEL:
                    float magnitude = (aimPoint - (Vector2)user.transform.position).magnitude;
                    previewSecondary.SetActive(magnitude < usedAbility.targetingData.Range);
                    previewSecondary.transform.position = aimPoint;
                    break;
                case OutOfRangeHandlingType.CLAMP:
                    previewSecondary.transform.position = usedAbility.ClampPointWithinRange(aimPoint);
                    break;
                case OutOfRangeHandlingType.CUSTOM:
                    throw new System.NotImplementedException();
            }
            Vector2 scale = new Vector2(PreviewScale.x, PreviewScale.y) * previewScaleMultiplier;

            previewSecondary.transform.localScale = new Vector3(scale.x, scale.y, 1);
        }

        void PreviewTargeted(Ability usedAbility, GameObject user)
        {
            Vector2 aim = user.GetComponent<PlayerInput>().aimPoint;
            ITargetable target = Ability.FindTargetable(aim, Affiliation);
            previewSecondary.transform.localScale = new Vector3(previewScaleMultiplier, previewScaleMultiplier, previewScaleMultiplier);
            if (target != null)
            {
                previewSecondary.GetComponent<SpriteRenderer>().color = Color.green;
                previewSecondary.transform.position = (Vector3)Lib.FindDownwardsInTree<Collider2D>(target.Attached)?.bounds.center;// + target.Attached.transform.position;
            }
            else
            {
                previewSecondary.GetComponent<SpriteRenderer>().color = Color.red;
                previewSecondary.transform.position = aim;
            }
        }

        public bool SetRuntimeData(Vector2 inputPoint, Vector2 originatorPosition)
        {
            inputDirectionNormalized = inputPoint - originatorPosition;
            inputMagnitude = inputDirectionNormalized.magnitude;
            inputDirectionNormalized = inputDirectionNormalized.normalized;

            if (inputMagnitude >= Range)
            {
                switch(OutOfRangeHandlingType)
                {
                    case OutOfRangeHandlingType.CANCEL:
                        return false;
                    case OutOfRangeHandlingType.CLAMP:
                        inputPoint = originatorPosition + inputDirectionNormalized * Range;
                        break;
                    case OutOfRangeHandlingType.NONE:
                        //intentionally do nothing
                        break;
                    case OutOfRangeHandlingType.CUSTOM:
                        throw new System.NotImplementedException();
                }
            }
            this.inputPoint = inputPoint;
            inputRotationZ = Vector2.SignedAngle(Vector2.up, inputDirectionNormalized);
            inputRotationZ = inputRotationZ < 0 ? inputRotationZ + 360.0f : inputRotationZ;
            inputTarget = Ability.FindTargetable(inputPoint, Affiliation);
            return IsInputSet();
        }

    }

}