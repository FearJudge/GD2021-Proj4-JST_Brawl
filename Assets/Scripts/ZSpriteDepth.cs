using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSpriteDepth : MonoBehaviour
{
    public struct RenderingSetting
    {
        public SpriteRenderer rend;
        public int initialLayerOrder;
    }

    RenderingSetting[] allSprites;
    public const int LAYERCHANGEONZCHANGE = 50;
    int prevZDepth = 0;
    public int zDepth = 0;
    public Transform root;

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer[] renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        allSprites = new RenderingSetting[renderers.Length];
        for (int a = 0; a < renderers.Length; a++)
        {
            allSprites[a].rend = renderers[a];
            allSprites[a].initialLayerOrder = renderers[a].sortingOrder;
        }
        prevZDepth = zDepth;
        Debug.Log("Found " + allSprites.Length + " skins!");
    }

    // Update is called once per frame
    void Update()
    {
        zDepth = Mathf.RoundToInt(root.transform.position.z * -3.5f);
        if (prevZDepth != zDepth) { UpdateAllZDepths(); }
    }

    void UpdateAllZDepths()
    {
        prevZDepth = zDepth;
        for (int a = 0; a < allSprites.Length; a++)
        {
            allSprites[a].rend.sortingOrder = allSprites[a].initialLayerOrder + (zDepth * LAYERCHANGEONZCHANGE);
        }
    }
}
