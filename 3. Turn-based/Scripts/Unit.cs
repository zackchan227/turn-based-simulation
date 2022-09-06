using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Turn_Based
{
    public class Unit : MonoBehaviour
    {
        Vector3 originPos;
        public List<Transform> targets;
        public List<List<Vector2>> paths;
        //public Vector2[][] paths;
        //public Dictionary<Transform,List<Vector2>> lstPathsToTarget;
        public bool isAttacker = false;
        public bool isAttackable = false;
        public int damage = 0;
        public int hp = 0;
        public int maxHP = 0;
        public Slider healthBar;
        public GameObject outline;
        public string unitName = "";
        private Unit currentTarget;
        bool onBattle = false;
        bool onMove = false;
        bool onStop = false;
        bool needFindNewTarget = false;
        int nearestIndex = -1;
        float nextTime = 0;
        AxieFigure af;
        void Start()
        {
            if (GameManager.Instance._isPlaying)
            {
                af = GetComponentInChildren<AxieFigure>();
                damage = UnityEngine.Random.Range(0, 3);
                if (isAttacker)
                {
                    hp = 16;
                    targets = new List<Transform>();
                    paths = new List<List<Vector2>>();
                    originPos = GetComponent<Transform>().position;
                    findNewNearestTarget();
                }
                else hp = 32;
                this.healthBar.gameObject.SetActive(true);
                this.healthBar.minValue = 0;
                this.healthBar.maxValue = hp;
                this.healthBar.value = hp;
                isAttackable = false;
                unitName = this.gameObject.name;
                maxHP = hp;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (onStop) return;
            if (GameManager.Instance.DefenderCount == 0) 
            {
                af.PlayVictoryAnim();
                onStop = true;
            }
            if(GameManager.Instance.AttackerCount == 0)
            {
                af.PlayVictoryAnim();
                onStop = true;
            }
            if (GameManager.Instance._isPlaying)
            {
                if (Time.time >= nextTime)
                {
                    if(!isAttacker)
                    {
                        if(checkAdjacent())
                        {
                        
                        }
                    }
                    else
                    {
                        if (needFindNewTarget)
                        {
                            findNewNearestTarget();
                        }
                        else
                        {
                            if (paths.Count - 1 < nearestIndex) needFindNewTarget = true;
                            if (paths[nearestIndex] != null)
                            {
                                if (!checkTargetExist())
                                {
                                    for (int i = 0; i < paths[nearestIndex].Count; i++)
                                    {
                                        ResetTileColor(paths[nearestIndex][i]);
                                    }
                                    needFindNewTarget = true;
                                }
                                else
                                {
                                    if(checkAdjacent())
                                    {
                                        onBattle = true;
                                    }
                                    else
                                    {
                                        //MoveToTarget();
                                        onBattle = false;
                                        checkThenMoveToTarget();
                                    }

                                    if (onBattle)
                                    {
                                        if(currentTarget.currentTarget == this) attack();
                                    }
                                    
                                }
                            }
                            else
                            {
                                needFindNewTarget = true;
                            }
                        }
                    }
                    nextTime += 1.0f / GameManager.Instance.gameSpeed;
                }
            }
        }


        private void FixedUpdate()
        {
            if (!isAttacker) return;
            if (GameManager.Instance._isPlaying)
            {
                if (currentTarget != null)
                {
                    if (currentTarget.healthBar.value > currentTarget.hp)
                    {
                        //currentTarget.healthBar.value -= 3.0f * GameManager.Instance.gameSpeed * Time.deltaTime;
                        currentTarget.healthBar.value = Mathf.Lerp(currentTarget.healthBar.value, currentTarget.hp, Time.deltaTime / GameManager.Instance.gameSpeed);
                    }

                    if (this.healthBar.value > this.hp)
                    {
                        //this.healthBar.value -= 3.0f * GameManager.Instance.gameSpeed * Time.deltaTime;
                        this.healthBar.value = Mathf.Lerp(this.healthBar.value, this.hp, Time.deltaTime / GameManager.Instance.gameSpeed);
                    }
                }
            }
        }


        private void MoveToTarget()
        {
            MoveTo(paths[nearestIndex][0], GameManager.Instance.gameSpeed);
            ResetTileColor(paths[nearestIndex][0]);
            paths[nearestIndex].RemoveAt(0);
            onMove = false;
        }

        private bool checkTargetExist()
        {
            if(targets[nearestIndex] != null)
            {
                return true;
            }
            return false;
        }

        private void checkThenMoveToTarget()
        {
            if (checkMoveable(paths[nearestIndex][0]))
            {
                GridManager.Instance.GetTileAtPosition(transform.position).RemoveUnit();
                MoveTo(paths[nearestIndex][0], GameManager.Instance.gameSpeed);
                ResetTileColor(paths[nearestIndex][0]);
                paths[nearestIndex].RemoveAt(0);
                onMove = false;
                GridManager.Instance.GetTileAtPosition(transform.position).SetUnit(this.gameObject);
            }
            else
            {
                findSecondNearestTarget();
            }
        }

        private bool checkAdjacent()
        {
            currentTarget = getAdjacentTarget();
            if (currentTarget != null)
            {
                return true;
            }
            return false;
        }

        private void attack()
        {
            AxieFigure targetAF;
            if (checkAdjacent())
            {
                targetAF = currentTarget.GetComponentInChildren<AxieFigure>();
                if (transform.position.x > currentTarget.GetComponent<Transform>().position.x) // target on the left side => flip X to left
                {
                    targetAF.flipX = true;
                }
                else // target on the right side => flip X to right
                {
                    targetAF.flipX = false;
                }

                this.healthBar.value = this.hp;
                currentTarget.healthBar.value = currentTarget.hp;

                currentTarget.hp -= this.damage;
                //Debug.Log("Attacker deals " + this.damage + " damage to Defender (Remaining: " + currentTarget.hp + " HP)");

                this.hp -= currentTarget.damage;
                //Debug.Log("Defender deals " + currentTarget.damage + " damage to Attacker (Remaining: " + this.hp + " HP)");

                targetAF.PlayAttackAnim();
                af.PlayAttackAnim();

                if (currentTarget.hp <= 0 || this.hp <= 0)
                {
                    onBattle = false;
                }

                if (currentTarget.healthBar.value <= currentTarget.hp && this.healthBar.value <= this.hp)
                {
                    currentTarget.healthBar.value = currentTarget.hp;
                    this.healthBar.value = this.hp;
                    targetAF.StopCurrentAnim();
                    af.StopCurrentAnim();
                }

                if (currentTarget.hp <= 0)
                {
                    targetAF.PlayDieAnim();
                    GameManager.Instance.DefenderCount--;
                    GameManager.Instance.needUpdateRelativePower = true;
                    // targets.RemoveAt(nearestIndex);
                    // paths.RemoveAt(nearestIndex);
                    Destroy(currentTarget.gameObject);
                    currentTarget = null;
                    onBattle = false;
                    needFindNewTarget = true;
                }
                if (this.hp <= 0)
                {
                    af.PlayDieAnim();
                    GameManager.Instance.AttackerCount--;
                    GameManager.Instance.needUpdateRelativePower = true;
                    Destroy(this.gameObject);     
                }
            }
            else nearestIndex = -1;
        }

        private void checkFlipUnit()
        {
            if (transform.position.x > targets[nearestIndex].position.x) // target on the left side => flip X to left
            {
                af.flipX = false;
            }
            else // target on the right side => flip X to right
            {
                af.flipX = true;
            }
        }

        private void findNewNearestTarget()
        {
            Debug.Log("Finding new nearest target");
            getAllDefenderPaths();
            if (targets.Count == 0)
            {
                Debug.Log("No New Target!");
                onStop = true;
                af.PlayVictoryAnim();
            }
            else
            {
                nearestIndex = checkNearestTarget();
                if (nearestIndex != -1)
                {
                    showMovingPath();
                    checkFlipUnit();
                    needFindNewTarget = false;
                }
                else
                {
                    Debug.Log("Nearest index = " + nearestIndex);
                    needFindNewTarget = true;
                }
            }
        }

        private void findSecondNearestTarget()
        {
            Debug.Log("Finding new nearest target");
            getAllDefenderPaths();
            if (targets.Count == 0 || targets == null)
            {
                Debug.Log("No New Target!");
                onStop = true;
                af.PlayVictoryAnim();
            }
            else
            {
                nearestIndex = checkNearestTarget();
                if(targets[nearestIndex] != null)
                {
                    targets.RemoveAt(nearestIndex);
                    paths.RemoveAt(nearestIndex);
                }
               
                nearestIndex = checkNearestTarget();
                if (nearestIndex != -1)
                {
                    showMovingPath();
                    checkFlipUnit();
                    needFindNewTarget = false;
                }
                else
                {
                    Debug.Log("Nearest index = " + nearestIndex);
                    needFindNewTarget = true;
                }
            }
        }

        private void getAllDefenderPaths()
        {
            targets.Clear();
            paths.Clear();
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("Defender").Length; i++)
            {
                targets.Add(GameObject.FindGameObjectsWithTag("Defender")[i].GetComponent<Transform>());
                paths.Add(findPath(targets[i]));
            }
        }

        private int checkNearestTarget()
        {
            if (targets.Count == 0 || targets == null) return -1;
            int result = 0;
            int minPath = paths[result].Count;
            for (int i = 1; i < paths.Count; i++)
            {
                if (minPath > paths[i].Count)
                {
                    result = i;
                    minPath = paths[i].Count;
                }
            }
            return result;
        }

        private Unit getAdjacentTarget()
        {
            List<Unit> lstUnit = new List<Unit>();
            int resultIndex = 0;

            if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x + 1, transform.position.y)) != null)
            {
                if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x + 1, transform.position.y)).StandingUnit != null
                    && GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x + 1, transform.position.y)).StandingUnit.tag != this.gameObject.tag)
                {
                    lstUnit.Add(GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x + 1, transform.position.y)).StandingUnit.GetComponent<Unit>());
                }
            }
            if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x - 1, transform.position.y)) != null)
            {
                if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x - 1, transform.position.y)).StandingUnit != null
                    && GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x - 1, transform.position.y)).StandingUnit.tag != this.gameObject.tag)
                {
                    lstUnit.Add(GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x - 1, transform.position.y)).StandingUnit.GetComponent<Unit>());
                }
            }

            if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y + 1)) != null)
            {
                if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y + 1)).StandingUnit != null
                    && GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y + 1)).StandingUnit.tag != this.gameObject.tag)
                {
                    lstUnit.Add(GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y + 1)).StandingUnit.GetComponent<Unit>());
                }
            }

            if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y - 1)) != null)
            {
                if (GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y - 1)).StandingUnit != null
                    && GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y - 1)).StandingUnit.tag != this.gameObject.tag)
                {
                    lstUnit.Add(GridManager.Instance.GetTileAtPosition(new Vector2(transform.position.x, transform.position.y - 1)).StandingUnit.GetComponent<Unit>());
                }
            }

            if (lstUnit.Count > 0)
            {
                isAttackable = true;
                if ((3 + GameManager.Instance.AttackerCount - GameManager.Instance.DefenderCount) % 3 == 0)
                {
                    damage = 4;
                }
                if ((3 + GameManager.Instance.AttackerCount - GameManager.Instance.DefenderCount) % 3 == 1)
                {
                    damage = 5;
                }
                if ((3 + GameManager.Instance.AttackerCount - GameManager.Instance.DefenderCount) % 3 == 0)
                {
                    damage = 3;
                }

                int minHP = lstUnit[0].hp;
                if (lstUnit.Count > 1)
                {
                    for (int i = 1; i < lstUnit.Count; i++)
                    {
                        if (lstUnit[i].hp < minHP)
                        {
                            resultIndex = i;
                            minHP = lstUnit[i].hp;
                        }
                    }
                }
            }
            else
            {
                isAttackable = false;
            }
            return lstUnit.Count > 0 ? lstUnit[resultIndex] : null;
        }

        private bool checkMoveable(Vector2 target)
        {
            if (!GridManager.Instance.GetTileAtPosition(target).Moveable)
            {
                return false;
            }
            return true;
        }

        private List<Vector2> findPath(Transform target)
        {
            List<Vector2> result = new List<Vector2>();
            Vector3 temp = transform.position;
            while (temp != target.position)
            {
                if (temp.x != target.position.x && temp.y != target.position.y)
                {
                    if (Mathf.Abs(temp.x + target.position.x) > Mathf.Abs(temp.y + target.position.y))
                    {
                        if (temp.x < target.position.x)
                        {
                            temp = new Vector2(temp.x + 1, temp.y);
                            result.Add(temp);
                        }
                        else if (temp.x > target.position.x)
                        {
                            temp = new Vector2(temp.x - 1, temp.y);
                            result.Add(temp);
                        }
                    }
                    else
                    {
                        if (temp.y < target.position.y)
                        {
                            temp = new Vector2(temp.x, temp.y + 1);
                            result.Add(temp);
                        }
                        else if (temp.y > target.position.y)
                        {
                            temp = new Vector2(temp.x, temp.y - 1);
                            result.Add(temp);
                        }
                    }
                }
                else
                {
                    if (temp.x != target.position.x)
                    {
                        if (temp.x < target.position.x)
                        {
                            temp = new Vector2(temp.x + 1, temp.y);
                            result.Add(temp);
                        }
                        else if (temp.x > target.position.x)
                        {
                            temp = new Vector2(temp.x - 1, temp.y);
                            result.Add(temp);
                        }
                    }
                    else if (temp.y != target.position.y)
                    {
                        if (temp.y < target.position.y)
                        {
                            temp = new Vector2(temp.x, temp.y + 1);
                            result.Add(temp);
                        }
                        else if (temp.y > target.position.y)
                        {
                            temp = new Vector2(temp.x, temp.y - 1);
                            result.Add(temp);
                        }
                    }
                }
            }
            return result;
        }

        private void showMovingPath()
        {
            for (int i = 0; i < paths[nearestIndex].Count - 1; i++)
            {
                GridManager.Instance.GetTileAtPosition(paths[nearestIndex][i]).GetComponent<SpriteRenderer>().color = Color.green;
            }
        }

        private void ResetTileColor(Vector3 tilePos)
        {
            bool isOffset = (tilePos.x % 2 == 0 && tilePos.y % 2 != 0) || (tilePos.x % 2 != 0 && tilePos.y % 2 == 0);
            GridManager.Instance.GetTileAtPosition(tilePos).Init(isOffset);
        }

        private void MoveTo(Vector3 destination, float speed)
        {
            af.PlayMoveAnim();
            while (transform.position != destination)
            {
                //transform.position = Vector3.MoveTowards(transform.position, destination, GameManager.Instance.gameSpeed / Time.deltaTime);
                transform.position = destination;
            }
            //af.StopCurrentAnim();
        }
    }
}

