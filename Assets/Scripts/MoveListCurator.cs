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
    [SerializeField] public InputStreamParser.MoveDetails[] moveListGeneral = new InputStreamParser.MoveDetails[0];
    [SerializeField] public InputStreamParser.MoveDetails[] moveListNoActionButtons = new InputStreamParser.MoveDetails[0];
    private InputStreamParser.MoveDetails[][] moveLists = new InputStreamParser.MoveDetails[3][];
    public int currentList = 0;

    public void Start()
    {
        moveLists[0] = new InputStreamParser.MoveDetails[moveListSword.Length + moveListGeneral.Length];
        moveLists[1] = new InputStreamParser.MoveDetails[moveListGun.Length + moveListGeneral.Length];
        moveLists[2] = new InputStreamParser.MoveDetails[moveListSpells.Length + moveListGeneral.Length];
        for (int a = 0; a < moveListSword.Length; a++)
        {
            moveLists[0][a] = moveListSword[a];
        }
        for (int b = 0; b < moveListGeneral.Length; b++)
        {
            moveLists[0][moveListSword.Length + b] = moveListGeneral[b];
        }
        for (int a = 0; a < moveListGun.Length; a++)
        {
            moveLists[1][a] = moveListGun[a];
        }
        for (int b = 0; b < moveListGeneral.Length; b++)
        {
            moveLists[1][moveListGun.Length + b] = moveListGeneral[b];
        }
        for (int a = 0; a < moveListSpells.Length; a++)
        {
            moveLists[2][a] = moveListSpells[a];
        }
        for (int b = 0; b < moveListGeneral.Length; b++)
        {
            moveLists[2][moveListSpells.Length + b] = moveListGeneral[b];
        }
    }

    public void ChangeList(int changeDirection)
    {
        currentList += changeDirection;
        if (currentList >= 2) { currentList = 0; } // TEMP DISABLE SPELLS!
        if (currentList >= moveLists.Length) { currentList = 0; }
        else if (currentList < 0) { currentList = moveLists.Length - 1; }
    }

    public InputStreamParser.MoveDetails[] ReturnCurrentMoves()
    {
        return moveLists[currentList];
    }

    public InputStreamParser.MoveDetails[] ReturnMovement()
    {
        return moveListNoActionButtons;
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
