using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGen : MonoBehaviour
{
    public IslandGen islandGen;
    [Space]
    public Vector2Int worldSize = new Vector2Int();
    [Space]
    public int numberOfPlates = 3;
    [Space]
    public int numberOfContinents = 1;
    public float percentOfMapX = 0.3f;
    public float percentOfMapY = 0.6f;
    [Space]
    public int numberOfIslands = 30;
    public int islandsSizeMin = 70;
    public int islandsSizeMax = 300;
    [Space]
    [Range(0f, 1f)]
    public float clusterChance = 0.2f;
    public int clusterMaxSize = 6;
    public float clusterSizeMod = 0.3f;
    [Space]
    [Range(0f, 1f)]
    public float massiveIslandChance = 0.2f;
    public float massiveSizeMod = 2.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("g"))
        {
            StartCoroutine(GenerateWorld());
        }
        if (Input.GetKeyDown("c"))
        {
            islandGen.Clear();
        }
    }

    public IEnumerator GenerateWorld()
    {
        Debug.Log("Generating World");
        Vector2Int circleCenter = new Vector2Int(-worldSize.x / 2, -worldSize.y / 2);
        int radius = Mathf.RoundToInt(Vector2Int.Distance(circleCenter, new Vector2Int(0, 0)));
        Vector2 radiusLimits = new Vector2(radius * 0.7f, radius * 1.3f);

        //goto End;

        GameObject centerMarker = new GameObject();
        centerMarker.name = "Center Marker";
        centerMarker.transform.position = new Vector3(circleCenter.x, circleCenter.y, 0f);

        int remainingIslands = numberOfIslands;
        float startAngle = 8f;
        float currentAngle = startAngle;
        float endAngle = 82f;
        int clusterSize = 0;

        for (int k = 0; k < remainingIslands; k++)
        {
            if (Random.Range(0f, 1f) < massiveIslandChance)
            {
                float a = Random.Range(startAngle, endAngle);
                islandGen.Create(
                        new Vector2Int(
                           Mathf.RoundToInt(Random.Range((worldSize.x / -2) + (islandsSizeMax * massiveSizeMod / 2), (worldSize.x / 2) - (islandsSizeMax * massiveSizeMod / 2))),
                           Mathf.RoundToInt(Random.Range((worldSize.y / -2) + (islandsSizeMax * massiveSizeMod / 2), (worldSize.y / 2) - (islandsSizeMax * massiveSizeMod / 2)))),
                        Mathf.RoundToInt(islandsSizeMin * massiveSizeMod),
                        Mathf.RoundToInt(islandsSizeMax * massiveSizeMod),
                        true);
                remainingIslands--;
            }
        }

        while (remainingIslands > 0)
        {
            currentAngle += Random.Range(0f, 15f);

            if (currentAngle >= endAngle)
            {
                currentAngle = startAngle;
            }

            if (Random.Range(0f, 1f) < clusterChance)
            {
                clusterSize = Random.Range(2, clusterMaxSize);
                int cR = Mathf.RoundToInt(Random.Range(radiusLimits.x, radiusLimits.y));
                float cA = currentAngle;
                for (int k = 0; k < clusterSize; k++)
                {
                    islandGen.Create(
                        new Vector2Int(
                            Mathf.RoundToInt(cR * Mathf.Cos(Mathf.Deg2Rad * cA * k)) + circleCenter.x,
                            Mathf.RoundToInt(cR * Mathf.Sin(Mathf.Deg2Rad * cA * k)) + circleCenter.y
                            ),
                        Mathf.RoundToInt(islandsSizeMin * clusterSizeMod),
                        Mathf.RoundToInt(islandsSizeMax * clusterSizeMod),
                        true);
                    cA = currentAngle;
                    cA += Random.Range(-5f, 5f);
                    cR += Random.Range(-islandsSizeMax, islandsSizeMax);
                    yield return null;
                }
                remainingIslands -= clusterSize;
            }
            else
            {
                int cR = Mathf.RoundToInt(Random.Range(radiusLimits.x, radiusLimits.y));
                islandGen.Create(
                        new Vector2Int(
                            Mathf.RoundToInt(cR * Mathf.Cos(Mathf.Deg2Rad * currentAngle)) + circleCenter.x,
                            Mathf.RoundToInt(cR * Mathf.Sin(Mathf.Deg2Rad * currentAngle)) + circleCenter.y
                            ),
                        islandsSizeMin,
                        islandsSizeMax,
                        true);
                remainingIslands--;
            }
            yield return null;
        }
        End:
        Debug.Log("Finished Triggering Landmass Generation");
    }
}
