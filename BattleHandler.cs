using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState
{
    START, PLAYERTURN, ENEMYTURN, WON, LOST
}

public class BattleHandler : MonoBehaviour
{
    public GameObject player1PF;
    public GameObject player2PF;
    public GameObject player3PF;
    public GameObject enemy1PF;
    public GameObject enemy2PF;
    public GameObject enemy3PF;

    public Transform playerBT;
    public Transform enemyBT;
    private int turn;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();
    private List<Unit> unitsInField = new List<Unit>();
    
    public BattleState state;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        setUpUnit(player1PF, playerBT, true);
        setUpUnit(player2PF, playerBT, true);
        setUpUnit(player3PF, playerBT, true);
        setUpUnit(enemy1PF, enemyBT, false);
        setUpUnit(enemy2PF, enemyBT, false);
        setUpUnit(enemy3PF, enemyBT, false);

        yield return new WaitForSeconds(2f);
        unitsInField = unitsInField.OrderByDescending(u => u.speed).ToList();
        turn = 0;
        takeTurn();
    }

    private void takeTurn()
    {
        Unit actingUnit = unitsInField[turn];

        if (playerUnits.Contains(actingUnit))
        {
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerAction(actingUnit));
        }
        else if (enemyUnits.Contains(actingUnit))
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyAction(actingUnit));
        }
        
        if (turn < unitsInField.Count - 1) turn++;
        else turn = 0;
    }

    private void setUpUnit(GameObject unit, Transform BT, bool isPlayer)
    {
        GameObject createdUnit = Instantiate(unit, BT);
        if (isPlayer)
        {
            Unit createdPlayer = createdUnit.GetComponent<Unit>();
            playerUnits.Add(createdPlayer);
            unitsInField.Add(createdPlayer);
        }
        else if (!isPlayer)
        {
            Unit createdEnemy = createdUnit.GetComponent<Unit>();
            enemyUnits.Add(createdEnemy);
            unitsInField.Add(createdEnemy);
        }
    }

    IEnumerator PlayerAction(Unit player)
    {
        bool inputtedAction = false, inputtedTarget = false, doubled = false;
        int action = -1, target = -1;
        Debug.Log("It's " + player.name + " turn");
        Debug.Log("Waiting for action");
        while (!inputtedAction)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Q was pressed: Attack");
                action = 1;
                doubled = false;
                inputtedAction = true;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("W was pressed: Attack");
                action = 1;
                doubled = true;
                inputtedAction = true;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("A was pressed: Heal");
                action = 2;
                doubled = false;
                inputtedAction = true;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("S was pressed: Heal");
                action = 2;
                doubled = true;
                inputtedAction = true;
            }
            if (inputtedAction) Debug.Log("Waiting for target");
            yield return null;
        }
        while (!inputtedTarget)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Target 1 was selected");
                target = 0;
                inputtedTarget = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Target 2 was selected");
                target = 1;
                inputtedTarget = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Target 3 was selected");
                target = 2;
                inputtedTarget = true;
            }
            yield return null;
        }

        if (action == 1) StartCoroutine(Attack(player, enemyUnits[target], doubled));
        else if (action == 2) StartCoroutine(Heal(player, playerUnits[target], doubled));
    }

    IEnumerator EnemyAction(Unit enemy)
    {
        yield return new WaitForSeconds(1f);
        int action = Random.Range(1, 3);
        int target = Random.Range(0, playerUnits.Count);
        bool doubled = Random.Range(0, 2) == 1;

        if (action == 1) StartCoroutine(Attack(enemy, playerUnits[target], doubled));
        else if (action == 2) StartCoroutine(Heal(enemy, enemyUnits[target], doubled));
    }
    
    IEnumerator Heal(Unit healer, Unit target, bool doubled)
    {
        int healAmount = 1;
        if (doubled) healAmount = 2;

        yield return new WaitForSeconds(1f);

        target.getHealed((healer.currentHP / 5) * healAmount);
        takeTurn();
    }

    IEnumerator Attack(Unit attacker, Unit target, bool doubled)
    {
        int dmg = 1;
        if (doubled) dmg = 2;
        bool isDead = target.takeDamage(attacker.attack * dmg);
        Debug.Log(attacker.name + " attacked " + target.name);

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            if (enemyUnits.Contains(target))
            {
                unitsInField.Remove(target);
                enemyUnits.Remove(target);
            }
            else if (playerUnits.Contains(target))
            {
                unitsInField.Remove(target);
                playerUnits.Remove(target);
            }
            target.self.SetActive(false);
            turn--;
        }
        bool endGame = checkUnits();
        if (!endGame) takeTurn();
        else if (endGame) EndBattle();
    }

    private bool checkUnits()
    {
        if (playerUnits.Count == 0)
        {
            state = BattleState.LOST;
            return true;
        }
        else if (enemyUnits.Count == 0)
        {
            state = BattleState.WON;
            return true;
        }
        return false;
    }
    
    private void EndBattle()
    {
        if (state == BattleState.WON) Debug.Log("You Won!");
        else if (state == BattleState.LOST) Debug.Log("You Lost...");
    }
}
