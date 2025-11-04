using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/*BattleState has the values of each possible state
in the game, which helps the battle handler understand
what action to take*/
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

    // Sets the BattleState to START and runs SetupBattle 
    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    /* Sets up all the units in the field and gives the turn to the 
    fastest unit on the field. Each unit is instantiated and put in
    a list called unitsInField, then that list is sorted based on the
    speed of each unit in it in descending order (highest to lowest).*/
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

    /* Gives the turn to either the player or the enemy based on
    who is next in the turn order. The turn order is determined
    by the speed of each unit. The unit with the highest speed goes
    first and the least goes last.*/
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

        /*Determines what the next turn index will be. if turn is
        still within the bounds of how many units there are in the
        field, it is implemented by one. If it is already equal to
        or greater than how many units are on the field (zero index),
        then it loops around to go from the top of the list*/
        if (turn < unitsInField.Count - 1) turn++;
        else turn = 0;
    }

    /* Setups up the unit by instantiating it into the scene and
    puts the unit in their respective lists. If a player's unit was
    made, it goes into the lists playerUnits and unitsInField. If an
    enemy was made, it ges into the lists enemyUnits and unitsInField*/
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

    /* Carries out the player's action based on the inputs made.
    For now, it takes into account what keyboard inputs are made
    to select the actions*/
    IEnumerator PlayerAction(Unit player)
    {
        bool inputtedAction = false, inputtedTarget = false, doubled = false;
        int action = -1, target = -1;
        Debug.Log("It's " + player.name + " turn");
        Debug.Log("Waiting for action");
        int swcheck = 0;
        while (!inputtedAction)
        {
            //If Q is pressed, the acting unit does an attack
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Q was pressed: Attack");
                action = 1;
                doubled = false;
                inputtedAction = true;
            }
            //If W is pressed, the acting unit does an attack
            //with double damage
            else if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("W was pressed: Attack");
                action = 1;
                doubled = true;
                inputtedAction = true;
            }
            //If A is pressed, the acting unit heals
            else if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("A was pressed: Heal");
                action = 2;
                doubled = false;
                inputtedAction = true;
            }
            //If S is pressed, the acting unit heals 
            //with double the amount
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
            //If keyboard 1 is pressed, the acting unit targets
            //the unit at index 0
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Target 1 was selected");
                target = 0;
                swcheck = StrengthWeaknessCheck(player, enemyUnits[target]);
                inputtedTarget = true;
            }
            //If keyboard 2 is pressed, the acting unit targets
            //the unit at index 1
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Target 2 was selected");
                target = 1;
                swcheck = StrengthWeaknessCheck(player, enemyUnits[target]);
                inputtedTarget = true;
            }
            //If keyboard 3 is pressed, the acting unit targets
            //the unit at index 2
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Target 3 was selected");
                target = 2;
                swcheck = StrengthWeaknessCheck(player, enemyUnits[target]);
                inputtedTarget = true;
            }
            yield return null;
        }

        // Runs the Attack action
        if (action == 1) StartCoroutine(Attack(player, enemyUnits[target], doubled, swcheck));
        // Runs the Heal action
        else if (action == 2) StartCoroutine(Heal(player, playerUnits[target], doubled));
    }

    /* Lets the enemy take an action between attacking or healing.
    For now, the enemies randomly chooses between attacking or
    healing, and targeting*/
    IEnumerator EnemyAction(Unit enemy)
    {
        yield return new WaitForSeconds(1f);
        bool doubled = false;
        List<int> playerHalf = new List<int>();
        List<int> enemyHalf = new List<int>();
        List<int> enemyDamaged = new List<int>();


        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i].currentHP < (playerUnits[i].maxHP / 2))
            {
                playerHalf.Add(i);
            }
        }

        for (int j = 0; j < enemyUnits.Count; j++)
        {
            if(enemyUnits[j].currentHP < enemyUnits[j].maxHP)
            {
                enemyDamaged.Add(j);
                if (enemyUnits[j].currentHP < (enemyUnits[j].maxHP / 2))
                {
                    enemyHalf.Add(j);
                }
            }
        }

        if (playerHalf.Count > 0)
        {
            int playerHalfIndex = Random.Range(0, playerHalf.Count);
            int attackTarget = playerHalf[playerHalfIndex];
            Unit target = playerUnits[attackTarget];
            int swcheck = StrengthWeaknessCheck(enemy, target);
            StartCoroutine(Attack(enemy, target, doubled, swcheck));
        }
        else if (enemyHalf.Count > 0)
        {
            int action = Random.Range(1, 4);
            int enemyHalfIndex = Random.Range(0, enemyHalf.Count);
            int healTarget = enemyHalf[enemyHalfIndex];
            int attackTarget = Random.Range(0, playerUnits.Count);
            Unit target = playerUnits[attackTarget];
            int swcheck = StrengthWeaknessCheck(enemy, target);
            if (action == 1) StartCoroutine(Attack(enemy, target, doubled, swcheck));
            else if (action == 2 || action == 3) StartCoroutine(Heal(enemy, enemyUnits[healTarget], doubled));
        }
        else if (enemyDamaged.Count > 0)
        {
            int action = Random.Range(1, 3);
            int enemyDamagedIndex = Random.Range(0, enemyDamaged.Count);
            int healTarget = enemyDamaged[enemyDamagedIndex];
            int attackTarget = Random.Range(0, playerUnits.Count);
            Unit target = playerUnits[attackTarget];
            int swcheck = StrengthWeaknessCheck(enemy, target);
            if (action == 1) StartCoroutine(Attack(enemy, target, doubled, swcheck));
            else if (action == 2) StartCoroutine(Heal(enemy, enemyUnits[healTarget], doubled));
        }
        else
        {
            int attackTarget = Random.Range(0, playerUnits.Count);
            Unit target = playerUnits[attackTarget];
            int swcheck = StrengthWeaknessCheck(enemy, target);
            StartCoroutine(Attack(enemy, target, doubled, swcheck));
        }
    }

    /* Runs the healing action. First checks whether or not the
    healed amount should be doubled, then runs the getHealed
    function of the targetted unit*/
    IEnumerator Heal(Unit healer, Unit target, bool doubled)
    {
        int healAmount = 1;
        if (doubled) healAmount = 2;

        yield return new WaitForSeconds(1f);

        target.getHealed((healer.currentHP / 5) * healAmount);
        Debug.Log(healer.name + " healed " + target.name);
        takeTurn();
    }

    /* Runs the attacking action. First checks whether or not the
    damage dealt should be doubled, then runs the takeDamage
    function of the targetted unit. If the function returns true
    (meaning the targetted unit's HP went to 0 or below), then it
    is removed from its respective lists and is deactivated in the
    scene. Finally, it runs the checkUnits method to see if the units
    of one side of the battle lost all its units. If so, the game ends.
    If not, the game continues and the next turn happens*/
    IEnumerator Attack(Unit attacker, Unit target, bool doubled, int swcheck)
    {
        double dmg = 1;

        if (swcheck == 1)
        {
            dmg *= 2;
        }
        else if (swcheck == 2)
        {
            dmg /= 2;
        }
        
        bool isDead = target.takeDamage(attacker.attack * dmg);
        Debug.Log(attacker.name + " attacked " + target.name + " for " + dmg);

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
            if (turn != unitsInField.IndexOf(attacker) + 1) turn--;
            target.self.SetActive(false);
        }
        bool endGame = checkUnits();
        if (!endGame) takeTurn();
        else if (endGame) EndBattle();
    }

    /* Checks whether one side (either the player or the enemy) has
    lost all their units. If the player's units are depleted, the
    BattleState is set to LOST and returns true, meaning the game
    has ended. If the enemy's units are depleted, the BattleState is
    set to WON instead and returns true. If each side still has at
    least one unit, the game continues*/
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

    /* Shows whether the player won or lost the battle.
    For now, it shows up in the debug console*/
    private void EndBattle()
    {
        if (state == BattleState.WON) Debug.Log("You Won!");
        else if (state == BattleState.LOST) Debug.Log("You Lost...");
    }
    
    /* Checks if the target is weak or strong or no anything to the target.
    If swcheck = 0, target is neither weak or strong to the attacker.
    If swcheck = 1, target is weak to the attacker.
    If swcheck = 3, target is strong to the attacker.*/
    private int StrengthWeaknessCheck(Unit attacker, Unit target)
    {
        string earth = "Earth";
        string air = "Air";
        string water = "Water";
        string fire = "Fire";
        int swcheck = 0;
        
        if ((Equals(attacker.element, earth)
        && Equals(target.element, water))
        ||
        (Equals(attacker.element, water)
        && Equals(target.element, fire))
        ||
        (Equals(attacker.element, fire)
        && Equals(target.element, air))
        ||
        (Equals(attacker.element, air)
        && Equals(target.element, earth)))
        {
            swcheck = 1;
        }
        else if ((Equals(attacker.element, fire)
        && Equals(target.element, water))
        ||
        (Equals(attacker.element, air)
        && Equals(target.element, fire))
        ||
        (Equals(attacker.element, earth)
        && Equals(target.element, air))
        ||
        (Equals(attacker.element, water)
        && Equals(target.element, earth)))
        {
            swcheck = 2;
        }
        else
        {
            swcheck = 0;
        }

        return swcheck;
    }

}
