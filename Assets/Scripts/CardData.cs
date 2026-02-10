using UnityEngine;

public enum Suit
{
    None,
    Hearts,
    Diamonds,
    Clubs,
    Spades,
}

public enum Rank
{
    None,
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King
}

[CreateAssetMenu(menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public Suit suit;
    public Rank rank;
    public Sprite frontSprite;


}