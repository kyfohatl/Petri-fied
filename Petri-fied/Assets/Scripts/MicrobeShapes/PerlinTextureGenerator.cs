using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTextureGenerator : MonoBehaviour
{
  [SerializeField]
  private int width = 128;
  [SerializeField]
  private int height = 128;

  [SerializeField]
  private float scale = 10f;

  // Start is called before the first frame update
  void OnValidate()
  {
    GetComponent<Renderer>().material.mainTexture = createTexture();
  }

  private Texture2D createTexture()
  {
    Texture2D tex = new Texture2D(width, height);

    for (int w = 0; w < width; w++)
    {
      for (int h = 0; h < height; h++)
      {
        float wPercent = (float)w / width * scale;
        float hPercent = (float)h / height * scale;

        float noise = Mathf.PerlinNoise(wPercent, hPercent);

        tex.SetPixel(w, h, new Color(noise, noise, noise));
      }
    }

    tex.Apply();

    return tex;
  }
}
