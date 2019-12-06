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
        hand = new GameObject[7];
        for (int i = 0; i < transform.Find("Hand").childCount; i++)
        {
            hand[i] = transform.Find("Hand").GetChild(i).gameObject;
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
                    currentCard.size = new Vector2(currentCard.size.x / 2.2f, currentCard.size.y / 2.2f);
                    BoxCollider2D boxCollider = (BoxCollider2D)hand[i].AddComponent<BoxCollider2D>();
                    boxCollider.size = currentCard.size;
                    hand[i].GetComponent<ClickOnCard>().card = turnHandler.activePlayer.cards[i].GetComponent<GenericCard>();
                    changedCardSize = true;
                }
                hand[i].transform.position = new Vector3(transform.position.x + i * 2.3f - 2.0f, transform.position.y, 0);
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

    void OnGUI()
    {
        if (!turnHandler.activePlayer.moving && GUI.Button(new Rect(530, 100, 100, 40), "Toggle Mode"))
        {
            if (mode == move)
            {
                turnHandler.activePlayer.ClearPath();
                if (turnHandler.activePlayer.transform.gameObject.name == "Pioneer")
                {
                    ShowCards();
                }
            }
            else
            {
                HideCards();
                turnHandler.activePlayer.selectedEnemy = null;
                selectedCard = null;
                Vector3Int playerCell = turnHandler.activePlayer.world.world.WorldToCell(turnHandler.activePlayer.transform.position);
                Debug.Log(turnHandler.activePlayer.attacked);
                if (turnHandler.activePlayer.moves > 0 && !turnHandler.activePlayer.attacked)
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
            if (turnHandler.activePlayer.selectedEnemy != null)
            {
                GUI.Label(new Rect(400, 430, 200, 20), "Selected enemy: " + turnHandler.activePlayer.selectedEnemy.tag);
                IEnemy enemy = turnHandler.FetchEnemyType(turnHandler.activePlayer.selectedEnemy);
                GUI.Label(new Rect(400, 460, 200, 20), "Health: " + enemy.health);
                GUI.Label(new Rect(500, 460, 200, 50), "Ability: " + enemy.text);
            }
            else
            {
                if (selectedCard != null)
                {
                    GUI.Label(new Rect(400, 430, 200, 20), "Choose a target on board.");
                }
                else
                {
                    GUI.Label(new Rect(400, 430, 200, 20), "Choose a card to play.");
                }
            }
            if (selectedCard != null)
            {
                GUI.Label(new Rect(400, 330, 200, 20), "Selected card: " + selectedCard.gameObject.name);
                GenericCard card = selectedCard.gameObject.GetComponent<GenericCard>();
                GUI.Label(new Rect(400, 360, 500, 30), card.text);
            }
            if (GUI.Button(new Rect(640, 100, 100, 40), "Use card") && turnHandler.activePlayer.selectedEnemy != null)
            {
                string type = selectedCard.GetType() + "";
                if (type == "WeaponCard")
                {
                    WeaponCard weapon = (WeaponCard)selectedCard;
                    turnHandler.activePlayer.attack = weapon.damage;
                    if (turnHandler.activePlayer.energy >= weapon.cost && turnHandler.activePlayer.Attack(turnHandler.activePlayer.selectedEnemy))
                    {
                        turnHandler.activePlayer.energy -= weapon.cost;
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
    }
}
