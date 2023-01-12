using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Card : MonoBehaviour
{
    /* Big mistake with the usage of enums is that I could have used them way more to improve readability.
       Tho in my defense in Bolt they can only be stored as ints, so that was how they could be accessed as from the network.*/
    public enum Card_Color
    {
        SPECIAL_CARDS = -1,
        YELLOW = 0,
        RED = 1,
        BLUE = 2,
        GREEN = 3
    }
    public enum Card_Type
    {
        PLUS_FOUR_COLORED = -2,
        COLOR_PICKER_COLORED = -1,
        ONE = 0,
        TWO = 1,
        THREE = 2,
        FOUR = 3,
        FIVE = 4,
        SIX = 5,
        SEVEN = 6,
        EIGHT = 7,
        NINE = 8,
        ZERO = 9,
        PLUS_TWO = 10,
        DENIAL = 11,
        REVERSE = 12,
        CARD_BACK = 0,
        COLOR_PICKER = 1,
        PLUS_FOUR = 2,
        BLANK = 3
    }

    public Sprite[] card_sprites;
    public Card_Color color;
    public Card_Type type;

    void Start()
    {
        //Loads all card images from the 8671 file. (it is located in the Resources folder. unity puts stuff in the resource folder into the actual game when you build the project)
        card_sprites = Resources.LoadAll("8671").OfType<Sprite>().ToArray();
    }

    void Update()
    {
        //-4 is used as a null signifier, because it is a value not used to represent anything else, so stuff can't accidentally become -4
        if ((int)type == -4)
        {
            GetComponent<Image>().sprite = null;
            GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }
        else if ((int)color == -1)
        {
            if (type == Card_Type.COLOR_PICKER)
                GetComponent<Image>().sprite = card_sprites[1];
            else if (type == Card_Type.PLUS_FOUR)
                GetComponent<Image>().sprite = card_sprites[6];
            GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else if ((int)type < 0)
        {
            if(type == Card_Type.COLOR_PICKER_COLORED)
            {
                GetComponent<Image>().sprite = card_sprites[2 + (int)color];
            }
            else if(type == Card_Type.PLUS_FOUR_COLORED)
            {
                GetComponent<Image>().sprite = card_sprites[7 + (int)color];
            }
            GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            GetComponent<Image>().sprite = card_sprites[11 + (int)color * 13 + (int)type];
            GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
    }
}