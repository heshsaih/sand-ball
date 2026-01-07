using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    private Rigidbody rb;
    [SerializeField]
    private InputAction inputAction;
    private Vector3 currentPos = new Vector3();
    private Vector3 initPos;
    [SerializeField]
    private float tolerance = .1f;
    private int currentPointIndex = 0;
    private int currentShape = 0;
    [SerializeField]
    private int pointCount = 100;
    [SerializeField]
    private int spiralTurns = 6;
    private List<List<Vector3>> points;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        GameObject camera = GameObject.FindWithTag("MainCamera");
        Vector3 startPos = new Vector3(camera.transform.position.x, 0, camera.transform.position.z);
        transform.position = startPos;

        initPos = transform.position;

        points = new List<List<Vector3>>{Spiral(), ArchimedeanSpiral(), foo()};
    }

    private List<Vector3> foo()
    {
        return new List<Vector3>
        {
        new Vector3(0.0f, transform.position.y, -4.0f),   
            
            new Vector3(1.5f, transform.position.y, -3.9f),
            new Vector3(2.5f, transform.position.y, -3.5f),  
            new Vector3(3.2f, transform.position.y, -2.8f), 

            new Vector3(3.5f, transform.position.y, -1.5f),  
            new Vector3(3.6f, transform.position.y, 0.0f),   
            new Vector3(3.5f, transform.position.y, 1.5f),  

            new Vector3(3.3f, transform.position.y, 2.8f),
            new Vector3(2.5f, transform.position.y, 3.5f),

            new Vector3(1.2f, transform.position.y, 3.9f),
            new Vector3(0.0f, transform.position.y, 4.0f),  
            new Vector3(-1.2f, transform.position.y, 3.9f),

            new Vector3(-2.5f, transform.position.y, 3.5f),
            new Vector3(-3.3f, transform.position.y, 2.8f), 

            new Vector3(-3.5f, transform.position.y, 1.5f),
            new Vector3(-3.6f, transform.position.y, 0.0f), 
            new Vector3(-3.5f, transform.position.y, -1.5f),

            new Vector3(-3.2f, transform.position.y, -2.8f), 
            new Vector3(-2.5f, transform.position.y, -3.5f), 
            new Vector3(-1.5f, transform.position.y, -3.9f),
            
            new Vector3(0.0f, transform.position.y, -4.0f)
        };
    }

    private List<Vector3> Spiral()
    {
        var pts = new List<Vector3>(pointCount);

        float xMin = -7f, xMax = 7f;
        float zMin = -4f, zMax = 4f;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float angle = t * 2f * Mathf.PI * spiralTurns;

            float x = Mathf.Lerp(xMin, xMax, t);
            float z = Mathf.Lerp(zMin, zMax, 0.5f * (1f + Mathf.Cos(angle)));

            pts.Add(new Vector3(x, transform.position.y, z));
        }
        return pts;
    }

    public List<Vector3> ArchimedeanSpiral()
    {
        const float maxRadius = 4f;

        List<Vector3> pts = new List<Vector3>(pointCount);

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float r = t * maxRadius;
            float a = t * spiralTurns * Mathf.PI * 2f;

            float x = r * Mathf.Cos(a);
            float z = r * Mathf.Sin(a);

            pts.Add(new Vector3(x, transform.position.y, z));
        }
        return pts;
    }

    void OnEnable()
    {
        inputAction.Enable();
    }

    void OnDisable()
    {
        inputAction.Disable();
    }

    void Update()
    {
        if (currentPointIndex >= points[currentShape].Count)
        {
            currentShape = (currentShape + 1) % points.Count;
            currentPointIndex = 0;
        }

        float distance = Vector3.Distance(currentPos, points[currentShape][currentPointIndex]);
        
        print($"{currentShape}, {currentPointIndex}");

        if (distance < tolerance)
        {
            currentPointIndex++;
            return;
        }

        Vector3 direction = (currentPos - points[currentShape][currentPointIndex]) / distance; 
        rb.AddForce(direction * .75f);


        currentPos = initPos - transform.position;
    }
}