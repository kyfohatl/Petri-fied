using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One face of the core of a microbe
public class MicrobeFace
{
  // The default displacement of points from the center of the mesh
  private const float defaultDisplacement = 0.5f;

  private Mesh mesh;
  // Controls the number of triangles for this face. i.e. the number of vertices per side of the face
  private int detailLevel;
  Vector3 upDir;
  Vector3 axisB;
  Vector3 axisC;

  private SurfaceDetailGenerator detailGenerator;

  public MicrobeFace(Mesh mesh, int detailLevel, Vector3 upDir)
  {
    this.mesh = mesh;
    this.detailLevel = detailLevel;
    this.upDir = upDir;
    this.detailGenerator = new SurfaceDetailGenerator();

    // The second axis is just the upDir axis but with the coordinates shuffled
    axisB = new Vector3(upDir.z, upDir.x, upDir.y);
    // The third axis can now be obtained by taking the cross product of the other two axis
    axisC = Vector3.Cross(upDir, axisB);
  }

  // Creates a mesh for the face
  public void createMesh(Vector3 offset, float scale)
  {
    // detailLevel is the number of vertices per side, so the total number of vertices on the quad face 
    // is detailLevel^2
    Vector3[] vertices = new Vector3[detailLevel * detailLevel];
    // Triangles index array.
    // A quad broken up into detailLevel^2 vertices will result in (detailLevel - 1)^2 mini-quads.
    // Each of these mini-quads has 2 triangles, each of which has 3 vertices, hence we must 
    // multiply by * 2 * 3 = * 6
    int[] triangles = new int[6 * (detailLevel - 1) * (detailLevel - 1)];

    // Step along each axis of the face, splitting the face up into mini-quads and then splitting 
    // them up into triangles for the face mesh
    int trianglesIdx = 0;
    for (int i = 0; i < detailLevel; i++)
    {
      // Calculate how far down one side we are
      float iProgress = (float)i / (detailLevel - 1);
      for (int j = 0; j < detailLevel; j++)
      {
        // Calculate how far down the other side we are
        float jProgress = (float)j / (detailLevel - 1);

        // We want to find the point in space on our cube where we are currently at
        // The point along the side would be (start + sideProgress * length) * axisDir
        // Each side of the cube goes from -1 to 1 on its axis --> That's a length of 2 and 
        // the start is at -1. Hence we get the formula below
        Vector3 axisBProgress = (jProgress * 2 - 1) * axisB;
        Vector3 axisCProgress = (iProgress * 2 - 1) * axisC;

        // Now we want to move 1 unit in the up direction to get the face itself
        Vector3 pointOnCubeFace = axisBProgress + axisCProgress + upDir;
        // To "inflate" the cube into a sphere, all we have to do is to ensure that all points are 
        // equidistant from the centre of the cube
        // To do this, we can normalize the resulting point
        Vector3 pointOnBaseSphereFace = pointOnCubeFace.normalized;

        // Add noise to make the microbe surface appear more natural
        float displacement = detailGenerator.getPerlinValue(pointOnBaseSphereFace, offset, scale);

        float displacementDif = displacement - defaultDisplacement;

        if (displacementDif > 0.165)
        {
          displacementDif *= 2f;
        }
        else
        {
          displacementDif /= 4f;
        }

        Vector3 pointOnSphereFace = pointOnBaseSphereFace; //* (defaultDisplacement + displacementDif);

        // The index of the vertex
        // Every iteration of i has detailLevel indices, so we must add i * detailLevel
        int vertexArrayIdx = j + i * detailLevel;
        // Place the vertex in the vertex array
        vertices[vertexArrayIdx] = pointOnSphereFace;

        // Add the two triangles for the mini-quad
        // Note that the vertices on the final row/col should not be the start of new triangles 
        // Since those triangles would fall outside the mini-quad
        if (i != detailLevel - 1 && j != detailLevel - 1)
        {
          // The vertices for the triangle need to be defined in a clockwise order to show in 
          // the right way
          // The first vertex is the current one, then the next one is one row down + 1
          // i.e. currentIndex + detailLevel + 1
          // The third vertex is directly below, so one row down
          // i.e. currentIndex + detail level
          triangles[trianglesIdx++] = vertexArrayIdx;
          triangles[trianglesIdx++] = vertexArrayIdx + detailLevel + 1;
          triangles[trianglesIdx++] = vertexArrayIdx + detailLevel;

          // Same idea for the second triangle, but in the other direction
          triangles[trianglesIdx++] = vertexArrayIdx;
          triangles[trianglesIdx++] = vertexArrayIdx + 1;
          triangles[trianglesIdx++] = vertexArrayIdx + detailLevel + 1;
        }
      }
    }

    // Finally adding the vertices and triangles to the mesh
    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
  }
}
