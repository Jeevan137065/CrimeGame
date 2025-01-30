using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Input;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using MonoGame.Extended.Timers;
using MonoGame.Extended;

namespace CrimeGame
{
    public abstract class GUI
    {
        public Rectangle Bounds { get; set; }
        public bool IsVisible { get; set; } = true;
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
    public abstract class GUIButton
    {
        public Rectangle Bounds { get; protected set; }
        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public bool IsHovered { get; private set; }
        protected GraphicsDevice _graphicsDevice;

        public GUIButton(Rectangle bounds, GraphicsDevice graphicsDevice)
        {
            Bounds = bounds;
            _graphicsDevice = graphicsDevice;
            NormalColor = Color.Gray;
            HoverColor = Color.LightGray;
        }

        public abstract void Update(MouseState mouseState, GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);

        protected void CheckHover(MouseState mouseState)
        {
            IsHovered = Bounds.Contains(mouseState.Position);
        }

    }
    public class Button : GUIButton
    {
        public Action OnClick { get; set; } // Callback when button is clicked

        public Button(Rectangle bounds, GraphicsDevice graphicsDevice, Action onClick) : base(bounds, graphicsDevice)
        {
            OnClick = onClick;
        }
        public override void Update(MouseState mouseState, GameTime gameTime)
        {
            CheckHover(mouseState);
            if (IsHovered && mouseState.LeftButton == ButtonState.Pressed) { OnClick?.Invoke(); }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(Bounds, IsHovered ? HoverColor : NormalColor);
        }

    }
    public class PressureButton : GUIButton
    {
        public float HoldTime { get; set; } = 2.0f; // Time required to hold
        private float _currentHoldTime = 0f;

        public Action OnHoldComplete { get; set; }

        public PressureButton(Rectangle bounds, GraphicsDevice graphicsDevice, Action onHoldComplete)
            : base(bounds, graphicsDevice)
        {
            OnHoldComplete = onHoldComplete;
        }

        public override void Update(MouseState mouseState, GameTime gameTime)
        {
            CheckHover(mouseState);

            if (IsHovered && mouseState.LeftButton == ButtonState.Pressed)
            {
                _currentHoldTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_currentHoldTime >= HoldTime)
                {
                    OnHoldComplete?.Invoke();
                    _currentHoldTime = 0f; // Reset hold time
                }
            }
            else
            {
                _currentHoldTime = 0f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(Bounds, NormalColor);

            // Draw progress bar
            int barWidth = (int)(Bounds.Width * (_currentHoldTime / HoldTime));
            Rectangle progressBar = new Rectangle(Bounds.X, Bounds.Y + Bounds.Height, barWidth, 5);
            spriteBatch.FillRectangle(progressBar, Color.Green);
        }
    }
    public class StateButton : GUIButton
    {
        public bool IsToggled { get; private set; }
        public Color ToggledColor { get; set; }

        public StateButton(Rectangle bounds, GraphicsDevice graphicsDevice)
            : base(bounds, graphicsDevice)
        {
            ToggledColor = Color.DarkGray;
        }

        public override void Update(MouseState mouseState, GameTime gameTime)
        {
            CheckHover(mouseState);

            if (IsHovered && mouseState.LeftButton == ButtonState.Pressed)
            {
                IsToggled = !IsToggled;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(Bounds, IsToggled ? ToggledColor : NormalColor);
        }
    }
    public class Panel : GUI
    {
        private Texture2D _texture;
        public Color BackgroundColor { get; set; } = Color.DarkGray;

        public Panel(Rectangle bounds, Texture2D texture)
        {
            Bounds = bounds;
            _texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            // Panels are static containers, no updates required for now
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            spriteBatch.Draw(_texture, Bounds, BackgroundColor);
        }
    }
    public class Label : GUI
    {
        private string _text;
        private SpriteFont _font;
        public Color TextColor { get; set; } = Color.White;

        public Label(Rectangle bounds, string text, SpriteFont font)
        {
            Bounds = bounds;
            _text = text;
            _font = font;
        }

        public override void Update(GameTime gameTime)
        {
            // Labels don't require updates
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            Vector2 textSize = _font.MeasureString(_text);
            Vector2 textPosition = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, _text, textPosition, TextColor);
        }
    }
    public class Checkbox : GUI
    {
        private Texture2D _texture;
        private bool _isChecked;

        public Color BoxColor { get; set; } = Color.White;
        public Color CheckColor { get; set; } = Color.Green;

        public Action<bool> OnToggle { get; set; }

        public Checkbox(Rectangle bounds, Texture2D texture, bool isChecked = false)
        {
            Bounds = bounds;
            _texture = texture;
            _isChecked = isChecked;
        }

        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();

            if (!IsVisible) return;

            if (Bounds.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed)
            {
                _isChecked = !_isChecked;
                OnToggle?.Invoke(_isChecked);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            spriteBatch.Draw(_texture, Bounds, BoxColor);

            if (_isChecked)
            {
                Rectangle checkBounds = new Rectangle(Bounds.X + 4, Bounds.Y + 4, Bounds.Width - 8, Bounds.Height - 8);
                spriteBatch.Draw(_texture, checkBounds, CheckColor);
            }
        }
    }
    public class Toolbar
    {
        private List<GUIButton> buttons;
        private GraphicsDevice graphicsDevice;

        private bool _isVisible = true;

        public Toolbar(GraphicsDevice _graphicsDevice)
        {
            graphicsDevice = _graphicsDevice;
            buttons = new List<GUIButton>();

            // Add buttons (for now, dummy names)
            buttons.Add(new Button(new Rectangle(800, 10, 120, 40), graphicsDevice, () => Console.WriteLine("Load")));
            buttons.Add(new Button(new Rectangle(930, 10, 120, 40), graphicsDevice, () => Console.WriteLine("Save")));
            buttons.Add(new Button(new Rectangle(1060, 10, 120, 40), graphicsDevice, () => Console.WriteLine("Undo")));
            buttons.Add(new Button(new Rectangle(1190, 10, 120, 40), graphicsDevice, () => Console.WriteLine("Redo")));
        }

        public void Update(MouseState mouseState, KeyboardState keyboardState, GameTime gameTime)
        {
            // Hide/Show with long press of P
            if (keyboardState.IsKeyDown(Keys.P))
            {
                _isVisible = !_isVisible;
            }

            if (_isVisible)
            {
                foreach (var button in buttons)
                {
                    button.Update(mouseState, gameTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isVisible)
            {
                foreach (var button in buttons)
                {
                    button.Draw(spriteBatch);
                }
            }
        }


    }
    class UndoRedoManager
    {
        private Stack<Action> undoStack = new Stack<Action>();
        private Stack<Action> redoStack = new Stack<Action>();

        public void AddUndoAction(Action action)
        {
            undoStack.Push(action);
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                Action action = undoStack.Pop();
                redoStack.Push(action);
                action.Invoke();
            }
        }
    }
    public class FPS
    {
        private int frameCount;
        private double elapsedTime;
        public int fps;

        //private readonly ImGUIRenderer guiRenderer;

        public FPS()
        { }
        public void LoadContent(ContentManager content)
        { }
        public int fpsCounter(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            frameCount++;

            // Update FPS once every second
            if (elapsedTime >= 1000)
            {
                fps = frameCount; // Calculate FPS
                frameCount = 0; // Reset frame count
                elapsedTime -= 1000; // Subtract 1 second from elapsed time to prevent drift
            }

            return fps;
        }
        public void Update(GameTime gameTime)
        {

        }
        public void Draw(SpriteBatch spriteBatch)
        { }
    }

}