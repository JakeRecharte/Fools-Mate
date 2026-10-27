**Fool's Mate**

*Working title. Alternates: Pawnbreaker, Pawnbroker.*

**Overview**

- Fool's Mate is a Balatro-inspired chess roguelike.
- YOU select your King, each with their own unique board and pieces, and fight through a series of opponents, upgrading your board and pieces with modifiers, or downright swapping them out.
- Collect points as you take down your opponent's pieces, just be sure to pay the Levy before you run out of turns.
- Spend points in the shop on match buffs and piece upgrades that turn the tide in your favor.
- Fight Boss enemies with their own twist: capture guarded fortresses, find your way through the fog of war, or even battle the board itself.

**Controls**
Keyboard:

- WASD or arrow keys to navigate menus, Esc to exit a menu, Tab (expect to change) to bring up run info

Mouse/touchpad:

- Point and click; dragging is also required (moving pieces)

**Art Assets**
Style:

- 640x360 pixel graphics
- Mostly deep purple (instead of black) and cream (for white pieces)
- Piece colors follow standard chess: black pieces are purple, white pieces are cream. Since the player's color is rolled each round (see Color Roll), the player's pieces are sometimes purple and sometimes cream
- Accent colors for menus, piece variants, etc.

Assets:

- An asset for each chess piece, plus variants of each piece (start with 4 obtainable variants)
- Animation when you click and drag a piece that shows a hand grabbing it (either full size or a mini cartoony hand)
- HUD/UI: simple, mostly information based
- Move preview: before pressing MOVE, show ghost paths for queued moves, the enemy's telegraphed attacks, and markers on squares where pieces will collide
- Reserved-square marker: an outline of your piece on the square it is moving into
- Shop/upgrades page
- Boss pieces
- Alternate board tiles, including tile modifiers (see Tiles)

**Audio Assets**
Styling:

- Mix between medieval/classical music and an 8-bit feel

Assets:

- Piece moving: a deep clunk
- Piece fighting/capturing: deep crumbling noise, followed by a ching to mark a point score
- Menu: clicking on chess pieces, menu/settings navigation, shop/purchase noise

Sourcing:

- Find suitable audio after gameplay is set. Fallback: CC0 music packs and an 8-bit SFX generator (sfxr/jsfxr/ChipTone).

**Game Flow**
Run structure:

- 4 tiers of 4 rounds each (16 rounds per run). The 4th round of each tier is a boss.
- Shop between rounds.
- Tier bosses are named after the chess titles, in ascending order:
  1. Candidate Master (CM)
  2. FIDE Master (FM)
  3. International Master (IM)
  4. Grandmaster (GM)
- Boss twists: guarded fortress, fog of war, the board itself (hazard tiles, see Tiles), 4th TBD.

Kings:

| King | Lives | Starting board |
|---|---|---|
| King | 2 | Regular board |
| Baron | 1 | Regular board, but the outermost pawns (a- and h-file) are upgraded to Knights |

- Your King is on the board and has 1 Health, like any other piece.
- **Lives** are a separate stat set by your King choice. You lose a life each time your King is taken or you fail to pay the Levy. Losing a life ends the round and you move on to the next one. Points earned in that round still count toward the shop. Losing all lives ends the run.

Color Roll:

- At the start of each round, the player is randomly assigned white or black. The enemy plays the other color.
- The board is always shown from the player's side: the player's pieces start at the bottom of the screen. When the player is black, the board is rotated 180°, as in chess apps.
- Since moves resolve at the same time, white has no first-move advantage. What changes is the layout: as in standard chess, both queens start on the d-file, so from the player's view the queen is left of the King when playing white and right of the King when playing black.
- Anything the player owns that sits on a square (tile modifiers, custom starting layouts) is stored from the player's point of view and placed on the real board at the start of each round (see Tiles).

Turn structure:

1. The enemy telegraphs its moves.
2. You spend energy to queue your moves, either dodging or sacrificing pieces against the telegraphed attacks.
3. Press MOVE. Your moves and the enemy's moves go off at the same time.

- Energy: moving one piece costs 1 energy.
- Not all enemy moves are telegraphed.
- Board: standard 8x8 to start.
- Standard chess rules apply: castling, en passant, pawn promotion, and check. If your King is in check you must respond to it, and you can't move into check. Only telegraphed attacks put your King in check. Untelegraphed attacks can take your King without warning, which keeps an element of real chess.

Round goal:

- Win a round by either:
  - paying the **Levy** (earning the round's point target) within the turn limit, or
  - capturing the enemy King.
- If the turn limit runs out before the Levy is paid, you lose a life and move on to the next round.
- Enemy Kings have 1 Health in regular rounds. Boss Kings are sturdier (Health TBD).

Level 1 (playtest target, tune this first):

| Levy | Turn limit | Energy per turn |
|---|---|---|
| 10 points | 5 turns | 2 |

Piece stats:

- Every piece has **Priority**, **Damage**, and **Health**.
- Points per capture use official chess piece values to start:

| Piece | Points | Priority | Damage | Health |
|---|---|---|---|---|
| Pawn | 1 | TBD | TBD | TBD |
| Knight | 3 | TBD | TBD | TBD |
| Bishop | 3 | TBD | TBD | TBD |
| Rook | 5 | TBD | TBD | TBD |
| Queen | 9 | TBD | TBD | TBD |
| King | Win | TBD | TBD | 1 |

**Move Resolution**
When MOVE is pressed:

- **Attack** (moving onto a piece that isn't moving): the defender takes the attacker's Damage and is captured if its Health hits 0. If the defender survives, the attacker is Bounced. To test: the attacker gets +1 Priority for taking the offense.
- **Collision** (two pieces move into the same square):
  1. Both pieces deal their Damage to each other at the same time. A piece at 0 Health dies.
  2. If one dies, the survivor takes the square and its owner earns the dead piece's points.
  3. If both die, the owner of the higher-Priority piece earns the points.
  4. If both survive, two tracks:
     - Track 1 (default): both pieces die. The owner of the higher-Priority piece earns the points.
     - Track 2 (to test): the higher-Priority piece takes the square and earns 1 point for taking it. The lower-Priority piece is Bounced.
  5. If Priority is tied in steps 3 or 4, both pieces die and no points are awarded.
- **Swap** (two pieces move into each other's squares): they pass through each other and swap squares, nothing else happens. Playtest this as a mechanic.
- **Dodge**: a piece that leaves a square that was being attacked dodges the attack.
- **Blocking**: sliding pieces (Bishop, Rook, Queen) are blocked by any piece on their path at the start of the turn, even if that piece is moving away.
- **Reserved squares**: a square one of your pieces is moving into is marked with that piece's outline, and none of your other pieces can move into it. The enemy's telegraphed destinations do not block your pieces.

**Bounce**
A shared event used by multiple mechanics (collisions, tiles, bosses, etc.):

- The piece moves to a random unoccupied square within 2 tiles.
- If there is no unoccupied square within 2 tiles, the piece dies.

**Enemy**

- No trained model. The enemy uses rule-based decision logic to choose which attacks to throw out each turn, which of them are telegraphed, and how it responds to your board.

**Upgrade**

- Currency: points are the only currency.
- Shop: use your captured points to buy piece upgrades from whatever the shop has in stock, or to upgrade tiles.
- Piece upgrades modify:
  - Priority
  - Damage
  - Health
  - Double Points: the piece earns double points from pieces it captures
- Upgrades belong to the piece, not to a square, so they carry over unchanged when the color roll flips the board.
- Piece storage (later addition): a bin of extra pieces bought from the shop. Between rounds, the player can swap pieces from the bin into their starting army.

**Tiles**

- Tiles give bonuses or detriments to the pieces on them and cannot be removed easily once placed.
- Example tile modifiers:
  - Damage on entry
  - Damage each turn a piece stays on it
  - Missing tile (hole in the board)
  - Damage buff when exiting the tile
- Placement after a color roll. When the board flips, each tile modifier has to land somewhere:
  - **A. Follows the piece (default):** the tile keeps the same file and its rank is flipped, so it stays under the same starting piece. A tile under your queen stays under your queen.
  - **B. Stays where it was on screen (possibility):** the tile rotates 180° with the board. It stays on the player's side in the same screen position, but on the d- and e-files it ends up under the King instead of the queen.
  - **C. Stays on its real square (possibility):** the tile doesn't move, so after a flip your modifiers are on the enemy's side and buff them. Better as a deliberate gamble than a default, for example a cheap but powerful "unstable" modifier, or a boss twist for the "the board itself" fight.
  - Each modifier could carry its own placement rule, so A and C can exist side by side.

**Die/Win**

- Use some sort of marker to indicate run progression vs past runs, maybe use chess ELO in a creative way
- If won, increase difficulty for the next run
- View new unlocked options: Kings, pieces, or board enchantments you can get on your run

Repeat:

- The cycle repeats; players try to get farther and farther, unlocking trophies

**Technical**

- Unity 6000.5, 2D URP
- Pixel Perfect Camera with a 640x360 reference resolution (integer scaling: x2 = 720p, x3 = 1080p, x4 = 1440p, x6 = 4K)
- Sprite import: Point (no filter) filtering, no compression, one consistent Pixels Per Unit across all art
- Pixels Per Unit: 32. Board squares are 32x32 px, so 1 Unity unit = 1 board square (8x8 board = 256x256 px)
- Code structure:
  - Game rules (board state, move resolution, scoring, enemy logic) live in plain C# classes that don't depend on Unity scenes or GameObjects.
  - Unity scripts (MonoBehaviours, Tilemap, UI) only display the board state and pass player input to the rules.
  - Game data (Kings, pieces, upgrades, tiles) lives in ScriptableObjects, so new content is made in the Inspector instead of in code.
  - Game rules use real chess colors and squares (a1 is always a1). Only the display knows about the 180° flip, through one conversion between squares and screen positions.
  - Two levels of state:
    - Run (kept between rounds): each piece is a unique object with its own ID, type or variant, stats, and upgrades, but no color or square. Tile modifiers are stored with their ID, placement rule, and a position from the player's point of view.
    - Round (rebuilt each round): the color is rolled and the army is placed on real squares. The board holds a piece grid of piece IDs and a modifier grid of modifier IDs.
  - This lets move resolution be unit tested with the Unity Test Framework without running the game.

**Testing**

- Playtest with people who haven't seen the game, especially once a full round is playable
- Playtest items: collision Track 1 vs Track 2, attacker +1 Priority, swap pass-through

**TO-DO**

- Shop: stock size, prices, rerolls
- Piece stats: Priority, Damage, Health for each piece
- Levy and turn limit for rounds after Level 1, and how energy grows
- 4th boss twist
- Boss King Health for each tier
- The 4 piece variants
- More Kings
- Piece storage bin: capacity, shop pricing, when swaps are allowed
- Which tile modifiers (if any) use placement B or C

**Concerns**

- Enemy logic: doesn't need a trained model, but the rule-based logic that chooses which attacks to telegraph still has to be designed and tuned
- Genuinely terrible with audio, might have to spend some extra time on this
- The scope of the game may be difficult for the duration of the project; might have to make a mini sprint version for the due date and then keep working on it later

**Discarded Concepts**

- 2D platformer with a few options for movement, but each time you use one it changes the obstacles; think two layers, with movement toggling between them
- 2D roguelike similar to Balatro but with dice; each side has different effects and modifiers
- Balatro but as chess - PICKED
