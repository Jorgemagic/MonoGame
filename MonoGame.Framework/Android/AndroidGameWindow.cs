#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
using System;
using System.Drawing;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Platform.Android;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
//using OpenTK.Graphics.ES11;
//using OpenTK.Graphics.ES20;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public class AndroidGameWindow : AndroidGameView, Android.Views.View.IOnTouchListener
    {
        private Rectangle clientBounds;
        internal Game game;
        private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
        private DateTime _now;
        private DisplayOrientation _currentOrientation;

        public AndroidGameWindow(Context context)
            : base(context)
        {
            Initialize();
        }

        private void Initialize()
        {
            clientBounds = new Rectangle(0, 0, Context.Resources.DisplayMetrics.WidthPixels, Context.Resources.DisplayMetrics.HeightPixels);

            // Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            // Initialize _lastUpdate
            _lastUpdate = DateTime.Now;

            this.RequestFocus();
            this.FocusableInTouchMode = true;

            this.SetOnTouchListener(this);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            Keyboard.KeyDown(keyCode);
            return true;
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            Keyboard.KeyUp();
            return true;
        }

        ~AndroidGameWindow()
        {
            //
        }

        protected override void CreateFrameBuffer()
        {
            try
            {
                // TODO  this.GLContextVersion = GLContextVersion.Gles2_0;
                GLContextVersion = GLContextVersion.Gles2_0;
                GraphicsDevice.openGLESVersion = GLContextVersion;
                base.CreateFrameBuffer();
            }
            catch (Exception)
            {
                //device doesn't support OpenGLES 2.0; retry with 1.1:
                GLContextVersion = GLContextVersion.Gles1_1;
                GraphicsDevice.openGLESVersion = GLContextVersion;
                base.CreateFrameBuffer();
            }

        }


        #region AndroidGameView Methods

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (GraphicsContext == null || GraphicsContext.IsDisposed)
                return;

            //Should not happen at all..
            if (!GraphicsContext.IsCurrent)
                MakeCurrent();

            if (game != null)
            {
                _drawGameTime.Update(_now - _lastUpdate);
                _lastUpdate = _now;
                game.DoDraw(_drawGameTime);
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (game != null)
            {
                ObserveDeviceRotation();

                _now = DateTime.Now;
                _updateGameTime.Update(_now - _lastUpdate);
                game.DoUpdate(_updateGameTime);
            }
        }

        #endregion


        private void ObserveDeviceRotation()
        {
            if (game.graphicsDeviceManager == null)
                return;

            // Calculate supported orientations if it has been left as "default"
            DisplayOrientation supportedOrientations = (game.graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations;
            if ((supportedOrientations & DisplayOrientation.Default) != 0)
            {
                if (game.GraphicsDevice.PresentationParameters.BackBufferWidth > game.GraphicsDevice.PresentationParameters.BackBufferHeight)
                {
                    supportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                }
                else
                {
                    supportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.PortraitUpsideDown;
                }
            }

            switch (Resources.Configuration.Orientation)
            {

                case Orientation.Portrait:
                    if ((supportedOrientations & DisplayOrientation.Portrait) != 0)
                    {
                        CurrentOrientation = DisplayOrientation.Portrait;
                        game.GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.Portrait;
                        TouchPanel.DisplayOrientation = DisplayOrientation.Portrait;
                    }
                    break;
                case Orientation.Landscape:
                    // TODO: Since the system cannot tell us if it is left or right, we may need to use one of the other sensors
                    // to determine actual orientation.  At this stage it chooses left (if set) over right (if set).
                    DisplayOrientation orientation = DisplayOrientation.Unknown;
                    if ((supportedOrientations & DisplayOrientation.LandscapeLeft) != 0)
                        orientation = DisplayOrientation.LandscapeLeft;
                    else if ((supportedOrientations & DisplayOrientation.LandscapeRight) != 0)
                        orientation = DisplayOrientation.LandscapeRight;

                    if (orientation != DisplayOrientation.Unknown)
                    {
                        CurrentOrientation = orientation;
                        game.GraphicsDevice.PresentationParameters.DisplayOrientation = orientation;
                        TouchPanel.DisplayOrientation = orientation;
                    }
                    break;

                case Orientation.Undefined:
                    if ((supportedOrientations & DisplayOrientation.Unknown) != 0)
                    {
                        CurrentOrientation = DisplayOrientation.Unknown;
                        TouchPanel.DisplayOrientation = DisplayOrientation.Unknown;
                    }
                    break;
                default:
                    break;
            }
        }

        private Dictionary<IntPtr, TouchLocation> _previousTouches = new Dictionary<IntPtr, TouchLocation>();


        public override bool OnTouchEvent(MotionEvent e)
        {
            return false;
        }

        public string ScreenDeviceName
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }


        public Rectangle ClientBounds
        {
            get
            {
                return clientBounds;
            }
        }

        public bool AllowUserResizing
        {
            get
            {
                return false;
            }
            set
            {
                // Do nothing; Ignore rather than raising and exception
            }
        }

        public DisplayOrientation CurrentOrientation
        {
            get
            {
                return _currentOrientation;
            }
            private set
            {
                if (value != _currentOrientation)
                {
                    _currentOrientation = value;

                    if (_currentOrientation == DisplayOrientation.Portrait || _currentOrientation == DisplayOrientation.PortraitUpsideDown)
                        Game.contextInstance.SetRequestedOrientation(ScreenOrientation.Portrait);
                    else if (_currentOrientation == DisplayOrientation.LandscapeLeft || _currentOrientation == DisplayOrientation.LandscapeRight)
                        Game.contextInstance.SetRequestedOrientation(ScreenOrientation.Landscape);

                    if (OrientationChanged != null)
                    {
                        OrientationChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler<EventArgs> OrientationChanged;
        public event EventHandler ClientSizeChanged;
        public event EventHandler ScreenDeviceNameChanged;

        //private Vector2 GetOffsetPosition(Vector2 position) 
        //{ 
        //    Vector2 translatedPosition = position; 
        //    switch (CurrentOrientation) 
        //    { 
        //        case DisplayOrientation.Portrait: 
        //            break;
        //        case DisplayOrientation.LandscapeRight: 
        //            translatedPosition = new Vector2(ClientBounds.Height - position.Y, position.X); 
        //            break; 
        //        case DisplayOrientation.LandscapeLeft: 
        //            translatedPosition = new Vector2(position.Y, ClientBounds.Width - position.X); 
        //            break; 
        //        case DisplayOrientation.PortraitUpsideDown: 
        //            translatedPosition = new Vector2(ClientBounds.Width - position.X, ClientBounds.Height - position.Y); 
        //            break; 
        //    } 
        //    return translatedPosition * UIScreen.MainScreen.Scale; 
        //}

        public bool OnTouch(View v, MotionEvent e)
        {
            TouchLocation tlocation;
            TouchCollection collection = TouchPanel.Collection;

            Vector2 position = Vector2.Zero;
            position.X = e.GetX(e.ActionIndex);
            position.Y = e.GetY(e.ActionIndex);
            int id = e.GetPointerId(e.ActionIndex);
            int index;

            switch (e.ActionMasked)
            {
                // DOWN
                case 0:
                case 5:
                    tlocation = new TouchLocation(id, TouchLocationState.Pressed, position);
                    collection.Add(tlocation);
                    break;
                // UP
                case 1:
                case 6:
                    index = collection.FindById(e.GetPointerId(e.ActionIndex), out tlocation);
                    if (index >= 0)
                    {
                        tlocation.State = TouchLocationState.Released;
                        collection[index] = tlocation;
                    }
                    break;
                // MOVE
                case 2:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        position.X = e.GetX(i);
                        position.Y = e.GetY(i);
                        index = collection.FindById(id, out tlocation);
                        if (index >= 0)
                        {
                            tlocation.State = TouchLocationState.Moved;
                            tlocation.Position = position;
                            collection[index] = tlocation;
                        }
                    }
                    break;
                // CANCEL, OUTSIDE
                case 3:
                case 4:
                    index = collection.FindById(id, out tlocation);
                    if (index >= 0)
                    {
                        tlocation.State = TouchLocationState.Invalid;
                        collection[index] = tlocation;
                    }
                    break;
            }

            return true;

            //for (int i = 0; i < e.PointerCount; i++)
            //{
            //    Vector2 position = Vector2.Zero;
            //    position.X = e.GetX(i);
            //    position.Y = e.GetY(i);

            //    TouchLocation tlocation;
            //    TouchCollection collection = TouchPanel.Collection;

            //    int index;
            //    int id = e.GetPointerId(i);

            //    switch (e.ActionMasked)
            //    {
            //        //case MotionEventActions.Down:
            //        case 0:
            //            //Log.Debug("TESTING", string.Format("DOWN: ActionIndex({5}) ActionMasked({6}) BlobCount=[{3}] BlobID=[{2}] BlobPos=({0:N},{1:N})", position.X, position.Y, id, e.PointerCount, e.Action, e.ActionIndex, e.ActionMasked));
            //            tlocation = new TouchLocation(id, TouchLocationState.Pressed, position);
            //            collection.Add(tlocation);
            //            break;
            //        //case MotionEventActions.PointerDown:
            //        case 5:
            //            //Log.Debug("TESTING", string.Format("DOWN: ActionIndex({5}) ActionMasked({6}) BlobCount=[{3}] BlobID=[{2}] BlobPos=({0:N},{1:N})", position.X, position.Y, id, e.PointerCount, e.Action, e.ActionIndex, e.ActionMasked));
            //            tlocation = new TouchLocation(e.GetPointerId(e.ActionIndex), TouchLocationState.Pressed, position);
            //            collection.Add(tlocation);
            //            break;
            //        //case MotionEventActions.Up:
            //        case 1:
            //            index = collection.FindById(id, out tlocation);
            //            if (index >= 0)
            //            {
            //                tlocation.State = TouchLocationState.Released;
            //                collection[index] = tlocation;
            //            }
            //            break;
            //        //case MotionEventActions.PointerUp:
            //        case 6:
            //            //Log.Debug("TESTING", string.Format("UP: ActionIndex({5}) ActionMasked({6}) BlobCount=[{3}] BlobID=[{2}] BlobPos=({0:N},{1:N})", position.X, position.Y, id, e.PointerCount, e.Action, e.ActionIndex, e.ActionMasked));
            //            index = collection.FindById(e.GetPointerId(e.ActionIndex), out tlocation);
            //            if (index >= 0)
            //            {
            //                tlocation.State = TouchLocationState.Released;
            //                collection[index] = tlocation;
            //            }
            //            break;
            //        //case MotionEventActions.Move:
            //        case 2:
            //            //Log.Debug("TESTING", string.Format("MOVE: Count=[{3}] ID=[{2}] ({0:N},{1:N})", position.X, position.Y, id, e.PointerCount));
            //            index = collection.FindById(id, out tlocation);
            //            if (index >= 0)
            //            {
            //                tlocation.State = TouchLocationState.Moved;
            //                tlocation.Position = position;
            //                collection[index] = tlocation;
            //            }
            //            break;
            //        default:
            //            //Log.Debug("TESTING", string.Format("OTHER {4}: ActionIndex({5}) ActionMasked({6}) BlobCount=[{3}] BlobID=[{2}] BlobPos=({0:N},{1:N})", position.X, position.Y, id, e.PointerCount, e.Action, e.ActionIndex, e.ActionMasked));
            //            index = collection.FindById(id, out tlocation);
            //            if (index >= 0)
            //            {
            //                tlocation.State = TouchLocationState.Released;
            //                collection[index] = tlocation;
            //            }
            //            break;
            //    }
            //}
            //return true;

            //TouchLocationState state = TouchLocationState.Invalid;

            //if (e.Action == MotionEventActions.Cancel)
            //{
            //    state = TouchLocationState.Invalid;
            //}
            //if (e.Action == MotionEventActions.Up)
            //{
            //    state = TouchLocationState.Released;
            //}
            //if (e.Action == MotionEventActions.Move)
            //{
            //    state = TouchLocationState.Moved;
            //    Mouse.SetPosition((int)e.GetX(), (int)e.GetY());
            //}
            //if (e.Action == MotionEventActions.Down)
            //{
            //    state = TouchLocationState.Pressed;
            //    Mouse.SetPosition((int)e.GetX(), (int)e.GetY());
            //}

            //TouchLocation tprevious;
            //TouchLocation tlocation;
            //Vector2 position = new Vector2(e.GetX(), e.GetY());
            //Vector2 translatedPosition = position;
            //if (state != TouchLocationState.Pressed && _previousTouches.TryGetValue(e.Handle, out tprevious))
            //{
            //    tlocation = new TouchLocation(e.Handle.ToInt32(), state, translatedPosition, e.Pressure, tprevious.State, tprevious.Position, tprevious.Pressure);
            //}
            //else
            //{
            //    tlocation = new TouchLocation(e.Handle.ToInt32(), state, translatedPosition, e.Pressure);
            //}

            //TouchPanel.Collection.Clear();
            //TouchPanel.Collection.Add(tlocation);

            //if (state != TouchLocationState.Released)
            //    _previousTouches[e.Handle] = tlocation;
            //else
            //    _previousTouches.Remove(e.Handle);
            //return true;

            //////////
            //TouchLocationState state = TouchLocationState.Invalid;

            //if (e.Action == MotionEventActions.Cancel)
            //{
            //    state = TouchLocationState.Invalid;
            //}
            //if (e.Action == MotionEventActions.Up)
            //{
            //    state = TouchLocationState.Released;
            //}
            //if (e.Action == MotionEventActions.Move)
            //{
            //    state = TouchLocationState.Moved;
            //    Mouse.SetPosition((int)e.GetX(), (int)e.GetY());
            //}
            //if (e.Action == MotionEventActions.Down)
            //{
            //    state = TouchLocationState.Pressed;
            //    Mouse.SetPosition((int)e.GetX(), (int)e.GetY());
            //}

            //TouchLocation tprevious;
            //TouchLocation tlocation;
            //Vector2 position = new Vector2(e.GetX(), e.GetY());
            //Vector2 translatedPosition = position;

            //switch (CurrentOrientation)
            //{
            //    case DisplayOrientation.Portrait:
            //        break;
            //    case DisplayOrientation.LandscapeRight:
            //        translatedPosition = new Vector2(ClientBounds.Height - position.Y, position.X);
            //        break;
            //    case DisplayOrientation.LandscapeLeft:
            //        translatedPosition = new Vector2(position.Y, ClientBounds.Width - position.X);
            //        break;
            //    case DisplayOrientation.PortraitUpsideDown:
            //        translatedPosition = new Vector2(ClientBounds.Width - position.X, ClientBounds.Height - position.Y);
            //        break;
            //}


            //if (state != TouchLocationState.Pressed && _previousTouches.TryGetValue(e.Handle, out tprevious))
            //{
            //    tlocation = new TouchLocation(e.Handle.ToInt32(), state, translatedPosition, e.Pressure, tprevious.State, tprevious.Position, tprevious.Pressure);
            //}
            //else
            //{
            //    tlocation = new TouchLocation(e.Handle.ToInt32(), state, translatedPosition, e.Pressure);
            //}

            //TouchPanel.Collection.Clear();
            //TouchPanel.Collection.Add(tlocation);

            //if (state != TouchLocationState.Released)
            //    _previousTouches[e.Handle] = tlocation;
            //else
            //    _previousTouches.Remove(e.Handle);

            //GamePad.Instance.Update(e);

            //return true;
        }
    }
}

