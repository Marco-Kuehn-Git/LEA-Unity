using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public enum TILE_TYPE {
    WATER,
    GRASS,
    STONE,
    TREE,
    CHEST,
    BUSH,
    CRYSTAL,
    FIRE,
    VASE,
    WOOD_Wall,
    WOOD_BLOCK,
    WATER_L,
    WATER_R,
    WATER_T,
    WATER_B,
    WATER_TL,
    WATER_TR,
    WATER_BL,
    WATER_BR,
    WATER_C_TL,
    WATER_C_TR,
    WATER_C_BL,
    WATER_C_BR,
    SAND
}

public class TileController : MonoBehaviour{

    [SerializeField] private GameObject[] treeSprites;
    [SerializeField] private GameObject woodWallSprite;


    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private Tilemap resourcesTilemap;

    [SerializeField] private TileBase[] grassTile;
    [SerializeField] private TileBase[] sandTile;
    [SerializeField] private TileBase[] waterTile;
    [SerializeField] private TileBase[] deepWaterTile;
    [SerializeField] private TileBase[] resourceTile;


    [SerializeField] private int mapSize = 200;
    [SerializeField] private float noisiness = 0.03f;
    [SerializeField] private float secondNoisiness = 0.1f;

    private TILE_TYPE[,] map;
    private TILE_TYPE[,] resourceMap;
    private int[,] health;
    private GameObject[,] spritesForMap;

    private void OldAwake() {
        map = new TILE_TYPE[mapSize, mapSize];
        int halfMapSize = mapSize / 2;
        float offset = Random.Range(0f, 100f);

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                if (new Vector2Int(x - halfMapSize, y - halfMapSize).magnitude > halfMapSize) {
                    map[x, y] = TILE_TYPE.WATER;
                } else if (Mathf.PerlinNoise(x * noisiness + offset, y * noisiness + offset) < 0.5f) {
                    if (Mathf.PerlinNoise(x * secondNoisiness + offset, y * secondNoisiness + offset) < 0.5f) {
                        map[x, y] = TILE_TYPE.WATER;
                    } else {
                        map[x, y] = TILE_TYPE.GRASS;
                    }
                } else {
                    map[x, y] = TILE_TYPE.GRASS;
                }
            }
        }

        TILE_TYPE[,] transformedMap = ApplayTileRules();

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                Vector3Int position = new Vector3Int(x - halfMapSize, y - halfMapSize, 0);
                SetTile(position, transformedMap[x, y]);
            }
        }
    }

    public void initMap(string initString) {
        int sizeX = int.Parse(initString.Substring(0, initString.IndexOf(" ")) );
        initString = initString.Substring(initString.IndexOf(" ") + 1);

        int sizeY = int.Parse(initString.Substring(0, initString.IndexOf(" ")));
        initString = initString.Substring(initString.IndexOf(" ") + 1);

        Debug.Log("initString.Length " + initString.Length);

        map = new TILE_TYPE[sizeX, sizeY];
        resourceMap = new TILE_TYPE[sizeX, sizeY];
        spritesForMap = new GameObject[sizeX, sizeY];
        health = new int[sizeX, sizeY];

        try {
            for (int i = 0; i < sizeX * sizeY; i++) {
                map[i / sizeX, i % sizeY] = (TILE_TYPE)((int)initString[i]);
            }
            for (int i = sizeX * sizeY; i < initString.Length; i+=4) {
                int x = (int)initString[i];
                int y = (int)initString[i + 1];
                resourceMap[x, y] = (TILE_TYPE)((int)initString[i + 2]);
                health[x, y] = (int)initString[i + 3];

                Debug.Log($"{x} {y} {(int)initString[i + 2]} {(int)initString[i + 3]}");
            }
        } catch (System.Exception e) {
            Debug.Log(e);
        }
            

        mapSize = sizeX;
        int halfMapSize = mapSize / 2;

        TILE_TYPE[,] transformedMap = ApplayTileRules();


        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                Vector3Int position = new Vector3Int(x, y, 0);
                try {
                    SetTile(position, transformedMap[x, y]);
                    SetTile(position, resourceMap[x, y]);
                    hitTile(position, health[x, y]);
                } catch (System.Exception e) {
                    Debug.Log(e);
                }
            }
        }
    }

    internal void setHealth(Vector3Int pos, int value) {
        hitTile(pos, value);
    }

    internal int GetHealth(int x, int y) {
        return health[x, y];
    }

    internal void hitTile(Vector3Int pos, int h) {
        health[pos.x, pos.y] = h;
        Debug.Log("h: "+ h + " "+ resourceMap[pos.x, pos.y]);
        if(h <= 1 && resourceMap[pos.x, pos.y] == TILE_TYPE.TREE) {
            delSprite(pos);
        }
    }

    public TILE_TYPE[,] ApplayTileRules() {
        TILE_TYPE[,] transformedMap = new TILE_TYPE[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++) {
            for (int j = 0; j < mapSize; j++) {
                TILE_TYPE type = transformTiles(i, j);
                transformedMap[i, j] = type;
            }
        }

        return transformedMap;
    }

    public TILE_TYPE GetTile(int x, int y) {
        return resourceMap[x, y];
    }

    public void SetTile(Vector3Int position, TILE_TYPE type, bool isGameActive = false) {
        switch (type) {
            case TILE_TYPE.WATER:
                if (isGameActive) {
                    resourcesTilemap.SetTile(position, null);
                    resourceMap[position.x, position.y] = TILE_TYPE.WATER;
                    delSprite(position);
                }
                break;
            case TILE_TYPE.GRASS:
                groundTilemap.SetTile(position, getRdmGrass());
                break;
            case TILE_TYPE.WATER_L:
                waterTilemap.SetTile(position, waterTile[0]);
                break;
            case TILE_TYPE.WATER_R:
                waterTilemap.SetTile(position, waterTile[1]);
                break;
            case TILE_TYPE.WATER_T:
                waterTilemap.SetTile(position, waterTile[2]);
                break;
            case TILE_TYPE.WATER_B:
                waterTilemap.SetTile(position, waterTile[3]);
                break;
            case TILE_TYPE.WATER_TL:
                waterTilemap.SetTile(position, waterTile[4]);
                break;
            case TILE_TYPE.WATER_TR:
                waterTilemap.SetTile(position, waterTile[5]);
                break;
            case TILE_TYPE.WATER_BL:
                waterTilemap.SetTile(position, waterTile[6]);
                break;
            case TILE_TYPE.WATER_BR:
                waterTilemap.SetTile(position, waterTile[7]);
                break;
            case TILE_TYPE.WATER_C_TL:
                waterTilemap.SetTile(position, waterTile[8]);
                break;
            case TILE_TYPE.WATER_C_TR:
                waterTilemap.SetTile(position, waterTile[9]);
                break;
            case TILE_TYPE.WATER_C_BL:
                waterTilemap.SetTile(position, waterTile[10]);
                break;
            case TILE_TYPE.WATER_C_BR:
                waterTilemap.SetTile(position, waterTile[11]);
                break;
            case TILE_TYPE.SAND:
                waterTilemap.SetTile(position, sandTile[0]);
                break;
            case TILE_TYPE.STONE:
                resourcesTilemap.SetTile(position, resourceTile[0]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.TREE:
                addSprite(getRandomTree(), position, new Vector2(0.5f, 0));
                resourcesTilemap.SetTile(position, resourceTile[1]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.CHEST:
                resourcesTilemap.SetTile(position, resourceTile[2]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.BUSH:
                resourcesTilemap.SetTile(position, resourceTile[3]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.CRYSTAL:
                resourcesTilemap.SetTile(position, resourceTile[4]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.FIRE:
                resourcesTilemap.SetTile(position, resourceTile[5]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.VASE:
                resourcesTilemap.SetTile(position, resourceTile[6]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.WOOD_Wall:
                addSprite (woodWallSprite, position, new Vector2(0.5f, 0.875f));
                resourcesTilemap.SetTile(position, resourceTile[7]);
                resourceMap[position.x, position.y] = type;
                break;
            case TILE_TYPE.WOOD_BLOCK:
                resourcesTilemap.SetTile(position, resourceTile[8]);
                resourceMap[position.x, position.y] = type;
                break;
        }
    }

    private void addSprite(GameObject gObj, Vector3Int position, Vector2 offset) {
        SpriteRenderer spriteRenderer = Instantiate(gObj, new Vector3(position.x + offset.x, position.y + offset.y, 0), Quaternion.identity).GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 150 - position.y;
        spritesForMap[position.x, position.y] = spriteRenderer.gameObject;
    }

    private void delSprite(Vector3Int position) {
        Debug.Log("spritesForMap[position.x, position.y] " + position.x + " " + position.y);
        Debug.Log(spritesForMap[position.x, position.y]);
        if (spritesForMap[position.x, position.y] != null) {
            Destroy(spritesForMap[position.x, position.y]);
            spritesForMap[position.x, position.y] = null;
        }
    }

    public bool setResource(int x, int y, TILE_TYPE type) {
        Vector3Int position = new Vector3Int(x, y, 0);
        if (resourcesTilemap.GetTile(position)) {
            if(type == TILE_TYPE.WATER) {
                resourcesTilemap.SetTile(position, null);
            } else {
                return false;
            }
        } else {
            resourcesTilemap.SetTile(position, resourceTile[0]);
        }
        return true;
    }

    private TILE_TYPE transformTiles(int x, int y) {
        short[] values = new short[9];

        values[0] = (x - 1 > 0 && y - 1 > 0 && map[x - 1, y - 1] == TILE_TYPE.GRASS) ? (short)1 : (short)0;
        values[1] = (y - 1 > 0 && map[x, y - 1] == TILE_TYPE.GRASS) ? (short)1 : (short)0;
        values[2] = (x + 1 < mapSize && y - 1 > 0 && map[x + 1, y - 1] == TILE_TYPE.GRASS) ? (short)1 : (short)0;

        values[3] = (x - 1 > 0 && map[x - 1, y] == TILE_TYPE.GRASS) ? (short)1 : (short)0;
        values[4] = (map[x, y] == TILE_TYPE.GRASS) ? (short)1 : (short)0;
        values[5] = (x + 1 < mapSize && map[x + 1, y] == TILE_TYPE.GRASS) ? (short)1 : (short)0;

        values[6] = (x - 1 > 0 && y + 1 < mapSize && map[x - 1, y + 1] == TILE_TYPE.GRASS) ? (short)1 : (short)0;
        values[7] = (y + 1 < mapSize && map[x, y + 1] == TILE_TYPE.GRASS) ? (short)1 : (short)0;
        values[8] = (x + 1 < mapSize && y + 1 < mapSize && map[x + 1, y + 1] == TILE_TYPE.GRASS) ? (short)1 : (short)0;

        return RuleTiles.GetTile(values);
    }


    private TileBase getRdmGrass() {
        int rdm = Random.Range(0, 100);
        if (rdm > 30) {
            return grassTile[14];
        } else if (rdm > 5) {
            return grassTile[Random.Range(0, 8)];
        }
        
        return grassTile[Random.Range(0, grassTile.Length)];
    }

    private GameObject getRandomTree() {
        GameObject tree;
        int rdmTree = Random.Range(0, 101);

        if(rdmTree <= 50) {
            tree = treeSprites[0];
        }
        else if (rdmTree <= 65) {
            tree = treeSprites[1];
        }
        else if (rdmTree <= 80) {
            tree = treeSprites[2];
        }
        else if (rdmTree <= 95) {
            tree = treeSprites[3];
        }
        else {
            tree = treeSprites[4];
        }
        return tree;
    }
}
