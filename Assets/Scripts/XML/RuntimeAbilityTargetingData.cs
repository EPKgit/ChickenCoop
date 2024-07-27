using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Targeting
{
    public class RuntimeAbilityTargetingData
    {
        public Affiliation Affiliation { get => _targetingData.affiliation; }
        public Targeting.TargetType TargetType { get => _targetingData.targetType; }
        public float Range { get => rangeOverride < 0 ? _targetingData.range : rangeOverride; }
        public Vector3 PreviewScale { get => _targetingData.previewScale; }
        public GameObject PreviewPrefab { get => _targetingData.rangePreviewPrefab; }
        public GameObject SecondaryPreviewPrefab { get => _targetingData.secondaryPreviewPrefab; }
        
        
        /// <summary>
        /// The base data that we get from the ability data xml
        /// </summary>
        private AbilityTargetingData _targetingData;

        public Vector2 inputPoint;
        public Vector2 relativeInputDirection;
        public float inputRotationZ;
        public ITargetable inputTarget;

        private GameObject preview;
        private GameObject previewSecondary;

        private float rangeOverride = -1;

        public RuntimeAbilityTargetingData(AbilityTargetingData targetingData)
        {
            _targetingData = targetingData;
            _isInputSet = false;

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

        /// <summary>
        /// Flag that describes if we have recieved input for our ability, this gets reset after every use
        /// </summary>
        public bool isInputSet
        {
            get
            {
                return _isInputSet && !(TargetType == TargetType.ENTITY_TARGETED && inputTarget == null);
            }
            set
            {
                if (!value)
                {
                    inputPoint = Vector2.zero;
                    inputTarget = null;
                }
                _isInputSet = value;
            }
        }
        private bool _isInputSet = false;

        

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
            previewSecondary.transform.localScale = new Vector3(PreviewScale.x, Range, 1);
        }

        void PreviewGround(Ability usedAbility, GameObject user)
        {
            previewSecondary.transform.position = usedAbility.ClampPointWithinRange(user.GetComponent<PlayerInput>().aimPoint);
            Vector2 scale = new Vector2(PreviewScale.x, PreviewScale.y);

            previewSecondary.transform.localScale = new Vector3(scale.x, scale.y, 1);
        }

        void PreviewTargeted(Ability usedAbility, GameObject user)
        {
            Vector2 aim = user.GetComponent<PlayerInput>().aimPoint;
            ITargetable target = Ability.FindTargetable(aim, Affiliation);
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

    }

}