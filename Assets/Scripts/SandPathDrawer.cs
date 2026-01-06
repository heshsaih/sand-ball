using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class SandPathDrawer : MonoBehaviour
{

    private Vector2Int position = new Vector2Int(256, 256);
    [SerializeField]
    private float spotSize = 5f;
    private SandController sandController;
    private GameObject[] groundObjects;
    [SerializeField]
    private ComputeShader sandShader;
    [SerializeField]
    private RenderTexture sandRT;

    void Awake()
    {
        groundObjects = GameObject.FindGameObjectsWithTag("Ground");
    }

    private void GetPosition()
    {
        float scaleX = sandController.transform.localScale.x;
        float scaleY = sandController.transform.localScale.z;//???

        float sandPosX = sandController.transform.position.x;
        float sandPosY = sandController.transform.position.z;//???

        int posX = sandRT.width / 2 - (int)(((transform.position.x - sandPosX) * sandRT.width / 2) / scaleX);
        int posY = sandRT.width / 2 - (int)(((transform.position.z - sandPosY) * sandRT.width / 2) / scaleY);

        position = new Vector2Int(posX, posY);
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < groundObjects.Length; i++)
        {
            if (Vector3.Distance(groundObjects[i].transform.position, transform.position) > spotSize * 5f) continue;
            sandController = groundObjects[i].GetComponent<SandController>();
            sandRT = sandController.sandRT;
            sandShader = sandController.sandShader;
            GetPosition();
            DrawSpot();
        }
    }

    private void DrawSpot()
    {
        if (sandRT == null || sandShader == null) return;

        int kernelHandle = sandShader.FindKernel(ShaderProperties.drawSpotKernel);
        sandShader.SetTexture(kernelHandle, ShaderProperties.sandImage, sandRT);
        sandShader.SetFloat(ShaderProperties.colorValue, 0);
        sandShader.SetFloat(ShaderProperties.resolution, sandRT.width);
        sandShader.SetFloat(ShaderProperties.posX, position.x);
        sandShader.SetFloat(ShaderProperties.posY, position.y);
        sandShader.SetFloat(ShaderProperties.spotSize, spotSize);

        sandShader.Dispatch(kernelHandle, sandRT.width / 8, sandRT.height / 8, 1);
    }
}
