using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public enum TILE_TYPE {
    WATER,
    GRASS,
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
    STONE,
    SAND
}

public class TileController : MonoBehaviour{

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

    private void Awake() {
        map = new TILE_TYPE[mapSize, mapSize];
        int halfMapSize = mapSize / 2;
        float offset = Random.Range(0f, 100f);

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                if(new Vector2Int(x - halfMapSize, y - halfMapSize).magnitude > halfMapSize) {
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


        TILE_TYPE[,] transformedMap = new TILE_TYPE[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++) {
            for (int j = 0; j < mapSize; j++) {
                TILE_TYPE type = transformTiles(i, j);
                if (type == TILE_TYPE.GRASS && Random.Range(0,100) < 10) {
                    type = TILE_TYPE.STONE;
                }
                transformedMap[i, j] = type;
            }
        }

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                Vector3Int position = new Vector3Int(x - halfMapSize, y - halfMapSize, 0);
                switch (transformedMap[x, y]) {
                    case TILE_TYPE.WATER:
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
                        groundTilemap.SetTile(position, grassTile[14]);
                        resourcesTilemap.SetTile(position, resourceTile[0]);
                        break;
                }
            }
        }
    }

    public void setTile(int x, int y) {
        Vector3Int position = new Vector3Int(x, y, 0);
        Debug.Log("GetTile " + resourcesTilemap.GetTile(position));
        if (resourcesTilemap.GetTile(position)) {
            resourcesTilemap.SetTile(position, null);
        } else {
            resourcesTilemap.SetTile(position, resourceTile[0]);
        }
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
}