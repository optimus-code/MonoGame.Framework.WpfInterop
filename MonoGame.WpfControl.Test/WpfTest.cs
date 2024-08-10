using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.WpfControl.Test
{
    public class WpfTest : WpfGame
    {
        private IGraphicsDeviceService _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        private Texture2D _whiteTexture;

        public override RenderMode RenderMode => RenderMode.Manual;

        protected override void Initialize( )
        {
            // Initialize the graphics device manager
            _graphicsDeviceManager = new WpfGraphicsDeviceService( this );

            // Call base initialize method
            base.Initialize( );

            // Create a SpriteBatch for drawing
            _spriteBatch = new SpriteBatch( GraphicsDevice );

            // Create a 1x1 white texture that we can scale to any size
            _whiteTexture = new Texture2D( GraphicsDevice, 1, 1 );
            _whiteTexture.SetData( new[] { Color.White } );

            // Now you can load any additional content if necessary
        }

        protected override void Update( GameTime time )
        {
            // Update logic if necessary
        }

        protected override void Draw( GameTime time )
        {
            // Clear the screen with a transparent color
            GraphicsDevice.Clear( Color.Transparent );

            // Begin the SpriteBatch to start drawing
            _spriteBatch.Begin( );

            // Draw a white square at position (100, 100) with size 50x50
            _spriteBatch.Draw( _whiteTexture, new Rectangle( 100, 100, 50, 50 ), Color.White );

            // End the SpriteBatch to finish drawing
            _spriteBatch.End( );

            // Call the base Draw method
            base.Draw( time );
        }

        protected override void Dispose( bool disposing )
        {
            // Dispose of the resources you've created
            _spriteBatch?.Dispose( );
            _whiteTexture?.Dispose( );

            base.Dispose( disposing );
        }
    }
}
