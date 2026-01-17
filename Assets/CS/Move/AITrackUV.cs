using UnityEngine;

public class AITrackUV : MonoBehaviour
{
    public Renderer rend;                 
    public int trackMaterialIndex = 0;    
    public Transform tankRoot;            
    public float uvSpeed = 1.5f;
    public float moveThreshold = 0.02f;   
    public bool scrollV = true;           

    MaterialPropertyBlock mpb;
    Vector3 prevPos;
    float offset;

    static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST"); // URP Lit

    void Awake()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (tankRoot == null) tankRoot = transform.root;

        mpb = new MaterialPropertyBlock();
        prevPos = tankRoot.position;
    }

    void Update()
    {
        float speed = (tankRoot.position - prevPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        prevPos = tankRoot.position;

        if (speed > moveThreshold)
        {
            offset = Mathf.Repeat(offset + uvSpeed * Time.deltaTime, 1f);
        }

        rend.GetPropertyBlock(mpb, trackMaterialIndex);

        // _BaseMap_ST = (tilingX, tilingY, offsetX, offsetY)
        Vector4 st = scrollV
            ? new Vector4(1f, 1f, 0f, offset)
            : new Vector4(1f, 1f, offset, 0f);

        mpb.SetVector(BaseMapST, st);
        rend.SetPropertyBlock(mpb, trackMaterialIndex);
    }
}
