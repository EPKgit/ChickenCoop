using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumberPopup : Poolable
{
    public TextMeshProUGUI tmp;

    public float risingSpeed = 0.8f;
    public float fadeOutTime = 2.0f;
    public AnimationCurve alphaCurve;
    public AnimationCurve scaleCurve;

    private RectTransform rectTransform;
    private Vector3 startPosition;
    private float currentTime;

    private void Awake()
    {
        if (tmp != null)
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }
        rectTransform = tmp.rectTransform;
        //InitializeVertexAnims();
    }

    public override void Reset()
    {
        currentTime = 0;
    }

    public void Setup(float dmg, Vector3 position)
    {
        const float COLOR_RANGE = 0.2f;
        tmp.text = string.Format("{0,0:F2}", dmg);
        float t = Mathf.Clamp(dmg, 0, DamageNumbersManager.instance.highEndDamage) / DamageNumbersManager.instance.highEndDamage;
        t = Mathf.Clamp(t, 0, 1 - COLOR_RANGE);
        Color low = DamageNumbersManager.instance.lowDamageColor;
        Color high = DamageNumbersManager.instance.highDamageColor;
        tmp.colorGradient = new VertexGradient(Color.Lerp(low, high, t), Color.Lerp(low, high, t + COLOR_RANGE / 2), Color.Lerp(low, high, t + COLOR_RANGE / 2), Color.Lerp(low, high, t + COLOR_RANGE)); //3 = bottom right
        rectTransform.localPosition = Vector3.zero;
        startPosition = position;
        transform.position = Camera.main.WorldToScreenPoint(startPosition);
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > fadeOutTime)
        {
            DestroySelf();
        }
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alphaCurve.Evaluate(currentTime / fadeOutTime));
        rectTransform.localScale = Vector3.one * scaleCurve.Evaluate(currentTime / fadeOutTime) * DamageNumbersManager.instance.sizeModifier;
        startPosition += Vector3.up * Time.deltaTime * risingSpeed;
        transform.position = Camera.main.WorldToScreenPoint(startPosition);

    }

    //void OnEnable()
    //{
    //    // Subscribe to event fired when text object has been regenerated.
    //    TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    //}

    //void OnDisable()
    //{
    //    TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    //}

    //void ON_TEXT_CHANGED(Object obj)
    //{
    //    if (obj == tmp)
    //        hasTextChanged = true;
    //}



    //private struct VertexAnim
    //{
    //    public float angleRange;
    //    public float angle;
    //    public float speed;
    //}

    //public float AngleMultiplier = 1.0f;
    //public float SpeedMultiplier = 1.0f;
    //public float CurveScale = 1.0f;

    //private bool hasTextChanged = true;
    //private int loopCount = 0;
    //VertexAnim[] vertexAnim;

    //private void InitializeVertexAnims()
    //{
    //    tmp.ForceMeshUpdate();

    //    // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
    //    vertexAnim = new VertexAnim[1024];
    //    for (int i = 0; i < 1024; i++)
    //    {
    //        vertexAnim[i].angleRange = Random.Range(10f, 25f);
    //        vertexAnim[i].speed = Random.Range(1f, 3f);
    //    }
    //}

    //private void AnimateVertices()
    //{

    //    Matrix4x4 matrix;
    //    TMP_TextInfo textInfo = tmp.textInfo;

    //    // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
    //    TMP_MeshInfo[] cachedMeshInfo = tmp.textInfo.CopyMeshInfoVertexData();

    //    // Get new copy of vertex data if the text has changed.
    //    if (hasTextChanged)
    //    {
    //        // Update the copy of the vertex data for the text object.
    //        cachedMeshInfo = tmp.textInfo.CopyMeshInfoVertexData();

    //        hasTextChanged = false;
    //    }

    //    int characterCount = tmp.textInfo.characterCount;

    //    // If No Characters then just yield and wait for some text to be added
    //    if (characterCount == 0)
    //    {
    //        return;
    //    }


    //    for (int i = 0; i < characterCount; i++)
    //    {
    //        TMP_CharacterInfo charInfo = tmp.textInfo.characterInfo[i];

    //        // Skip characters that are not visible and thus have no geometry to manipulate.
    //        if (!charInfo.isVisible)
    //            continue;

    //        // Retrieve the pre-computed animation data for the given character.
    //        VertexAnim vertAnim = vertexAnim[i];

    //        // Get the index of the material used by the current character.
    //        int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

    //        // Get the index of the first vertex used by this text element.
    //        int vertexIndex = textInfo.characterInfo[i].vertexIndex;

    //        // Get the cached vertices of the mesh used by this text element (character or sprite).
    //        Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

    //        // Determine the center point of each character at the baseline.
    //        //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
    //        // Determine the center point of each character.
    //        Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

    //        // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
    //        // This is needed so the matrix TRS is applied at the origin for each character.
    //        Vector3 offset = charMidBasline;

    //        Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

    //        destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
    //        destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
    //        destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
    //        destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

    //        vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
    //        Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

    //        matrix = Matrix4x4.TRS(jitterOffset * CurveScale, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

    //        destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
    //        destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
    //        destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
    //        destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

    //        destinationVertices[vertexIndex + 0] += offset;
    //        destinationVertices[vertexIndex + 1] += offset;
    //        destinationVertices[vertexIndex + 2] += offset;
    //        destinationVertices[vertexIndex + 3] += offset;

    //        vertexAnim[i] = vertAnim;
    //    }

    //    // Push changes into meshes
    //    for (int i = 0; i < textInfo.meshInfo.Length; i++)
    //    {
    //        textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
    //        tmp.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
    //    }

    //    loopCount += 1;
    //}
}
