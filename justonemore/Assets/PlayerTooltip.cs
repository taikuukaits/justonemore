using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTooltip : PlayerTriggerable
{

    public string tip; 

    public override void OnPlayerTrigger(Player player)
    {

        FindObjectOfType<ToolTip>().ShowText(tip);        
        Destroy(this.gameObject);
        

    
    }


}
