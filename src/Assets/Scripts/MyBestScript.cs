using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
struct Particle2
{
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 col;
}

public class MyBestScript : MonoBehaviour
{
    [SerializeField] private Material material;

    [SerializeField] private ComputeShader computeShader;

    private int updateKernel;
    private ComputeBuffer buffer;

    static int THREAD_NUM = 64;
    static int PARTICLE_NUM = ((65536 + THREAD_NUM - 1) / THREAD_NUM) * THREAD_NUM;

    void OnEnable()
    {
        buffer = new ComputeBuffer(
            PARTICLE_NUM,
            Marshal.SizeOf(typeof(Particle2)),
            ComputeBufferType.Default);


        var initKernel = computeShader.FindKernel("initialize");
        computeShader.SetBuffer(initKernel, "Particles", buffer);
        computeShader.Dispatch(initKernel, PARTICLE_NUM / THREAD_NUM, 1, 1);

        updateKernel = computeShader.FindKernel("update");
        computeShader.SetBuffer(updateKernel, "Particles", buffer);

        material.SetBuffer("Particles", buffer);
    }

    private void OnDisable()
    {
        buffer.Release();
    }
    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(updateKernel, PARTICLE_NUM / THREAD_NUM, 1, 1);
    }

    void OnRenderObject()
    {

        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, PARTICLE_NUM);
    }
}
