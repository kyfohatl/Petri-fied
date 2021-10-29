using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Creates the core "body" of a microbe. Inspired by Sebastian League's planet generation tutorial
// https://www.youtube.com/watch?v=QN39W020LqU   Title: "Procedural Planets"
public class MicrobeCore : MonoBehaviour
{
  [SerializeField, Range(2, 100)]
  private int faceDetailLevel = 8;
  [SerializeField, HideInInspector]
  // The mesh filters for each face
  private MeshFilter[] filters;
  // Each face of the original cube
  private MicrobeFace[] microbeFaces;
  // The player material
  public Material material;

  // A cube has 6 faces
  private const int numFaces = 6;
  // All directions of each face
  private Vector3[] dirs = { Vector3.right, Vector3.left, Vector3.down, Vector3.up, Vector3.back, Vector3.forward };

  // The scale of the perlin noise used for the generation of surface detail
  [SerializeField]
  private float scale = 15f;
  // The offset on the perlin noise from which the shapes are generated
  [SerializeField]
  private Vector3 offset = new Vector3(5f, 5f, 5f);

  private void OnValidate()
  {
    // Set up the mesh objects
    setupMeshObjects();

    // Now create the mesh for each mesh object
    foreach (var face in microbeFaces)
    {
      face.createMesh(offset, scale);
    }
  }

  // Sets up the mesh objects by adding mesh filters, material and the base empty mesh
  private void setupMeshObjects()
  {
    microbeFaces = new MicrobeFace[numFaces];

    if (filters == null || filters.Length == 0)
    {
      filters = new MeshFilter[numFaces];
    }

    for (int i = 0; i < numFaces; i++)
    {
      if (filters[i] == null)
      {
        // Create the game object which will hold the face mesh
        GameObject face = new GameObject("mesh");
        face.transform.parent = transform;

        // Add a material
        face.AddComponent<MeshRenderer>().sharedMaterial = new Material(material);
        // Add a mesh filter
        filters[i] = face.AddComponent<MeshFilter>();
        // Create the base empty mesh
        filters[i].sharedMesh = new Mesh();
      }

      if (filters[i] != null)
      {
        if (filters[i].sharedMesh == null)
        {
          filters[i].sharedMesh = new Mesh();
        }
        if (filters[i].GetComponent<MeshRenderer>().sharedMaterial == null)
        {
          filters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(material);
        }
      }

      // Now create the face itself
      microbeFaces[i] = new MicrobeFace(filters[i].sharedMesh, faceDetailLevel, dirs[i]);
    }
  }

  public void setMaterial(Material material)
  {
    this.material = new Material(material);

    foreach (MeshFilter face in filters)
    {
      face.GetComponent<MeshRenderer>().sharedMaterial = new Material(material);
    }
  }

  private void Update()
  {
	if (this.gameObject.GetComponent<MeshRenderer>().material != this.material)
    {
      setMaterial(gameObject.GetComponent<MeshRenderer>().material);
    }
  }
}
