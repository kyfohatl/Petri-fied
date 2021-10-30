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
  // The default material
  public Material defaultMaterial;
  // The lock-on material
  public Material lockOnMaterial;
  // When true, means that the microbe is locked onto
  public bool isLockedOn = false;

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
    createFaceMeshes();
  }

  // private void Start()
  // {
  //   this.shader = Shader.Find("Unlit/SurfaceNoise");
  //   this.material = new Material(shader);

  //   if (this.GetComponent<MeshRenderer>() == null)
  //   {
  //     this.gameObject.AddComponent<MeshRenderer>().material = this.material;
  //   }

  //   // Set up the mesh objects
  //   setupMeshObjects();

  //   // Now create the mesh for each mesh object
  //   foreach (var face in microbeFaces)
  //   {
  //     face.createMesh(offset, scale);
  //   }
  // }

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

        // Add a mesh filter
        filters[i] = face.AddComponent<MeshFilter>();
        // Add a material
        if (isLockedOn)
        {
          face.AddComponent<MeshRenderer>().sharedMaterial = new Material(lockOnMaterial);
        }
        else
        {
          face.AddComponent<MeshRenderer>().sharedMaterial = new Material(defaultMaterial);
        }

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
          if (isLockedOn)
          {
            filters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(lockOnMaterial);
          }
          else
          {
            filters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(defaultMaterial);
          }
        }
      }

      // Now create the face itself
      microbeFaces[i] = new MicrobeFace(filters[i].sharedMesh, faceDetailLevel, dirs[i]);
    }
  }

  // Creates a mesh for each of the faces of the microbe
  private void createFaceMeshes()
  {
    foreach (var face in microbeFaces)
    {
      face.createMesh(offset, scale);
    }
  }

  // If the given status reflects a change in the lock-on status, the material of the mesh will be switched 
  // and the face meshes recreated
  public void SetLockedOnStatus(bool newStatus)
  {
    if (isLockedOn != newStatus)
    {
      // The lock-on status has changed
      isLockedOn = newStatus;
      // Redo mesh setup
      setupMeshObjects();
      // Now recreate the face meshes
      createFaceMeshes();
    }
  }

  // public void setMaterial(Material material)
  // {
  //   this.material = new Material(material);

  //   foreach (MeshFilter face in filters)
  //   {
  //     face.GetComponent<MeshRenderer>().sharedMaterial = new Material(material);
  //   }
  // }

  // private void Update()
  // {
  //   MeshRenderer mr = this.gameObject.GetComponent<MeshRenderer>();
  //   if (mr != null && mr.material != this.material)
  //   {
  //     setMaterial(gameObject.GetComponent<MeshRenderer>().material);
  //   }
  // }
}
