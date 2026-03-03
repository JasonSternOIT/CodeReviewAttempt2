using Microsoft.VisualBasic.Devices;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CodeReview
{
    public partial class MainForm : Form
    {
        enum State
        {
            TitleScreen,
            GamePlay
        }
        State state = State.TitleScreen;

        enum BackgroundState
        {
            First,
            Second,
            Third,
            Fourth
        }

        BackgroundState backgroundState = BackgroundState.First;

        int updateBackgroundCount = 0;
       
        int miliFrame = 0;
        int miliFrameDelay = 0;

        enum Food
        {
            Candy,
            Chips,
            Ramen,
            Soda
        }

        int candyDelay = 0;
        int chipsDelay = 0;
        int ramenDelay = 0;
        int sodaDelay = 0;

        Random random = new Random();

        DateTimeOffset start;

        public MainForm()
        {
            InitializeComponent();
            AllowTransparency = true;
            TransparencyKey = Color.Black;
            avatar.BringToFront();
            mili.BringToFront();
            updateLoopTimer.Start();
            transitionState(State.TitleScreen);
            ramen.AutoSize = false;
            PrivateFontCollection privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CFPunkAttitude-Regular.ttf"));
            score.Font = new Font(privateFonts.Families[0], 12);
            time.Font = score.Font;
        }

        void transitionState(State newState)
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
                        candy.Visible = false;
                        chips.Visible = false;   
                        ramen.Visible = false;
                        soda.Visible = false;
                        Cursor.Show();
                    }
                    break;
                case State.GamePlay:
                    {
                        start = DateTimeOffset.Now;
                        score.Tag = 0;
                        score.Visible = true;
                        time.Visible = true;
                        title.Visible = false;
                        avatar.Visible = false;
                        mili.Visible = true;
                        candy.Visible = true;
                        chips.Visible = true;
                        ramen.Visible = true;
                        soda.Visible = true;
                        initializeFood(Food.Candy);
                        initializeFood(Food.Chips);
                        initializeFood(Food.Soda);
                        initializeFood(Food.Ramen);
                        updateScore();

                        Cursor.Hide();
                    }
                    break;
            }
        }

        private void updateScore()
        {
            score.Text = $"Score: {(int)score.Tag}";
        }

        void initializeFood(Food food)
        {
            PictureBox foodObject = null;
            switch(food)
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
                default:
                    return;
            }
            foodObject.Size = new Size(8, 8);
            foodObject.Location = new Point (32 + (random.Next() % (Size.Width - 128)), 32 + (random.Next() % (Size.Height - 128)));
        }

        void updateFood(Food food)
        {
            PictureBox foodObject = null;
            bool update = false;
            switch (food)
            {
                case Food.Candy:
                    foodObject = candy;
                    candyDelay++;
                    if(candyDelay > 1)
                    {
                        candyDelay = 0;
                        update = true;
                    }    
                    break;
                case Food.Chips:
                    foodObject = chips;
                    chipsDelay++;
                    if (chipsDelay > 2)
                    {
                        chipsDelay = 0;
                        update = true;
                    }
                    break;
                case Food.Ramen:
                    foodObject = ramen;
                    ramenDelay++;
                    if (ramenDelay > 3)
                    {
                        ramenDelay = 0;
                        update = true;
                    }
                    break;
                case Food.Soda:
                    foodObject = soda;
                    sodaDelay++;
                    if (sodaDelay > 4)
                    {
                        sodaDelay = 0;
                        update = true;
                    }
                    break;
                default:
                    return;
            }
            if (update)
            {
                foodObject.Size = new Size(foodObject.Size.Width + 2, foodObject.Size.Height + 2);
                if (foodObject.Size.Width > 180)
                {
                    initializeFood(food);
                }
                else
                {
                    foodObject.Location = new Point(foodObject.Location.X - 1, foodObject.Location.Y - 1);
                }
            }
            if(foodObject.Size.Width > 128)
            {
                if(foodObject.Bounds.Contains(mili.Location))
                {
                    score.Tag = (int)score.Tag + 1;
                    initializeFood(food);
                }
            }    
        }

        void updateMiliPosition()
        {
            mili.Location = PointToClient(Cursor.Position);
        }

        void updateMiliFrame()
        {
            miliFrameDelay++;
            if (miliFrameDelay >= 10)
            {
                miliFrameDelay = 0;
                mili.Image = miliImages.Images[miliFrame];
                miliFrame++;
                if (miliFrame >= miliImages.Images.Count)
                {
                    miliFrame = 0;
                }
            }
        }

        private void updateLoopTimer_Tick(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.TitleScreen:
                    {
                        if ((Control.MouseButtons & MouseButtons.Left) != 0)
                        {
                            transitionState(State.GamePlay);
                        }
                    }
                    break;
                case State.GamePlay:
                    {
                        updateMiliPosition();
                        updateMiliFrame();
                        updateFood(Food.Candy);
                        updateFood(Food.Chips);
                        updateFood(Food.Ramen);
                        updateFood(Food.Soda);
                        updateScore();
                        if(gameOver())
                        {
                            transitionState(State.TitleScreen);
                        }
                    }
                    break;
            }

            updateBackground();
        }

        private bool gameOver()
        {
            var endTime = start.AddMinutes(1);

            var timeRemaining = DateTimeOffset.Now - endTime;
            time.Text = $"Time: {timeRemaining.Seconds}";

            return (DateTimeOffset.Now > endTime);
        }

        private void updateBackground()
        {
            updateBackgroundCount++;
            if (updateBackgroundCount == 5)
            {
                updateBackgroundCount = 0;
                switch (backgroundState)
                {
                    case BackgroundState.First:
                        background1.BackColor = Color.HotPink;
                        background2.BackColor = Color.DeepPink;
                        background3.BackColor = Color.Cyan;
                        background4.BackColor = Color.DarkTurquoise;
                        background5.BackColor = Color.HotPink;
                        background6.BackColor = Color.DeepPink;
                        backgroundState = BackgroundState.Second;
                        break;
                    case BackgroundState.Second:
                        background1.BackColor = Color.DarkTurquoise;
                        background2.BackColor = Color.HotPink;
                        background3.BackColor = Color.DeepPink;
                        background4.BackColor = Color.Cyan;
                        background5.BackColor = Color.DarkTurquoise;
                        background6.BackColor = Color.HotPink;
                        backgroundState = BackgroundState.Third;
                        break;
                    case BackgroundState.Third:
                        background1.BackColor = Color.Cyan;
                        background2.BackColor = Color.DarkTurquoise;
                        background3.BackColor = Color.HotPink;
                        background4.BackColor = Color.DeepPink;
                        background5.BackColor = Color.Cyan;
                        background6.BackColor = Color.DarkTurquoise;
                        backgroundState = BackgroundState.Fourth;
                        break;
                    case BackgroundState.Fourth:
                        background1.BackColor = Color.DeepPink;
                        background2.BackColor = Color.Cyan;
                        background3.BackColor = Color.DarkTurquoise;
                        background4.BackColor = Color.HotPink;
                        background5.BackColor = Color.DeepPink;
                        background6.BackColor = Color.Cyan;
                        backgroundState = BackgroundState.First;
                        break;
                }
            }
        }
    }
}
