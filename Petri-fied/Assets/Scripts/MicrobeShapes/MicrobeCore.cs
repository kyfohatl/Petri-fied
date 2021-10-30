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
  // The power-up outline material
  public Material powerUpMaterial;
  // When true, means that the microbe is locked onto
  public bool isLockedOn = false;
  // The number of power-ups currently active on the microbe
  private int numActivePowerUps = 0;

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

  private void Start()
  {
    // Set up the mesh objects
    setupMeshObjects();

    // Now create the mesh for each mesh object
    createFaceMeshes();

    // Revert material properties to default
    RevertMaterialProperty("invincible");
    RevertMaterialProperty("speed");
    RevertMaterialProperty("magnet");
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

        // Add a mesh filter
        filters[i] = face.AddComponent<MeshFilter>();
        // Add a material
        if (isLockedOn)
        {
          face.AddComponent<MeshRenderer>().sharedMaterial = new Material(lockOnMaterial);
        }
        else if (numActivePowerUps > 0)
        {
          face.AddComponent<MeshRenderer>().sharedMaterial = new Material(powerUpMaterial);
        }
        else
        {
          face.AddComponent<MeshRenderer>().sharedMaterial = new Material(defaultMaterial);
        }

        // Create the base empty mesh
        filters[i].sharedMesh = new Mesh();
      }
      else
      {
        if (filters[i].sharedMesh == null)
        {
          filters[i].sharedMesh = new Mesh();
        }

        if (isLockedOn)
        {
          filters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(lockOnMaterial);
        }
        else if (numActivePowerUps > 0)
        {
          filters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(powerUpMaterial);
        }
        else
        {
          //Debug.Log(defaultMaterial);
          //Debug.Log(filters[i].GetComponent<MeshRenderer>().sharedMaterial);
          if (defaultMaterial == null)
          {
            Debug.Log(this.gameObject.tag);
          }
          filters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(defaultMaterial);
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

  // Reduces the active power-up count on the microbe, and if reaching zero will and not locked on will
  // revert the material to default
  public void ReduceActivePowerUps()
  {
    if (numActivePowerUps > 0)
    {
      numActivePowerUps--;
    }

    Debug.Log("isLockedOn: " + isLockedOn + " numActv: " + numActivePowerUps);

    if (!isLockedOn && numActivePowerUps <= 0)
    {
      // Redo mesh setup
      setupMeshObjects();
      // Now recreate the face meshes
      createFaceMeshes();
    }
  }

  public void IncreaseActivePowerUps()
  {
    if (!isLockedOn && numActivePowerUps == 0)
    {
      // Increment power-up count before redo-ing the meshes to make sure the material changes
      numActivePowerUps++;
      // Redo mesh setup
      setupMeshObjects();
      // Now recreate the face meshes
      createFaceMeshes();
    }
    else
    {
      numActivePowerUps++;
    }
  }

  public void ChangeMaterialProperty(string prop)
  {
    switch (prop)
    {
      case "invincible":
        // Revert other powerup visual effects in case they are running
        RevertMaterialProperty("speed");
        RevertMaterialProperty("magnet");

        // Now set the new powerup effects
        defaultMaterial.SetFloat("_IsInvincible", 1.0f);
        lockOnMaterial.SetFloat("_IsInvincible", 1.0f);
        powerUpMaterial.SetFloat("_IsInvincible", 1.0f);
        setupMeshObjects();
        createFaceMeshes();
        break;

      case "speed":
        // Revert other powerup visual effects in case they are running
        RevertMaterialProperty("invincible");
        RevertMaterialProperty("magnet");

        // Now set the new powerup effects
        defaultMaterial.SetFloat("_IsSpeed", 1.0f);
        lockOnMaterial.SetFloat("_IsSpeed", 1.0f);
        powerUpMaterial.SetFloat("_IsSpeed", 1.0f);
        setupMeshObjects();
        createFaceMeshes();
        break;

      case "magnet":
        // Revert other powerup visual effects in case they are running
        RevertMaterialProperty("speed");
        RevertMaterialProperty("invincible");

        // Now set the new powerup effects
        defaultMaterial.SetFloat("_IsMagnet", 1.0f);
        lockOnMaterial.SetFloat("_IsMagnet", 1.0f);
        powerUpMaterial.SetFloat("_IsMagnet", 1.0f);
        setupMeshObjects();
        createFaceMeshes();
        break;

      default:
        Debug.Log("Given property type does not exist: " + prop);
        break;
    }
  }

  public void RevertMaterialProperty(string prop)
  {
    switch (prop)
    {
      case "invincible":
        defaultMaterial.SetFloat("_IsInvincible", 0.0f);
        lockOnMaterial.SetFloat("_IsInvincible", 0.0f);
        powerUpMaterial.SetFloat("_IsInvincible", 0.0f);
        setupMeshObjects();
        createFaceMeshes();
        break;

      case "speed":
        defaultMaterial.SetFloat("_IsSpeed", 0.0f);
        lockOnMaterial.SetFloat("_IsSpeed", 0.0f);
        powerUpMaterial.SetFloat("_IsSpeed", 0.0f);
        setupMeshObjects();
        createFaceMeshes();
        break;

      case "magnet":
        defaultMaterial.SetFloat("_IsMagnet", 0.0f);
        lockOnMaterial.SetFloat("_IsMagnet", 0.0f);
        powerUpMaterial.SetFloat("_IsMagnet", 0.0f);
        setupMeshObjects();
        createFaceMeshes();
        break;

      default:
        Debug.Log("Given property type does not exist: " + prop);
        break;
    }
  }

  public void ChangeLockOnOutlineColor(Color newColor)
  {
    lockOnMaterial.SetColor("_OutlineColor", newColor);
    setupMeshObjects();
    createFaceMeshes();
  }

  public void RevertAllMaterialProperties()
  {
    numActivePowerUps = 0;
    RevertMaterialProperty("invincible");
    RevertMaterialProperty("speed");
    RevertMaterialProperty("magnet");
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
