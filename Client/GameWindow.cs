﻿using Hammerhand.Data;
using Hammerhand.Entities;
using Hammerhand.Entities.Items;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hammerhand.Client
{
    public class GameWindow : RenderWindow
    {
        public enum Priority { Background, Mainground, Foreground }
        public static readonly FloatRect FOV = new FloatRect(0, 0, 256, 256);
        public static readonly Vector2f Center = new Vector2f(128, 128);
        private Dictionary<Priority, Queue<QueuedEntity>> Queue = new Dictionary<Priority, Queue<QueuedEntity>>();
        private ConcurrentQueue<QueuedEntity> BackgroundItems = new ConcurrentQueue<QueuedEntity>();
        private ConcurrentQueue<QueuedEntity> MaingroundItems = new ConcurrentQueue<QueuedEntity>();
        private ConcurrentQueue<QueuedEntity> ForegroundItems = new ConcurrentQueue<QueuedEntity>();
        private ConcurrentQueue<QueuedEntity> ConcurrentQueue = new ConcurrentQueue<QueuedEntity>();
        private Clock FrameClock = new Clock();
        public bool ShowHitboxes = false;
        public Game Game;
        public GameWindow(Game game) : base(game.Options.FullscreenEnabled? VideoMode.FullscreenModes[0] : new VideoMode(game.Options.Width, game.Options.Height), Game.Title, game.Options.FullscreenEnabled? Styles.Fullscreen : Styles.Default)
        {
            Game = game;
            SetVerticalSyncEnabled(true);
            SetIcon(192, 192, Game.Assets.GrabImage("Icon").CopyToImage().Pixels);
            Queue.Add(Priority.Background, new Queue<QueuedEntity>());
            Queue.Add(Priority.Mainground, new Queue<QueuedEntity>());
            Queue.Add(Priority.Foreground, new Queue<QueuedEntity>());
            Watch(Priority.Background, Game.Stage);
            Watch(Priority.Mainground, Game.Stage);
            Watch(Priority.Foreground, Game.Stage);
            Watch(Priority.Foreground, Game.Stage.Lights);
            SetView(AdjustView());
            Closed += OnClosed;
            GainedFocus += OnGainedFocus;
            JoystickButtonPressed += OnJoystickButtonPressed;
            JoystickButtonReleased += OnJoystickButtonReleased;
            JoystickConnected += OnJoystickConnected;
            JoystickDisconnected += OnJoystickDisconnected;
            JoystickMoved += OnJoystickMoved;
            KeyPressed += OnKeyPressed;
            KeyReleased += OnKeyReleased;
            LostFocus += OnLostFocus;
            MouseButtonPressed += OnMouseButtonPressed;
            MouseButtonReleased += OnMouseButtonReleased;
            MouseEntered += OnMouseEntered;
            MouseLeft += OnMouseLeft;
            MouseMoved += OnMouseMoved;
            MouseWheelScrolled += OnMouseWheelScrolled;
            Resized += OnResized;
            TextEntered += OnTextEntered;
        }
        public View AdjustView()
        {
            return new View(Center, new Vector2f(255, (float) Size.Y / Size.X * 255.0F));
        }
        public void RunOnce()
        {
            QueuedEntity _item;
            DispatchEvents();
            Clear(Color.Black);
            while (BackgroundItems.TryDequeue(out _item))
            {
                _item.OnQueue(this, Priority.Background, ConcurrentQueue);
            }
            Swap(BackgroundItems, _item);
            while (MaingroundItems.TryDequeue(out _item))
            {
                _item.OnQueue(this, Priority.Mainground, ConcurrentQueue);
            }
            Swap(MaingroundItems, _item);
            while (ForegroundItems.TryDequeue(out _item))
            {
                _item.OnQueue(this, Priority.Foreground, ConcurrentQueue);
            }
            Swap(ForegroundItems, _item);
            Display();
        }
        private void Swap(ConcurrentQueue<QueuedEntity> queue, QueuedEntity _item)
        {
            while (ConcurrentQueue.TryDequeue(out _item))
            {
                queue.Enqueue(_item);
            }
        }
        public void Watch(Priority priority, QueuedEntity item)
        {
            switch (priority)
            {
                case Priority.Background:
                    BackgroundItems.Enqueue(item);
                    break;
                case Priority.Mainground:
                    MaingroundItems.Enqueue(item);
                    break;
                case Priority.Foreground:
                    ForegroundItems.Enqueue(item);
                    break;
            }
        }
        public override void Display()
        {
            float _fps = 1.0F / FrameClock.ElapsedTime.AsSeconds();
            SetTitle(string.Format("{0} [FPS: {1}]", Game.Title, _fps));
            Game.FrameDelta = 60.0F / _fps;
            FrameClock.Restart();
            base.Display();
        }
        private void OnClosed(object sender, EventArgs e)
        {
            Close();
        }
        private void OnGainedFocus(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnJoystickButtonPressed(object sender, JoystickButtonEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnJoystickButtonReleased(object sender, JoystickButtonEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnJoystickConnected(object sender, JoystickConnectEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnJoystickDisconnected(object sender, JoystickConnectEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnJoystickMoved(object sender, JoystickMoveEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnKeyReleased(object sender, KeyEventArgs e)
        {
            switch (e.Code)
            {
                case Keyboard.Key.F10:
                    ShowHitboxes = !ShowHitboxes;
                    break;
                case Keyboard.Key.F11:
                    Game.Options.FullscreenEnabled = !Game.Options.FullscreenEnabled;
                    Game.OpenWindow();
                    break;
            }
        }
        private void OnLostFocus(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
        }
        public bool _mouseDown = false;
        public Cup _cup = null;
        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = true;
            if (_cup == null)
            {
                _cup = new Cup(Game);
                _cup.Position = MapPixelToCoords(new Vector2i(e.X, e.Y));
                Watch(Priority.Mainground, _cup);
            }
        }
        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
            _cup = null;
        }
        private void OnMouseEntered(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnMouseLeft(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            if (_mouseDown && _cup != null)
            {
                Vector2f mouse = MapPixelToCoords(new Vector2i(e.X, e.Y));
                _cup.Velocity = new Vector2f(mouse.X - _cup.Position.X, mouse.Y - _cup.Position.Y);
            }
        }
        private void OnMouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            // throw new NotImplementedException();
        }
        private void OnResized(object sender, SizeEventArgs e)
        {
            SetView(AdjustView());
        }
        private void OnTextEntered(object sender, TextEventArgs e)
        {
            // throw new NotImplementedException();
        }
    }
}
