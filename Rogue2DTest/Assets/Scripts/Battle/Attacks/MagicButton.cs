﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicButton : MonoBehaviour {

    public BaseAttack magicAttackToPerform;

    public void CastMagicAttack()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().Input3(magicAttackToPerform);
    }
}
