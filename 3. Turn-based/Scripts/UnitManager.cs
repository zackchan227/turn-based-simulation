using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game_Turn_Based
{
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance;

        //private List<ScriptableUnit> _units;
        //public BaseAttacker SelectedAttacker;

        public GameObject attackerPrefab;
        public GameObject defenderPrefab;
        public Transform unitParent;

        void Awake()
        {
            Instance = this;

            //_units = Resources.LoadAll<ScriptableUnit>("Units").ToList();

        }

        public void SpawnAttackerAtPos(Vector2 vt)
        {
                GameObject spawnedAttacker = Instantiate(attackerPrefab,unitParent);
                spawnedAttacker.transform.position = vt;
                Tile randomSpawnTile = GridManager.Instance.GetTileAtPosition(vt);
                randomSpawnTile.SetUnit(spawnedAttacker);
        }

        public void SpawnDefenderAtPos(Vector2 vt)
        {
            GameObject spawnedDefender = Instantiate(defenderPrefab, unitParent);
            spawnedDefender.transform.position = vt;
            Tile randomSpawnTile = GridManager.Instance.GetTileAtPosition(vt);
            randomSpawnTile.SetUnit(spawnedDefender);
        }

        public void SpawnDefenderInSquare(Vector2 vt)
        {

        }

        public void SpawnDefenderInRectangle(Vector2 vt)
        {

        }

        public void SpawnDefenderInTriangle(Vector2 vt)
        {

        }

        public void SpawnAttackerRandomPos()
        {
            for (int i = 0; i < GameManager.Instance.AttackerCount; i++)
            {
                GameObject spawnedAttacker = Instantiate(attackerPrefab,unitParent);       
                int minX = -(GridManager.Instance.getWidth()/2);
                int maxX = GridManager.Instance.getWidth()/2;
                int minY = -GridManager.Instance.getHeight()/2;
                int maxY = GridManager.Instance.getHeight()/2;
                spawnedAttacker.transform.position = new Vector3(UnityEngine.Random.Range(minX,maxX),UnityEngine.Random.Range(minY,maxY),0);
                var randomSpawnTile = GridManager.Instance.GetTileAtPosition(spawnedAttacker.transform.position);
                while(randomSpawnTile.StandingUnit != null)
                {
                    spawnedAttacker.transform.position = new Vector3(UnityEngine.Random.Range(minX,maxX),UnityEngine.Random.Range(minY,maxY),0);
                    randomSpawnTile = GridManager.Instance.GetTileAtPosition(spawnedAttacker.transform.position);
                }
                spawnedAttacker.name = "Attacker " + (i + 1);
                randomSpawnTile.SetUnit(spawnedAttacker);
            }
        }

        public void SpawnDefenderRandomPos()
        {
            for (int i = 0; i < GameManager.Instance.DefenderCount; i++)
            {
                GameObject spawnedDenfender = Instantiate(defenderPrefab,unitParent);
                int minX = -(GridManager.Instance.getWidth()/2);
                int maxX = GridManager.Instance.getWidth()/2;
                int minY = -GridManager.Instance.getHeight()/2;
                int maxY = GridManager.Instance.getHeight()/2;
                spawnedDenfender.transform.position = new Vector3(UnityEngine.Random.Range(minX,maxX),UnityEngine.Random.Range(minY,maxY),0);
                var randomSpawnTile = GridManager.Instance.GetTileAtPosition(spawnedDenfender.transform.position);
                while(randomSpawnTile.StandingUnit != null)
                {
                    spawnedDenfender.transform.position = new Vector3(UnityEngine.Random.Range(minX,maxX),UnityEngine.Random.Range(minY,maxY),0);
                    randomSpawnTile = GridManager.Instance.GetTileAtPosition(spawnedDenfender.transform.position);
                }
                spawnedDenfender.tag = "Defender";
                spawnedDenfender.name = "Defender " + (i + 1);
                randomSpawnTile.SetUnit(spawnedDenfender);
            }
        }
    }
}