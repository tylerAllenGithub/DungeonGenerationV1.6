using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour {

    private BattleStateMachine BSM;
	public EnemyStateMachine ESM;

    public BaseHero hero;
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
    private float maxCoolDown = 5f;

    private Image progressBar;
    public GameObject selector;
    //Ienumerator
    public GameObject targetToAttack;
    private Vector3 startPosition;
    private bool actionStarted = false;
    private float animationSpeed = 10f;

    private bool alive = true;

    //hero panel
    private HeroPanelStats panelStats;
    public GameObject heroPanel;
    private Transform heroSpacer;

    // Use this for initialization
    void Start ()
    {   
		anim = gameObject.GetComponent<Animator> ();
        //find spacer and connect
        heroSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").Find("HeroSpacer");
        //create panel and fill in hero information
        CreateHeroPanel();

        startPosition = transform.position;
        currentCoolDown = Random.Range(0, 2.5f);
        selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
    }
	
	// Update is called once per frame
	void Update ()
    {
		switch(currentState)
        {
            case (TurnState.PROCESSING):
                FillProgressBar();
                break;

            case (TurnState.READY):
                BSM.heroesToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;

            case (TurnState.WAITING):
                //idle state
                break;
            case (TurnState.ACTION):
                StartCoroutine(timeForAction());
                break;

            case (TurnState.DEAD):
                if(!alive)
                {
                    return;
                }
                else
                {
                    //change tag
                    this.gameObject.tag = "DeadHero";
                    //not targetable by enemies
                    BSM.playersInBattle.Remove(this.gameObject);
                    //cannot use hero anymore
                    BSM.heroesToManage.Remove(this.gameObject);
                    //deactivate selector if on
                    selector.SetActive(false);
                    //reset GUI
                    BSM.actionPanel.SetActive(false);
                    BSM.enemySelectPanel.SetActive(false);
                    //remove turn from perform list
                    for(int i = 0; i < BSM.performList.Count; i++)
                    {
                        if (BSM.performList[i].attackerGameObject == this.gameObject)
                        {
                            BSM.performList.Remove(BSM.performList[i]);
                        }
                    }
                    //change hero to dead animation
                    this.GetComponent<SpriteRenderer>().flipY = true;
                    //reset hero input
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                    alive = false;
                }
                break;
        }
	}

    void FillProgressBar()
    {
        currentCoolDown = currentCoolDown + Time.deltaTime;
        float calculateCoolDown = currentCoolDown / maxCoolDown;
        progressBar.transform.localScale = new Vector3(Mathf.Clamp(calculateCoolDown,0,1), progressBar.transform.localScale.y, progressBar.transform.localScale.z);
        if(currentCoolDown >= maxCoolDown)
        {
            currentState = TurnState.READY;
        }
    }

    public IEnumerator timeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        //animate enemy to the hero to be attacked
        Vector3 targetPosition = new Vector3(targetToAttack.transform.position.x + 1.5f, targetToAttack.transform.position.y, targetToAttack.transform.position.z);
        while (MoveTowardsPosition(targetPosition))
        {
            yield return null;
        }


		anim.SetTrigger ("KnightAttack");
		//source.PlayOneShot (swordatk, 10);
		BattleSound.instance.PlaySwordAtk();

        //wait
        yield return new WaitForSeconds(0.5f);
        //animate attack and do damage
        DoDamage();
        //animate back to start position
        Vector3 originalPosition = startPosition;
        while (MoveTowardsPosition(originalPosition))
        {
            yield return null;
        }
        //remove from perform list
        BSM.performList.RemoveAt(0);

        if (BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        {
            //reset battle state machine to waiting
            BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
            currentCoolDown = 0f;
            currentState = TurnState.PROCESSING;
        }
        else
        {
            currentState = TurnState.WAITING;
        }

        actionStarted = false;
    }

    private bool MoveTowardsPosition(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animationSpeed * Time.deltaTime));
    }
		
    void DoDamage()
    {
        float calcDamage = hero.attack + BSM.performList[0].chosenAttack.attackDamage;
        targetToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calcDamage);
    }

    public void TakeDamage(float damageAmount)
    {
		BattleSound.instance.PlayKnightHurt();
		anim.SetTrigger ("KnightHit");
        damageAmount -= hero.defense;
        if (damageAmount <= 1)
            damageAmount = 1;
        hero.currentHealth -= damageAmount;
        if (hero.currentHealth <= 0)
        {
            hero.currentHealth = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }

    void CreateHeroPanel()
    {
        heroPanel = Instantiate(heroPanel) as GameObject;
        panelStats = heroPanel.GetComponent<HeroPanelStats>();

        panelStats.heroName.text = hero.theName;
        panelStats.heroHealth.text = "HP: " + hero.currentHealth + "/" + hero.baseHealth;
        panelStats.heroMp.text = "MP: " + hero.currentMP + "/" + hero.baseMP;

        progressBar = panelStats.progressBar;
        heroPanel.transform.SetParent(heroSpacer, false);
    }

    void UpdateHeroPanel()
    {
        panelStats.heroHealth.text = "HP: " + hero.currentHealth + "/" + hero.baseHealth;
        panelStats.heroMp.text = "MP: " + hero.currentMP + "/" + hero.baseMP;
    }
}
