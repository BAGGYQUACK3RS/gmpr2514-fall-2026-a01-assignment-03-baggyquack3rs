using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lesson06_MosquitoAttack;

public class Mosquito
{
    private const float _DyingDuration = 1f, _DecisionDelay = 2f;
    private float _dyingTimer = 0, _decisionTimer = 0, _decision;

    private SimpleAnimation _animationAlive, _animationDying;
    private Vector2 _position;
    private float _speed;
    internal Vector2 Direction {get; set;} = Vector2.Zero;

    private Rectangle _gameBoundingBox;

    private FireBall _fBall;
    private bool _isEmpty = false;

    private enum State {Alive, Dying, Dead}
    private State _state = State.Alive;

    internal bool Alive { get => _state == State.Alive;}

    private Random _rand= new Random();

    private Texture2D _texture;
    internal Rectangle BoundingBox
    {
        get 
        { 
            return new Rectangle((int)_position.X, (int)_position.Y, _texture.Width / 11, _texture.Height);
        }
    }

    internal void Initialize(Vector2 position, float speed, Vector2 direction, Rectangle gameBoundingBox)
    {
        _position = position;
        _speed = speed;
        Direction = direction;
        _gameBoundingBox = gameBoundingBox;

        _fBall = new FireBall();
        _fBall.Initialize(40, gameBoundingBox);
        _decisionTimer = (float)_rand.NextDouble() * _DecisionDelay;
    }

    internal void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>("Mosquito");
        _animationAlive = new SimpleAnimation(_texture, _texture.Width / 11, _texture.Height, 11, 8f);
        _fBall.LoadContent(content);

        Texture2D dyingTexture = content.Load<Texture2D>("Poof");
        int frameWidth = 16;
        int frameHeight = dyingTexture.Height;
        int frameCount = dyingTexture.Width / frameWidth;
        float framesPerSecond = 8f;
        _animationDying = new SimpleAnimation(dyingTexture, frameWidth, frameHeight, frameCount, framesPerSecond);
    
    }
    internal void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _decision = _rand.Next(1, 50); 

        switch(_state)
        {
            case State.Alive:
                _position += Direction * _speed * dt;
                if(BoundingBox.Left < _gameBoundingBox.Left || BoundingBox.Right > _gameBoundingBox.Right)
                    Direction = new Vector2(Direction.X * -1, Direction.Y);
                _animationAlive?.Update(gameTime);

                _decisionTimer += dt;
                if (_decisionTimer >= _DecisionDelay)
                {
                    _decisionTimer = 0f;
                    
                    _decision = _rand.Next(1, 3); 
                    if (_decision == 1)
                    {
                        Shoot();
                    }
                }
                break;
            case State.Dying:
                _dyingTimer += dt;
                if(_dyingTimer >= _DyingDuration)
                    _state = State.Dead;
                _animationDying?.Update(gameTime);
                break;
            case State.Dead:
                break;
        }  

        _fBall.Update(gameTime);
        if (_fBall.Shootable)
        {
            _isEmpty = false;
        }
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
        _fBall.Draw(spriteBatch);

        switch(_state)
        {
            case State.Alive:
                _animationAlive?.Draw(spriteBatch, _position, SpriteEffects.None);
                break;
            case State.Dying:
                _animationDying?.Draw(spriteBatch, _position, SpriteEffects.None);
                break;
            case State.Dead:
                break;
        }
        
    }

    internal void Shoot()
    {
        if(_fBall.Shootable)
        {
            float cbPositionX = BoundingBox.Center.X - _fBall.BoundingBox.Width / 2;
            float cbPositionY = BoundingBox.Bottom;
            _fBall.Shoot(new Vector2(cbPositionX, cbPositionY), new Vector2(0, 1));
            _isEmpty = true;
        }
    }

    internal bool ProcessCollision(Rectangle boundingBox)
    {
        if(_fBall.ProcessCollision(boundingBox))
            return true;
        else
        return false;
    }

    internal void Die()
    {
        _state = State.Dying;
        _dyingTimer = 0;
    }
}