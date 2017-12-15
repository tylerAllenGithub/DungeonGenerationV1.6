using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStateMachine : MonoBehaviour {


    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
    }

    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        INPUT2,
        DONE
    }

    public PerformAction battleStates;
    public Transform spacer;
    public List<TurnHandler> performList = new List<TurnHandler>();
    public List<GameObject> playersInBattle = new List<GameObject>();
    public List<GameObject> enemiesInBattle = new List<GameObject>();
    
    public HeroGUI heroInput;
    public GameObject enemyButton;

    public List<GameObject> heroesToManage = new List<GameObject>();
    private TurnHandler heroChoice;

    //hero attacks etc...
    public GameObject actionPanel;
    public GameObject enemySelectPanel;
    public GameObject magicPanel;
    public Transform actionSpacer;
    public Transform magicSpacer;
    public GameObject actionButton;
    public GameObject magicButton;
    private List<GameObject> atkBtns = new List<GameObject>();

    //enemy select buttons
    private List<GameObject> enemyBtns = new List<GameObject>();

    //after battle completion
    public GameObject winPanel;

    // Use this for initialization
    void Start ()
    {
        actionPanel.SetActive(false);
        enemySelectPanel.SetActive(false);
        magicPanel.SetActive(false);
        winPanel.SetActive(false);

        battleStates = PerformAction.WAIT;
        enemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        playersInBattle.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        heroInput = HeroGUI.ACTIVATE;

        EnemyButtons();
	}
	
	// Update is called once per frame
	void Update ()
    {
		switch (battleStates)
        {
            case (PerformAction.WAIT):
                if(performList.Count > 0)
                {
                    battleStates = PerformAction.TAKEACTION;
                }
                break;

            case (PerformAction.TAKEACTION):
                GameObject performer = GameObject.Find(performList[0].attacker);
                if(performList[0].type == "Enemy")
                {

                    EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine>();
                    for (int i = 0; i < playersInBattle.Count; i++)
                    {
                        if (performList[0].targetOfAttacker == playersInBattle[i])
                        {
                            ESM.targetToAttack = performList[0].targetOfAttacker;
                            ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                            break;
                        }
                        else
                        {
                            performList[0].targetOfAttacker = playersInBattle[Random.Range(0, playersInBattle.Count)];
                            ESM.targetToAttack = performList[0].targetOfAttacker;
                            ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                        }
                    }
                }
                if (performList[0].type == "Hero")
                {
                    HeroStateMachine HSM = performer.GetComponent<HeroStateMachine>();
                    HSM.targetToAttack = performList[0].targetOfAttacker;
                    HSM.currentState = HeroStateMachine.TurnState.ACTION;
                }
                battleStates = PerformAction.PERFORMACTION;
                break;

            case (PerformAction.PERFORMACTION):
                //idle state
                break;

            case (PerformAction.CHECKALIVE):
                if (playersInBattle.Count < 1)
                    battleStates = PerformAction.LOSE;
                else if (enemiesInBattle.Count < 1)
                    battleStates = PerformAction.WIN;
                else
                {
                    ClearAttackPanel();
                    heroInput = HeroGUI.ACTIVATE;
                }
                break;

            case (PerformAction.WIN):
                Debug.Log("You have won the battle!");
                for (int i = 0; i < playersInBattle.Count; i++)
                {
                    playersInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
                }
                winPanel.SetActive(true);
                break;

            case (PerformAction.LOSE):
                Debug.Log("You have died.");
                break;
        }

        switch (heroInput)
        {
            case (HeroGUI.ACTIVATE):
                if (heroesToManage.Count > 0)
                {
                    heroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    heroChoice = new TurnHandler();
                    actionPanel.SetActive(true);
                    CreateAttackButtons();
                    heroInput = HeroGUI.WAITING;
                }
                break;

            case (HeroGUI.WAITING):
                //idle
                break;

            case (HeroGUI.DONE):
                heroInputDone();
                break;
        }
	}

    public void CollectActions(TurnHandler action)
    {
        performList.Add(action);
    }

    public void EnemyButtons()
    {
        //clean up
        foreach(GameObject enemyBtn in enemyBtns)
        {
            Destroy(enemyBtn);
        }
        enemyBtns.Clear();

        //create buttons
        foreach(GameObject enemy in enemiesInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();
            EnemyStateMachine currentEnemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.GetComponentInChildren <Text>();
            buttonText.text = currentEnemy.enemy.theName;

            button.enemyObject = enemy;
            newButton.transform.SetParent(spacer,false);
            enemyBtns.Add(newButton);
        }
    }

    public void Input1()//attack button
    {
        heroChoice.attacker = heroesToManage[0].name;
        heroChoice.attackerGameObject = heroesToManage[0];
        heroChoice.type = "Hero";
        heroChoice.chosenAttack = heroesToManage[0].GetComponent<HeroStateMachine>().hero.usableAttacks[0];
        actionPanel.SetActive(false);
        enemySelectPanel.SetActive(true);
    }

    public void Input2(GameObject enemyChosen)//choose enemy
    {
        heroChoice.targetOfAttacker = enemyChosen;
        heroInput = HeroGUI.DONE;
    }

    public void Input3(BaseAttack chosenSpell)//chosen magic attack
    {
        heroChoice.attacker = heroesToManage[0].name;
        heroChoice.attackerGameObject = heroesToManage[0];
        heroChoice.type = "Hero";

        heroChoice.chosenAttack = chosenSpell;
        magicPanel.SetActive(false);
        enemySelectPanel.SetActive(true);
    }

    public void Input4()//switching to magic attacks
    {
        actionPanel.SetActive(false);
        magicPanel.SetActive(true);
    }

    void heroInputDone()
    {
        performList.Add(heroChoice);

        ClearAttackPanel(); 

        heroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        heroesToManage.RemoveAt(0);
        heroInput = HeroGUI.ACTIVATE;
    }

    void ClearAttackPanel()
    {
        enemySelectPanel.SetActive(false);
        actionPanel.SetActive(false);
        magicPanel.SetActive(false);

        foreach (GameObject atkBtn in atkBtns)
        {
            Destroy(atkBtn);
        }
        atkBtns.Clear();
    }

    void CreateAttackButtons()
    {
        GameObject attackButton = Instantiate(actionButton) as GameObject;
        Text attackButtonText = attackButton.transform.Find("Text").GetComponent<Text>();
        attackButtonText.text = "Attack";
        attackButton.GetComponent<Button>().onClick.AddListener(() => Input1());
        attackButton.transform.SetParent(actionSpacer, false);

        atkBtns.Add(attackButton);

        GameObject magicAttackButton = Instantiate(actionButton) as GameObject;
        Text magicAttackButtonText = magicAttackButton.transform.Find("Text").GetComponent<Text>();
        magicAttackButtonText.text = "Magic";
        magicAttackButton.GetComponent<Button>().onClick.AddListener(() => Input4());
        magicAttackButton.transform.SetParent(actionSpacer, false);
        atkBtns.Add(magicAttackButton);

        if (heroesToManage[0].GetComponent<HeroStateMachine>().hero.magicAttacks.Count > 0)
        {
            foreach(BaseAttack magic in heroesToManage[0].GetComponent<HeroStateMachine>().hero.magicAttacks)
            {
                GameObject spellButton = Instantiate(magicButton) as GameObject;
                Text spellButtonText = spellButton.transform.Find("Text").GetComponent<Text>();
                spellButtonText.text = magic.name;
                MagicButton MGB = spellButton.GetComponent<MagicButton>();
                MGB.magicAttackToPerform = magic;
                spellButton.transform.SetParent(magicSpacer, false);
                atkBtns.Add(spellButton);
            }
        }
        else
        {
            magicAttackButton.GetComponent<Button>().interactable = false;
        }
    }
}
