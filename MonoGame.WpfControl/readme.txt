# MonoGame Wpf Interop

This adds Wpf support to MonoGame.

You can host as many MonoGame controls in Wpf as you want. Note that WPF is limited to 60 FPS.

# Important changes

1. Derive from MonoGame.Framework.WpfInterop.WpfGame instead of Microsoft.Xna.Framework.Game
2. Keyboard and Mouse events from MonoGame classes will not work with this implementation. Use WpfKeyboard and WpfMouse instead. Read this issue for details: https://github.com/MarcStan/MonoGame.Framework.WpfInterop/issues/1
3. GraphicsDeviceManager can no longer be used (it requires a reference to Game). Use WpfGraphicsDeviceService instead (it requires a reference to WpfGame/D3D11Host).

## Example

public class MyGame : WpfGame
{
	private IGraphicsDeviceService _graphicsDeviceManager;
	private WpfKeyboard _keyboard;
	private WpfMouse _mouse;

	protected override void Initialize()
	{
		base.Initialize();

		// must be initialized. required by Content loading and rendering (will add itself to the Services)
		_graphicsDeviceManager = new WpfGraphicsDeviceService(this);

		// wpf and keyboard need reference to the host control in order to receive input
		// this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
		_keyboard = new WpfKeyboard(this);
		_mouse = new WpfMouse(this);
	}

	protected override void Update(GameTime time)
	{
		// every update we can now query the keyboard & mouse for our WpfGame
		var mouseState = _mouse.GetState();
		var keyboardState = _keyboard.GetState();
	}

	protected override void Draw(GameTime time)
	{
	}
}


Now you can use this class in any of your WPF application:

<MyGame Width="800" Height="480" />