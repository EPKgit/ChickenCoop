using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityTargetingData
{
    public enum TargetType
    {
        NONE,
        LINE_TARGETED,
        ENTITY_TARGETED,
        GROUND_TARGETED,
        CUSTOM_TARGETING,
        MAX,
    }
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
    /// The scale perpindicular to the cast direction that the ability covers e.g. the width of the line
    /// </summary>
    public float perpendicularScale;

    public void Preview(Ability usedAbility, GameObject user)
    {
        preview = usedAbility.GameObjectManipulation(rangePreviewPrefab, true);
        switch (targetType)
        {
            case TargetType.NONE:
            {

            } break;
            case TargetType.LINE_TARGETED:
            {
                previewSecondary = usedAbility.GameObjectManipulation(secondaryPreviewPrefab, true);
                PreviewLine(usedAbility, user);
            } break;
            case TargetType.ENTITY_TARGETED:
            {

            } break;
            case TargetType.GROUND_TARGETED:
            {

            } break;
            case TargetType.CUSTOM_TARGETING:
            {

            } break;
        }
    }

    public void PreviewUpdate(Ability usedAbility, GameObject user)
    {
        preview.transform.localScale = new Vector3(range, range, 1);
        preview.transform.position = user.transform.position;
        switch (targetType)
        {
            case TargetType.NONE:
            {

            }
            break;
            case TargetType.LINE_TARGETED:
            {
                PreviewLine(usedAbility, user);
            }
            break;
            case TargetType.ENTITY_TARGETED:
            {

            }
            break;
            case TargetType.GROUND_TARGETED:
            {

            }
            break;
            case TargetType.CUSTOM_TARGETING:
            {

            }
            break;
        }
    }

    public void Cleanup(Ability usedAbility, GameObject user)
    {
        if(preview)
        {
            usedAbility.GameObjectManipulation(preview, false);
            preview = null;
        }
        if(secondaryPreviewPrefab)
        {
            usedAbility.GameObjectManipulation(previewSecondary, false);
            previewSecondary = null;
        }
    }

    void PreviewLine(Ability usedAbility, GameObject user)
    {
        previewSecondary.transform.position = user.transform.position;
        Vector3 direction = user.GetComponent<PlayerInput>().GetAimPoint() - (Vector2)user.transform.position;
        bool right = Vector3.Dot(Vector3.right, direction) > 0;
        if (!right)
        {
            previewSecondary.transform.rotation = Quaternion.Euler(0, 0, Vector3.Angle(Vector3.up, direction));
        }
        else
        {
            previewSecondary.transform.rotation = Quaternion.Euler(0, 0, 360 - Vector3.Angle(Vector3.up, direction));
        }
        previewSecondary.transform.localScale = new Vector3(perpendicularScale, range, 1);
    }

}
