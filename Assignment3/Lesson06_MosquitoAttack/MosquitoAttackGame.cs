using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Lesson06_MosquitoAttack;

public class MosquitoAttackGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int _WindowWidth = 550;
    private const int _WindowHeight = 400;

    private Texture2D _background;
    private SpriteFont _arial;
    private string _message;

    private Cannon _cannon;
    private Mosquito[] _mosquitoes;

    private enum GameState { Menu, Over, Paused, Level01 }
    private GameState _gameState = GameState.Menu;
    private KeyboardState _kbCurrent, _kbPrevious;

    private Rectangle GameBoundingBox
    {
        get { return new Rectangle(0, 0, _WindowWidth, _WindowHeight); }
    }

    public MosquitoAttackGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = _WindowWidth;
        _graphics.PreferredBackBufferHeight = _WindowHeight;
        _graphics.ApplyChanges();

        Reset();

        base.Initialize();
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _background = Content.Load<Texture2D>("Background");
        _arial = Content.Load<SpriteFont>("SystemArialFont");
        
        _cannon.LoadContent(Content);
        foreach (Mosquito m in _mosquitoes)
            m.LoadContent(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        _kbCurrent = Keyboard.GetState();

        switch (_gameState)
        {
            case GameState.Menu:
                if (Pressed(Keys.Enter))
                {
                    Reset();
                    _gameState = GameState.Level01;
                    _message = "";
                }
                break;

            case GameState.Level01:
                #region keyboard input
                if (_kbCurrent.IsKeyDown(Keys.A))
                    _cannon.Direction = new Vector2(-1, 0);
                else if (_kbCurrent.IsKeyDown(Keys.D))
                    _cannon.Direction = new Vector2(1, 0);
                else
                    _cannon.Direction = Vector2.Zero;

                if (Pressed(Keys.Space))
                {
                    if (!_cannon.IsEmpty)
                    {
                        _cannon.Shoot();
                        _message = "";
                    }
                    else
                    {
                        _message = "MAGAZINE EMPTY! PRESS R TO RELOAD!";
                    }
                }

                if (Pressed(Keys.R))
                {
                    _cannon.Reload();
                    _message = "";
                }

                if (Pressed(Keys.P))
                {
                    _gameState = GameState.Paused;
                    _message = "GAME PAUSED. PRESS P TO RESUME.";
                }
                #endregion

                _cannon.Update(gameTime);

                foreach (Mosquito m in _mosquitoes)
                {
                    m.Update(gameTime);
                    
                    if (_cannon.Alive && m.ProcessCollision(_cannon.BoundingBox))
                    {
                        _cannon.Die();
                    }

                    if (m.Alive && _cannon.ProcessCollision(m.BoundingBox))
                    {
                        m.Die();
                    }
                }

                bool allDead = true;
                foreach (Mosquito m in _mosquitoes)
                {
                    if (m.Alive)
                    {
                        allDead = false;
                        break;
                    }
                }

                if (allDead)
                {
                    _gameState = GameState.Over;
                    _message = "VICTORY! PRESS ENTER TO REPLAY";
                }

                if (!_cannon.Alive)
                {
                    _gameState = GameState.Over;
                    _message = "GAME OVER! PRESS ENTER TO REPLAY";
                }
                break;

            case GameState.Paused:
                if (Pressed(Keys.P))
                {
                    _gameState = GameState.Level01;
                }
                break;

            case GameState.Over:
                if (Pressed(Keys.Enter))
                {
                    _gameState = GameState.Menu;
                }
                break;
        }

        _kbPrevious = _kbCurrent;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        switch (_gameState)
        {
            case GameState.Menu:
                _spriteBatch.Draw(_background, Vector2.Zero, Color.DarkSlateGray);
                _spriteBatch.DrawString(_arial, "THE EVIL MOSQUITOS ATTACK", new Vector2(40, GameBoundingBox.Height/2), Color.Yellow);
                _spriteBatch.DrawString(_arial, "PRESS ENTER TO PLAY!", new Vector2(40, GameBoundingBox.Height/2 + 30), Color.White);
                break;

            case GameState.Level01:
                _spriteBatch.Draw(_background, Vector2.Zero, Color.White);
                _cannon.Draw(_spriteBatch);
                foreach (Mosquito m in _mosquitoes)
                    m.Draw(_spriteBatch);
                break;

            case GameState.Paused:
                _spriteBatch.Draw(_background, Vector2.Zero, Color.Silver);
                _cannon.Draw(_spriteBatch);
                foreach (Mosquito m in _mosquitoes)
                    m.Draw(_spriteBatch);
                _spriteBatch.DrawString(_arial, _message, new Vector2(40, GameBoundingBox.Height/2), Color.Black);
                break;

            case GameState.Over:
                _spriteBatch.Draw(_background, Vector2.Zero, Color.DarkRed);
                _spriteBatch.DrawString(_arial, _message, new Vector2(40, GameBoundingBox.Height/2), Color.White);
                break;
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private bool Pressed(Keys key)
    {
        return _kbCurrent.IsKeyDown(key) && _kbPrevious.IsKeyUp(key);
    }

    private void Reset()
    {
        _cannon = new Cannon();
        _cannon.Initialize(new Vector2(50, 325), 150, GameBoundingBox);
        _cannon.LoadContent(Content);

        _mosquitoes = new Mosquito[10];
        for (int c = 0; c < _mosquitoes.Length; c++)
        {
            _mosquitoes[c] = new Mosquito();
        }

        Random random = new Random();
        foreach (Mosquito m in _mosquitoes)
        {
            int direction = random.Next(1, 3);
            if (direction == 2)
                direction = -1;
            int xPosition = random.Next(1, _WindowWidth - 50);
            int yPosition = random.Next(1, 151);
            int speed = random.Next(150, 251);
            m.Initialize(new Vector2(xPosition, yPosition), speed, new Vector2(direction, 0), GameBoundingBox);
            m.LoadContent(Content);
        }
    }
}