using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoGame.WpfControl.Test
{
    public class WpfTest : WpfGame
    {
        private IGraphicsDeviceService _graphicsDeviceManager;

        public override RenderMode RenderMode => RenderMode.Manual;
        protected override void Initialize( )
        {
            // must be initialized. required by Content loading and rendering (will add itself to the Services)
            // note that MonoGame requires this to be initialized in the constructor, while WpfInterop requires it to
            // be called inside Initialize (before base.Initialize())
            _graphicsDeviceManager = new WpfGraphicsDeviceService( this );

            // must be called after the WpfGraphicsDeviceService instance was created
            base.Initialize( );

            // content loading now possible
        }

        protected override void Update( GameTime time )
        {

        }

        protected override void Draw( GameTime time )
        {
            GraphicsDevice.Clear( Microsoft.Xna.Framework.Color.CornflowerBlue );

            // Your drawing code here

            base.Draw( time );
        }
    }
}