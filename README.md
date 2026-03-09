# Blackjack Roguelike

A Blackjack game with roguelike scoring mechanics built in Unity. Play hands, build win streaks, exploit suit synergies, and drain the dealer's chips before they drain yours.

> Base card feel and visual system adapted from [André Cardoso's Balatro Feel project](https://github.com/cardosoandre) (Mix and Jam).

---

## What It Is

Classic Blackjack rules extended with a scoring layer inspired by *Balatro*: every hand you win generates chips based on card values, suit combinations, hand composition, and your current win streak. The goal isn't just to beat 21 — it's to squeeze the most chip value out of every hand.

---

## Core Loop

1. **Deal** — Two cards dealt to player and dealer (dealer's second card stays face-down)
2. **Decide** — Hit, Stand, play or discard from your consumable hand
3. **Resolve** — Dealer auto-plays to 17+, hands compared, chips transfer
4. **Score** — Chips won/lost calculated with bonuses, multipliers, and streak modifiers
5. **Repeat** — Until someone hits 0 chips or the deck runs dry

---

## Scoring System

Scoring goes beyond win/lose. Each round calculates:

**Base Chips**
- Sum of all card values in your hand
- Suit bonuses: Flush (+5), Zebra alternating colors (+3), All face cards (+4), Paired starting hand (+2)

**Tier Multiplier** (based on how you won)
- Blackjack (2-card 21): 2.0×
- 3-card 21: 1.75×
- 5+ card hand: 1.5×
- Regular win: 1.0×
- Push: 0.25×
- Loss: −1.0×

**Streak Multiplier** (consecutive wins)
- 2 wins: 1.25× / 3 wins: 1.5× / 5 wins: 2.0× / 7+ wins: 3.0×

All multipliers stack. A Blackjack on a 7-win streak hits 6.0× before hand bonuses.

---

## Implemented Features

- Full 52-card deck with shuffle and running count tracking (Hi-Lo system)
- Cards dealt face-down, flipped mid-transfer to destination
- Real-time bust probability display based on remaining deck composition
- Ace value automatically adjusts (11 → 1) to avoid busts
- Three card holder areas: Player hand, Dealer hand, Consumable hand
- Animated score breakdown UI reveals all calculation components after each hand
- Chip economy: Player starts at 100, Dealer at 50 — zero-sum transfers each round
- Game over states: lose all chips, deck depleted, or dealer goes bust

---

## Planned / Potential Directions

- **Joker cards** — passive modifiers that alter scoring rules mid-run
- **Shop phase** — spend chips between rounds on new cards or jokers
- **Multiple decks / deck building** — draft and customize your deck composition
- **Special card effects** — individual cards with unique scoring triggers
- **Persistent progression** — unlockable jokers, card backs, or rule modifiers across runs
- **Sound design pass** — full audio feedback for streaks, bonuses, and game events

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 6000.3.8f1 |
| Language | C# |
| Animation | DOTween |
| UI | Unity UI + TextMesh Pro |
| Input | Unity Input System |
| Architecture | State machine, event-driven, singleton managers |

---

## Getting Started

1. Clone or download the repo
2. Open the root folder in Unity 6000.3 or higher
3. Press Play
