using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Targeting
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

    public enum OutOfRangeHandlingType
    {
        NONE, // just let us cast outside the range
        CANCEL, //cancel the ability if we try and use it out of range
        CLAMP, // clamp the point to be the closest point that is in range
        CUSTOM, // query the ability for what to do
        MAX
    }

    [System.Serializable]
    public class AbilityTargetingData
    {
        /// <summary>
        /// The type of targeting this data governs
        /// </summary>
        public TargetType targetType = TargetType.MAX;

        /// <summary>
        /// The circle showing max range prefab
        /// </summary>
        public GameObject rangePreviewPrefab = null;

        /// <summary>
        /// The secondary casting icon will change based on the targeting type
        /// </summary>
        public GameObject secondaryPreviewPrefab = null;

        /// <summary>
        /// The distance from the caster that the targeting is valid at. Also used to scale the preview
        /// </summary>
        public float range = -1;

        /// <summary>
        /// Used to determine what to do when the player attempts to use an ability targeted outside of
        /// the range of the ability
        /// </summary>
        public OutOfRangeHandlingType outOfRangeHandlingType = OutOfRangeHandlingType.CLAMP;

        /// <summary>
        /// The scale of the preview, this means different things to different targeting types
        /// </summary>
        public Vector3 previewScale = Vector3.one * -1;

        /// <summary>
        /// The affiliation of the potential targets for entity targeting
        /// </summary>
        public Affiliation affiliation = Affiliation.MAX;
    }

}