using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    private int previousPointIndex = 0;
    private int currentShape = 0;
    [SerializeField]
    private int pointCount = 100;
    [SerializeField]
    private int spiralTurns = 6;
    private List<List<Vector3>> points;

    private Thread thread = null;
    private UdpClient udpClient;
    private List<Vector3> recievedPoints;
    private bool shouldDrawReceived = false;

    private float transformY;

    private System.Random random = new System.Random();
    private float velocityModifier = 1.0f;
    private Vector3 randomOffset;

    void getRandomOffset(float mod = 1)
    {
        randomOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0,
                                   UnityEngine.Random.Range(-1f, 1f));
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        GameObject camera = GameObject.FindWithTag("MainCamera");
        Vector3 startPos = new Vector3(camera.transform.position.x, 0,
                                       camera.transform.position.z);
        transform.position = startPos;

        initPos = transform.position;
        transformY = transform.position.y;

        points = new List<List<Vector3>> { ArchimedeanSpiral(), Spiral(), foo() };
        getRandomOffset();
    }

    private void listen()
    {

        udpClient = new UdpClient(42069);

        while (true)
        {
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref ip);
                string encoded = Encoding.UTF8.GetString(data);
                parseReceivedPoints(encoded);
                if (!shouldDrawReceived)
                {
                    previousPointIndex = currentPointIndex;
                }
                shouldDrawReceived = true;
                getRandomOffset(2);
                currentPointIndex = 0;
            }
            catch (Exception e)
            {
                print(e.ToString());
            }
        }
    }

    private void parseReceivedPoints(string data)
    {
        string[] foo = string.Join("", data.Split('[', ']', ' ')).Split(',');
        List<Vector3> result = new List<Vector3>();

        try
        {
            for (int i = 0; i < 41; i += 2)
            {
                float x =
                    float.Parse(foo[i], CultureInfo.InvariantCulture.NumberFormat);
                float y =
                    float.Parse(foo[i + 1], CultureInfo.InvariantCulture.NumberFormat);
                result.Add(new Vector3(x, transformY, y));
            }
            float returnX =
                float.Parse(foo[0], CultureInfo.InvariantCulture.NumberFormat);
            float returnY =
                float.Parse(foo[1], CultureInfo.InvariantCulture.NumberFormat);
            result.Add(new Vector3(returnX, transformY, returnY));
            recievedPoints = result;
        }
        catch (Exception e)
        {
            print($"Failed to parse received data: ${e}");
            return;
        }
    }

    private List<Vector3> foo()
    {
        return new List<Vector3> { new Vector3(0.0f, transformY, -4.0f),
                               new Vector3(1.5f, transformY, -3.9f),
                               new Vector3(2.5f, transformY, -3.5f),
                               new Vector3(3.2f, transformY, -2.8f),
                               new Vector3(3.5f, transformY, -1.5f),
                               new Vector3(3.6f, transformY, 0.0f),
                               new Vector3(3.5f, transformY, 1.5f),
                               new Vector3(3.3f, transformY, 2.8f),
                               new Vector3(2.5f, transformY, 3.5f),

                               new Vector3(1.2f, transformY, 3.9f),
                               new Vector3(0.0f, transformY, 4.0f),
                               new Vector3(-1.2f, transformY, 3.9f),

                               new Vector3(-2.5f, transformY, 3.5f),
                               new Vector3(-3.3f, transformY, 2.8f),
                               new Vector3(-3.5f, transformY, 1.5f),
                               new Vector3(-3.6f, transformY, 0.0f),
                               new Vector3(-3.5f, transformY, -1.5f),

                               new Vector3(-3.2f, transformY, -2.8f),
                               new Vector3(-2.5f, transformY, -3.5f),
                               new Vector3(-1.5f, transformY, -3.9f),
                               new Vector3(0.0f, transformY, -4.0f) };
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

            pts.Add(new Vector3(x, transformY, z));
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

            pts.Add(new Vector3(x, transformY, z));
        }
        return pts;
    }

    void OnEnable()
    {
        inputAction.Enable();
        thread = new Thread(new ThreadStart(listen));
        thread.IsBackground = true;
        thread.Start();
    }

    void OnDisable()
    {
        inputAction.Disable();
        thread.Abort();
        thread = null;
    }

    void Update()
    {
        List<Vector3> currentShapePoints;
        if (shouldDrawReceived && recievedPoints != null)
        {
            currentShapePoints = recievedPoints;
        }
        else
        {
            currentShapePoints = points[currentShape];
        }

        if (currentPointIndex >= currentShapePoints.Count)
        {
            if (shouldDrawReceived)
            {
                shouldDrawReceived = false;
                currentPointIndex = previousPointIndex;
            }
            else
            {
                currentShape = (currentShape + 1) % points.Count;
                currentPointIndex = 0;
            }
        }

        Vector3 effectivePos = currentShapePoints[currentPointIndex];
        if (!shouldDrawReceived)
        {
            effectivePos += randomOffset;
        }

        float distance = Vector3.Distance(currentPos, effectivePos);

        if (distance < tolerance)
        {
            currentPointIndex++;
            if (!shouldDrawReceived)
            {
                rb.angularDamping = UnityEngine.Random.Range(.5f, 2f);
                velocityModifier = UnityEngine.Random.Range(1f, 1.5f);
                getRandomOffset(.1f);
            }
            else
            {
                rb.angularDamping = speed * 2;
                velocityModifier = 2f;
            }
            return;
        }

        Vector3 direction = (currentPos - effectivePos) / distance;
        Vector3 result = direction * speed * velocityModifier;
        rb.AddForce(result);
        print(result);

        currentPos = initPos - transform.position;
    }
}
