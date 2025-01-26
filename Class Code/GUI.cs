using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Input;
using System;

namespace CrimeGame;

public abstract class GUI {
        public Rectangle Bounds { get; set; }
        public bool IsVisible { get; set; } = true; 
        public abstract void Update(GameTime gameTime, MouseState mouseStaate);    
        public abstract void Draw(SpriteBatch spriteBatch);}

public class Button : GUI
{
    public string Text { get; set; }
    public Color BackgroundColor { get; set; } = Color.Gray;
    public Color HoverColor { get; set; } = Color.DarkGray;
    public Color TextColor { get; set; } = Color.White;

    private SpriteFont _font;
    private bool _isHovered;
    private Texture2D whiteTexture;

    public Action OnClick { get; set; } // Callback when button is clicked

    public Button(Rectangle bounds, string text, SpriteFont font)
    {
        Bounds = bounds;
        Text = text;
        _font = font;
    }
    public void SetTexture (Texture2D texture)
    {
        whiteTexture = texture;
    }
    public override void Update(GameTime gameTime, MouseState mouseState)
    {
        _isHovered = Bounds.Contains(mouseState.Position);

        if (_isHovered && mouseState.LeftButton == ButtonState.Pressed)
        {
            OnClick?.Invoke();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {


        if (!IsVisible) return;
        // Draw the button background
        spriteBatch.Draw(
            texture: whiteTexture, // Use a 1x1 white texture
            destinationRectangle: Bounds,
            color: _isHovered ? HoverColor : BackgroundColor
        );

        // Draw the button text
        Vector2 textSize = _font.MeasureString(Text);
        Vector2 textPosition = new Vector2(
            Bounds.X + (Bounds.Width - textSize.X) / 2,
            Bounds.Y + (Bounds.Height - textSize.Y) / 2
        );

        spriteBatch.DrawString(_font, Text, textPosition, TextColor);
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

    public override void Update(GameTime gameTime, MouseState mouseState)
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

public class Panel : GUI
{
    private Texture2D _texture;
    public Color BackgroundColor { get; set; } = Color.DarkGray;

    public Panel(Rectangle bounds, Texture2D texture)
    {
        Bounds = bounds;
        _texture = texture;
    }

    public override void Update(GameTime gameTime, MouseState mouseState)
    {
        // Panels are static containers, no updates required for now
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        spriteBatch.Draw(_texture, Bounds, BackgroundColor);
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

    public override void Update(GameTime gameTime, MouseState mouseState)
    {
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

public class FPS
    {
        private int frameCount;
        private double elapsedTime;
        public int fps;

        //private readonly ImGUIRenderer guiRenderer;
        
        public FPS()
        {}
        public void LoadContent(ContentManager content)
        {}
        public int fpsCounter(GameTime gameTime) {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            frameCount++;
            if (elapsedTime >= 1000){
                fps = frameCount;
                frameCount = 0;
                elapsedTime = 0;}
            return fps;
        }
        public void Update(GameTime gameTime)
        {
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {}
    }

