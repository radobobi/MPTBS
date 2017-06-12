using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitmapGeneratorTest : MonoBehaviour
{

    private const int size = 1024;

    // Perlin noise parameters
    public float sampleSize = 1.0f;
    public int octaves = 8;
    public float frequencyBase = 2;
    public float persistence = 1.1f;

    private int perlinIterations1 = 10;

    public float[,] Heightmap;

    // Use this for initialization
    void Start()
    {
        UnityEngine.Random.InitState(0);
        DrawTexture();
    }

    private void DrawTexture()
    {
        Texture2D texture = new Texture2D(size, size);
        GetComponent<Renderer>().material.mainTexture = texture;

        float[,] heightmap = GeneratePerlinHeights2();

        for (int i = 0; i < texture.height; i++)
        {
            for (int j = 0; j < texture.width; j++)
            {
                Color color = HeightToColor(heightmap[i,j]);
                texture.SetPixel(i, j, color);
            }
        }

        Heightmap = heightmap;
        texture.Apply();

        GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.zero);
    }

    private float[,] GeneratePerlinHeights()
    {
        // What does this do?
        float[] seed = new float[octaves];

        for (int i = 0; i < octaves; i++)
        {
            seed[i] = UnityEngine.Random.Range(0.0f, 100.0f);
        }

        float[,] heightmap = new float[size, size];

        // Sample perlin noise to get elevations
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                float elevation = 0.0f;
                float amplitude = Mathf.Pow(persistence, octaves);
                float frequency = 1.0f;
                float maxVal = 0.0f;

                for (int o = 0; o < octaves; o++)
                {
                    float sample = (Mathf.PerlinNoise(
                        seed[o] + (float)i * sampleSize / (size * frequency),
                        seed[o] + (float)j * sampleSize / (size * frequency)) - 0.5f) * amplitude;
                    elevation += sample;
                    maxVal += amplitude;
                    amplitude /= persistence;
                    frequency *= frequencyBase;
                }

                elevation = elevation / maxVal;
                heightmap[i,j] = elevation;
            }
        }

        return heightmap;
    }

    private float[,] GeneratePerlinHeights2()
    {
        // What does this do?
        float[] seed = new float[perlinIterations1];

        for (int i = 0; i < perlinIterations1; i++)
        {
            seed[i] = UnityEngine.Random.Range(0.0f, 100.0f);
        }

        float[,] heightmap = new float[size, size];

        // Sample perlin noise to get elevations
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                float elevation = 0.0f;

                for (int x = 0; x < perlinIterations1; x++)
                {
                    float sample = (Mathf.PerlinNoise(
                        seed[x] + (float)8*i / size,
                        seed[x] + (float)8*j / size) - 0.3f) * Mathf.Pow(0.5f, x);
                    elevation += sample;
                }

                elevation = elevation / (2);
                heightmap[i, j] = elevation;
            }
        }

        return heightmap;
    }

    private Color HeightToColor(float height)
    {
        if (height < 0)
        {
            return Color.Lerp(Color.blue, Color.white, (-height)/0.5f);
        }
        else if (height >= 0 && height < 0.3)
        {
            return Color.Lerp(Color.green, Color.yellow, height/0.3f);
        }
        else if (height >= 0.3 && height < 0.5)
        {
            return Color.Lerp(Color.yellow, Color.red, (height-0.3f)/0.2f);
        }
        else
        {
            return Color.Lerp(Color.white, Color.red, (height-0.5f)/0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
