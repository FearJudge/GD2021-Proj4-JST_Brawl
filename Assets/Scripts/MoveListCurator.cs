using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveListCurator : MonoBehaviour
{
    // 1st: Sword [Easy, Fast, Okay Damage]
    // 2nd: Gun [Moderate, Very Fast, Excellent Range]
    // 3rd: Spells/Familiars [Hard, Slow, Massive Damage and Range and special properties]
    [SerializeField] public InputStreamParser.MoveDetails[] moveListSword = new InputStreamParser.MoveDetails[0];
    [SerializeField] public InputStreamParser.MoveDetails[] moveListGun = new InputStreamParser.MoveDetails[0];
    [SerializeField] public InputStreamParser.MoveDetails[] moveListSpells = new InputStreamParser.MoveDetails[0];
    private InputStreamParser.MoveDetails[][] moveLists = new InputStreamParser.MoveDetails[3][];
    public int currentList = 0;

    public void Start()
    {
        moveLists[0] = moveListSword;
        moveLists[1] = moveListGun;
        moveLists[2] = moveListSpells;
    }

    public void ChangeList(int changeDirection)
    {
        currentList += changeDirection;
        if (currentList >= moveLists.Length) { currentList = 0; }
        else if (currentList < 0) { currentList = moveLists.Length - 1; }
    }

    public InputStreamParser.MoveDetails[] ReturnCurrentMoves()
    {
        return moveLists[currentList];
    }

    public void Upgrade(UpgradeLibrary.MoveUpgrade moveupgrade)
    {
        for (int a = 0; a < moveLists.Length; a++)
        {
            for (int b = 0; b < moveLists[a].Length; b++)
            {
                moveLists[a][b].AddUpgrade(moveupgrade);
            }
        }
    }
}
