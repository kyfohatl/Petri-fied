using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceDetailGenerator
{
  // Returns a psuedo-random perlin value based on the given point
  public float getPerlinValue(Vector3 point, Vector3 offset, float scale)
  {
    Vector3 scaledVector = point * scale + offset;

    // With this approach, we turn the 2D perlin noise into 3d by adding all combinations of 
    // coordinates and getting the average
    float xy = Mathf.PerlinNoise(scaledVector.x, scaledVector.y);
    float xz = Mathf.PerlinNoise(scaledVector.x, scaledVector.z);
    float yx = Mathf.PerlinNoise(scaledVector.y, scaledVector.x);
    float yz = Mathf.PerlinNoise(scaledVector.y, scaledVector.z);
    float zx = Mathf.PerlinNoise(scaledVector.z, scaledVector.x);
    float zy = Mathf.PerlinNoise(scaledVector.z, scaledVector.y);

    return (xy + xz + yx + yz + zx + zy) / 6;
  }
}
