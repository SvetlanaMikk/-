using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Card8
{
    using Card = (Suit, string);

    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private bool IsComputerTurn = false;

        private readonly int cardWidth = 80;
        private readonly int cardHeight = 110;
        private Rectangle cardRect;
        private readonly Rectangle buttonRect = new(0, 0, 60, 60);

        private int centerX;
        private int centerY;

        private Image shirtImage = new Bitmap(1, 1);
        private Image backgroundImage = new Bitmap(1, 1);

        private readonly Dictionary<Suit, Image[]> cardImages = [];

        private GameFinishState gameFinishState = GameFinishState.None;

        private bool checkSuitNeeded = false;
        private Suit? checkedSuit = null;

        private bool CardIsRight(Card card)
        {
            var lastDiscard = GetLastDiscard();
            if (checkedSuit.HasValue)
            {
                if (card.Item1 == checkedSuit.Value) return true;
            }
            else
            {
                if (card.Item1 == lastDiscard.Item1) return true;
            }
            
            if (card.Item2 == lastDiscard.Item2) return true;
            return false;
        }

        private void LoadBackgroundImage()
        {
            var formRect = new Rectangle(0, 0, this.Width, this.Height);
            backgroundImage = Helpers.Resized(Image.FromFile($"background.png"), formRect);
        }

        private void LoadSuitImages(Suit suit)
        {
            string suitName = Helpers.GetSuitName(suit);
            var images = new Image[Helpers.CardNumbers.Length];
            cardImages.Add(suit, images);
            for(int i = 0; i < Helpers.CardNumbers.Length; i++)
            {
                var value = Helpers.CardNumbers[i];
                cardImages[suit][i] = Helpers.Resized(Image.FromFile($"cards\\{suitName}-{value}.png"), cardRect);
            }
        }

        private readonly List<Card> deck = [];

        private readonly List<Card> computerHand = [];
        private readonly List<Card> playerHand = [];

        private readonly List<Card> discard = [];

        private Card GetLastDiscard() => discard[^1];

        void MakeDeckSuit(Suit suit)
        {
            for (int i = 0; i < Helpers.CardNumbers.Length; i++)
            {
                deck.Add((suit, Helpers.CardNumbers[i]));
            }
        }

        private Card HandOver()
        {
            var result = deck[0];
            deck.RemoveAt(0);
            return result;
        }

        private void InitialHandOver(List<Card> hand)
        {
            for(int i = 0; i < 7; i++)
            {
                hand.Add(HandOver());
            }
        }

        private readonly List<PictureBox> pictureBoxes = [];
        private int pictureBoxOldIndex = 0;
        private int pictureBoxIndex = 0;

        private PictureBox TakePictureBox()
        {
            if (pictureBoxIndex >= pictureBoxes.Count)
            {
                pictureBoxes.Add(new PictureBox());
            }

            var result = pictureBoxes[pictureBoxIndex];
            Helpers.RemoveEvent(result, "s_clickEvent");
            this.Controls.Add(result);
            pictureBoxIndex++;
            return result;
        }

        private void DisplayGameState_start()
        {
            pictureBoxIndex = 0;
        }

        private void DisplayGameState_finish()
        {
            for(int i = pictureBoxIndex; i < pictureBoxOldIndex; i++)
            {
                var pictureBox = pictureBoxes[i];
                this.Controls.Remove(pictureBox);
            }
            pictureBoxOldIndex = pictureBoxIndex;
        }

        private void DisplayCard(Card card, int x = 0, int y = 0, bool isShirt = false, EventHandler? click = null)
        {
            var pbCard = TakePictureBox();
            pbCard.Width = cardWidth;
            pbCard.Height = cardHeight;
            pbCard.Left = x;
            pbCard.Top = y;
            pbCard.Image = isShirt ? shirtImage : cardImages[card.Item1][Helpers.CardNumber2Index(card.Item2)];

            pbCard.Tag = card;
            if (click is not null)
            {
                pbCard.Click += click;
            } 
        }

        private void DisplayHand(List<Card> hand, int y = 0, bool isShirt = false, EventHandler? click = null)
        {
            int startX = this.Width > 80 * hand.Count ? centerX - hand.Count * 80 / 2 : 0;
            int step = this.Width > 80 * hand.Count ? 80 : (int)(this.Width / hand.Count);
            for (int i = hand.Count -1; i >= 0; i--)
            {
                DisplayCard(hand[i], x: startX + step * i, y: y, isShirt: isShirt, click: click);
            }
        }

        private void DisplayBackground()
        {
            var pbBackground = TakePictureBox();
            pbBackground.Width = this.Width;
            pbBackground.Height = this.Height;
            pbBackground.Left = 0;
            pbBackground.Top = 0;
            pbBackground.Image = backgroundImage;
        }
        private void DisplayMessage(string name)
        {
            var formRect = new Rectangle(0, 0, this.Width, this.Height);
            var messageImage = Helpers.Resized(Image.FromFile($"messages\\{name}.png"), formRect);

            var pbMessage = TakePictureBox();
            pbMessage.Width = this.Width;
            pbMessage.Height = this.Height;
            pbMessage.Left = 0;
            pbMessage.Top = 0;
            pbMessage.Image = messageImage;
        }

        private void DisplaySuitLabel(string name, int x = 0, int y = 0)
        {
            var suitImage = Helpers.Resized(Image.FromFile($"suits\\{name}.jpg"), buttonRect);
            var pbSuit = TakePictureBox();
            pbSuit.Width = 60;
            pbSuit.Height = 60;
            pbSuit.Left = x;
            pbSuit.Top = y;
            pbSuit.Tag = name;
            pbSuit.Image = suitImage;
            pbSuit.Click += PbSuit_Click;
        }

        private void DisplayCheckSuitButton(string name, int x = 0, int y = 0)
        {
            var suitImage = Helpers.Resized(Image.FromFile($"suits\\{name}.jpg"), buttonRect);
            var pbSuit = TakePictureBox();
            pbSuit.Width = 60;
            pbSuit.Height = 60;
            pbSuit.Left = x;
            pbSuit.Top = y;
            pbSuit.Tag = name;
            pbSuit.Image = suitImage;
            pbSuit.Click += PbSuit_Click;
        }

        private void DisplayCheckSuit()
        {
            DisplayCheckSuitButton("hearts", x: 340, y: 240);
            DisplayCheckSuitButton("crosses", x: 425, y: 240);
            DisplayCheckSuitButton("spades", x: 515, y: 240);
            DisplayCheckSuitButton("diamonds", x: 600, y: 240);
        }

        private void DisplayGameState()
        {
            DisplayGameState_start();
            if (gameFinishState == GameFinishState.WinPlayer)
            {
                DisplayMessage("WinPlayer");
            } 
            else if (gameFinishState == GameFinishState.WinComputer)
            {
                DisplayMessage("WinComputer");
            } 
            else if (gameFinishState == GameFinishState.Tie)
            {
                DisplayMessage("Tie");
            } 
            else
            {
                if (checkSuitNeeded)
                {
                    DisplayCheckSuit();
                }

                DisplayHand(computerHand, y: 20, isShirt: true);

                if (deck.Count > 0)
                {
                    DisplayCard(deck[0], x: centerX - 40, y: centerY - 75, isShirt: true, click: DeckCard_Click);
                }

                if (checkedSuit.HasValue)
                {
                    DisplaySuitLabel(Helpers.GetSuitName(checkedSuit.Value), x: centerX + 100, y: centerY - 100);
                }

                if (discard.Count > 0)
                {
                    DisplayCard(GetLastDiscard(), x: centerX - 40 + 100, y: centerY - 75);
                }

                DisplayHand(playerHand, y: 320, click: PlayerHandCard_Click);
                DisplayBackground();
            }
            DisplayGameState_finish();
        }

        private void CheckFinishState()
        {
            if (playerHand.Count == 0) gameFinishState = GameFinishState.WinPlayer;
            if (computerHand.Count == 0) gameFinishState = GameFinishState.WinComputer;
            if (deck.Count != 0) return;

            foreach(var card in playerHand)
            {
                if (CardIsRight(card))
                {
                    return;
                }
            }

            gameFinishState = GameFinishState.Tie;
        }

        private void ComputerTurn()
        {
            while(true)
            {
                var availableCards = new List<Card>();
                foreach (var card in computerHand)
                {
                    if (CardIsRight(card))
                    {
                        availableCards.Add(card);
                    }
                }

                if (availableCards.Count > 0)
                {
                    var takenCard = availableCards[Helpers.RandomNumber(availableCards.Count)];

                    computerHand.Remove(takenCard);
                    discard.Add(takenCard);
                    checkedSuit = null;
                    if (takenCard.Item2 == "8")
                    {
                        checkedSuit = Helpers.RandomNumber(4) switch
                        {
                            0 => Suit.Hearts,
                            1 => Suit.Crosses,
                            2 => Suit.Spades,
                            3 => Suit.Diamonds,
                            _ => throw new ApplicationException()
                        };
                    }
                        
                    if (computerHand.Count == 0)
                    {
                        gameFinishState = GameFinishState.WinComputer;
                    }
                    return;
                }

                if (deck.Count == 0)
                {
                    gameFinishState = GameFinishState.Tie;
                    return;
                }
                computerHand.Add(HandOver());
            }
        }

        private async void ComputerTurnAndDisplay()
        {
            IsComputerTurn = true;
            await Task.Delay(1000);
            ComputerTurn();
            DisplayGameState();
            IsComputerTurn = false;
        }

        private void PbSuit_Click(object? sender, EventArgs e)
        {
            if (sender is null) return;
            PictureBox pictureBox = (PictureBox)sender;
            if (pictureBox.Tag is null) return;
            var suit = (string)pictureBox.Tag;
            checkedSuit = suit switch
            {
                "hearts" => Suit.Hearts,
                "crosses" => Suit.Crosses,
                "spades" => Suit.Spades,
                "diamonds" => Suit.Diamonds,
                _ => throw new ApplicationException(),
            };
            checkSuitNeeded = false;
            DisplayGameState();
            ComputerTurnAndDisplay();
        }

        private void PlayerHandCard_Click(object? sender, EventArgs e)
        {
            if (sender is null) return;
            if (IsComputerTurn) return;
            PictureBox pictureBox = (PictureBox)sender;
            if (pictureBox.Tag is null) return;
            var card = (Card)pictureBox.Tag;
            if (!CardIsRight(card))
            {
                MessageBox.Show($"Card must be one suit or value with discard.");
                return;
            }
            playerHand.Remove(card);
            discard.Add(card);
           
            checkedSuit = null;
            CheckFinishState();
            if (card.Item2 == "8")
            {
                checkSuitNeeded = true;
                DisplayGameState();
            } else
            {
                DisplayGameState();
                ComputerTurnAndDisplay();
            }   
        }

        private void DeckCard_Click(object? sender, EventArgs e)
        {
            if (sender is null) return;
            if (IsComputerTurn) return;
            playerHand.Add(HandOver());
            CheckFinishState();
            DisplayGameState();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            centerX = Width / 2;
            centerY = Height / 2;
            cardRect = new Rectangle(0, 0, cardWidth, cardHeight);

            shirtImage = Helpers.Resized(Image.FromFile("cards\\shirt.jpg"), cardRect);

            LoadBackgroundImage();

            LoadSuitImages(Suit.Hearts);
            LoadSuitImages(Suit.Crosses);
            LoadSuitImages(Suit.Spades);
            LoadSuitImages(Suit.Diamonds);

            MakeDeckSuit(Suit.Hearts);
            MakeDeckSuit(Suit.Crosses);
            MakeDeckSuit(Suit.Spades);
            MakeDeckSuit(Suit.Diamonds);
            deck.Shuffle();

            InitialHandOver(playerHand);
            InitialHandOver(computerHand);

            while(true)
            {
                var card = HandOver();
                if (card.Item2 != "8")
                {
                    discard.Add(card);
                    break;
                }
                deck.Add(card);
            }

            DisplayGameState();
        }
    }

    public enum GameFinishState
    {
        None, Tie, WinComputer, WinPlayer
    }

    public enum Suit
    {
        Hearts, Crosses, Spades, Diamonds
    }

    public static class Helpers
    {
        private static readonly Random rng = new();

        public static int RandomNumber(int limit)
        {
            return rng.Next(0, limit);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public static void RemoveEvent(Control ctl, string event_name)
        {
            FieldInfo? field_info = typeof(Control).GetField(event_name,
                BindingFlags.Static | BindingFlags.NonPublic);

            PropertyInfo? property_info = ctl.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field_info is null) return;
            object? obj = field_info.GetValue(ctl);
            if (property_info is null) return;
            EventHandlerList? event_handlers =
                (EventHandlerList?)property_info.GetValue(ctl, null);
            if (event_handlers is null) return;
            if (obj is null) return;
            event_handlers.RemoveHandler(obj, event_handlers[obj]);
        }

        private static readonly string[] cardNumbers = ["2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"];

        public static string[] CardNumbers { get { return cardNumbers; } }

        public static string GetSuitName(Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "hearts",
                Suit.Crosses => "crosses",
                Suit.Spades => "spades",
                Suit.Diamonds => "diamonds",
                _ => throw new ApplicationException(),
            };
        }

        public static int CardNumber2Index(string number)
        {
            return Array.IndexOf(cardNumbers, number);
        }

        public static Bitmap Resized(Image source, Rectangle tgtRect)
        {
            var result = new Bitmap(tgtRect.Width, tgtRect.Height);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(source, tgtRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
            }
            return result;
        }
    }
    
}
