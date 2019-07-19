using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameObject[] choose_color_objects;
    void Update()
    {
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        TMPro.TMP_Text text_element = GameObject.Find("Players").GetComponent<TMPro.TMP_Text>();
        text_element.text = "";
        foreach(BoltEntity i in gsm.state.ConnectedPlayers)
        {
            if (i == null) continue;
            string new_line = "";
            if (i.NetworkId == gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId)
                new_line += ">";
            new_line += i.GetComponent<PlayerController>().state.Name;
            int number_of_cards = 0;
            foreach(CardObject card in i.GetState<IPlayerState>().Hand)
            {
                if (card.Type == -4) break;
                number_of_cards++;
            }
            new_line += ": " + number_of_cards.ToString();
            new_line += "\n";
            text_element.text += new_line;
        }
    }

    public void BringUpCardChoose(Card.Card_Type type, int index)
    {
        if(type == Card.Card_Type.COLOR_PICKER)
        for(int i = 0; i < 4; i++)
        {
            choose_color_objects[i].SetActive(true);
            choose_color_objects[i].GetComponent<Card>().type = Card.Card_Type.COLOR_PICKER_COLORED;
            choose_color_objects[i].GetComponent<Card>().color = (Card.Card_Color)i;
        }
        else if(type == Card.Card_Type.PLUS_FOUR)
        for (int i = 0; i < 4; i++)
        {
            choose_color_objects[i].SetActive(true);
            choose_color_objects[i].GetComponent<Card>().type = Card.Card_Type.PLUS_FOUR_COLORED;
            choose_color_objects[i].GetComponent<Card>().color = (Card.Card_Color)i;
        }
    }

    public void PlaceCardUI(int hand_index)
    {
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if(i.GetComponent<BoltEntity>().Source == null)
            {
                if (gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId != i.GetComponent<BoltEntity>().NetworkId) return;
                i.GetComponent<PlayerController>().Place_Card(hand_index);
                return;
            }
        }
    }

    public void PlaceBlackCard(int color)
    {
        int type = (int)GameObject.Find("ChooseCard").transform.GetChild(color).GetComponent<Card>().type;

        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (i.GetComponent<BoltEntity>().Source == null)
            {
                if (gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId != i.GetComponent<BoltEntity>().NetworkId) return;
                NewTopDeck evnt = NewTopDeck.Create();
                evnt.Color = color;
                evnt.Type = type;
                evnt.Send();

                if (type == (int)Card.Card_Type.PLUS_FOUR_COLORED)
                {
                    PlusN evnt1 = PlusN.Create();
                    evnt1.NumberOfCards = 4;
                    evnt1.Send();
                }

                NextRound evnt2 = NextRound.Create();
                evnt2.Skip = false;
                evnt2.Send();

                for(int x = 0; x < 4; x++)
                {
                    GameObject.Find("ChooseCard").transform.GetChild(x).gameObject.SetActive(false);
                }

                return;
            }
        }
    }

    public void Page_Number_Minus()
    {
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (i.GetComponent<BoltEntity>().Source == null)
            {
                int number_of_cards = 0;
                foreach (CardObject card in i.GetComponent<PlayerController>().state.Hand)
                {
                    if (card.Type != -4) number_of_cards++;
                }
                int number_of_pages = Mathf.CeilToInt((float)number_of_cards / 10);
                if (i.GetComponent<PlayerController>().page_number < number_of_pages - 1)
                {
                    i.GetComponent<PlayerController>().page_number++;
                }
                return;
            }
        }
    }

    public void Page_Number_Plus()
    {
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (i.GetComponent<PlayerController>().page_number != 0)
            {
                i.GetComponent<PlayerController>().page_number--;
            }
        }
    }

    public void Draw_From_Deck()
    {
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (i.GetComponent<BoltEntity>().Source == null)
            {
                if (gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId != i.GetComponent<BoltEntity>().NetworkId) return;
                IPlayerState player_state = i.GetComponent<PlayerController>().state;
                gsm.Draw_From_Deck(player_state);
                NextRound evnt = NextRound.Create();
                evnt.Send();
                return;
            }
        }
    }
}