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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using PurplePen.MapModel;
using PurplePen.MapView;
using ColorMatrix = PurplePen.MapModel.ColorMatrix;
using PurplePen.Graphics2D;
using System.IO;
using System.Linq;


namespace PurplePen
{
    // The map display represents everything that is cached in the ViewCache and normally shown
    // on the map. It includes the map proper, as well as the courses and so forth drawn on top.
    // The IMapDisplay interface is the communication channel with the ViewCache and the MapViewer
    // controls -- it simply has to be able to draw itself, and notify when parts change.
    class MapDisplay: IMapDisplay
    {
        MapType mapType;    // Map type. Note that OpenMapper and OCAD files both called MapType.OCAD. See MapVersion property to distinguish.
        string filename;

        // Used for MapType.Bitmap or MapType.PDF
        IGraphicsBitmap bitmap;     // the bitmap
        IGraphicsBitmap dimmedBitmap;  // the dimmed bitmap.
        float bitmapDpi;     // dpi for bitmap

        MapFileFormat mapVersion;       // OCAD version. (OCAD maps only)
        Map map;                // The map to draw. (OCAD maps only)

        PdfMapFile pdfMapFile;  // pdfMapFile (PDF maps only)

        CourseLayout course;    // The course to display.
        Map courseMap;              // The courses, rendered into a map.

        float mapIntensity = 1.0F;   // Intensity to display the map at.
        ColorModel colorModel = ColorModel.CMYK; // color model to use (cannot be OCADCompatible)
        bool antialiased = false;        // anti-alias (high quality) the map display?
        bool ocadOverprintEffect = false; // overprint effect for colors in the OCAD map (not the purple of the course on top).
        bool showBounds = false;       // show symbols bounds (for testing)

        int? lowerPurpleMapLayer;      // If non-null, place the lower purple of the map just above this OCAD ID color.

        RectangleF? printArea;          // print area to display, or null for none.

        // Clones this map display.
        public MapDisplay Clone()
        {
            MapDisplay newMapDisplay = (MapDisplay) MemberwiseClone();

            newMapDisplay.dimmedBitmap = null;         // clones should not share dimmed bitmaps.
            newMapDisplay.UpdateDimmedBitmap();

            return newMapDisplay;
        }

        // Clones this map display, and set the clone to full intensity
        public MapDisplay CloneToFullIntensity()
        {
            MapDisplay newMapDisplay = (MapDisplay)MemberwiseClone();

            newMapDisplay.dimmedBitmap = null;         // clones should not share dimmed bitmaps.
            newMapDisplay.mapIntensity = 1;

            return newMapDisplay;
        }

        // Map type we're drawing.
        public MapType MapType
        {
            get
            {
                return mapType;
            }
        }

        // File name of the map.
        public string FileName
        {
            get
            {
                return filename;
            }
        }

        // OCAD version of the map.
        public MapFileFormat MapVersion
        {
            get
            {
                return mapVersion;
            }
        }

        // Scale of the map
        public float MapScale
        {
            get
            {
                if (map != null) {
                    using (map.Read())
                        return map.MapScale;
                }
                else
                    return 0;
            }
        }

        // Dpi of the bitmap
        public float Dpi
        {
            get
            {
                if (mapType == MapType.Bitmap)
                    return bitmapDpi;
                else
                    return 0;
            }

            set
            {
                bitmapDpi = value;
                RaiseChanged(null);        // redraw everything.
            }
        }

        // Real world coordinates
        public RealWorldCoords RealWorldCoords
        {
            get
            {
                if (map != null) {
                    using (map.Read())
                        return map.RealWorldCoords;
                }
                else {
                    return new RealWorldCoords();
                }
            }
        }

        // Bounds of the map, or empty if no map.
        public RectangleF MapBounds
        {
            get
            {
                switch (mapType) {
                case MapType.Bitmap:
                case MapType.PDF:
                    int pixelWidth = bitmap.PixelWidth;
                    int pixelHeight = bitmap.PixelHeight;
                    if (pixelWidth == 0 || pixelHeight == 0) {
                        return new RectangleF();
                    }
                    else {
                        return Geometry.TransformRectangle(BitmapTransform(), new RectangleF(0, 0, pixelWidth, pixelHeight));
                    }

                    case MapType.OCAD:
                    if (map != null) {
                        using (map.Read())
                            return map.Bounds;
                    }
                    else
                        return new RectangleF();

                case MapType.None:
                    return new RectangleF();

                default:
                    Debug.Fail("bad maptype");
                    return new RectangleF();
                }
            }
        }

        // Bounds of both the map and the course.
        public RectangleF Bounds
        {
            get
            {
                RectangleF mapBounds = MapBounds;
                RectangleF courseBounds = new RectangleF();
                
                if (courseMap != null) {
                    using (courseMap.Read())
                        courseBounds = courseMap.Bounds;
                }

                if (mapBounds.IsEmpty)
                    return courseBounds;
                else if (courseBounds.IsEmpty)
                    return mapBounds;
                else
                    return RectangleF.Union(mapBounds, courseBounds);
            }
        }


        // Colors in the map, or empty list if no map or bitmap map.
        public List<SymColor> GetMapColors()
        {
            if (mapType == MapType.OCAD && map != null) {
                using (map.Read())
                    return new List<SymColor>(map.AllColors);
            }
            else {
                return new List<SymColor>();
            }
        }

        // Templates in the map, or empty list in no map or bitmap map.
        public IList<TemplateInfo> GetMapTemplates()
        {
            if (mapType == MapType.OCAD && map != null) {
                using (map.Read())
                    return map.Templates;
            }
            else {
                return new List<TemplateInfo>();
            }
        }

        // Get a coordinate mapper for the map, or null if mapping coordinates isn't possible.
        public CoordinateMapper CoordinateMapper
        {
            get {
                if (mapType == MapType.OCAD && map != null)
                    return new CoordinateMapper(map);
                else
                    return null;
            }
        }

        // Intensity to draw the map at.
        public float MapIntensity
        {
            get
            {
                return mapIntensity;
            }

            set
            {
                if (MapIntensity != value) {
                    mapIntensity = value;
                    UpdateDimmedBitmap();
                    RaiseChanged(null);
                }
            }
        }

        // Color model to use
        public ColorModel ColorModel
        {
            get { return colorModel; }
            set
            {
                if (colorModel != value) {
                    colorModel = value;
                    RaiseChanged(null);
                }
            }
        }

        public ImageFormat BitmapImageFormat
        {
            get
            {
                if (MapType == MapType.Bitmap && bitmap != null)
                    return ((GDIPlus_Bitmap)bitmap).Bitmap.RawFormat;
                else
                    return ImageFormat.MemoryBmp;
            }
        }


        // Missing fonts?
        public string[] MissingFonts()
        {
            if (mapType == MapType.OCAD)
                return map.MissingFonts;
            else
                return null;
        }

        // Nonrenderable objects?
        public string[] NonRenderableObjects()
        {
            if (mapType == MapType.OCAD)
                return map.NotRenderableObjects;
            else
                return null;
        }

        // Anti-alias the map display?
        public bool AntiAlias
        {
            get
            {
                return antialiased;
            }
            set
            {
                if (antialiased != value) {
                    antialiased = value;
                    RaiseChanged(null);
                }
            }
        }

        // User Overprint effect for OCAD map
        public bool OcadOverprintEffect
        {
            get
            {
                return ocadOverprintEffect;
            }
            set
            {
                if (ocadOverprintEffect != value) {
                    ocadOverprintEffect = value;
                    RaiseChanged(null);
                }
            }
        }

        // If non-null, place the lower purple of the map just above this OCAD ID color.
        public int? LowerPurpleMapLayer {
            get {
                return lowerPurpleMapLayer;
            }
            set {
                if (lowerPurpleMapLayer != value) {
                    lowerPurpleMapLayer = value;
                    RaiseChanged(null);
                }
            }
        }


        // Are we printing?
        public bool Printing { get; set; }

        // Anti-alias the map display?
        public bool ShowSymbolBounds
        {
            get
            {
                return showBounds;
            }
            set
            {
                if (showBounds != value) {
                    showBounds = value;
                    RaiseChanged(null);
                }
            }
        }

        // Get a color matrix from the current map intensity value. Can return null if intensity is 1.0.
        ColorMatrix ComputeColorMatrix()
        {
            if (mapIntensity < 0.99F) {
                float[][] colorMatrixElements = { 
                           new float[] {(float)mapIntensity,  0,  0,  0, 0},
                           new float[] {0,  (float)mapIntensity,  0,  0, 0},
                           new float[] {0,  0,  (float)mapIntensity,  0, 0},
                           new float[] {0,  0,  0,  1, 0},
                           new float[] {(float) (1-mapIntensity), (float) (1-mapIntensity), (float) (1-mapIntensity), 0, 1}};
                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                return colorMatrix;
            }
            else
                return null;            // full intensity.
        }

        // Computer the transform from coordinates of the bitmap to world coordinates
        Matrix BitmapTransform()
        {
            // (worldcoord in mm) / 25.4F * dpi = pixels
            float scaleFactor = bitmapDpi / 25.4F;

            Matrix matrix = new Matrix();
            matrix.Translate(0, bitmap.PixelHeight);
            matrix.Scale(scaleFactor, -scaleFactor);
            matrix.Invert();
            return matrix;
        }

        // Set the map file used to draw. 
        public void SetMapFile(MapType newMapType, string newFilename)
        {
            this.mapType = newMapType;
            this.filename = newFilename;
            this.bitmap = null;
            this.map = null;

            if (newMapType == MapType.None) {
                map = null;
                mapVersion = new MapFileFormat(MapFileFormatKind.None, 0);
                bitmap = null;
                pdfMapFile = null;
            }
            else if (newMapType == MapType.OCAD) {
                map = new Map(MapUtil.TextMetricsProvider, new GDIPlus_FileLoader(Path.GetDirectoryName(newFilename)));
                mapVersion = InputOutput.ReadFile(newFilename, map);
                bitmap = null;
                pdfMapFile = null;
            }
            else if (newMapType == MapType.Bitmap) {
                map = null;
                mapVersion = new MapFileFormat(MapFileFormatKind.None, 0);
                Bitmap bm = (Bitmap)Image.FromFile(newFilename);
                bitmap = new GDIPlus_Bitmap(bm);
                bitmapDpi = bm.HorizontalResolution;
                pdfMapFile = null;
            }
            else if (newMapType == MapType.PDF) {
                string errorText;
                map = null;
                mapVersion = new MapFileFormat(MapFileFormatKind.None, 0);
                Size bitmapSize;
                pdfMapFile = MapUtil.ValidatePdf(newFilename, out bitmapDpi, out bitmapSize, out errorText);
                if (pdfMapFile == null) {
                    this.mapType = MapType.None;
                    bitmap = null;
                }
                else {
                    Bitmap bm = (Bitmap)Image.FromFile(pdfMapFile.PngFileName);
                    bitmap = new GDIPlus_Bitmap(bm);
                }
            }
            else {
                Debug.Fail("bad maptype");
            }

            UpdateDimmedBitmap();
            RaiseChanged(null);        // redraw everything.
        }


        // Set the courses being displayed.
        public void SetCourse(CourseLayout newCourse)
        {
            if (! object.Equals(course, newCourse)) {
                // The ordering of layers depends on whether we have a lower purple map layer.
                CourseLayout.MapRenderOptions renderOptions = new CourseLayout.MapRenderOptions();
                renderOptions.LowerColorsBelowWhiteOut = (LowerPurpleMapLayer != null);

                course = newCourse;
                if (course == null)
                    courseMap = null;
                else
                    courseMap = course.RenderToMap(renderOptions);

                RaiseChanged(null);
            }
        }

        // Set the print area to display, or null to not display print area.
        public void SetPrintArea(RectangleF? printArea)
        {
            if (!this.printArea.Equals(printArea)) {
                this.printArea = printArea;
                RaiseChanged(null);
            }
        }

        // Update the dimmed bitmap. If the intensity is < 1 and we're using a bitmap, dim it.
        public void UpdateDimmedBitmap()
        {
            if (dimmedBitmap != null)
                dimmedBitmap.Dispose();
            dimmedBitmap = null;

            // Only dim bitmap if size isn't too large. Otherwise takes too much memory.
            if ((mapType == MapType.Bitmap || mapType == MapType.PDF) && mapIntensity < 0.99F && (bitmap.PixelWidth * bitmap.PixelHeight) < 36000000) {
                Bitmap dimmed = null;
                try {
                    dimmed = new Bitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                }
                catch (Exception) {
                    return;  // typically because not enough memory. Uses alternate dimming method.
                }

                Graphics g = Graphics.FromImage(dimmed);
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(ComputeColorMatrix());
                g.DrawImage(((GDIPlus_Bitmap)bitmap).Bitmap, new Rectangle(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), 0, 0, bitmap.PixelWidth, bitmap.PixelHeight, GraphicsUnit.Pixel, imageAttributes);
                g.Dispose();

                dimmedBitmap = new GDIPlus_Bitmap(dimmed);
            }
            else {
                dimmedBitmap = null;
            }
        }


        // Draw the ocad map part.
        void DrawOcadMap(IGraphicsTarget grTarget, RectangleF visRect, RenderOptions renderOptions)
        {
            using (map.Write()) {
                map.Draw(grTarget, visRect, renderOptions, null);
            }
        }

        // Draw the bitmap map part.
        void DrawBitmapMap(IGraphicsTarget grTarget, RectangleF visRect, float minResolution)
        {
            // Setup transform.
            grTarget.PushTransform(BitmapTransform());

            // Setup drawing map and intensity.
            BitmapScaling scalingMode = antialiased ? BitmapScaling.MediumQuality : BitmapScaling.NearestNeighbor;
            if (bitmap.PixelHeight * bitmap.PixelWidth > 50000000)
                scalingMode = BitmapScaling.NearestNeighbor;            // Turn off high quality scaling for very large bitmaps.

            // Get source bitmap. Use the dimmed bitmap if there is one.
            IGraphicsBitmap sourceBitmap;
            if (dimmedBitmap != null)
                sourceBitmap = dimmedBitmap;
            else
                sourceBitmap = bitmap;

            // Draw it.
            grTarget.DrawBitmap(sourceBitmap, new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), scalingMode, minResolution);

            if (mapIntensity < 0.99 && sourceBitmap == bitmap) {
                // Dimming desired, but we don't have a dimmed bitmap. Use an alpha mask instead.
                CmykColor dimmedWhite = CmykColor.FromCmyka(0, 0, 0, 0, 1-mapIntensity);
                object brush = new object();
                grTarget.CreateSolidBrush(brush, dimmedWhite);
                grTarget.FillRectangle(brush, new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }

            // Pop transform
            grTarget.PopTransform();
        }

        // For bitmap or PDF background, write the bitmap to the given file with the given format.
        public void WriteBitmapMap(string fileName, ImageFormat format, out float dpi)
        {
            dpi = this.bitmapDpi;
            Bitmap bmp = ((GDIPlus_Bitmap)bitmap).Bitmap;
            
            BitmapUtil.SaveBitmap(bmp, fileName, format);
        }

        // Draw the map and course onto a bitmap of the given size. The given rectangle is mapped onto the whole bitmap, then
        // the given clip region is applied.
        public void Draw(Bitmap bitmap, Matrix transform, Region clipRegion = null)
        {
            Debug.Assert(colorModel == ColorModel.CMYK || colorModel == ColorModel.RGB);
            GDIPlus_ColorConverter colorConverter = (colorModel == ColorModel.CMYK) ? new SwopColorConverter() : new GDIPlus_ColorConverter();

            if (bitmap.Height == 0 || bitmap.Width == 0)
                return;

            // Note that courses always drawn full intensity.
            using (var grTargetDimmed = new GDIPlus_BitmapGraphicsTarget(bitmap, CmykColor.FromCmyk(0,0,0,0), transform, colorConverter, mapIntensity))
            using (var grTargetUndimmed = new GDIPlus_BitmapGraphicsTarget(bitmap, null, transform, colorConverter)) {
                float minResolution = GetMinResolution(transform);
                Matrix transformInverse = transform.Clone();
                transformInverse.Invert();
                RectangleF clipBounds = Geometry.TransformRectangle(transformInverse, new RectangleF(0, 0, bitmap.Width, bitmap.Height));
                DrawHelper(grTargetDimmed, grTargetUndimmed, grTargetUndimmed, clipBounds, minResolution);
            }
        }

        float GetMinResolution(Matrix transform)
        {
            Matrix m = transform.Clone();
            m.Invert();
            return Geometry.TransformDistance(1, m);
        }


        // Draw the map and course onto a graphics target. The color model is ignored. The intensity
        // must be 1, and purple blending is never performed.
        public void Draw(IGraphicsTarget grTarget, RectangleF visRect, float minResolution)
        {
            Debug.Assert(MapIntensity == 1.0F);
            DrawHelper(grTarget, grTarget, grTarget, visRect, minResolution);
        }

        private short? ColorIdBelow(Map map, short colorId)
        {
            using (map.Read()) {
                List<SymColor> allColors = map.AllColors.ToList();
                int index = allColors.FindIndex(sc => sc.OcadId == colorId);
                return (index > 0) ? allColors[index - 1].OcadId : (short?)null;
            }
        }

        // Draw the map and course onto a graphics. A helper for the other two draw methods.
        private void DrawHelper(IGraphicsTarget grTargetOcadMap, IGraphicsTarget grTargetBitmapMap, IGraphicsTarget grTargetCourses, RectangleF visRect, float minResolution)
        {
            RenderOptions renderOptions = new RenderOptions();
            renderOptions.minResolution = minResolution;
            short? colorIdBelowWhiteOut = null;

            // Get the color ID of the color just below the white-out color, which is where we divide between lower purple
            // and upper purple.
            if (LowerPurpleMapLayer != null && courseMap != null) {
                colorIdBelowWhiteOut = ColorIdBelow(courseMap, CourseLayout.WHITEOUT_COLOR_OCADID);
            }

            if (Printing)
                renderOptions.usePatternBitmaps = false;   // don't use pattern bitmaps when printing, they cause some problems in some printer drivers and we want best quality.
            else if (antialiased && minResolution < 0.007F)  // use pattern bitmaps unless high quality and zoomed in very far
                renderOptions.usePatternBitmaps = false;
            else
                renderOptions.usePatternBitmaps = true;

            renderOptions.showSymbolBounds = showBounds;
            renderOptions.renderTemplates = RenderTemplateOption.MapAndTemplates;
            renderOptions.blendOverprintedColors = ocadOverprintEffect;

            // First draw the real map.
            switch (mapType) {
            case MapType.OCAD:
                grTargetOcadMap.PushAntiAliasing(Printing ? false : antialiased);       // don't anti-alias on printer

                if (LowerPurpleMapLayer != null) {
                    // Only draw the part below lower purple.
                    renderOptions.colorBeginDrawExclusive = null;
                    renderOptions.colorEndDrawInclusive = LowerPurpleMapLayer;
                }

                DrawOcadMap(grTargetOcadMap, visRect, renderOptions);
                grTargetOcadMap.PopAntiAliasing();
                break;

            case MapType.Bitmap:
            case MapType.PDF:
                grTargetBitmapMap.PushAntiAliasing(Printing ? false : antialiased);       // don't anti-alias on printer
                DrawBitmapMap(grTargetBitmapMap, visRect, minResolution);
                grTargetBitmapMap.PopAntiAliasing();
                break;

            case MapType.None:
                break;
            }

            // Now draw the courseMap on top.
            if (LowerPurpleMapLayer != null) {
                // Only draw the part below the white-out layer, which is all of the lower purple.
                renderOptions.colorBeginDrawExclusive = null;
                renderOptions.colorEndDrawInclusive = colorIdBelowWhiteOut;
            }

            DrawCourseMap(grTargetCourses, visRect, renderOptions);

            if (LowerPurpleMapLayer != null) {
                // Draw the part of the map above the lower purple.
                grTargetOcadMap.PushAntiAliasing(Printing ? false : antialiased);       // don't anti-alias on printer

                if (LowerPurpleMapLayer != null) {
                    // Only draw the part below lower purple.
                    renderOptions.colorBeginDrawExclusive = LowerPurpleMapLayer;
                    renderOptions.colorEndDrawInclusive = null;
                }

                DrawOcadMap(grTargetOcadMap, visRect, renderOptions);
                grTargetOcadMap.PopAntiAliasing();

                // Draw the rest of the course map on top.
                renderOptions.colorBeginDrawExclusive = colorIdBelowWhiteOut;
                renderOptions.colorEndDrawInclusive = null;
                DrawCourseMap(grTargetCourses, visRect, renderOptions);
            }

            DrawPrintAreaOutline(grTargetCourses, visRect);
        }

        private void DrawCourseMap(IGraphicsTarget grTargetCourses, RectangleF visRect, RenderOptions renderOptions)
        {
            if (Printing)
                grTargetCourses.PushAntiAliasing(false);
            else
                grTargetCourses.PushAntiAliasing(true);   // always anti-alias the course unless printing

            // Always turn blending on.
            bool saveBlendOverprintedColors = renderOptions.blendOverprintedColors;
            renderOptions.blendOverprintedColors = true;

            if (courseMap != null) {
                using (courseMap.Read())
                    courseMap.Draw(grTargetCourses, visRect, renderOptions, null);
            }

            // Restore old blending setting.
            renderOptions.blendOverprintedColors = saveBlendOverprintedColors;

            grTargetCourses.PopAntiAliasing();
        }

        // Draw a gray outline to show the print area.
        private void DrawPrintAreaOutline(IGraphicsTarget grTargetCourses, RectangleF visRect)
        {
            grTargetCourses.PushAntiAliasing(false);

            if (printArea.HasValue && !printArea.Value.Contains(visRect)) {
                object printAreaOutline = new object();

                grTargetCourses.CreateSolidBrush(printAreaOutline, CmykColor.FromCmyka(0, 0, 0, 1, 0.12F));
                if (printArea.Value.Top > visRect.Top) {
                    RectangleF draw = RectangleF.FromLTRB(visRect.Left, visRect.Top, visRect.Right, printArea.Value.Top);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
                if (printArea.Value.Bottom < visRect.Bottom) {
                    RectangleF draw = RectangleF.FromLTRB(visRect.Left, printArea.Value.Bottom, visRect.Right, visRect.Bottom);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
                if (printArea.Value.Left > visRect.Left) {
                    RectangleF draw = RectangleF.FromLTRB(visRect.Left, printArea.Value.Top, printArea.Value.Left, printArea.Value.Bottom);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
                if (printArea.Value.Right < visRect.Right) {
                    RectangleF draw = RectangleF.FromLTRB(printArea.Value.Right, printArea.Value.Top, visRect.Right, printArea.Value.Bottom);
                    grTargetCourses.FillRectangle(printAreaOutline, draw);
                }
            }

            grTargetCourses.PopAntiAliasing();
        }

        public event MapDisplayChanged Changed;

        // Raise the changed event.
        private void RaiseChanged(Region region)
        {
            if (Changed != null)
                Changed(region);
        }
    }
}
