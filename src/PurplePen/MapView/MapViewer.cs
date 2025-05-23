/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using PurplePen.Graphics2D;

namespace PurplePen.MapView
{
    public class MapViewer : Control {
        #region Fields

        private IMapDisplay mapDisplay;							// The map display we are viewing.
        private ViewCache viewcache;							// The view cache for this map
        private Bitmap compositedBitmap;                        // Bitmap from view cache, composited with highlights/grid.
        private long lastViewCacheChangeNumber = -1;            // Last change number from viewCache

        // Timer for handling hover events.
        private System.Windows.Forms.Timer hoverTimer = new System.Windows.Forms.Timer();

        // Mouse left/right button and drag state.
        public const int LeftMouseButton = 0;
        public const int RightMouseButton = 1;
        public const int CountMouseButtons = 2;
        bool[] mouseDown = new bool[CountMouseButtons];			// state of mouse buttons
        bool[] canDrag = new bool[CountMouseButtons];			// is a drag allowed with this button?
        bool[] mouseDrag = new bool[CountMouseButtons];			// is a drag in progress with this button?
        PointF[] downPos = new PointF[CountMouseButtons];		// position mouse button went down, in world coordinates
        int[] downTime = new int[CountMouseButtons];			// time mouse button went down, from Environment.TickCount

        // Mouse location (in world coords)
        PointF mouseLocation;
        bool mouseInView;  // if false, mouseLocation is invalid.

        // Defines what part of the map we are viewing.
        private PointF centerPoint = new PointF(0,0);			// center point in world coordinates.
        private float zoom = 1.0F;							    // zoom, 1.0 == approx real world size.
        private readonly float pixelPerMm;						// number of pixel/mm on this display
        private Matrix xformWorldToPixel;						// transformation world->pixel coord
        private Matrix xformPixelToWorld;						// transformation pixel->world coord
        private RectangleF viewport;							// visible area in world coordinates

        private float minZoom = 0.1F, maxZoom = 10F;            // limits of zoom.

        bool gridOn = false;									// Is grid visible
        bool drawBounds = false;                        // Draw bounds of objects for testing?

        // The current highlight overlay.
        IMapViewerHighlight[] currentHighlights;

        // Map dragging state
        bool dragScrollingInProgress = false;					// Are we dragging the map around?
        MouseButtons endDragScrollButton;                       // Which mouse button ends drag scrolling.
        Point lastDragScrollPoint;								// last point we dragged to

        // Events that we raise
        public enum DragAction { None, MapDrag, ImmediateDrag, DelayedDrag };
        public delegate void PointerEventHandler(object sender, bool inViewport, PointF location);
        public delegate DragAction MouseEventHandler(object sender, MouseAction action, int buttonNumber, bool[] whichButtonsDown, PointF location, PointF locationStart);
        public event EventHandler OnViewportChange;
        public event PointerEventHandler OnPointerMove;
        public event PointerEventHandler OnPointerHover;
        public event MouseEventHandler OnMouseEvent;

        public enum WheelAction { None, Zoom, Scroll}

        // Various constants
        const int HandleRadius = 4;
        const int HandleDiameter = HandleRadius * 2 + 1;
        const float MaxClickDistance = 1.7F; // maximum distance in pixels to move to be a click
        const int MaxClickTime = 300;        // maximum time to be a click
        const float MinDragDistance = 2.8F;  // minimum distance in pixels to move to be a drag

        readonly Cursor DragCursor = Util.LoadCursor("DragCursor.cur");

        #endregion Fields

        #region Construction
        public MapViewer() {
            // Don't redraw the background.
            SetStyle(ControlStyles.Opaque, true);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            Graphics g = CreateGraphics();
            pixelPerMm = g.DpiX / 25.4F;
            g.Dispose();

            hoverTimer.Tick += hoverTimer_Tick;
        }

        public void SetMap(IMapDisplay mapDisplay) {
            if (this.mapDisplay != null)
                this.mapDisplay.Changed -= MapChanged;
            this.mapDisplay = mapDisplay;
            if (mapDisplay != null)
                mapDisplay.Changed += MapChanged;
            viewcache = new ViewCache(mapDisplay);
            ViewportChanged();
        }
        #endregion Construction

        #region Transformation between world/view coordinates
        // Calculate the world->client matrix and viewport that corresponds to the area we are viewing,
        // based on the size of the control and the centerPoint and zoom.  The returned viewport has
        // Top and Bottom reversed, to correspond to the convention that Top <= Bottom.
        void CalculateWorldTransform() {
            // Get size, midpoint of the window in pixel coords.
            Size sizeInPixels = this.ClientSize;
            PointF midpoint = new PointF(sizeInPixels.Width / 2.0F, sizeInPixels.Height / 2.0F);

            // Calculate the world->window transform.
            Matrix matrix = new Matrix();
            float scaleFactor = ScaleFactor;
            matrix.Translate(midpoint.X, midpoint.Y);
            matrix.Scale(scaleFactor, -scaleFactor);  // y scale is negative to get to cartesian orientation.
            matrix.Translate(- centerPoint.X, - centerPoint.Y);
            xformWorldToPixel = matrix.Clone();

            // Invert it to get the window->world transform.
            matrix.Invert();
            xformPixelToWorld = matrix;

            // Calculate the viewport in world coordinates.
            PointF[] pts = { new PointF(0.0F, (float) sizeInPixels.Height), new PointF((float) sizeInPixels.Width, 0.0F) };
            xformPixelToWorld.TransformPoints(pts);
            viewport = new RectangleF(pts[0].X, pts[0].Y, pts[1].X - pts[0].X, pts[1].Y - pts[0].Y);
        }

        float ScaleFactor
        {
            get { return pixelPerMm * zoom; }
        }

        // Give the zoom factor that would fit the given width in world coordinates in the viewport.
        public float ZoomFactorForWorldWidth(int pixelWidth, float worldWidth)
        {
            float scaleFactor = pixelWidth / worldWidth;
            return scaleFactor / pixelPerMm;
        }

        public Matrix GetWorldToPixelXform() {
            return xformWorldToPixel;
        }

        // Transform rectangle from world to pixel coordinates. 
        public RectangleF WorldToPixel(RectangleF rectWorld) {
            PointF[] pts = {rectWorld.Location, new PointF(rectWorld.Right, rectWorld.Bottom)};
            xformWorldToPixel.TransformPoints(pts);
            // Note that Y's are reversed, so we reverse the rectangle to make the rect height always positive.
            return new RectangleF(new PointF(pts[0].X, pts[1].Y), new SizeF(pts[1].X - pts[0].X, pts[0].Y - pts[1].Y)); 
        }

        // Transform rectangle from pixel to world coordinates. 
        public RectangleF PixelToWorld(RectangleF rectPixel) {
            PointF[] pts = {rectPixel.Location, new PointF(rectPixel.Right, rectPixel.Bottom)};
            xformPixelToWorld.TransformPoints(pts);

            // Note that Y's are reversed, so we reverse the rectangle to make the rect height always positive.
            return new RectangleF(new PointF(pts[0].X, pts[1].Y), new SizeF(pts[1].X - pts[0].X, pts[0].Y - pts[1].Y)); 
        }

        // Transform one point from world to pixel coordinates. 
        public PointF WorldToPixel(PointF ptWorld) {
            PointF[] pts = {ptWorld};
            xformWorldToPixel.TransformPoints(pts);
            return pts[0];
        }

        // Transform one point from pixel to world coordinates. 
        public PointF PixelToWorld(PointF ptPixel) {
            PointF[] pts = {ptPixel};
            xformPixelToWorld.TransformPoints(pts);
            return pts[0];
        }

        // Transform distance from world to pixel coordinates. 
        public float WorldToPixelDistance(float distWorld) {
            PointF[] pts = {new PointF(distWorld, 0)};
            xformWorldToPixel.TransformVectors(pts);
            return pts[0].X;
        }

        // Transform distance from pixe to world coordinates. 
        public float PixelToWorldDistance(float distPixel) {
            PointF[] pts = {new PointF(distPixel, 0)};
            xformPixelToWorld.TransformVectors(pts);
            return pts[0].X;
        }

        // Zoom, keeping the given point at the same location in pixel coordinates.
        public void ZoomAroundPoint(PointF zoomPtWorld, float newZoom) {
            PointF zoomPtPixel = WorldToPixel(zoomPtWorld);
            ZoomFactor = newZoom;
            PointF zoomPtWorldNew = PixelToWorld(zoomPtPixel);
            PointF centerPtWorld = CenterPoint;
            centerPtWorld.X += zoomPtWorld.X - zoomPtWorldNew.X;
            centerPtWorld.Y += zoomPtWorld.Y - zoomPtWorldNew.Y;
            CenterPoint = centerPtWorld;
        }

        // Update the center point to enforce scrolling bounds (if desired)
        PointF ConstrainCenterPoint(PointF potentialCenter, SizeF viewportSize, RectangleF scrollBounds)
        {
            PointF newCenter = potentialCenter;

            if (ConstrainedScrolling == ConstrainedScrollingMode.None || scrollBounds.IsEmpty || viewportSize.IsEmpty)
                return potentialCenter;

            if (scrollBounds.Width < viewportSize.Width) {
                // map is narrower than viewport. Constrain to be fully within bounds.
                if (ConstrainedScrolling == ConstrainedScrollingMode.PinCenter || ConstrainedScrolling == ConstrainedScrollingMode.PinTop) {
                    newCenter.X = (scrollBounds.Left + scrollBounds.Right) / 2.0F;
                }
                else if (ConstrainedScrolling == ConstrainedScrollingMode.KeepSome) {
                    newCenter.X = Math.Min(newCenter.X, scrollBounds.Right + viewportSize.Width / 2.0F - viewportSize.Width / 10.0F);
                    newCenter.X = Math.Max(newCenter.X, scrollBounds.Left - viewportSize.Width / 2.0F + viewportSize.Width / 10.0F);
                }
                else {
                    newCenter.X = Math.Min(newCenter.X, scrollBounds.Left + viewportSize.Width / 2.0F);
                    newCenter.X = Math.Max(newCenter.X, scrollBounds.Right - viewportSize.Width / 2.0F);
                }
            }
            else {
                // map is wider than viewport. Constrain to have nothing outside map visibiel
                if (ConstrainedScrolling == ConstrainedScrollingMode.KeepSome) {
                    newCenter.X = Math.Min(newCenter.X, scrollBounds.Right + viewportSize.Width / 2.0F - viewportSize.Width / 10.0F);
                    newCenter.X = Math.Max(newCenter.X, scrollBounds.Left - viewportSize.Width / 2.0F + viewportSize.Width / 10.0F);
                }
                else {
                    newCenter.X = Math.Max(newCenter.X, scrollBounds.Left + viewportSize.Width / 2.0F);
                    newCenter.X = Math.Min(newCenter.X, scrollBounds.Right - viewportSize.Width / 2.0F);
                }
            }

            if (scrollBounds.Height < viewportSize.Height) {
                // map is narrower than viewport. Constrain to be fully within bounds.
                if (ConstrainedScrolling == ConstrainedScrollingMode.PinCenter) {
                    newCenter.Y = (scrollBounds.Top + scrollBounds.Bottom) / 2.0F;
                }
                else if (ConstrainedScrolling == ConstrainedScrollingMode.PinTop) {
                    newCenter.Y = scrollBounds.Bottom - viewportSize.Height / 2.0F;

                }
                else if (ConstrainedScrolling == ConstrainedScrollingMode.KeepSome) {
                    newCenter.Y = Math.Min(newCenter.Y, scrollBounds.Bottom + viewportSize.Height / 2.0F - viewportSize.Height / 10.0F);
                    newCenter.Y = Math.Max(newCenter.Y, scrollBounds.Top - viewportSize.Height / 2.0F + viewportSize.Height / 10.0F);
                }
                else {
                    newCenter.Y = Math.Min(newCenter.Y, scrollBounds.Top + viewportSize.Height / 2.0F);
                    newCenter.Y = Math.Max(newCenter.Y, scrollBounds.Bottom - viewportSize.Height / 2.0F);
                }
            }
            else {
                // map is wider than viewport. Constrain to have nothing outside map visibiel
                if (ConstrainedScrolling == ConstrainedScrollingMode.KeepSome) {
                    newCenter.Y = Math.Min(newCenter.Y, scrollBounds.Bottom + viewportSize.Height / 2.0F - viewportSize.Height / 10.0F);
                    newCenter.Y = Math.Max(newCenter.Y, scrollBounds.Top - viewportSize.Height / 2.0F + viewportSize.Height / 10.0F);
                }
                else {
                    newCenter.Y = Math.Max(newCenter.Y, scrollBounds.Top + viewportSize.Height / 2.0F);
                    newCenter.Y = Math.Min(newCenter.Y, scrollBounds.Bottom - viewportSize.Height / 2.0F);
                }
            }

            return newCenter;
        }

        Graphics GetWorldGraphics() {
            Graphics g = CreateGraphics();
            g.Transform = xformWorldToPixel;
            return g;
        }

        // Get bounds to constrain scrolling within.
        RectangleF GetScrollBounds()
        {
            if (mapDisplay != null) {
                RectangleF bounds = mapDisplay.Bounds;
                bounds.Inflate(1 / ScaleFactor, 1 / ScaleFactor); // Add up to 1 pixel boundary.
                return bounds;
            }
            else {
                return new RectangleF();
            }
        }

        // Apply scrolling constraints.
        public void Recenter()
        {
            CenterPoint = ConstrainCenterPoint(centerPoint, viewport.Size, GetScrollBounds());
        }

        // Get the size of one pixel.
        public float PixelSize
        {
            get
            {
                return PixelToWorldDistance(1);
            }
        }

        #endregion

        #region Property accessors
        public float ZoomFactor {
            get { return zoom; }
            set { 
                // clamp zoom to a reasonable value.
                if (value < minZoom)
                    value = minZoom;
                if (value > maxZoom)
                    value = maxZoom;

                if (zoom != value) {
                    zoom = value;
                    SizeF newViewportSize = new SizeF((float) this.Width / ScaleFactor, (float) this.Height / ScaleFactor);
                    ViewportChanged();
                }
            }
        }

        public PointF CenterPoint {
            get { return centerPoint; }
            set {
                if (centerPoint != value) {
                    centerPoint = ConstrainCenterPoint(value, viewport.Size, GetScrollBounds());
                    ViewportChanged();			
                }
            }
        }

        public PointF PointerLocation {
            get {
                return mouseLocation;
            }
        }

        public bool PointerInView {
            get { return mouseInView; }
        }

        public RectangleF Viewport {
            get {
                return viewport;
            }

            set {
                float oldZoom = zoom; 
                PointF oldCenterPoint = centerPoint;
                double newHorizZoom = viewport.Width / value.Width * zoom;
                double newVertZoom = viewport.Height / value.Height * zoom;

                zoom = (float) Math.Min(newHorizZoom, newVertZoom);
                if (zoom < minZoom)
                    zoom = minZoom;
                if (zoom > maxZoom)
                    zoom = maxZoom;

                centerPoint = new PointF((value.Left + value.Right) / 2.0F, (value.Top + value.Bottom) / 2.0F);

                SizeF newViewportSize = new SizeF(viewport.Width * oldZoom / zoom, viewport.Height * oldZoom / zoom);
                centerPoint = ConstrainCenterPoint(centerPoint, newViewportSize, GetScrollBounds());

                if (zoom != oldZoom || centerPoint != oldCenterPoint)
                    ViewportChanged();
            }
        }

        public bool ShowGrid {
            get { 
                return gridOn;
            }

            set {
                if (value && !gridOn) {
                    // Grid turning on.
                    using (Graphics g = GetWorldGraphics()) {
                        DrawGrid(g, viewport);
                    }
                    gridOn = true;
                }
                else if (!value && gridOn) {
                    // Grid turning off.
                    Invalidate();
                    gridOn = false;
                }
            }
        }

        [DefaultValue(10F)]
        public float MaxZoomFactor
        {
            get { return maxZoom; }
            set
            {
                maxZoom = value;
                ZoomFactor = zoom;
            }
        }

        [DefaultValue(0.1F)]
        public float MinZoomFactor
        {
            get { return minZoom; }
            set
            {
                minZoom = value;
                ZoomFactor = zoom;
            }
        }

        public bool ShowSymbolBounds
        {
            get
            {
                return drawBounds;
            }
            set
            {
                if (drawBounds != value) {
                    drawBounds = value;
                    Invalidate();
                }
            }
        }

        // The range in which the center point can scroll, in world coordination
        private RectangleF ScrollingRange
        {
            get
            {
                float left, right, top, bottom;
                SizeF viewportSize = Viewport.Size;
                RectangleF scrollBounds = GetScrollBounds();

                if (scrollBounds.Width < viewportSize.Width) {
                    left = right = (scrollBounds.Left + scrollBounds.Right) / 2.0F;
                }
                else {
                    left = scrollBounds.Left + viewportSize.Width / 2.0F;
                    right = scrollBounds.Right - viewportSize.Width / 2.0F;
                }

                if (scrollBounds.Height < viewportSize.Height) {
                    top = bottom = (scrollBounds.Top + scrollBounds.Bottom) / 2.0F;
                }
                else {
                    top = scrollBounds.Top + viewportSize.Height / 2.0F;
                    bottom = scrollBounds.Bottom - viewportSize.Height / 2.0F;
                }

                return RectangleF.FromLTRB(left, top, right, bottom);
            }
        }

        public bool VScrollEnable
        {
            get
            {
                RectangleF scrollingRange = ScrollingRange;
                return !(scrollingRange.Height <= 0);
            }
        }

        public int VScrollLargeChange
        {
            get {
                RectangleF scrollingRange = ScrollingRange;
                if (ScrollingRange.Height <= 0)
                    return 100;
                int value = (int)Math.Round(100F * viewport.Height / (scrollingRange.Height + viewport.Height));
                return Math.Max(0, Math.Min(value, 100));
            }
        }

        public int VScrollSmallChange
        {
            get {
                RectangleF scrollingRange = ScrollingRange;
                if (ScrollingRange.Height <= 0)
                    return 0;
                int value = (int)Math.Round(100F * (40F / ScaleFactor) / (scrollingRange.Height + viewport.Height));
                return Math.Max(0, Math.Min(value, 100));
            }
        }

        public int VScrollValue
        {
            get
            {
                RectangleF scrollingRange = ScrollingRange;
                if (scrollingRange.Height <= 0)
                    return 0;
                int value = (int)Math.Round((100F - (VScrollLargeChange - 1)) * ((scrollingRange.Bottom - centerPoint.Y) / scrollingRange.Height));
                value =  Math.Max(0, Math.Min(value, 100));
                return value;
            }
            set
            {
                RectangleF scrollingRange = ScrollingRange;
                if (VScrollLargeChange < 100) {
                    float newY = scrollingRange.Bottom - (((float)value / (100F - (VScrollLargeChange - 1))) * ScrollingRange.Height);
                    ScrollView(0, (int) Math.Round((newY - centerPoint.Y) * ScaleFactor));
                }
            }
        }

        public int HoverDelay { get; set; }

        public bool MiddleButtonAutoDrag { get; set; } = true;

        public WheelAction MouseWheelAction { get; set; } = MapViewer.WheelAction.Zoom;

        public Size MouseWheelScrollAmount { get; set; } = new Size(0, 20);

        public enum ConstrainedScrollingMode { None, KeepSome, KeepAll, PinTop, PinCenter}

        public ConstrainedScrollingMode ConstrainedScrolling { get; set; } = ConstrainedScrollingMode.None;

        #endregion Property Accessors

        #region Change handling
        void MapChanged(Region regionChanged) {
            if (regionChanged != null) {
                // Transform the changed region into pixel coordinates and invalidate it.
                Region copy = regionChanged.Clone();
                copy.Transform(xformWorldToPixel);
                Invalidate(copy); 
            }
            else {
                Invalidate();
            }

            // Check if we need to scroll things to be within bounds again.
            PointF constrainedCenter = ConstrainCenterPoint(centerPoint, viewport.Size, GetScrollBounds());
            if (centerPoint != constrainedCenter) {
                CenterPoint = constrainedCenter;
            }
        }

        // Indicates that the viewport was changed, and recalculates
        // everything based on ClientSize, Zoom, and CenterPoint.
        void ViewportChanged() {
            CalculateWorldTransform();
            Invalidate();

            if (OnViewportChange != null)
                OnViewportChange(this, EventArgs.Empty);
        }

        #endregion Change handling


        #region IMapViewerHighlight drawing

        void DrawHighlights(Graphics g, Rectangle visRect, IMapViewerHighlight[] highlights)
        {
            if (highlights == null)
                return;

            // Set the rendering origin so the hatch brushes draw correctly and don't scroll weirdly.
            PointF origin = WorldToPixel(new PointF(0, 0));
            g.RenderingOrigin = Util.PointFromPointF(origin);
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            foreach (IMapViewerHighlight h in highlights) {
                h.DrawHighlight(g, xformWorldToPixel);
            }
        }

        void EraseHighlights(Graphics g, Rectangle visRect, IMapViewerHighlight[] highlights) {
            if (highlights == null)
                return;

            // Get brush that erases.
            Brush eraseBrush = viewcache.GetCacheBrush(ClientSize, viewport, xformWorldToPixel);

            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            foreach (IMapViewerHighlight h in highlights) {
                h.EraseHighlight(g, xformWorldToPixel, eraseBrush);
            }
        }

        public void ChangeHighlight(IMapViewerHighlight[] newHighlights)
        {
            if (newHighlights == currentHighlights)
                return;		// nothing to do.

            Rectangle dirtyOld = DirtyRect(currentHighlights);
            Rectangle dirtyNew = DirtyRect(newHighlights);
            Rectangle dirty;
            if (dirtyOld.IsEmpty)
                dirty = dirtyNew;
            else if (dirtyNew.IsEmpty)
                dirty = dirtyOld;
            else 
                dirty = Rectangle.Union(dirtyOld, dirtyNew);
            dirty.Intersect(ClientRectangle);

            currentHighlights = newHighlights;

            if (!dirty.IsEmpty) {
                dirty.Inflate(1, 1);
                CompositeBitmap(true);  // Force recomposite of bitmap.
                using (Graphics g = CreateGraphics()) {
                    Draw(g, dirty);
                }
            }
        }

        private Rectangle DirtyRect(IMapViewerHighlight[] highlights)
        {
            if (highlights == null || highlights.Length == 0)
                return new Rectangle();

            RectangleF accum = new RectangleF();
            foreach (IMapViewerHighlight h in highlights) {
                RectangleF bound = WorldToPixel(h.GetHighlightBounds());
                int borderPixels = h.GetBorderPixels();
                bound.Inflate(borderPixels, borderPixels);
                if (accum.IsEmpty)
                    accum = bound;
                else
                    accum = RectangleF.Union(accum, bound);
            }

            if (accum.IsEmpty)
                return new Rectangle();

            return Rectangle.FromLTRB((int)Math.Floor(accum.Left), (int)Math.Floor(accum.Top), (int)Math.Ceiling(accum.Right), (int)Math.Ceiling(accum.Bottom));
        }

        #endregion

        #region Hover handling

        private PointF lastHoverLocation;

        private void DisableHoverTimer()
        {
            hoverTimer.Enabled = false;
            lastHoverLocation = new PointF(float.NaN, float.NaN);
        }

        private void ResetHoverTimer(PointF location)
        {
            if (location != lastHoverLocation) {
                hoverTimer.Stop();
                hoverTimer.Interval = this.HoverDelay;
                hoverTimer.Enabled = true;
                hoverTimer.Start();
                lastHoverLocation = location;
            }
        }

        void hoverTimer_Tick(object sender, EventArgs e)
        {
            if (hoverTimer.Enabled && OnPointerHover != null) {
                OnPointerHover(this, mouseInView, mouseLocation);
            }

            hoverTimer.Enabled = false;
        }

        #endregion

        #region Mouse left/right button handling
        int ButtonNumberForEvent(MouseButtons b) {
            switch (b) {
            case MouseButtons.Left:		return LeftMouseButton;
            case MouseButtons.Right:	return RightMouseButton;
            default:					return -1;
            }
        }


        void PointerMoved(bool mouseInViewport, int xViewport, int yViewport) {
            this.mouseInView = mouseInViewport;

            if (mouseInView) {
                mouseLocation = PixelToWorld(new PointF(xViewport, yViewport));

                if (OnMouseEvent != null)
                    OnMouseEvent(this, MouseAction.Move, -1, mouseDown, mouseLocation, mouseLocation);

                ResetHoverTimer(mouseLocation);

                // See if dragging is occurring/starting for any button
                for (int buttonNumber = 0; buttonNumber < CountMouseButtons; ++buttonNumber) {
                    // is a drag being started?
                    if (mouseDown[buttonNumber] && !mouseDrag[buttonNumber] && canDrag[buttonNumber] &&
                        WorldToPixelDistance(Util.DistanceF(mouseLocation, downPos[buttonNumber])) >= MinDragDistance) {
                        mouseDrag[buttonNumber] = true;
                        DisableHoverTimer();
                    }

                    // is a drag in progress?
                    if (OnMouseEvent != null && mouseDown[buttonNumber] && mouseDrag[buttonNumber]) {
                        OnMouseEvent(this, MouseAction.Drag, buttonNumber, mouseDown, mouseLocation, downPos[buttonNumber]);
                        DisableHoverTimer();
                    }
                }
            }
            else {
                mouseLocation = new PointF();
                DisableHoverTimer();
            }

            if (OnPointerMove != null)
                OnPointerMove(this, mouseInView, mouseLocation);
        }

        void MouseButtonDown(int buttonNumber, int xViewport, int yViewport) {
            PointF worldMouse = PixelToWorld(new PointF(xViewport, yViewport));

            mouseDown[buttonNumber] = true;
            mouseDrag[buttonNumber] = false;
            canDrag[buttonNumber] = false;
            downPos[buttonNumber] = worldMouse;
            downTime[buttonNumber] = Environment.TickCount;

            if (OnMouseEvent != null) {
                DragAction dragAction = OnMouseEvent(this, MouseAction.Down, buttonNumber, mouseDown, worldMouse, worldMouse);
                canDrag[buttonNumber] = (dragAction == DragAction.ImmediateDrag || dragAction == DragAction.DelayedDrag);
                if (dragAction == DragAction.ImmediateDrag) {
                    mouseDrag[buttonNumber] = true;
                    DisableHoverTimer();
                }

                if (dragAction == DragAction.MapDrag) {
                    // Map dragging has been requested.
                    BeginMapDragging(new Point(xViewport, yViewport), (buttonNumber == LeftMouseButton) ? MouseButtons.Left : MouseButtons.Right);
                }
            }

            return;
        }

        void MouseButtonUp(int buttonNumber, int xViewport, int yViewport) {
            PointF worldMouse = PixelToWorld(new PointF(xViewport, yViewport));
            bool wasDown = mouseDown[buttonNumber];
            bool wasDrag = mouseDrag[buttonNumber];
            bool wasClick = false;

            if (wasDown && !wasDrag &&
                WorldToPixelDistance(Util.DistanceF(worldMouse, downPos[buttonNumber])) <= MaxClickDistance &&
                Environment.TickCount - downTime[buttonNumber] <= MaxClickTime) {
                wasClick = true;
            }


            mouseDown[buttonNumber] = false;
            mouseDrag[buttonNumber] = false;

            if (OnMouseEvent != null) {
                if (wasDrag)
                    OnMouseEvent(this, MouseAction.DragEnd, buttonNumber, mouseDown, worldMouse, downPos[buttonNumber]);
                else if (wasClick)
                    OnMouseEvent(this, MouseAction.Click, buttonNumber, mouseDown, downPos[buttonNumber], downPos[buttonNumber]);
                else if (wasDown)
                    OnMouseEvent(this, MouseAction.Up, buttonNumber, mouseDown, worldMouse, downPos[buttonNumber]);
            }

            return;
        }

        // Cancel any drags in progress.
        void CancelAllDrags()
        {
            for (int buttonNumber = 0; buttonNumber < mouseDrag.Length; buttonNumber++) {
                mouseDown[buttonNumber] = false;
                if (mouseDrag[buttonNumber]) {
                    mouseDrag[buttonNumber] = false;
                    OnMouseEvent(this, MouseAction.DragCancel, buttonNumber, mouseDown, downPos[buttonNumber], downPos[buttonNumber]);
                }
            }
        }

        #endregion

        #region Drag scrolling (middle button drag, or as request is response to a mouse down event.)

        // Scroll the given rectangle into view. Doesn't change the zoom factor.
        public void ScrollRectangleIntoView(RectangleF rect)
        {
            float dx = 0, dy = 0;
            int dxPixels, dyPixels;

            if (rect.Left < viewport.Left && rect.Right <= viewport.Right)
                dx = Math.Min(viewport.Left - rect.Left, viewport.Right - rect.Right);
            else if (rect.Right > viewport.Right && rect.Left >= viewport.Left)
                dx = -Math.Min(rect.Right - viewport.Right, rect.Left - viewport.Left);

            if (rect.Top < viewport.Top && rect.Bottom <= viewport.Bottom)
                dy = -Math.Min(viewport.Top - rect.Top, viewport.Bottom - rect.Bottom);
            else if (rect.Bottom > viewport.Bottom && rect.Top >= viewport.Top)
                dy = Math.Min(rect.Bottom - viewport.Bottom, rect.Top - viewport.Top);

            if (dx == 0 && dy == 0)
                return;         // Nothing to do.

            // Scroll a whole number of pixels.
            dxPixels = (int) Math.Ceiling(WorldToPixelDistance(dx));
            dyPixels = (int) Math.Ceiling(WorldToPixelDistance(dy));

            ScrollView(dxPixels, dyPixels);
        }


        [DllImport("user32.dll")]
        private static extern bool ScrollWindow(IntPtr hwnd, int dx, int dy, IntPtr lpRect, IntPtr lpClipRect);

        // Scroll the view by a certain number of PIXELs.
        public void ScrollView(int dxPixels, int dyPixels) {
            PointF centerPixels = WorldToPixel(centerPoint);
            PointF newCenterPixel = new PointF(centerPixels.X - dxPixels, centerPixels.Y - dyPixels);
            PointF newCenter = PixelToWorld(newCenterPixel);

            PointF constrainedCenter = ConstrainCenterPoint(newCenter, viewport.Size, GetScrollBounds());
            if (constrainedCenter != newCenter) {
                PointF constrainedPixelCenter = WorldToPixel(constrainedCenter);
                dxPixels = (int) Math.Truncate(centerPixels.X - constrainedPixelCenter.X);
                dyPixels = (int) Math.Truncate(centerPixels.Y - constrainedPixelCenter.Y);
                newCenterPixel = new PointF(centerPixels.X - dxPixels, centerPixels.Y - dyPixels);
                newCenter = PixelToWorld(newCenterPixel);
            }

            bool success;
            success = ScrollWindow(this.Handle, dxPixels, dyPixels, IntPtr.Zero, IntPtr.Zero);

            if (success) {
                // Clear the "uncovered area" (looks better).
                Rectangle rect = this.ClientRectangle;
                rect.Offset(dxPixels, dyPixels);
                using (Graphics g = CreateGraphics()) {
                    g.ExcludeClip(rect);
                    g.Clear(Color.White);
                }

                centerPoint = newCenter;
                CalculateWorldTransform();

                if (OnViewportChange != null)
                    OnViewportChange(this, EventArgs.Empty);
            }
            else {
                Debug.Fail("Why did the Win32 API call fail?");
                CenterPoint = newCenter; // Use another way to scroll if the API doesn't work.
            }
        }

        public void BeginMapDragging(Point pt, MouseButtons endingButton) {
            dragScrollingInProgress = true;
            endDragScrollButton = endingButton;
            lastDragScrollPoint = pt;
            this.Cursor = DragCursor;
            DisableHoverTimer();
        }

        void EndMapDragging(Point pt) {
            dragScrollingInProgress = false;
            this.Cursor = Cursors.Default;
        }

        void MapDrag(Point pt) {
            Debug.Assert(dragScrollingInProgress);
            ScrollView(pt.X - lastDragScrollPoint.X, pt.Y - lastDragScrollPoint.Y);

            lastDragScrollPoint = pt;
        }
        #endregion Drag scrolling 

        #region Drawing
        void DrawGrid(Graphics g, RectangleF visRect) {
            Pen pen = new Pen(Color.FromArgb(100, Color.MidnightBlue), 0.0F);
            
            // Draw the grid.
            for (float x = (int)((visRect.Left) / 10.0F) * 10.0F; x <= visRect.Right; x += 10.0F) {
                g.DrawLine(pen, x, visRect.Top, x, visRect.Bottom);
            }
            for (float y = (int)((visRect.Top) / 10.0F) * 10.0F; y <= visRect.Bottom; y += 10.0F) {
                g.DrawLine(pen, visRect.Left, y, visRect.Right, y);
            }

            pen.Dispose();
        }

        void CompositeBitmap(bool highlightsHaveChanged)
        {
            long changeNumber;
            Bitmap bitmap = viewcache.GetCacheBitmap(ClientSize, viewport, xformWorldToPixel, out changeNumber);
            if (!gridOn && (currentHighlights == null || currentHighlights.Length == 0)) {
                // Nothing to composite.
                compositedBitmap = bitmap;
            }
            else {
                if (changeNumber != lastViewCacheChangeNumber || highlightsHaveChanged) {
                    compositedBitmap = (Bitmap)bitmap.Clone();
                    using (Graphics g = Graphics.FromImage(compositedBitmap)) {
                        Rectangle clientRect = ClientRectangle;
                        DrawHighlights(g, clientRect, currentHighlights);
                        if (gridOn)
                            DrawGrid(g, clientRect);
                    }
                }
            }

            lastViewCacheChangeNumber = changeNumber;
        }

       

        private void Draw(Graphics g, Rectangle clip) {
            PointF[] pts = new PointF[2];

            if (mapDisplay == null) {
                g.Clear(Color.White);
                return;
            }

            CompositeBitmap(false);
            FastBitmapPaint.PaintBitmap(g, compositedBitmap, clip, new Point(clip.Left, clip.Top));
        }

        // Creates a bitmap that is an exact snapshot of what the 
        // view would be. The size of the bitmap will be exactly the client size of the view.
        public Bitmap CreateSnapshotView() {
            Rectangle rect = new Rectangle(new Point(0,0), ClientSize);
            Bitmap bitmap = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(bitmap)) {
                Draw(g, rect);
            }

            return bitmap;
        }

        #endregion Drawing

        #region Hit testing
        // Hit test against highlighted handles, return the location of the best one, or (Nan, Nan) if none.
        public PointF HitTestHandles(PointF locationTest, bool testBeziers) {
            return new PointF(float.NaN, float.NaN);
#if false
            float bestDistance = float.MaxValue;
            PointF bestPoint = new PointF(float.NaN, float.NaN);

            if (currentHighlights == null)
                return bestPoint;

            RectangleF hitBounds;

            // Compute the hit bounds rectangle. A point must be within this rectangle
            // to be considered. This is equivalent to making sure the actual handle square is clicked on
            Point locationPixel = Util.PointFromPointF(WorldToPixel(locationTest));
            Rectangle rectPixel = new Rectangle(locationPixel.X - HandleRadius, locationPixel.Y - HandleRadius, HandleDiameter, HandleDiameter);
            hitBounds = PixelToWorld(rectPixel);

            // Determine the closest point of those handles that were clicked on.
            for (int i = 0; i < currentHighlights.handles.Length; ++i) {
                PointF pt = currentHighlight.handles[i];

                // Don't check bezier handles unless requested to.
                if (!testBeziers && currentHighlight.handleKinds[i] == PointKind.BezierControl)
                    continue;

                if (hitBounds.Contains(pt)) {
                    float distance = Util.DistanceF(pt, locationTest);
                    if (distance < bestDistance) {
                        bestDistance = distance;
                        bestPoint = pt;
                    }
                }
            }

            return bestPoint;
#endif
        }
        #endregion Hit testing


        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // MapViewer
            // 
            this.BackColor = System.Drawing.Color.White;
            this.CausesValidation = false;
            this.ForeColor = System.Drawing.Color.Black;
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MapViewer_MouseWheel);
            this.MouseLeave += new System.EventHandler(this.MapViewer_MouseLeave);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapViewer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapViewer_MouseMove);
            this.MouseCaptureChanged += new System.EventHandler(this.MapViewer_MouseCaptureChanged);
            this.Resize += new System.EventHandler(this.MapViewer_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MapViewer_Paint);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MapViewer_MouseUp);
            this.ResumeLayout(false);

        }
        #endregion

        #region Event handlers

        private void MapViewer_Paint(object sender, System.Windows.Forms.PaintEventArgs e) {
            Draw(e.Graphics, e.ClipRectangle);
        }

        private void MapViewer_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle) { 
                if (MiddleButtonAutoDrag)
                    BeginMapDragging(new Point(e.X, e.Y), e.Button);
            }
            else {
                int buttonNumber = ButtonNumberForEvent(e.Button);
                if (buttonNumber >= 0)
                    MouseButtonDown(buttonNumber, e.X, e.Y);
            }
        }

        private void MapViewer_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (dragScrollingInProgress && e.Button == endDragScrollButton) { 
                EndMapDragging(new Point(e.X, e.Y));
            }
            else {
                int buttonNumber = ButtonNumberForEvent(e.Button);
                if (buttonNumber >= 0)
                    MouseButtonUp(buttonNumber, e.X, e.Y);
            }
        }

        private void MapViewer_Resize(object sender, System.EventArgs e) {
            ViewportChanged();

            // Check if we need to scroll things to be within bounds again.
            PointF constrainedCenter = ConstrainCenterPoint(centerPoint, viewport.Size, GetScrollBounds());
            if (centerPoint != constrainedCenter) {
                CenterPoint = constrainedCenter;
            }
        }

        private void MapViewer_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (dragScrollingInProgress)
                MapDrag(new Point(e.X, e.Y));
            else
                PointerMoved(true, e.X, e.Y);
        }

        private void MapViewer_MouseLeave(object sender, System.EventArgs e) {
            PointerMoved(false, 0, 0);
        }

        private void MapViewer_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (dragScrollingInProgress)
                EndMapDragging(lastDragScrollPoint);

            CancelAllDrags();
        }

        private void MapViewer_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (MouseWheelAction == WheelAction.Zoom) {
                MouseWheelZoom(e);
            }
            else if (MouseWheelAction == WheelAction.Scroll) {
                MouseWheelScroll(e);
            }
        }

        private void MouseWheelZoom(MouseEventArgs e)
        {
            const double zoomChange = 1.1892;  // The four root of 2. So four scrolls zooms by 2x.
            int wheelDelta = e.Delta;
            float zoom = ZoomFactor;

            // Determine the point to zoom around.
            Rectangle rect = this.ClientRectangle;
            PointF zoomPtPixel, zoomPtWorld;

            if (rect.Contains(e.X, e.Y))
                zoomPtPixel = new PointF(e.X, e.Y);
            else
                return;
            zoomPtWorld = PixelToWorld(zoomPtPixel);

            // Determine the new zoom factor.
            double zoomAmount = Math.Pow(zoomChange, (Math.Abs(e.Delta) / 120F));
            if (wheelDelta > 0) {
                zoom *= (float)zoomAmount;
            }
            else {
                zoom /= (float)zoomAmount;
            }

            ZoomAroundPoint(zoomPtWorld, zoom);
        }

        private void MouseWheelScroll(MouseEventArgs e)
        {
            double wheelDelta = (double) e.Delta / 120.0;  // Number of wheel scrolls.

            int scrollX = (int)Math.Round(MouseWheelScrollAmount.Width * wheelDelta);
            int scrollY = (int)Math.Round(MouseWheelScrollAmount.Height * wheelDelta);
            ScrollView(scrollX, scrollY);
        }


        #endregion Event Handlers

        #region Keyboard handling

        protected override bool IsInputKey(Keys keyData)
        {
            // Capture arrow keys, tab, Enter.
            if ((keyData == Keys.Left) || (keyData == Keys.Right) || (keyData == Keys.Up) || (keyData == Keys.Down) || (keyData == Keys.Tab) || (keyData == Keys.Enter))
                return true;
            else
                return base.IsInputKey(keyData);
        }

        #endregion
    }


    // Types of mouse actions.
    public enum MouseAction {
        Down,	   // mouse button pressed down
        Move,      // mouse was moved
        Drag,      // mouse was dragged with a button down, occurs together with (and after) MouseMove if dragging enabled

        // When mouse button is released, exactly one of the follow three occurs.
        Up,        // mouse button released (dragging disabled) 
        DragEnd,   // mouse button released (if dragging enabled)
        Click,     // mouse button release after no/little movement 

        // If a drag is started, but the mouse is taken away before finishing, a DragCancel event occurs
        DragCancel,
    }


}
