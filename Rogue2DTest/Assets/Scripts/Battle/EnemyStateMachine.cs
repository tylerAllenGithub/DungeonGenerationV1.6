using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour {

    private BattleStateMachine BSM;

	public HeroStateMachine HSM;

    public BaseEnemy enemy;

	public BattleSound source;

	public Animator anim;

    public enum TurnState
    {
        PROCESSING, //waiting for progress bar to fill
        READY, //add to list, progress bar filled
        WAITING, //idle
        ACTION,
        DEAD
    }


    public TurnState currentState;

    private float currentCoolDown = 0f;
    private float maxCoolDown = 10f;

    private Vector3 startPosition;
    public GameObject selector;

    private bool actionStarted = false;
    public GameObject targetToAttack;
    private float animationSpeed = 10f;

    private bool alive = true;

    // Use this for initialization
    void Start () {
		anim = gameObject.GetComponent<Animator> ();
        currentCoolDown = Random.Range(0, 2.5f);
        selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        startPosition = transform.position;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                FillProgressBar();
                break;

            case (TurnState.READY):
                ChooseAction();
                currentState = TurnState.WAITING;
                break;

            case (TurnState.WAITING):
                //idle
                break;

		case (TurnState.ACTION):
			StartCoroutine (timeForAction ());
                break;

        case (TurnState.DEAD):
                if (!alive)
                {
                    return;
                }
                else
                {
                    //tag as dead
                    this.gameObject.tag = "DeadEnemy";
                    //no longer attackable
                    BSM.enemiesInBattle.Remove(this.gameObject);
                    //disable selector
                    selector.SetActive(false);
                    for (int i = 0; i < BSM.performList.Count; i++)
                    {
                        if (BSM.performList[i].attackerGameObject == this.gameObject)
                        {
                            BSM.performList.Remove(BSM.performList[i]);
                        }
                    }
                    //animate death?
                    this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    alive = false;
                    //reset enemy select buttons
                    BSM.EnemyButtons();
                    //check if battle is over
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                }
                break;
        }
    }

    void FillProgressBar()
    {
        currentCoolDown = currentCoolDown + Time.deltaTime;
        
        if (currentCoolDown >= maxCoolDown)
        {
            currentState = TurnState.READY;
        }
    }

    void ChooseAction()
    {
        TurnHandler myAttack = new TurnHandler();
        myAttack.attacker = enemy.theName;
        myAttack.type = "Enemy";
        myAttack.attackerGameObject = this.gameObject;
        myAttack.targetOfAttacker = BSM.playersInBattle[Random.Range(0, BSM.playersInBattle.Count)];
        int num = Random.Range(0, enemy.usableAttacks.Count);
        myAttack.chosenAttack = enemy.usableAttacks[num];
        BSM.CollectActions(myAttack);
    }

    public IEnumerator timeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        //animate enemy to the hero to be attacked
        Vector3 targetPosition = new Vector3(targetToAttack.transform.position.x - 1.5f, targetToAttack.transform.position.y, targetToAttack.transform.position.z);
        while (MoveTowardsPosition(targetPosition))
        {
            yield return null;
        }
		//animate attack and do damage
		anim.SetTrigger ("enemyAttack");
		DoDamage();

		//wait
        yield return new WaitForSeconds(0.5f);
	
        //animate back to start position
        Vector3 originalPosition = startPosition;
        while (MoveTowardsPosition(originalPosition))
        {
            yield return null;
        }
        //remove from perform list
        BSM.performList.RemoveAt(0);
        //reset battle state machine to waiting
        BSM.battleStates = BattleStateMachine.PerformAction.WAIT;

        actionStarted = false;

        currentCoolDown = 0f;
        currentState = TurnState.PROCESSING;
    }

    private bool MoveTowardsPosition(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animationSpeed * Time.deltaTime));
    }

    void DoDamage()
    {
        float calcDamage = enemy.attack + BSM.performList[0].chosenAttack.attackDamage;
        targetToAttack.GetComponent<HeroStateMachine>().TakeDamage(calcDamage);
    }

    public void TakeDamage(float damageAmount)
    {
		BattleSound.instance.PlayOrcHurt();
        anim.SetTrigger ("enemyHurt");
        damageAmount -= enemy.defense;
        if (damageAmount <= 1)
            damageAmount = 1;
        enemy.currentHealth -= damageAmount;
        if (enemy.currentHealth <= 0)
        {
            enemy.currentHealth = 0;
            currentState = TurnState.DEAD;
        }
    }
}
