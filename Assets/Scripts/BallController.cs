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
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
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
        Vector2 foo = inputAction.ReadValue<Vector2>();
        Vector3 newMovement = new Vector3(foo.x, 0, foo.y);
        rb.AddForce(newMovement * speed);
    }
}
