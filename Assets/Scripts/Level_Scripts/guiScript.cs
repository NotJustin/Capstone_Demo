using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class guiScript : MonoBehaviour
{
    /// The script takes information from Player, displays it on the screen, and communicates to the Tile_Selector_Script whenever the user interacts with the button.

    Turn_Handler turnHandler;

    public int mode;
    private int move = 0, attack = 1;

    GUIStyle style;

    SpriteRenderer spriteRenderer;
    bool changedSize = false;
    bool changedCardSize = false;

    public GameObject cardOne;
    public GameObject cardTwo;
    public GameObject cardThree;
    public GameObject cardFour;
    public GameObject cardFive;
    public GameObject cardSix;
    public GameObject cardSeven;
    GameObject[] hand;

    public GenericCard selectedCard;

    void Start()
    {
        mode = move;
        turnHandler = GetComponent<Turn_Handler>();
        style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.white;
        spriteRenderer = GameObject.FindWithTag("ActivePlayerSprite").GetComponent<SpriteRenderer>();
        spriteRenderer.size = new Vector2(spriteRenderer.size.x / 3, spriteRenderer.size.y / 3);
        hand = new GameObject[2];
        Transform handTransform = transform.Find("Hand");
        for (int i = 0; i < handTransform.childCount; i++)
        {
            hand[i] = handTransform.GetChild(i).gameObject;
        }
    }

    public void ShowCards()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (turnHandler.activePlayer.cards[i].GetComponent<GenericCard>().sprite != null && !turnHandler.activePlayer.cards[i].GetComponent<GenericCard>().used)
            {
                hand[i].GetComponent<SpriteRenderer>().sprite = turnHandler.activePlayer.cards[i].GetComponent<GenericCard>().sprite;
                if (!changedCardSize)
                {
                    SpriteRenderer currentCard = hand[i].GetComponent<SpriteRenderer>();
                    currentCard.size = new Vector2(currentCard.size.x * 1.3f, currentCard.size.y * 1.3f);
                    BoxCollider2D boxCollider = (BoxCollider2D)hand[i].AddComponent<BoxCollider2D>();
                    boxCollider.size = currentCard.size;
                    hand[i].GetComponent<ClickOnCard>().card = turnHandler.activePlayer.cards[i].GetComponent<GenericCard>();
                    changedCardSize = true;
                }
                hand[i].transform.position = new Vector3(transform.position.x + i * 4.2f + 4.0f, transform.position.y - 0.5f, 0);
            }
            changedCardSize = false;
        }
    }

    public void HideCards()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public void SetCardsToUnused()
    {
        if (turnHandler.activePlayer.cards.Length == 0)
        {
            return;
        }
        for (int i = 0; i < hand.Length; i++)
        {
            turnHandler.activePlayer.cards[i].GetComponent<GenericCard>().used = false;
        }
    }

    float originalWidth = 1050.0f;
    float originalHeight = 530.0f;
    float x;
    float y;
    Vector3 scale;
    void OnGUI()
    {
        x = Screen.width / originalWidth; // calculate hor scale
        y = Screen.height / originalHeight; // calculate vert scale
        scale = new Vector3(x, y, 1.0f);
        var svMat = GUI.matrix; // save current matrix
                                // substitute matrix - only scale is altered from standard
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(530, 100, 100, 40), "Toggle Mode"))
        {
            if (mode == move)
            {
                turnHandler.activePlayer.ClearPath();
                ShowCards();
            }
            else
            {
                HideCards();
                turnHandler.activePlayer.selectedEnemy = null;
                selectedCard = null;
                Vector3Int playerCell = turnHandler.activePlayer.world.world.WorldToCell(turnHandler.activePlayer.transform.position);
                if (turnHandler.activePlayer.moves > 0)
                {
                    turnHandler.activePlayer.HighlightStartPosition();
                }
            }
            mode = mode>move ? move:attack;
        }

        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(750, 100, 100, 40), "End turn"))
        {
            if (turnHandler.playerTurn)
            {
                SetCardsToUnused();
                HideCards();
                mode = move;
                turnHandler.activePlayer.finished = true;
                turnHandler.activePlayer.turnStarted = false;
                turnHandler.playerTurn = false;
                turnHandler.activePlayer.ClearPath();
                turnHandler.enemyTurn = true;
            }
        }
        if (mode == move)
        {
            if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(640, 100, 100, 40), "Confirm moves"))
            {
                if (turnHandler.activePlayer != null && turnHandler.activePlayer.pendingMoves > 0)
                {
                    turnHandler.confirm = true;
                }
            }
            GUI.Label(new Rect(505, 220, 200, 20), "Moves remaining: " + turnHandler.activePlayer.moves);
            GUI.Label(new Rect(505, 240, 200, 20), "Moves pending: " + turnHandler.activePlayer.pendingMoves);
        }
        else if (mode == attack && turnHandler.activePlayer != null)
        {
            WeaponCard weapon = null;
            if (selectedCard != null)
            {
                weapon = (WeaponCard)selectedCard;
                GUI.Label(new Rect(400, 260, 200, 50), "Selected card: " + selectedCard.gameObject.name);
                GenericCard card = selectedCard.gameObject.GetComponent<GenericCard>();
                GUI.Label(new Rect(400, 290, 200, 100), card.text);
            }
            if (turnHandler.activePlayer.selectedEnemy != null)
            {
                IEnemy enemy = turnHandler.FetchEnemyType(turnHandler.activePlayer.selectedEnemy);
                string name = turnHandler.activePlayer.selectedEnemy.name;
                GUI.Label(new Rect(400, 430, 200, 20), "Selected enemy: " + name.Substring(0, name.Length - 7));
                GUI.Label(new Rect(400, 460, 200, 20), "Health: " + enemy.health);
                GUI.Label(new Rect(500, 460, 200, 20), "Armor: " + enemy.armor);
                GUI.Label(new Rect(400, 490, 400, 50), "Ability: " + enemy.text);
            }
            else
            {
                if (weapon != null && !weapon.aoe)
                {
                    GUI.Label(new Rect(400, 430, 200, 20), "Choose a target on board.");
                }
                else if (weapon == null)
                {
                    GUI.Label(new Rect(400, 430, 200, 20), "Choose a card to play.");
                }
            }
            if (GUI.Button(new Rect(640, 100, 100, 40), "Use card") && (turnHandler.activePlayer.selectedEnemy != null || (weapon != null && weapon.aoe)))
            {
                string type = selectedCard.GetType() + "";
                if (type == "WeaponCard")
                {
                    weapon = (WeaponCard)selectedCard;
                    turnHandler.activePlayer.attack = weapon.damage;
                    int cost = weapon.cost;
                    int moveGain = weapon.moves;
                    if(turnHandler.activePlayer.decreaseWeaponCost)
                    {
                        cost -= turnHandler.activePlayer.decreaseWeaponCostBy;
                    }
                    else
                    {
                        turnHandler.activePlayer.decreaseWeaponCostBy = 0;
                    }
                    if (turnHandler.activePlayer.energy >= cost)
                    {
                        if (weapon.aoe && turnHandler.activePlayer.AOEAttack(weapon))
                        {
                            if (turnHandler.activePlayer.selectedEnemy == null)
                            {
                                cost -= weapon.energyGain;
                                moveGain += weapon.moveGain;
                            }
                            else
                            {
                                turnHandler.FetchEnemyType(turnHandler.activePlayer.selectedEnemy).armor -= weapon.armorRemove;
                            }
                            turnHandler.activePlayer.charge += weapon.chargeGain;
                            turnHandler.activePlayer.energy -= cost;
                            turnHandler.activePlayer.decreaseWeaponCost = weapon.decreaseNextWeaponCost;
                            turnHandler.activePlayer.decreaseWeaponCostBy = weapon.decreaseNextWeaponCostBy;
                            turnHandler.activePlayer.moves = moveGain;
                            foreach (GameObject obj in hand)
                            {
                                if (obj.GetComponent<SpriteRenderer>().sprite == weapon.sprite)
                                {
                                    obj.GetComponent<SpriteRenderer>().sprite = null;
                                    Destroy(obj.GetComponent<BoxCollider2D>());
                                    weapon.used = true;
                                }
                            }
                        }
                        else if (turnHandler.activePlayer.Attack(weapon, turnHandler.activePlayer.selectedEnemy))
                        {
                            if (turnHandler.activePlayer.selectedEnemy == null)
                            {
                                cost -= weapon.energyGain;
                                moveGain += weapon.moveGain;
                            }
                            else
                            {
                                turnHandler.FetchEnemyType(turnHandler.activePlayer.selectedEnemy).armor -= weapon.armorRemove;
                            }
                            turnHandler.activePlayer.charge += weapon.chargeGain;
                            turnHandler.activePlayer.energy -= cost;
                            turnHandler.activePlayer.decreaseWeaponCost = weapon.decreaseNextWeaponCost;
                            turnHandler.activePlayer.decreaseWeaponCostBy = weapon.decreaseNextWeaponCostBy;
                            turnHandler.activePlayer.moves = moveGain;
                            foreach (GameObject obj in hand)
                            {
                                if (obj.GetComponent<SpriteRenderer>().sprite == weapon.sprite)
                                {
                                    obj.GetComponent<SpriteRenderer>().sprite = null;
                                    Destroy(obj.GetComponent<BoxCollider2D>());
                                    weapon.used = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (turnHandler.enemyTurn && turnHandler.activeEnemy != null)
        {
            turnHandler.activeEnemy.UpdateRoom();
            GUI.Label(new Rect(505, 200, 200, 20), "Enemy room number: " + turnHandler.activeEnemy.room.number);
        }
        else if (turnHandler.playerTurn && turnHandler.activePlayer != null)
        {
            GUI.Label(new Rect(520, 35, 200, 20), turnHandler.activePlayer.transform.name + "'s Turn", style);
            GUI.Label(new Rect(400, 150, 230, 20), "Health: " + turnHandler.activePlayer.health);
            GUI.Label(new Rect(500, 150, 230, 20), "Plating: " + turnHandler.activePlayer.armor);
            GUI.Label(new Rect(600, 150, 230, 20), "Energy: " + turnHandler.activePlayer.energy);
            spriteRenderer.sprite = turnHandler.activePlayer.sprite;
            spriteRenderer.transform.position = new Vector3(transform.position.x - 1.5f, transform.position.y + 4.5f, 0);
            if (!changedSize)
            {
                spriteRenderer.size = new Vector2(spriteRenderer.size.x / 3, spriteRenderer.size.y / 3);
                changedSize = true;
            }
            if (turnHandler.activePlayer.room == null)
            {
                GUI.Label(new Rect(505, 200, 200, 20), "Player is in doorway");
            }
            else
            {
                GUI.Label(new Rect(175, 415, 200, 20), "Room " + turnHandler.activePlayer.room.number, style);
            }
        }
        //GUI.Label(new Rect(200, 15, 200, 20), "Total # of unopened doors: " + turnHandler.activePlayer.world.rooms.Count);
        /*if (turnHandler.enemyList.Count > 0 && turnHandler.activeEnemy.awaitMovement)
        {
            GUI.Label(new Rect(5, 180, 200, 20), "Selected: " + turnHandler.activeEnemy.obj.transform.name);
        }
        else
        {
            GUI.Label(new Rect(5, 180, 200, 20), "Selected: " + turnHandler.activePlayer.transform.name);
        }*/
        GUI.matrix = svMat; // restore matrix
    }
}
