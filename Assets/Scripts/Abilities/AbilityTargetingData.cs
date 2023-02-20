using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Targeting
{

    [System.Serializable]
    public class AbilityTargetingData
    {
        public enum TargetType
        {
            NONE,
            LINE_TARGETED,
            ENTITY_TARGETED,
            GROUND_TARGETED,
            // CUSTOM_TARGETING,
            MAX,
        }

        public Vector2 inputPoint;
        public float inputRotationZ;
        public ITargetable inputTarget;

        /// <summary>
        /// Flag that describes if we have recieved input for our ability, this gets reset after every use
        /// </summary>
        public bool isInputSet
        {
            get
            {
                return _isInputSet && !(targetType == TargetType.ENTITY_TARGETED && inputTarget == null);
            }
            set
            {
                if(!value)
                {
                    inputPoint = Vector2.zero;
                    inputTarget = null;
                }
                _isInputSet = value;
            }
        }
        private bool _isInputSet = false;

        /// <summary>
        /// The type of targeting this data governs
        /// </summary>
        public TargetType targetType;

        /// <summary>
        /// The circle showing max range prefab
        /// </summary>
        public GameObject rangePreviewPrefab;

        /// <summary>
        /// The secondary casting icon will change based on the targeting type
        /// </summary>
        public GameObject secondaryPreviewPrefab;

        private GameObject preview;
        private GameObject previewSecondary;

        /// <summary>
        /// The distance from the caster that the targeting is valid at. Also used to scale the preview
        /// </summary>
        public float range;

        /// <summary>
        /// The scale of the preview, this means different things to different targeting types
        /// </summary>
        public Vector3 previewScale;

        /// <summary>
        /// The affiliation of the potential targets for entity targeting
        /// </summary>
        public Targeting.Affiliation affiliation;

        public void Preview(Ability usedAbility, GameObject user)
        {
            if (targetType != TargetType.NONE)
            {
                preview = usedAbility.GameObjectManipulation(rangePreviewPrefab.Equals(null) ? InGameUIManager.instance.rangeIndicatorPrefab : rangePreviewPrefab, true);
            }
            switch (targetType)
            {
                
                case TargetType.LINE_TARGETED:
                {
                    previewSecondary = usedAbility.GameObjectManipulation
                    (
                        secondaryPreviewPrefab.Equals(null) ? InGameUIManager.instance.arrowIndicatorPrefab : secondaryPreviewPrefab, 
                        true
                    );
                    PreviewLine(usedAbility, user);
                }
                break;
                case TargetType.ENTITY_TARGETED:
                {
                    previewSecondary = usedAbility.GameObjectManipulation
                    (
                        secondaryPreviewPrefab.Equals(null) ? InGameUIManager.instance.crosshairIndicatorPrefab : secondaryPreviewPrefab,
                        true
                    );
                    PreviewTargeted(usedAbility, user);
                }
                break;
                case TargetType.GROUND_TARGETED:
                {
                    previewSecondary = usedAbility.GameObjectManipulation
                    (
                        secondaryPreviewPrefab.Equals(null) ? InGameUIManager.instance.circleIndicatorPrefab : secondaryPreviewPrefab, 
                        true
                    );
                    PreviewGround(usedAbility, user);
                }
                break;
                // case TargetType.CUSTOM_TARGETING:
                // {

                // }
                // break;
            }
        }

        public void PreviewUpdate(Ability usedAbility, GameObject user)
        {
            if (targetType != TargetType.NONE)
            {
                preview.transform.localScale = new Vector3(range, range, 1);
                preview.transform.position = user.transform.position;
            }
            switch (targetType)
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
            previewSecondary.transform.localScale = new Vector3(previewScale.x, range, 1);
        }

        void PreviewGround(Ability usedAbility, GameObject user)
        {
            previewSecondary.transform.position = usedAbility.ClampPointWithinRange(user.GetComponent<PlayerInput>().aimPoint);
            Vector2 scale = new Vector2(previewScale.x, previewScale.y);
            if(usedAbility.aoe > 0)
            {
                scale = new Vector2(usedAbility.aoe, usedAbility.aoe);
            }

            previewSecondary.transform.localScale = new Vector3(scale.x, scale.y, 1);
        }

        void PreviewTargeted(Ability usedAbility, GameObject user)
        {
            Vector2 aim = user.GetComponent<PlayerInput>().aimPoint;
            ITargetable target = Ability.FindTargetable(aim, affiliation);
            if(target != null)
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