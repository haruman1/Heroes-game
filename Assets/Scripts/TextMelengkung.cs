using UnityEngine;
using TMPro;

[ExecuteAlways]
public class CurvedText : MonoBehaviour
{
    private TMP_Text m_TextComponent;
    public float radius = 50f;
    public float curveSpeed = 10f;

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    void OnTextChanged(Object obj)
    {
        if (obj == null || obj == m_TextComponent)
        {
            CurveText();
        }
    }

    void Start()
    {
        CurveText();
    }

    void CurveText()
    {
        m_TextComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Hitung kurva menggunakan sinus/cosinus
            Vector3 offset = new Vector3(Mathf.Sin(i / curveSpeed) * radius, Mathf.Cos(i / curveSpeed) * radius, 0);
            
            // Terapkan ke vertex teks
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}