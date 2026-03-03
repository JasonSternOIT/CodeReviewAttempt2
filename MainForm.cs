using System.Drawing.Text;
using System.Media;

namespace CodeReview
{
    public partial class MainForm : Form
    {
        enum State
        {
            TitleScreen,
            GamePlay,
            GameOver
        }
        State state = State.TitleScreen;

        int currentScore = 0;

        int backgroundAnimationState = 0;
       
        int miliFrameIndex = 0;

        enum Food
        {
            Candy,
            Chips,
            Ramen,
            Soda,
            Lunchable,
            Seaweed,
            Mac
        }
        
        struct Delays
        {
            public int updateBackground = 0;
            public int miliFrame = 0;
            public int candy = 0;
            public int chips = 0;
            public int ramen = 0;
            public int soda = 0;
            public int lunchable = 0;
            public int seaweed = 0;
            public int mac = 0;
            public int fail = 0;

            public Delays()
            {
            }
        }

        Delays delay = new Delays();

        readonly Random random = new();

        DateTimeOffset startTime;
        DateTimeOffset endTime;

        readonly Food[] snackTime;

        SoundPlayer backgroundSound1;
        SoundPlayer backgroundSound2;

        Color[] backgroundColors = {
            Color.HotPink,
            Color.DeepPink,
            Color.MediumVioletRed,
            Color.Cyan,
            Color.DarkTurquoise,
            Color.Teal,
            Color.HotPink,
            Color.DeepPink,
            Color.MediumVioletRed,
            Color.Cyan,
            Color.DarkTurquoise,
            Color.Teal,
            Color.HotPink,
            Color.DeepPink,
            Color.MediumVioletRed,
            Color.Cyan,
            Color.DarkTurquoise,
            Color.Teal
        };
        List<PictureBox> backgroundObjects;

        public MainForm()
        {
            snackTime = (Food[])Enum.GetValues(typeof(Food));

            InitializeComponent();

            if(MessageBox.Show($"Transparency is broken layering PictureBox objects in WinForms.{Environment.NewLine}" +
                $"Regardless, you will have graphical issues. Depending on what you have underneath," +
                $"transparency might or might not lead to a more fun gaming experience.{Environment.NewLine}Enable?",
                $"Enable Transparency?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AllowTransparency = true;
                TransparencyKey = Color.Black;
            }
            else
            {
                AllowTransparency = false;
            }

            avatar.BringToFront();
            mili.BringToFront();

            PrivateFontCollection privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CFPunkAttitude-Regular.ttf"));
            score.Font = new Font(privateFonts.Families[0], 12);
            time.Font = score.Font;
            gameOverFinalScore.Font = score.Font;
            fail.Font = new Font(privateFonts.Families[0], 24);
            clickToStart.Font = fail.Font;

            try
            {
                backgroundSound1 = new SoundPlayer(@"looping-video-game-background-music-for-a-hungry-girl-searching-for-snacks-1.wav");
                backgroundSound2 = new SoundPlayer(@"looping-video-game-background-music-for-a-hungry-girl-searching-for-snacks-2.wav");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sound files: {ex.Message}");
                throw;
            }

            backgroundObjects = [background1, background2, background3, background4, background5, background6];

            TransitionState(State.TitleScreen);
            updateLoopTimer.Start();
        }

        void SetSnackVisibility(bool visible)
        {
            foreach (Food snack in snackTime)
            {
                PictureBox foodObject = getFoodObjectInstance(snack);
                foodObject.Visible = visible;
            }
        }

        void setFailVisibility(bool visible)
        {
            fail.Visible = visible;
            lightningLeft.Visible = visible;
            lightningRight.Visible = visible;
            delay.fail = visible ? 24 : 0;
        }

        void TransitionState(State newState)
        {
            state = newState;
            switch (state)
            {
                case State.TitleScreen:
                    {
                        score.Visible = false;
                        time.Visible = false;
                        title.Visible = true;
                        avatar.Visible = true;
                        mili.Visible = false;
                        clickToStart.Visible = true;
                        gameOverFinalScore.Visible = false;
                        
                        SetSnackVisibility(false);
                        Cursor.Show();
                        backgroundSound2.Stop();
                        backgroundSound1.PlayLooping();
                    }
                    break;
                case State.GamePlay:
                    {
                        startTime = DateTimeOffset.Now;
                        endTime = startTime.AddMinutes(1);
                        currentScore = 0;                        
                        score.Visible = true;
                        time.Visible = true;
                        title.Visible = false;
                        avatar.Visible = false;
                        mili.Visible = true;
                        clickToStart.Visible = false;
                        gameOverFinalScore.Visible = false; 
                        foreach (Food snack in snackTime)
                        {
                            InitializeFood(snack);
                        }
                        SetSnackVisibility(true);

                        UpdateScore();
                        UpdateTimeRemaining();

                        Cursor.Hide();
                        backgroundSound1.Stop();
                        backgroundSound2.PlayLooping();
                    }
                    break;
                case State.GameOver:
                    {
                        score.Visible = false;
                        time.Visible = false;
                        title.Visible = false;
                        avatar.Visible = true;
                        mili.Visible = false;
                        clickToStart.Visible = false;
                        gameOverFinalScore.Visible = true;
                        SetSnackVisibility(false);
                        UpdateGameOverFinalScore();
                        Cursor.Show();
                        backgroundSound2.Stop();
                        backgroundSound1.PlayLooping();
                    }
                    break;
            }
            setFailVisibility(false);
        }

        private void UpdateScore()
        {
            score.Text = $"Score: {currentScore}";
        }

        private void UpdateGameOverFinalScore()
        {
            gameOverFinalScore.Text = $"GaMe OvEr{Environment.NewLine}Score: {currentScore}";
        }

        void InitializeFood(Food food)
        {
            PictureBox foodObject = getFoodObjectInstance(food);
            if (foodObject == null)
            {
                return;
            }
            foodObject.Size = new Size(8, 8);
            foodObject.Location = new Point(32 + (random.Next() % (Size.Width - 128)), 32 + (random.Next() % (Size.Height - 128)));
        }

        private PictureBox getFoodObjectInstance(Food food)
        {
            PictureBox? foodObject = null;
            switch (food)
            {
                case Food.Candy:
                    foodObject = candy;
                    break;
                case Food.Chips:
                    foodObject = chips;
                    break;
                case Food.Ramen:
                    foodObject = ramen;
                    break;
                case Food.Soda:
                    foodObject = soda;
                    break;
                case Food.Lunchable:
                    foodObject = lunchable;
                    break;
                case Food.Seaweed:
                    foodObject = seaweed;
                    break;
                case Food.Mac:
                    foodObject = mac;
                    break;
                default:
                    break;
            }

#pragma warning disable CS8603 // Possible null reference return.
            return foodObject;
#pragma warning restore CS8603 // Possible null reference return.
        }

        void UpdateFood(Food food)
        {
            PictureBox foodObject = getFoodObjectInstance(food);
            bool update = false;
            switch (food)
            {
                case Food.Candy:
                    delay.candy++;
                    if (delay.candy > 1)
                    {
                        delay.candy = 0;
                        update = true;
                    }
                    break;
                case Food.Chips:
                    delay.chips++;
                    if (delay.chips > 2)
                    {
                        delay.chips = 0;
                        update = true;
                    }
                    break;
                case Food.Ramen:
                    delay.ramen++;
                    if (delay.ramen > 3)
                    {
                        delay.ramen = 0;
                        update = true;
                    }
                    break;
                case Food.Soda:
                    delay.soda++;
                    if (delay.soda > 4)
                    {
                        delay.soda = 0;
                        update = true;
                    }
                    break;
                case Food.Lunchable:
                    delay.lunchable++;
                    if (delay.lunchable > 5)
                    {
                        delay.lunchable = 0;
                        update = true;
                    }
                    break;
                case Food.Seaweed:
                    delay.seaweed++;
                    if (delay.seaweed > 6)
                    {
                        delay.seaweed = 0;
                        update = true;
                    }
                    break;
                case Food.Mac:
                    delay.mac++;
                    if (delay.mac > 7)
                    {
                        delay.mac = 0;
                        update = true;
                    }
                    break;
                default:
                    return;
            }
            if (update)
            {
                UpdateFoodSize(food, foodObject);
            }
            CollisionCheckFoodWithMili(food, foodObject);
        }

        private void CollisionCheckFoodWithMili(Food food, PictureBox foodObject)
        {
            if (foodObject.Size.Width > 128 && foodObject.Bounds.Contains(mili.Location))
            {
                currentScore += ((int)food + 1);
                InitializeFood(food);
            }
        }

        private void UpdateFoodSize(Food food, PictureBox foodObject)
        {
            foodObject.Size = new Size(foodObject.Size.Width + 2, foodObject.Size.Height + 2);
            if (foodObject.Size.Width > 180)
            {
                currentScore--;
                setFailVisibility(true);                
                InitializeFood(food);
            }
            else
            {
                foodObject.Location = new Point(foodObject.Location.X - 1, foodObject.Location.Y - 1);
            }
        }

        void UpdateMiliPosition()
        {
            mili.Location = PointToClient(Cursor.Position);
        }

        void UpdateMiliFrame()
        {
            delay.miliFrame++;
            if (delay.miliFrame >= 10)
            {
                delay.miliFrame = 0;
                mili.Image = miliImages.Images[miliFrameIndex];
                miliFrameIndex++;
                if (miliFrameIndex >= miliImages.Images.Count)
                {
                    miliFrameIndex = 0;
                }
            }
        }

        private void UpdateLoopTimer_Tick(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.TitleScreen:
                    {
                        if ((Control.MouseButtons & MouseButtons.Left) != 0)
                        {
                            TransitionState(State.GamePlay);
                        }
                    }
                    break;
                case State.GamePlay:
                    {
                        UpdateMiliPosition();
                        UpdateMiliFrame();
                        foreach (Food snack in snackTime)
                        {
                            UpdateFood(snack);
                        }
                        UpdateScore();
                        UpdateTimeRemaining();
                        UpdateFailMessage();
                        if (GameOver())
                        {
                            TransitionState(State.GameOver);
                        }
                    }
                    break;
                case State.GameOver:
                    {
                        if ((Control.MouseButtons & MouseButtons.Left) != 0)
                        {
                            TransitionState(State.TitleScreen);
                            Application.DoEvents();
                            Thread.Sleep(5000);
                        }
                    }
                    break;
            }

            UpdateBackground();
        }

        private void UpdateFailMessage()
        {
            if (delay.fail > 0)
            {
                delay.fail--;
                if (delay.fail == 0)
                {
                    setFailVisibility(false);
                }
            }
        }

        private bool GameOver()
        {
            return (DateTimeOffset.Now > endTime);
        }

        private void UpdateTimeRemaining()
        {
            var timeRemaining = DateTimeOffset.Now - endTime;
            time.Text = $"Time: {timeRemaining.Seconds}";
        }

        private void UpdateBackground()
        {
            delay.updateBackground++;
            if (delay.updateBackground == 5)
            {
                delay.updateBackground = 0;
                backgroundAnimationState++;
                if (backgroundAnimationState >= backgroundObjects.Count)
                {
                    backgroundAnimationState = 0;
                }
                int colorOffset = backgroundAnimationState;
                for (int backgroundIndex = 0; backgroundIndex < backgroundObjects.Count; colorOffset++, backgroundIndex++)
                {
                    backgroundObjects[backgroundIndex].BackColor = backgroundColors[colorOffset];
                }
            }
        }
    }
}
