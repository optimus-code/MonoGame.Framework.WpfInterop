﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;

namespace WpfTest
{
	public class ContentScene : WpfGame
	{
		#region Fields

		private int posX = 100, posY = 100;
		private IGraphicsDeviceService _graphicsDeviceManager;
		private WpfKeyboard _keyboard;
		private KeyboardState _keyboardState;
		private WpfMouse _mouse;
		private MouseState _mouseState;
		private float _rotation;
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;

		#endregion

		#region Methods

		protected override void Draw(GameTime time)
		{
			GraphicsDevice.Clear(_mouseState.LeftButton == ButtonState.Pressed ? Color.Black : Color.CornflowerBlue);

			// since we share the GraphicsDevice with all hosts, we need to save and reset the states
			// this has to be done because spriteBatch internally sets states and doesn't reset themselves, fucking over any 3D rendering (which happens in the DemoScene)

			var blend = GraphicsDevice.BlendState;
			var depth = GraphicsDevice.DepthStencilState;
			var raster = GraphicsDevice.RasterizerState;
			var sampler = GraphicsDevice.SamplerStates[0];

			_spriteBatch.Begin();
			_spriteBatch.Draw(_texture, new Rectangle(posX, posY, 100, 20), null, Color.White, _rotation, new Vector2(_texture.Width, _texture.Height) / 2f, SpriteEffects.None, 0);
			_spriteBatch.End();

			GraphicsDevice.BlendState = blend;
			GraphicsDevice.DepthStencilState = depth;
			GraphicsDevice.RasterizerState = raster;
			GraphicsDevice.SamplerStates[0] = sampler;
		}

		protected override void Initialize()
		{
			base.Initialize();
			_graphicsDeviceManager = new WpfGraphicsDeviceService(this);

			_texture = Texture2D.FromFile(_graphicsDeviceManager.GraphicsDevice, "Content/hello.png");

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_keyboard = new WpfKeyboard(this);
			_mouse = new WpfMouse(this);
		}

		protected override void Update(GameTime time)
		{
			_mouseState = _mouse.GetState();
			_keyboardState = _keyboard.GetState();

			if (!_keyboardState.IsKeyDown(Keys.Space))
			{
				_rotation += (float)(2f * time.ElapsedGameTime.TotalSeconds);
			}
		}

		#endregion
	}
}