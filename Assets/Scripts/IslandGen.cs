using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IslandGen : MonoBehaviour
{
    struct NumScramble
    {
        public bool finished;
        bool final;
        int[] nums;
        public int SetupNum(int a_length, int start)
        {
            nums = new int[a_length];
            for (int k = 0; k < a_length; k++)
            {
                nums[k] = k + start;
            }
            finished = false;
            final = false;

            return getNum();
        }
        public int getNum()
        {
            int rand;
            int num;

            if (final)
            {
                finished = true;
                return nums[0];
            }
            else if (nums.Length == 2)
            {
                rand = Random.Range(0, 2);
                num = nums[rand];
                nums[0] = nums[1 - rand];
                final = true;
                return num;
            }

            rand = Random.Range(1, nums.Length) - 1;
            int[] newNums = new int[nums.Length - 1];
            num = nums[rand];

            int incr = 0;
            for (int k = 0; k < nums.Length; k++)
            {
                if (k == rand)
                {
                    incr = -1;
                    continue;
                }

                newNums[k + incr] = nums[k];
            }
            nums = newNums;
            return num;
        }
    }

    public Tilemap tilemap;
    public TileBase landTile;
    public TileBase waterTile;
    public TileBase markerTile;
    [Space]
    public Transform islandLocMarkers;
    [Space]
    public float oneSeedPerDist = 30f;
    [Space]
    public float load = 0f;
    [Space]
    public float seedMinDist = 0.5f;
    public float landChance = 0.75f;
    [Range(0f, 1f)]
    public float adjWaterWt = 0.5f;
    public float adjLandWt = 1.2f;
    [Min(1)]
    public float angleMult = 4f;
    [Space]
    [Range(0f, 1f)]
    public float impactChance = 0.2f;
    public float cleanImpactChance = 0.45f;
    [Space]
    [Range(0f, 100f)]
    public int minBumps = 0;
    [Range(0, 100)]
    public int maxBumps = 5;

    List<int[,]> islands = new List<int[,]>();
    List<int> indexes = new List<int>();
    float[] weights = new float[2];
    TileBase[] tileTypes = new TileBase[2];
    float seedRatio = 0;

    public float loadingSize = 1f;
    public float loadingCurrent = 1f;
    bool loading = false;

    // Start is called before the first frame update
    void Start()
    {
        weights[0] = adjWaterWt;
        weights[1] = adjLandWt;

        tileTypes[0] = waterTile;
        tileTypes[1] = landTile;

        seedRatio = 1f / oneSeedPerDist;

        NumScramble numScramble = new NumScramble();
        int i = 0;
        for (int k = numScramble.SetupNum(10, 0); !numScramble.finished; k = numScramble.getNum())
        {
            i++;
        }
    }
    void Update()
    {
        if (loading)
        {
            load = loadingCurrent / loadingSize;
            if (loadingCurrent >= loadingSize)
            {
                //loading = false;
            }
        }

        if (Input.GetKeyDown("d"))
        {
            Create(new Vector2Int(), 70, 300, false);
        }
    }
    public void Create(Vector2Int a_StartPos, int a_MinimumSize, int a_MaximumSize, bool a_AllowCraters)
    {
        StartCoroutine(CreateIsland(a_StartPos, a_MinimumSize, a_MaximumSize, a_AllowCraters));
    }
    public void Clear()
    {
        loading = true;
        islands = new List<int[,]>();
        indexes = new List<int>();
        tilemap.ClearAllTiles();
    }
    //void GenerateIslandRound()
    //{
    //    int w = Random.Range(MinSize.x, MaxSize.x);
    //    int h = Random.Range(MinSize.y, MaxSize.y);

    //    int coreW = Mathf.RoundToInt((w - 0.1f * w) * 0.5f);
    //    int coreH = Mathf.RoundToInt((h - 0.1f * w) * 0.5f);

    //    int lSide = coreW > coreH ? coreW : coreH;

    //    Vector2Int offset = new Vector2Int(Mathf.RoundToInt(w * 0.25f), Mathf.RoundToInt(h * 0.25f));

    //    islands.Add(new TileBase[w, h]);
    //    indexes.Add(islands.Count - 1);
    //    int ii = indexes.Count - 1;

    //    for (int k = 0; k < w; k++)
    //    {
    //        for (int l = 0; l < h; l++)
    //        {
    //            islands[indexes[ii]][k, l] = waterTile;
    //        }
    //    }

    //    Vector2Int[] seeds = new Vector2Int[Random.Range(2, 5)];
    //    Debug.Log(seeds.Length);
    //    for (int k = 0; k < seeds.Length; k++)
    //    {
    //        seeds[k] = new Vector2Int(Random.Range(0, coreW) + offset.x, Random.Range(0, coreH) + offset.y);
    //        islands[indexes[ii]][seeds[k].x, seeds[k].y] = landTile;
    //    }

    //    foreach (Vector2Int p in seeds)
    //    {
    //        float degIncr;

    //        for (int r = 1; r < lSide / 3 * Random.Range(1f, 1.2f); r++)
    //        {
    //            degIncr = 360f / (angleMult * r);
    //            for (int k = 0; k < angleMult * r; k++)
    //            {
    //                int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + p.x;
    //                int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + p.y;
    //                islands[indexes[ii]][x, y] = landTile;
    //            }

    //        }

    //        int d = GetSmallest(new int[] { p.x, w - p.x, p.y, h - p.y });

    //        NumScramble numScramble = new NumScramble();
    //        for (int r = lSide / 3; r < d - 1; r++)
    //        {
    //            degIncr = 360f / (angleMult * r);
    //            for (int k = numScramble.SetupNum(Mathf.RoundToInt(angleMult * r), 0); !numScramble.finished; k = numScramble.getNum())
    //            {
    //                int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + p.x;
    //                int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + p.y;

    //                if (islands[indexes[ii]][x, y] == landTile)
    //                {
    //                    continue;
    //                }


    //                float landProb =
    //                    weights[islands[indexes[ii]][x - 1, y + 1]] *
    //                    weights[islands[indexes[ii]][x, y + 1]] *
    //                    weights[islands[indexes[ii]][x + 1, y + 1]] *

    //                    weights[islands[indexes[ii]][x - 1, y]] *
    //                    weights[islands[indexes[ii]][x + 1, y]] *

    //                    weights[islands[indexes[ii]][x - 1, y - 1]] *
    //                    weights[islands[indexes[ii]][x, y - 1]] *
    //                    weights[islands[indexes[ii]][x + 1, y - 1]];

    //                islands[indexes[ii]][x, y] = Random.Range(0f, 1f) < landChance * landProb ? landTile : waterTile;
    //            }
    //        }
    //    }


    //    if (Random.Range(0f, 1f) < impactChance)
    //    {
    //        bool cleanImpact = Random.Range(0f, 1f) < cleanImpactChance;
    //        Vector2Int impact = new Vector2Int(Random.Range(0, coreW) + offset.x, Random.Range(0, coreH) + offset.y);
    //        islands[indexes[ii]][impact.x, impact.y] = waterTile;
    //        float degIncr;

    //        for (int r = 1; r < lSide / 3; r++)
    //        {
    //            degIncr = 360f / ((angleMult) * r);
    //            for (int k = 0; k < angleMult * r; k++)
    //            {
    //                if (Random.Range(0f, 1f) > 1f / r * 10f && !cleanImpact)
    //                {
    //                    continue;
    //                }
    //                int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + impact.x;
    //                int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + impact.y;
    //                islands[indexes[ii]][x, y] = waterTile;
    //            }

    //        }
    //    }

    //    if (Random.Range(0f, 1f) < riverChance)
    //    {
    //        int startPoint = Random.Range(0, seeds.Length);
    //        Vector2Int[] posts = { new Vector2Int(0, 0), new Vector2Int(0, h), new Vector2Int(w, h), new Vector2Int(w, 0) };
    //        int i = Random.Range(0, 4);
    //    }

    //    List<TileBase> finalIsland = new List<TileBase>();
    //    List<Vector3Int> positions = new List<Vector3Int>();

    //    for (int k = 0; k < w; k++)
    //    {
    //        for (int l = 0; l < h; l++)
    //        {
    //            finalIsland.Add(islands[indexes[ii]][k, l]);
    //            positions.Add(new Vector3Int(k, l, 0));
    //        }
    //    }

    //    BoundsInt bounds = new BoundsInt(-w / 2, -h / 2, 0, w, h, 1);
    //    tilemap.SetTiles(positions.ToArray(), finalIsland.ToArray());
    //}
    IEnumerator CreateIsland(Vector2Int a_Pos, int a_Min, int a_Max, bool a_Craters)
    {
        int w = Random.Range(a_Min, a_Max);
        int h = w;
        float corePerct = 0.5f;
        int core = Mathf.RoundToInt(w * corePerct);

        float distToCenter = Vector2Int.Distance(new Vector2Int(), new Vector2Int(w / 2, h / 2));
        loadingSize++;

        int offset = Mathf.RoundToInt(w * ((1f - corePerct) / 2));

        islands.Add(new int[w, h]);
        indexes.Add(islands.Count - 1);
        int ii = indexes.Count - 1;
        for (int k = 0; k < w; k++)
        {
            for (int l = 0; l < h; l++)
            {
                islands[indexes[ii]][k, l] = 0;
            }
        }

        List<Vector3Int> seeds = new List<Vector3Int>();
        Vector2Int seedsCenter = new Vector2Int();
        for (int k = 0; k < Mathf.RoundToInt(Random.Range(w * seedRatio, h * seedRatio)); k++)
        {
            Vector3Int temp = new Vector3Int(Random.Range(0, core) + offset, Random.Range(0, core) + offset, Mathf.RoundToInt((core / 3) * Random.Range(0.8f, 1.2f)));
            float distMult = Vector2Int.Distance(new Vector2Int(temp.x, temp.y), new Vector2Int(w / 2, h / 2)) / distToCenter;
            distMult += 0.6f;
            //Debug.Log($"{Vector2Int.Distance(new Vector2Int(temp.x, temp.y), new Vector2Int(w / 2, h / 2))} / {distToCenter} = {distMult}");
            temp.z = Mathf.RoundToInt(temp.z * distMult);
            bool tooClose = false;
            foreach (Vector2Int s in seeds)
            {
                tooClose = Vector2Int.Distance(new Vector2Int(temp.x, temp.y), s) < core * seedMinDist * distMult;
                if (tooClose)
                {
                    break;
                }
            }
            if (tooClose) continue;
            islands[indexes[ii]][temp.x, temp.y] = 1;
            seedsCenter.x += temp.x;
            seedsCenter.y += temp.y;
            seeds.Add(temp);
        }

        seedsCenter.x /= seeds.Count;
        seedsCenter.y /= seeds.Count;

        a_Pos.x -= seedsCenter.x;
        a_Pos.y -= seedsCenter.y;

        //for (int k = 0; k < w; k++)
        //{
        //    islands[indexes[ii]][0, k] = 1;
        //    islands[indexes[ii]][w - 1, k] = 1;
        //    islands[indexes[ii]][k, h - 1] = 1;
        //    islands[indexes[ii]][k, 0] = 1;
        //}

        //for (int k = 0; k < core; k++)
        //{
        //    islands[indexes[ii]][offset, k + offset] = 1;
        //    islands[indexes[ii]][offset + core, k + offset] = 1;
        //    islands[indexes[ii]][k + offset, offset] = 1;
        //    islands[indexes[ii]][k + offset, offset + core] = 1;
        //}

        //goto End;

        float degIncr;
        //var watch = new System.Diagnostics.Stopwatch();
        foreach (Vector3Int p in seeds)
        {
            int perimSeeds = Random.Range(minBumps, maxBumps);
            loadingSize += p.z + perimSeeds;
            //watch.Start();
            for (int r = 1; r < p.z; r++)
            {
                degIncr = 360f / (angleMult * r);
                for (int k = 0; k < angleMult * r; k++)
                {
                    int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + p.x;
                    int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + p.y;

                    try
                    {
                        if (islands[indexes[ii]][x, y] == 1) continue;
                        islands[indexes[ii]][x, y] = 1;
                    }
                    catch { Debug.Log($"Out of Bounds | Array Size = {w}, {h}; Tried Pos = {x}, {y} Seed Radius = {p.z}"); break; }
                }
                loadingCurrent++;
            }
            for (int l = 0; l < perimSeeds; l++)
            {
                bool add = Random.Range(0, 2) == 0;
                float a = Random.Range(0f, 360f);
                Vector2Int pos = new Vector2Int(
                    Mathf.RoundToInt(p.z * Mathf.Cos(Mathf.Deg2Rad * a)) + p.x,
                    Mathf.RoundToInt(p.z * Mathf.Sin(Mathf.Deg2Rad * a)) + p.y);
                try { islands[indexes[ii]][pos.x, pos.y] = add ? 1 : 0; }
                catch { continue; }
                int size = Mathf.RoundToInt(Random.Range(0.8f, 1.2f) * 0.3f * p.z);
                for (int r = 1; r < size; r++)
                {
                    degIncr = 360f / (angleMult * r);
                    for (int k = 0; k < angleMult * r; k++)
                    {
                        int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + pos.x;
                        int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + pos.y;

                        try { islands[indexes[ii]][x, y] = add ? 1 : 0; }
                        catch { continue; }
                    }
                }
                loadingCurrent++;
            }
            //watch.Stop();
            //Debug.Log($"Seed Growth Exexution time is {watch.ElapsedMilliseconds} ms");
            //watch.Reset();
            yield return null;
        }

        yield return null;
        //goto End;
        Coroutine[] routines = new Coroutine[seeds.Count];
        int i = 0;
        foreach (Vector3Int p in seeds)
        {
            int d = GetSmallest(new int[] { p.x, w - p.x, p.y, h - p.y });
            routines[i] = StartCoroutine(DevelopIslandSeed(p, d, ii));
            i++;
        }

        foreach (Coroutine c in routines)
        {
            loadingCurrent++;
            yield return c;
        }

        if (Random.Range(0f, 1f) < impactChance && a_Craters)
        {
            bool cleanImpact = Random.Range(0f, 1f) < cleanImpactChance;
            Vector2Int impact = new Vector2Int(Random.Range(0, core) + offset, Random.Range(0, core) + offset);
            islands[indexes[ii]][impact.x, impact.y] = 0;
            tilemap.SetTile(new Vector3Int(impact.x, impact.y, 1), waterTile);

            for (int r = 1; r < core / 3; r++)
            {
                degIncr = 360f / ((angleMult) * r);
                for (int k = 0; k < angleMult * r; k++)
                {
                    if (Random.Range(0f, 1f) > 1f / r * 10f && !cleanImpact)
                    {
                        continue;
                    }
                    int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + impact.x;
                    int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + impact.y;
                    islands[indexes[ii]][x, y] = 0;
                }
                yield return null;
            }
        }
        End:
        //watch.Start();
        List<TileBase> tiles = new List<TileBase>();
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int k = 0; k < w; k++)
        {
            for (int l = 0; l < h; l++)
            {
                if (islands[indexes[ii]][k, l] == 1)
                {
                    tiles.Add(landTile);
                    positions.Add(new Vector3Int(a_Pos.x + k, a_Pos.y + l, 1));
                }
                else if (islands[indexes[ii]][k, l] == 2)
                {
                    tiles.Add(markerTile);
                    positions.Add(new Vector3Int(a_Pos.x + k, a_Pos.y + l, 1));
                }
            }
        }
        tilemap.SetTiles(positions.ToArray(), tiles.ToArray());
        //watch.Stop();
        //Debug.Log($"Tilemap Placing Exexution time is {watch.ElapsedMilliseconds} ms");

        Debug.Log($"Finished Generation #{ii}");
        RemoveIslandSave(ii);
        AddLocMarker(a_Pos + seedsCenter, ii);
        loadingCurrent++;
    }
    IEnumerator DevelopIslandSeed(Vector3Int a_Seed, int a_Radius, int a_ii)
    {
        NumScramble numScramble = new NumScramble();
        float degIncr;
        bool justWater = false;
        bool justIntersect = true;
        var watch = new System.Diagnostics.Stopwatch();

        //watch.Start();
        loadingSize += a_Radius - 1;

        for (int r = a_Seed.z / 3; r < a_Radius - 1; r++)
        {
            degIncr = 360f / (angleMult * r);
            for (int k = numScramble.SetupNum(Mathf.RoundToInt(angleMult * r), 0); !numScramble.finished; k = numScramble.getNum())
            {
                int x = Mathf.RoundToInt(r * Mathf.Cos(Mathf.Deg2Rad * degIncr * k)) + a_Seed.x;
                int y = Mathf.RoundToInt(r * Mathf.Sin(Mathf.Deg2Rad * degIncr * k)) + a_Seed.y;

                if (islands[indexes[a_ii]][x, y] == 1)
                {
                    continue;
                }
                justIntersect = false;

                float landProb =
                    weights[islands[indexes[a_ii]][x - 1, y + 1]] *
                    weights[islands[indexes[a_ii]][x, y + 1]] *
                    weights[islands[indexes[a_ii]][x + 1, y + 1]] *

                    weights[islands[indexes[a_ii]][x - 1, y]] *
                    weights[islands[indexes[a_ii]][x + 1, y]] *

                    weights[islands[indexes[a_ii]][x - 1, y - 1]] *
                    weights[islands[indexes[a_ii]][x, y - 1]] *
                    weights[islands[indexes[a_ii]][x + 1, y - 1]];

                islands[indexes[a_ii]][x, y] = Random.Range(0f, 1f) < landChance * landProb ? 1 : 0;
                if (islands[indexes[a_ii]][x, y] == 1)
                {
                    justWater = false;
                }
            }
            if (justWater && !justIntersect)
            {
                break;
            }
            justWater = true;
            justIntersect = true;
            loadingCurrent++;
            yield return null;
        }
        //watch.Stop();
        //Debug.Log($"Land Growth Exexution time is {watch.ElapsedMilliseconds} ms");
        yield return null;
    }
    //void GenerateIslandSquare()
    //{
    //    int w = Random.Range(MinSize.x, MaxSize.x);
    //    int h = Random.Range(MinSize.y, MaxSize.y);

    //    int coreW = Mathf.RoundToInt((w - 0.1f * w) * 0.5f);
    //    int coreH = Mathf.RoundToInt((h - 0.1f * w) * 0.5f);

    //    islands.Add(new TileBase[w, h]);
    //    indexes.Add(islands.Count - 1);
    //    int ii = indexes.Count - 1;

    //    for (int k = 0; k < w; k++)
    //    {
    //        for (int l = 0; l < h; l++)
    //        {
    //            islands[indexes[ii]][k, l] = waterTile;
    //        }
    //    }

    //    Vector2Int offset = new Vector2Int(Mathf.RoundToInt(w * 0.25f), Mathf.RoundToInt(h * 0.25f));
    //    for (int k = 0; k < coreW; k++)
    //    {
    //        for (int l = 0; l < coreH; l++)
    //        {
    //            islands[indexes[ii]][k + offset.x, l + offset.y] = landTile;
    //        }
    //    }

    //    float landProb = landChance;
    //    int largestOffset = offset.y < offset.x ? offset.y : offset.x;
    //    for (int i = 0; i < largestOffset; i++)
    //    {
    //        float mod = 1f - (i / largestOffset);
    //        NumScramble nScr = new NumScramble();
    //        for (int k = nScr.SetupNum(coreW + i + i + 2, offset.x - i - 1); !nScr.finished; k = nScr.getNum())
    //        {
    //            float n1;
    //            float n2;
    //            float n3;

    //            int yPos = offset.y - 1 - i;

    //            try { n1 = islands[indexes[ii]][k, yPos + 1] == landTile ? adjLandWt * mod : adjWaterWt; /*islands[islandI][k, offset.y + 1 + i] = markerTile;*/}
    //            catch { n1 = adjWaterWt; }
    //            try
    //            {
    //                n2 = islands[indexes[ii]][k - 1, yPos] == landTile ? adjLandWt * mod : adjWaterWt;
    //                n3 = islands[indexes[ii]][k + 1, yPos] == landTile ? adjLandWt * mod : adjWaterWt;
    //            }
    //            catch { n2 = adjWaterWt; n3 = adjWaterWt; };

    //            if (n1 == adjWaterWt && n2 == adjWaterWt && n3 == adjWaterWt)
    //            {
    //                islands[indexes[ii]][k, yPos] = waterTile;
    //            }
    //            else if (n1 == adjLandWt && n2 == adjLandWt && n3 == adjLandWt)
    //            {
    //                islands[indexes[ii]][k, yPos] = landTile;
    //            }
    //            else
    //            {
    //                islands[indexes[ii]][k, yPos] = Random.Range(0f, 1f) < landChance * n1 * n2 * n3 ? landTile : waterTile;
    //            }

    //            yPos = offset.y + coreH + i;

    //            try { n1 = islands[indexes[ii]][k, yPos - 1] == landTile ? adjLandWt * mod : adjWaterWt; }
    //            catch { n1 = adjWaterWt; }
    //            try
    //            {
    //                n2 = islands[indexes[ii]][k - 1, yPos] == landTile ? adjLandWt * mod : adjWaterWt;
    //                n3 = islands[indexes[ii]][k + 1, yPos] == landTile ? adjLandWt * mod : adjWaterWt;
    //            }
    //            catch { n2 = adjWaterWt; n3 = adjWaterWt; };

    //            if (n1 == adjWaterWt && n2 == adjWaterWt && n3 == adjWaterWt)
    //            {
    //                islands[indexes[ii]][k, yPos] = waterTile;
    //            }
    //            else if (n1 == adjLandWt && n2 == adjLandWt && n3 == adjLandWt)
    //            {
    //                islands[indexes[ii]][k, yPos] = landTile;
    //            }
    //            else
    //            {
    //                islands[indexes[ii]][k, yPos] = Random.Range(0f, 1f) < landChance * n1 * n2 * n3 ? landTile : waterTile;
    //            }

    //            islands[indexes[ii]][k, yPos] = Random.Range(0f, 1f) < landChance * n1 * n2 * n3 ? landTile : waterTile;
    //        }

    //        for (int k = nScr.SetupNum(coreH + i + i + 3, offset.y - i - 1); !nScr.finished; k = nScr.getNum())
    //        {
    //            float n1;
    //            float n2;
    //            float n3;

    //            int xPos = offset.x - 1 - i;

    //            try { n1 = islands[indexes[ii]][xPos + 1, k] == landTile ? adjLandWt * mod : adjWaterWt; }
    //            catch { n1 = adjWaterWt; }
    //            try
    //            {
    //                n2 = islands[indexes[ii]][xPos, k - 1] == landTile ? adjLandWt * mod : adjWaterWt;
    //                n3 = islands[indexes[ii]][xPos, k + 1] == landTile ? adjLandWt * mod : adjWaterWt;
    //            }
    //            catch { n2 = adjWaterWt; n3 = adjWaterWt; }

    //            if (n1 == adjWaterWt && n2 == adjWaterWt && n3 == adjWaterWt)
    //            {
    //                islands[indexes[ii]][xPos, k] = waterTile;
    //            }
    //            else if (n1 == adjLandWt && n2 == adjLandWt && n3 == adjLandWt)
    //            {
    //                islands[indexes[ii]][xPos, k] = landTile;
    //            }
    //            else
    //            {
    //                islands[indexes[ii]][xPos, k] = Random.Range(0f, 1f) < landChance * n1 * n2 * n3 ? landTile : waterTile;
    //            }

    //            xPos = offset.x + coreW + i;

    //            try { n1 = islands[indexes[ii]][xPos - 1, k] == landTile ? adjLandWt * mod : adjWaterWt; }
    //            catch { n1 = adjWaterWt; }
    //            try
    //            {
    //                n2 = islands[indexes[ii]][xPos, k - 1] == landTile ? adjLandWt * mod : adjWaterWt;
    //                n3 = islands[indexes[ii]][xPos, k - 1] == landTile ? adjLandWt * mod : adjWaterWt;
    //            }
    //            catch { n2 = adjWaterWt; n3 = adjWaterWt; }

    //            if (n1 == adjWaterWt && n2 == adjWaterWt && n3 == adjWaterWt)
    //            {
    //                islands[indexes[ii]][xPos, k] = waterTile;
    //            }
    //            else if (n1 == adjLandWt && n2 == adjLandWt && n3 == adjLandWt)
    //            {
    //                islands[indexes[ii]][xPos, k] = landTile;
    //            }
    //            else
    //            {
    //                islands[indexes[ii]][xPos, k] = Random.Range(0f, 1f) < landChance * n1 * n2 * n3 ? landTile : waterTile;
    //            }
    //        }
    //    }

    //    List<TileBase> finalIsland = new List<TileBase>();
    //    List<Vector3Int> positions = new List<Vector3Int>();

    //    for (int k = 0; k < w; k++)
    //    {
    //        for (int l = 0; l < h; l++)
    //        {
    //            finalIsland.Add(islands[indexes[ii]][k, l]);
    //            positions.Add(new Vector3Int(k, l, 0));
    //        }
    //    }

    //    BoundsInt bounds = new BoundsInt(-w / 2, -h / 2, 0, w, h, 1);
    //    tilemap.SetTiles(positions.ToArray(), finalIsland.ToArray());
    //}
    int GetSmallest(int[] a_Nums)
    {
        int s = a_Nums[0];
        foreach (int n in a_Nums)
        {
            if (n < s)
            {
                s = n;
            }
        }
        return s;
    }
    void RemoveIslandSave(int a_IslandIndex)
    {
        islands.RemoveAt(indexes[a_IslandIndex]);
        for (int k = 0; k < indexes.Count; k++)
        {
            if (indexes[k] > indexes[a_IslandIndex])
            {
                indexes[k]--;
            }
        }

        return;
    }
    void AddLocMarker(Vector2Int a_Loc, int a_ID)
    {
        GameObject marker = new GameObject();
        marker.transform.SetParent(islandLocMarkers);
        marker.name = $"Island {a_ID} | {a_Loc}";
        marker.transform.position = new Vector3(a_Loc.x, a_Loc.y, 0f);
    }
    void AddLocMarker(Vector2Int[] a_Seeds, Vector2Int a_Pos, int a_ID)
    {
        Vector3 loc = new Vector3();
        foreach (Vector2Int p in a_Seeds)
        {
            loc.x += p.x;
            loc.y += p.y;
        }

        loc.x /= a_Seeds.Length;
        loc.x += a_Pos.x;
        loc.y /= a_Seeds.Length;
        loc.y += a_Pos.y;
        loc.z = 1f;

        GameObject marker = new GameObject();
        marker.transform.SetParent(islandLocMarkers);
        marker.name = $"Island {a_ID} | {loc}";
        marker.transform.position = loc;
    }
}
