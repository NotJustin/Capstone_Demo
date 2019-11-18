using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Sample : MonoBehaviour {

    public CardDeckInterface cardDeck;

    public Text selectedCardText;

    public Button nextSampleBtn;

    public Button prevSampleBtn;

    private string sampleDescription;
    	
    void Start()
    {
        nextSampleBtn.onClick.AddListener(OnNextClick);
        prevSampleBtn.onClick.AddListener(OnPrevClick);

        sampleDescription = selectedCardText.text;

        /*
         * You can use OnCardSelected callback 
         * if you prefer to wait for the animation finished
         * or the way in the Update() function to get the index right after the user click the card
         */
        cardDeck.OnCardSelected = OnCardChanged;
    }

    void OnCardChanged(int cardIndex)
    {
        Card currentCard = cardDeck.GetCurrentCard();

        if (currentCard != null)
            selectedCardText.text = sampleDescription + currentCard.tooltipMessage;
    }

	// Update is called once per frame
	void Update () {

        /*
         * You can use OnCardSelected callback 
         * if you prefer to wait for the animation finished
         * or the way below to get the index right after the user is click the card
         */
        //Card currentCard = cardDeck.GetCurrentCard();

        //if (currentCard != null)
        //    selectedCardText.text = sampleDescription + currentCard.tooltipMessage;

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            cardDeck.SetCurrentIndex(cardDeck.GetCurrentIndex() - 1);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            cardDeck.SetCurrentIndex(cardDeck.GetCurrentIndex() + 1);
        }
    }

    void OnPrevClick()
    {
		if (SceneManager.GetActiveScene().buildIndex > 0)
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex-1);
    }

    void OnNextClick()
    {
		if (SceneManager.GetActiveScene().buildIndex < 3)
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
