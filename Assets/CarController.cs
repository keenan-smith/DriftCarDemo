using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float thrust;
    public float torque;
    public Rigidbody rb;
    public Transform t;
    public StreamWriter outfile;
    public StreamWriter outfile2;
    public StreamWriter accelFile;
    public StreamWriter velocityFile;
    public int updateCount = 0;
    public Vector3 lastVelocity;
    public Vector3 acceleration;
    public Vector3 velocity;
    public Vector3 lastPosition;
    public Vector3 relativeVelocity;
    // Start is called before the first frame update

    public static float RandomGaussian(float minValue = -1.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        t = GetComponent<Transform>();
        File.Delete("gaussian_positions.txt");
        File.Delete("positions.txt");
        File.Delete("accelerations.txt");
        File.Delete("velocity.txt");
        outfile = new StreamWriter("gaussian_positions.txt");
        outfile2 = new StreamWriter("positions.txt");
        accelFile = new StreamWriter("accelerations.txt");
        velocityFile = new StreamWriter("velocity.txt");
        outfile.Write("[(0, 0)");
        outfile2.Write("[(0, 0)");
        accelFile.Write("[(0, 0)");
        velocityFile.Write("[(0, 0)");
    }

    private void OnDestroy()
    {
        outfile.Write("]");
        outfile2.Write("]");
        accelFile.Write("]");
        velocityFile.Write("]");
        outfile.Dispose();
        outfile2.Dispose();
        accelFile.Dispose();
        velocityFile.Dispose();
    }

    private void FixedUpdate()
    {
        relativeVelocity = Quaternion.Inverse(transform.rotation) * rb.linearVelocity;
        acceleration = (relativeVelocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = relativeVelocity;
        accelFile.Write($", ({acceleration.x}, {acceleration.z})");
        outfile2.Write($", ({t.position.x}, {t.position.z})");
        if (updateCount % 10 == 0)
        {
            Vector3 curPos = new Vector3(t.position.x + RandomGaussian(), t.position.y, t.position.z + RandomGaussian());
            velocity = (curPos - lastPosition) / (Time.fixedDeltaTime * 10);
            lastPosition = curPos;
            outfile.Write($", ({curPos.x}, {curPos.z})");
            velocityFile.Write($", ({velocity.x}, {velocity.z})");
        }
        updateCount++;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 200), t.position.ToString());
        GUI.Label(new Rect(10, 30, 200, 200), acceleration.ToString());
        GUI.Label(new Rect(10, 50, 200, 200), relativeVelocity.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddRelativeForce(Vector3.forward * thrust);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddRelativeForce(Vector3.back * thrust);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddRelativeTorque(Vector3.down * torque);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddRelativeTorque(Vector3.up * torque);
        }
    }
}
