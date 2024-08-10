using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MonoGame.Framework.WpfInterop
{
    /// <summary>
    /// Specifies the rendering mode of the D3D11Host.
    /// </summary>
    public enum RenderMode
    {
        Continuous,
        Manual
    }

    /// <summary>
    /// Host a Direct3D 11 scene.
    /// </summary>
    public class D3D11Host : Image, IDisposable
    {
        #region Fields

        private static readonly object _graphicsDeviceLock = new object( );
        private readonly Stopwatch _timer;
        private static GraphicsDevice _graphicsDevice;
        private static bool? _isInDesignMode;
        private static int _referenceCount;
        private D3D11Image _d3D11Image;
        private bool _disposed;
        private TimeSpan _lastRenderingTime;
        private bool _loaded;
        private RenderTarget2D _renderTarget;
        private bool _resetBackBuffer;
        private TimeSpan _timeSinceStart = TimeSpan.Zero;
        private bool _isDirty = true;

        #endregion

        #region Constructors

        public D3D11Host( )
        {
            Stretch = Stretch.Fill;
            _timer = new Stopwatch( );
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        ~D3D11Host( )
        {
            Dispose( false );
        }

        #endregion

        #region Properties

        public static bool IsInDesignMode
        {
            get
            {
                if ( !_isInDesignMode.HasValue )
                    _isInDesignMode = ( bool ) DependencyPropertyDescriptor.FromProperty( DesignerProperties.IsInDesignModeProperty, typeof( FrameworkElement ) ).Metadata.DefaultValue;

                return _isInDesignMode.Value;
            }
        }

        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public GameServiceContainer Services { get; } = new GameServiceContainer( );

        /// <summary>
        /// Gets or sets the rendering mode.
        /// </summary>
        public RenderMode RenderMode { get; set; } = RenderMode.Continuous;

        #endregion

        #region Methods

        public void Dispose( )
        {
            Dispose( true );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( _disposed )
                return;
            _disposed = true;
        }

        protected virtual void Initialize( ) { }

        protected override void OnRenderSizeChanged( SizeChangedInfo sizeInfo )
        {
            _resetBackBuffer = true;
            _isDirty = true; // Mark as dirty on resize
            base.OnRenderSizeChanged( sizeInfo );

            if ( RenderMode == RenderMode.Manual )
            {
                RenderIfDirty( );
            }
        }

        protected virtual void Render( GameTime time ) { }

        private static void InitializeGraphicsDevice( )
        {
            lock ( _graphicsDeviceLock )
            {
                _referenceCount++;
                if ( _referenceCount == 1 )
                {
                    var presentationParameters = new PresentationParameters
                    {
                        DeviceWindowHandle = IntPtr.Zero,
                    };
                    _graphicsDevice = new GraphicsDevice( GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters );
                }
            }
        }

        private static void UninitializeGraphicsDevice( )
        {
            lock ( _graphicsDeviceLock )
            {
                _referenceCount--;
                if ( _referenceCount == 0 )
                {
                    _graphicsDevice.Dispose( );
                    _graphicsDevice = null;
                }
            }
        }

        private void CreateBackBuffer( )
        {
            _d3D11Image.SetBackBuffer( null );
            _renderTarget?.Dispose( );
            int width = Math.Max( ( int ) ActualWidth, 1 );
            int height = Math.Max( ( int ) ActualHeight, 1 );
            _renderTarget = new RenderTarget2D( _graphicsDevice, width, height, false, SurfaceFormat.Bgr32, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents, true );
            _d3D11Image.SetBackBuffer( _renderTarget );
        }

        private void InitializeImageSource( )
        {
            _d3D11Image = new D3D11Image( );
            _d3D11Image.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
            CreateBackBuffer( );
            Source = _d3D11Image;
        }

        private void OnIsFrontBufferAvailableChanged( object sender, DependencyPropertyChangedEventArgs eventArgs )
        {
            if ( _d3D11Image.IsFrontBufferAvailable )
            {
                StartRendering( );
                _resetBackBuffer = true;
            }
            else
            {
                StopRendering( );
            }
        }

        private void OnLoaded( object sender, RoutedEventArgs eventArgs )
        {
            if ( IsInDesignMode || _loaded )
                return;

            _loaded = true;
            InitializeGraphicsDevice( );
            InitializeImageSource( );
            Initialize( );
            StartRendering( );
        }

        private void OnRendering( object sender, EventArgs eventArgs )
        {
            if ( !_timer.IsRunning )
                return;

            if ( _resetBackBuffer )
                CreateBackBuffer( );

            var renderingEventArgs = ( RenderingEventArgs ) eventArgs;
            if ( _lastRenderingTime != renderingEventArgs.RenderingTime || _resetBackBuffer )
            {
                _lastRenderingTime = renderingEventArgs.RenderingTime;

                // Only render if in continuous mode or dirty
                if ( RenderMode == RenderMode.Continuous || _isDirty )
                {
                    GraphicsDevice.SetRenderTarget( _renderTarget );
                    var diff = _timer.Elapsed - _timeSinceStart;
                    _timeSinceStart = _timer.Elapsed;
                    Render( new GameTime( _timer.Elapsed, diff ) );
                    GraphicsDevice.Flush( );
                    _isDirty = false; // Reset dirty flag after rendering
                }
            }

            _d3D11Image.Invalidate( );
            _resetBackBuffer = false;
        }

        private void OnUnloaded( object sender, RoutedEventArgs eventArgs )
        {
            if ( IsInDesignMode )
                return;

            StopRendering( );
            Dispose( );
            UnitializeImageSource( );
            UninitializeGraphicsDevice( );
        }

        private void StartRendering( )
        {
            if ( _timer.IsRunning )
                return;

            CompositionTarget.Rendering += OnRendering;
            _timer.Start( );
        }

        private void StopRendering( )
        {
            if ( !_timer.IsRunning )
                return;

            CompositionTarget.Rendering -= OnRendering;
            _timer.Stop( );
        }

        private void UnitializeImageSource( )
        {
            _d3D11Image.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
            Source = null;

            _d3D11Image?.Dispose( );
            _d3D11Image = null;
            _renderTarget?.Dispose( );
            _renderTarget = null;
        }

        /// <summary>
        /// Marks the control as dirty, meaning it needs to be re-rendered.
        /// </summary>
        public void MarkAsDirty( )
        {
            _isDirty = true;

            if ( RenderMode == RenderMode.Manual )
            {
                RenderIfDirty( );
            }
        }

        /// <summary>
        /// Forces a render if the control is dirty.
        /// </summary>
        private void RenderIfDirty( )
        {
            if ( _isDirty )
            {
                OnRendering( this, EventArgs.Empty );
            }
        }

        #endregion
    }
}