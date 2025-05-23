﻿/* Copyright (c) 2006-2007, Peter Golde
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
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Threading;
using PurplePen.MapView;
using PurplePen.MapModel;

using PurplePen.DebugUI;
using PurplePen.Graphics2D;
using PurplePen.Livelox;
using static PurplePen.BitmapCreationSettings;
using PurplePen.Livelox.ApiContracts;

namespace PurplePen
{
    partial class MainFrame : DpiFixedForm, IUserInterface
    {
        Controller controller;
        SymbolDB symbolDB;
        MapDisplay mapDisplay;
        MapDisplay topologyMapDisplay;

        long changeNum = 0;         // When this changes, state information needs to be updated in the UI.

        TextPart[] selectionDesc;         // The current selection description.

        DescriptionPrintSettings descPrintSettings = new DescriptionPrintSettings();     // printing settings for the description;
        PunchPrintSettings punchPrintSettings = new PunchPrintSettings();     // printing settings for the description;
        CoursePrintSettings coursePrintSettings = new CoursePrintSettings();   // printing settings for courses.
        CoursePdfSettings coursePdfSettings = null;   // PDF creation settings for courses.
        OcadCreationSettings ocadCreationSettingsPrevious = null;     // creation settings for OCAD creation, if it has been done before.
        ExportKmlSettings exportKmlSettingsPrevious = null;     // creation settings for KML creation, if it has been done before.
        BitmapCreationSettings bitmapCreationSettingsPrevious = null; // creation settings for image creation, if it has been done before.
        RouteGadgetCreationSettings routeGadgetCreationSettingsPrevious = null;  // creation settings for RouteGadget creation, if it has been done before.
        GpxCreationSettings gpxCreationSettingsPrevious = null;  // creation settings for Gpx creation, if it has been done before.
        LiveloxPublishSettings liveloxPublishSettingsPrevious = null;  // settings for Livelox export, if it has been done before.

        Uri helpFileUrl;                       // URL of the help file.

        Point lastTooltipLocation;
        bool showToolTips = true;
        bool hidePrintArea = false;   // If true, don't show print area right now despite settings.

        bool checkForUpdatedMapFile = false;  // If true, check for updated map file on next idle.

        const double TRACKBAR_MIN = 0.25;      // minimum zoom on the zoom trackbar
        const double TRACKBAR_MAX = 10.0;     // maximum zoom on the zoom trackbar

        const string HELP_FILE_NAME = "Purple Pen Help.chm";

        const float DEFAULT_MAP_INTENSITY = 0.7F;

        int vScrollbarWidth;  // width of a default scroll bar.

        Image dangerousImage, oobImage;

        public MainFrame()
        {
            Font = SystemFonts.MessageBoxFont;
            InitializeComponent();

            // Set height of tab strip appropriately.
            courseTabs.Height -= (courseTabs.DisplayRectangle.Height + 5);

            // Using the property designer for these doesn't totally work.
            veryLowIntensityMenu.Tag = 0.4;
            lowIntensityMenu.Tag = 0.55;
            mediumIntensityMenu.Tag = 0.7;
            highIntensityMenu.Tag = 0.85;
            fullIntensityMenu.Tag = 1.0;
                
            // Set the trackbar properties that can't be done in the designer.
            zoomTracker.TrackBar.TickStyle = TickStyle.None;
            zoomTracker.TrackBar.Minimum = 0;
            zoomTracker.TrackBar.Maximum = 100;

            // Get the size of a vertical scroll bar.
            VScrollBar scrollBar = new VScrollBar();
            vScrollbarWidth = scrollBar.GetPreferredSize(new Size(200, 200)).Width;
            scrollBar.Dispose();

            showToolTips = Settings.Default.ShowPopupInfo;

            SetMenuIcons();

            Application.Idle += new EventHandler(Application_Idle);
        }

        // Set the icons on the menu to match the corresponding toolbar icons.
        void SetMenuIcons()
        {
            openMenu.Image = openToolStripButton.Image;
            saveMenu.Image = saveToolStripButton.Image;
            undoMenu.Image = undoToolStripButton.Image;
            redoMenu.Image = redoToolStripButton.Image;
            addControlMenu.Image = addControlToolStripButton.Image;
            addStartMenu.Image = addStartToolStripButton.Image;
            addFinishMenu.Image = addFinishToolStripButton.Image;
            deleteMenu.Image = deleteToolStripButton.Image;
            deleteItemMenu.Image = deleteToolStripButton.Image;
            addMapExchangeMenu.Image = mapExchangeToolStripMenu.Image;
            addMapFlipMenuItem.Image = mapFlipMenuItem.Image;
            mapExchangeControlMenuItem.Image = mapExchangeControlToolStripMenuItem.Image;
            mapExchangeSeparateMenuItem.Image = mapExchangeSeparateToolStripMenuItem.Image;
            addMapIssueMenu.Image = mapIssuePointToolStripMenuItem.Image;
            addOptCrossingMenu.Image = optionalCrossingPointToolStripMenuItem.Image;
            addMandatoryCrossingMenu.Image = mandatoryCrossingPointToolStripMenuItem.Image;
            addWaterMenu.Image = waterLocationToolStripMenuItem.Image;
            addRegMarkMenu.Image = registrationMarkToolStripMenuItem.Image;
            addForbiddenMenu.Image = forbiddenRouteMarkingToolStripMenuItem.Image;
            addFirstAidMenu.Image = firstAidLocationToolStripMenuItem.Image;
            addDescriptionsMenu.Image = descriptionsToolStripMenuItem.Image;
            oobImage = addOutOfBoundsMenu.Image = outOfBoundsToolStripMenuItem.Image;
            dangerousImage = addDangerousMenu.Image = dangerousToolStripMenuItem.Image;
            addConstructionMenu.Image = constructionToolStripMenuItem.Image;
            addBoundaryMenu.Image = boundaryToolStripMenuItem.Image;
            addTextMenu.Image = textToolStripMenuItem.Image;
            addImageMenu.Image = imageToolStripMenuItem.Image;
            addLineMenu.Image = lineToolStripMenuItem.Image;
            addRectangleMenu.Image = rectangleToolStripMenuItem.Image;
            addEllipseMenu.Image = ellipseToolStripMenuItem.Image;
            whiteOutMenu.Image = whiteOutToolStripMenuItem.Image;
            addGapMenu.Image = addGapToolStripButton.Image;
            addBendMenu.Image = addBendToolStripButton.Image;
            addVariationMenu.Image = addVariationToolStripButton.Image;
        }

        public void Initialize(Controller controller, SymbolDB symbolDB)
        {
            this.controller = controller;
            this.symbolDB = symbolDB;
            descriptionControl.SymbolDB = symbolDB;
        }

        public bool HidePrintArea
        {
            get { return hidePrintArea; }
            set
            {
                hidePrintArea = value;
                controller.ForceChangeUpdate();
            }
        }

        // Get the current location of the mouse pointer.
        public bool GetCurrentLocation(out PointF location, out float pixelSize)
        {
            pixelSize = mapViewer.PixelSize;

            if (mapViewer.PointerInView) {
                location = mapViewer.PointerLocation;
                return true;
            }
            else {
                location = new PointF(0, 0);
                return false;
            }
        }

        public void InitiateMapDragging(PointF initialPos, System.Windows.Forms.MouseButtons buttonEnd)
        {
            mapViewer.BeginMapDragging(Util.PointFromPointF(mapViewer.WorldToPixel(initialPos)), buttonEnd);
        }

        // Prompt the user for a file name to open.
        public string GetOpenFileName()
        {
            openFileDialog.FileName = null;
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                return openFileDialog.FileName;
            else
                return null;
        }

        // Prompt the user for a file name to open.
        public string GetSaveFileName(string initialName)
        {
            saveFileDialog.FileName = initialName;
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
                return saveFileDialog.FileName;
            else
                return null;
        }

        // Show an error message, with no choice.
        public void ErrorMessage(string message)
        {
            IWin32Window owner = this;
            if (!this.Visible)
                owner = null;

            if (descriptionControl != null)
                descriptionControl.CloseAnyPopup();

            MessageBox.Show(owner, message, MiscText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        // Show an warning message, with no choice.
        public void WarningMessage(string message)
        {
            if (descriptionControl != null)
                descriptionControl.CloseAnyPopup();

            MessageBox.Show(this, message, MiscText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        }

        // Show an informational message, with no choice.
        public void InfoMessage(string message)
        {
            if (descriptionControl != null)
                descriptionControl.CloseAnyPopup();

            MessageBox.Show(this, message, MiscText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        // Show a ok-cancel message.
        public bool OKCancelMessage(string message, bool okDefault)
        {
            if (descriptionControl != null)
                descriptionControl.CloseAnyPopup();

            DialogResult result = MessageBox.Show(this, message, MiscText.AppTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Information, okDefault ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2);
            return result == DialogResult.OK;
        }

        // Ask a yes-no question.
        public bool YesNoQuestion(string message, bool yesDefault)
        {
            if (descriptionControl != null)
                descriptionControl.CloseAnyPopup();

            DialogResult result = MessageBox.Show(this, message, MiscText.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question, yesDefault ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2);
            return result == DialogResult.Yes;
        }

        // Ask a yes-no-cancel question.
        public DialogResult YesNoCancelQuestion(string message, bool yesDefault)
        {
            if (descriptionControl != null)
                descriptionControl.CloseAnyPopup();

            return MessageBox.Show(this, message, MiscText.AppTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, yesDefault ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2);
        }

        public DialogResult MovingSharedControl(string controlCode, string otherCourses)
        {
            using (MoveControlChoiceDialog dialog = new MoveControlChoiceDialog(controlCode, otherCourses)) {
                return dialog.ShowDialog();
            }
        }

        // Update the title of the window to match the file name.
        void UpdateWindowTitle()
        {
            string newWindowTitle = string.Format("{0} - {1}", Path.GetFileNameWithoutExtension(controller.FileName), MiscText.AppTitle);
            if (this.Text != newWindowTitle)
                this.Text = newWindowTitle;
        }

        // Update the status text.
        void UpdateStatusText()
        {
            string statusText = controller.StatusText;
            if (statusText != statusLabel.Text)
                statusLabel.Text = statusText;
        }

        // Update the map file on display.
        void UpdateMapFile()
        {
            if (mapDisplay != controller.MapDisplay) {
                // The mapDisplay object is new. This currently only happens on startup.
                mapDisplay = controller.MapDisplay;
                mapDisplay.MapIntensity = Settings.Default.MapIntensity;
                mapDisplay.AntiAlias = Settings.Default.MapHighQuality;
                controller.ShowAllControls = Settings.Default.ViewAllControls;
                mapViewer.SetMap(mapDisplay);
                ShowRectangle(mapDisplay.MapBounds);
            }

            if (mapDisplay.MapType != controller.MapType || mapDisplay.FileName != controller.MapFileName || (controller.MapType == MapType.Bitmap && mapDisplay.Dpi != controller.MapDpi)) {
                // A new map file has been loaded, or the DPI has changed.

                mapViewer.ZoomFactor = 1.0F;   // used if the map bounds are empty, then this zoom factor is preserved.
                ShowRectangle(mapDisplay.MapBounds);

                // Reset the OCAD file creating settings dialog to default settings.
                ocadCreationSettingsPrevious = null;
                bitmapCreationSettingsPrevious = null;
            }

            if (mapDisplay.OcadOverprintEffect != controller.OcadOverprintEffect) {
                mapDisplay.OcadOverprintEffect = controller.OcadOverprintEffect;
            }

            if (mapDisplay.LowerPurpleMapLayer != controller.LowerPurpleMapLayer) {
               mapDisplay.LowerPurpleMapLayer = controller.LowerPurpleMapLayer;
            }
        }

        // Check for missing fonts in the map file, and warn about them. Only do if the window is visible, of course.
        void CheckForMissingFonts()
        {
            if (this.Visible) {
                string[] missingFonts = controller.MissingFontList();      // This only returns missing fonts once!

                if (missingFonts != null && missingFonts.Length > 0) {
                    // We have some missing fonts. Show the dialog.
                    MissingFonts dialog = new MissingFonts();
                    dialog.MapName = Path.GetFileName(controller.MapFileName);
                    dialog.MissingFontList = missingFonts;

                    dialog.ShowDialog();

                    controller.IgnoreMissingFontsForever(dialog.IgnoreMissingFonts);
                }
            }
        }

        // Update the tabs to match the state of the application. Most commonly, the set of tabs won't
        // change, so this procedure does no actually state changes of the control in that case.
        void UpdateTabs()
        {
            string[] tabNames = controller.GetTabNames();
            int tabCount = tabNames.Length;
            int oldTabCount = courseTabs.TabCount;

            for (int i = 0; i < tabCount; ++i) {
                if (i >= courseTabs.TabPages.Count) {
                    // Add a tab.
                    Debug.Assert(i == courseTabs.TabPages.Count);
                    courseTabs.TabPages.Add(tabNames[i]);
                }
                else {
                    // Rename a tab (if needed).
                    string tabName = tabNames[i];
                    if (courseTabs.TabPages[i].Text != tabName)
                        courseTabs.TabPages[i].Text = tabName;
                }
            }

            // Remove any extra tabs.
            for (int i = tabCount; i < oldTabCount; ++i) {
                courseTabs.TabPages.RemoveAt(tabCount);
            }

            int activeTab = controller.ActiveTab;

            if (activeTab != courseTabs.SelectedIndex)
                courseTabs.SelectedIndex = activeTab;
        }

        // Update the course in the map pane.
        void UpdateCourse()
        {
            mapDisplay.SetCourse(controller.GetCourseLayout());
        }

        // Update the topology pane display.
        void UpdateTopology()
        {
            if (topologyMapDisplay == null) {
                topologyMapDisplay = new MapDisplay();
                topologyMapDisplay.SetMapFile(MapType.None, null);
                topologyMapDisplay.AntiAlias = false;
                topologyMapDisplay.Printing = true;
                mapViewerTopology.SetMap(topologyMapDisplay);
            }

            CourseLayout topologyCourseLayout = controller.GetTopologyLayout();
            topologyMapDisplay.SetCourse(topologyCourseLayout);

            if (topologyCourseLayout == null)
                radioButtonDescriptions.Checked = true;
            radioButtonTopology.Enabled = (topologyCourseLayout != null);

            // Get zoom factor for the width, but constrained by min/max on the mapViewerTopology
            float desiredZoomFactor = mapViewerTopology.ZoomFactorForWorldWidth(panelTopology.Width - vScrollbarWidth, topologyMapDisplay.Bounds.Width);
            mapViewerTopology.ZoomFactor = desiredZoomFactor;
            mapViewerTopology.Recenter();

            UpdateTopologyScrollBars();
        }

        // Update the print area in the map pane.
        void UpdatePrintArea()
        {
            if (hidePrintArea || !Settings.Default.ShowPrintArea)
                mapDisplay.SetPrintArea(null);
            else
                mapDisplay.SetPrintArea(controller.GetCurrentPrintAreaRectangle(PrintAreaKind.OnePart));
        }

        // Update the part banner in the map pane.
        void UpdatePartBanner()
        {
            if (controller.NumberOfParts <= 1 && !controller.HasVariations) {
                SetBannerVisibility(false);
            }
            else {
                if (controller.HasVariations) {
                    coursePartBanner.AvailableVariations = controller.GetVariations();
                    coursePartBanner.CurrentVariation = controller.CurrentVariation;
                    coursePartBanner.EnableVariations = true;
                }
                else {
                    coursePartBanner.AvailableVariations = null;
                    coursePartBanner.EnableVariations = false;
                }
                
                if (controller.NumberOfParts >= 2) {
                    coursePartBanner.NumberOfParts = controller.NumberOfParts;
                    coursePartBanner.SelectedPart = controller.CurrentPart;
                    coursePartBanner.EnableParts = true;
                    UpdatePartBannerProperties();
                }
                else {
                    coursePartBanner.EnableParts = false;
                    coursePartBanner.EnableProperties = false;
                }
                
                SetBannerVisibility(true);
            }
        }

        // Update the properties button in the part banner.
        void UpdatePartBannerProperties()
        {
            // Don't enable properties for all parts.
            coursePartBanner.EnableProperties = (controller.CurrentPart >= 0);
        }

        void SetBannerVisibility(bool bannerVisible)
        {
            if (!coursePartBanner.Visible && bannerVisible) {
                // Banner becoming visible.
                coursePartBanner.Visible = true;
                mapViewer.ScrollView(0, - coursePartBanner.Height / 2);
            }
            else if (coursePartBanner.Visible && !bannerVisible) {
                // Banner becoming hidden.
                mapViewer.ScrollView(0, coursePartBanner.Height / 2);
                coursePartBanner.Visible = false;
            }
        }

        // Update the description in the description pane.
        void UpdateDescription()
        {
            CourseView.CourseViewKind kind;
            bool isCoursePart, hasCustomLength;

            descriptionControl.Description = controller.GetDescription(out kind, out isCoursePart, out hasCustomLength);
            descriptionControl.CourseKind = kind;
            descriptionControl.ScoreColumn = controller.GetScoreColumn();
            descriptionControl.IsCoursePart = isCoursePart;
            descriptionControl.HasCustomLength = hasCustomLength;
            descriptionControl.LangId = controller.GetDescriptionLanguage();
        }

        // Update the selected line.
        void UpdateSelection()
        {
            int firstLine, lastLine;
            controller.GetHighlightedDescriptionLines(out firstLine, out lastLine);
            descriptionControl.SetSelection(firstLine, lastLine);
        }

        // Update the highlights.
        void UpdateHighlight()
        {
            IMapViewerHighlight[] highlights = controller.GetHighlights(Pane.Map);

            if (controller.ScrollHighlightIntoView && highlights != null && highlights.Length >= 1) {
                // Get the bounds of all the highlights.
                RectangleF bounds = highlights[0].GetHighlightBounds();
                for (int i = 1; i < highlights.Length; ++i) 
                    bounds = RectangleF.Union(bounds, highlights[i].GetHighlightBounds());

                // Scroll the highlights into view.
                mapViewer.ScrollRectangleIntoView(bounds);
            }

            mapViewer.ChangeHighlight(highlights);
        }

        void UpdateTopologyHighlight()
        {
            IMapViewerHighlight[] highlights = controller.GetHighlights(Pane.Topology);

            mapViewerTopology.ChangeHighlight(highlights);
        }

        // Update all the labels and scroll-bars in the main frame.
        void UpdateLabelsAndScrollBars()
        {
            UpdatePointerLabel(mapViewer.PointerInView, mapViewer.PointerLocation);

            string zoomPercent = string.Format("{0}%", (int) Math.Round(mapViewer.ZoomFactor * 100));
            zoomAmountLabel.Text = MiscText.Zoom + ": " + zoomPercent;
            double zoomTrackValue = (Math.Log10(mapViewer.ZoomFactor) - Math.Log10(TRACKBAR_MIN)) * (100 / (Math.Log10(TRACKBAR_MAX) - Math.Log10(TRACKBAR_MIN)));
            if (zoomTrackValue < 0)
                zoomTrackValue = 0;
            else if (zoomTrackValue > 100)
                zoomTrackValue = 100;
            zoomTracker.Value = (int) Math.Round(zoomTrackValue);

            PointF center = mapViewer.CenterPoint;

            // Also update the scroll bars.
            RectangleF fullSize = new RectangleF(-1000, -1000, 2000, 2000);  // TODO: this should be the full size of the map.
            RectangleF viewport = mapViewer.Viewport;

            horizScroll.Minimum = (int) Math.Round(fullSize.Left);
            horizScroll.Maximum = (int) Math.Round(fullSize.Right);
            vertScroll.Maximum = -(int) Math.Round(fullSize.Top);
            vertScroll.Minimum = -(int) Math.Round(fullSize.Bottom);
            horizScroll.Value = (int) Math.Round(Math.Max(Math.Min(center.X, fullSize.Right), fullSize.Left));
            vertScroll.Value = (int) Math.Round(-Math.Max(Math.Min(center.Y, fullSize.Bottom), fullSize.Top));
            horizScroll.LargeChange = (int) Math.Round(viewport.Width * 0.9);
            vertScroll.LargeChange = (int) Math.Round(viewport.Height * 0.9);
            horizScroll.SmallChange = (int) Math.Round(viewport.Width / 8);
            vertScroll.SmallChange = (int) Math.Round(viewport.Height / 8);

        }

        void UpdateTopologyScrollBars()
        {
            if (mapViewerTopology.VScrollEnable) {
                topologyScrollBar.SmallChange = mapViewerTopology.VScrollSmallChange;
                topologyScrollBar.LargeChange = mapViewerTopology.VScrollLargeChange;
                topologyScrollBar.Value = mapViewerTopology.VScrollValue;
                if (!topologyScrollBar.Visible) {
                    topologyScrollBar.Visible = true;
                }
            }
            else {
                if (topologyScrollBar.Visible) {
                    topologyScrollBar.Visible = false;
                }
            }

        }

        // Update the label that shows the current pointer location.
        void UpdatePointerLabel(bool inViewport, System.Drawing.PointF location)
        {
            Debug.Assert(inViewport == mapViewer.PointerInView);
            Debug.Assert(location == mapViewer.PointerLocation);

            if (inViewport) {
                locationDisplay.Text = string.Format(" X:{0,-6:##0.0} Y:{1,-6:##0.0}", location.X, location.Y);
            }
            else {
                locationDisplay.Text = "";
            }
        }

        // Update a single menu item or toolbar item as hidden, disabled, or enabled.
        private void UpdateMenuItem(ToolStripItem menuItem, CommandStatus status)
        {
            switch (status) {
            case CommandStatus.Hidden:
                menuItem.Visible = false;
                break;

            case CommandStatus.Disabled:
                menuItem.Visible = true;
                menuItem.Enabled = false;
                break;

            case CommandStatus.Enabled:
                menuItem.Visible = true;
                menuItem.Enabled = true;
                break;

            default:
                Debug.Fail("bad command status");
                break;
            }
        }

        // Update menu item and toolbar buttons enabled/disabled state.
        private void UpdateMenusToolbarButtons()
        {
            // CONSIDER: this is called often (all idle states). We might need a way to make sure that this is called less often, like the other update commands.

            // Update Undo/Redo status
            UndoStatus status = controller.GetUndoStatus();

            if (controller.CanCancelMode()) {
                // Clear selection doubles as cancel current mode.
                cancelMenu.ShortcutKeyDisplayString = MiscText.Esc;     // Esc doesn't actually work as a shortcut key, but make it look like it.
                cancelMenu.Text = MiscText.CancelOperationWithShortcut;
                cancelMenu.Enabled = true;

                undoMenu.Enabled = false;
                undoToolStripButton.Enabled = false;
                redoMenu.Enabled = false;
                redoToolStripButton.Enabled = false;
            }
            else {
                cancelMenu.ShortcutKeyDisplayString = MiscText.Esc;     // Esc doesn't actually work as a shortcut key, but make it look like it.
                cancelMenu.Text = MiscText.ClearSelectionWithShortcut;
                cancelMenu.Enabled = true;

                if (status.CanUndo) {
                    undoToolStripButton.Enabled = true;
                    undoToolStripButton.Text = MiscText.Undo + " " + status.UndoName;
                    undoToolStripButton.ToolTipText = undoToolStripButton.Text + " (" + MiscText.CtrlZ + ")";
                    undoMenu.Enabled = true;
                    undoMenu.Text = MiscText.UndoWithShortcut + " " + status.UndoName;
                }
                else {
                    undoToolStripButton.Enabled = false;
                    undoToolStripButton.Text = MiscText.Undo;
                    undoToolStripButton.ToolTipText = undoToolStripButton.Text + " (" + MiscText.CtrlZ + ")";
                    undoMenu.Enabled = false;
                    undoMenu.Text = MiscText.UndoWithShortcut;
                }

                if (status.CanRedo) {
                    redoToolStripButton.Enabled = true;
                    redoToolStripButton.Text = MiscText.Redo + " " + status.RedoName;
                    redoToolStripButton.ToolTipText = redoToolStripButton.Text + " (" + MiscText.CtrlY + ")";
                    redoMenu.Enabled = true;
                    redoMenu.Text = MiscText.RedoWithShortcut + " " + status.RedoName;
                }
                else {
                    redoToolStripButton.Enabled = false;
                    redoToolStripButton.Text = MiscText.Redo;
                    redoToolStripButton.ToolTipText = redoToolStripButton.Text + " (" + MiscText.CtrlY + ")";
                    redoMenu.Enabled = false;
                    redoMenu.Text = MiscText.RedoWithShortcut;
                }
            }

            // Update checkmark on View/Show Popup Information
            showPopupsMenu.Checked = showToolTips;

            // Update checkmark on View/Show Print Area
            showPrintAreaMenu.Checked = Settings.Default.ShowPrintArea;

            // Update Delete menu item
            deleteToolStripButton.Enabled =  deleteMenu.Enabled = deleteItemMenu.Enabled = controller.CanDeleteSelection();

            // Update Create Files menu item
            string suffix = (createOcadFilesMenu.Text.EndsWith("...", StringComparison.CurrentCultureIgnoreCase)) ? "..." : "";
            createOcadFilesMenu.Text = controller.CreateOcadFilesText(true) + suffix;

            // Update Delete Course menu item.
            deleteCourseMenu.Enabled = controller.CanDeleteCurrentCourse();

            // Update Duplicate Course menu item
            duplicateCourseMenu.Enabled = controller.CanDuplicateCurrentCourse();

            // Update contextual Item menu items.
            UpdateMenuItem(addBendMenu, controller.CanAddBend());
            UpdateMenuItem(addBendToolStripButton, controller.CanAddBend());
            UpdateMenuItem(removeBendMenu, controller.CanRemoveBend());
            UpdateMenuItem(addGapMenu, controller.CanAddGap());
            UpdateMenuItem(addGapToolStripButton, controller.CanAddGap());
            UpdateMenuItem(removeGapMenu, controller.CanRemoveGap());
            UpdateMenuItem(rotateMenu, controller.CanRotate());
            UpdateMenuItem(stretchMenu, controller.CanStretch());
            UpdateMenuItem(changeTextMenu, controller.CanChangeText());
            UpdateMenuItem(changeLineAppearanceMenu, controller.CanChangeLineAppearance());
            UpdateMenuItem(addTextLineMenu, controller.CanAddTextLine());
            UpdateMenuItem(mapFlipMenuItem, controller.CanAddMapFlipControl());
            UpdateMenuItem(addMapFlipMenuItem, controller.CanAddMapFlipControl());
            UpdateMenuItem(splitToolStripMenuItem, controller.CanAddCuttingLine());
            UpdateMenuItem(mapExchangeControlMenuItem, controller.CanAddMapExchangeControl());
            UpdateMenuItem(mapExchangeControlToolStripMenuItem, controller.CanAddMapExchangeControl());
            UpdateMenuItem(mapExchangeSeparateMenuItem, controller.CanAddMapExchangeSeparate());
            UpdateMenuItem(mapExchangeSeparateToolStripMenuItem, controller.CanAddMapExchangeSeparate());
            UpdateMenuItem(mapExchangeToolStripMenu, controller.CanAddMapExchangeSeparate().Combine(controller.CanAddMapExchangeControl()));
            UpdateMenuItem(addMapExchangeMenu, controller.CanAddMapExchangeSeparate().Combine(controller.CanAddMapExchangeControl()));
            UpdateMenuItem(deleteForkMenu, controller.CanDeleteFork());
            UpdateMenuItem(courseVariationReportMenu, controller.CanGetVariationReport());
            UpdateMenuItem(otherCoursesMenu, controller.CanChangeExtraCourseDisplay());
            UpdateMenuItem(clearOtherCoursesMenu, controller.CanClearExtraCourseDisplay());

            // Update standards checkboxes and other menu items related to standards.
            string descriptionStandard = controller.GetDescriptionStandard();
            descriptionStd2004Menu.Checked = (descriptionStandard == "2004");
            descriptionStd2018Menu.Checked = (descriptionStandard == "2018");
            string mapStandard = controller.GetMapStandard();
            mapStd2000Menu.Checked = (mapStandard == "2000");
            mapStd2017Menu.Checked = (mapStandard == "2017");
            mapStdSpr2019Menu.Checked = (mapStandard == "Spr2019");
            dangerousToolStripMenuItem.Visible = (mapStandard == "2000");
            addDangerousMenu.Visible = (mapStandard == "2000");
            if (mapStandard == "2000") {
                outOfBoundsToolStripMenuItem.Image = addOutOfBoundsMenu.Image = oobImage;
            }
            else {
                outOfBoundsToolStripMenuItem.Image = addOutOfBoundsMenu.Image = dangerousImage;
            }

            // Update help menu
            UpdateMenuItem(helpTranslatedMenu, TranslatedWebSiteExists() ? CommandStatus.Enabled : CommandStatus.Hidden);

            FlaggingKind currentFlagging;
            CommandStatus flaggingStatus = controller.CanSetLegFlagging(out currentFlagging);
            UpdateMenuItem(legFlaggingMenu, flaggingStatus);
            if (flaggingStatus == CommandStatus.Enabled) {
                switch (currentFlagging) {
                case FlaggingKind.None:
                    entireFlaggingMenu.Checked = beginFlaggingMenu.Checked = endFlaggingMenu.Checked = false;
                    noFlaggingMenu.Checked = true; break;
                case FlaggingKind.All:
                    noFlaggingMenu.Checked = beginFlaggingMenu.Checked = endFlaggingMenu.Checked = false;
                    entireFlaggingMenu.Checked = true; break;
                case FlaggingKind.Begin:
                    noFlaggingMenu.Checked = entireFlaggingMenu.Checked = endFlaggingMenu.Checked = false;
                    beginFlaggingMenu.Checked = true; break;
                case FlaggingKind.End:
                    noFlaggingMenu.Checked = entireFlaggingMenu.Checked = beginFlaggingMenu.Checked = false;
                    endFlaggingMenu.Checked = true; break;
                }
            }

            CourseDesignator[] displayedCourses;
            bool showAllControls;
            UpdateMenuItem(changeDisplayedCoursesMenu, controller.CanChangeDisplayedCourses(out displayedCourses, out showAllControls));

            // Update Zoom menu items -- check the correct one (if any).
            float currentZoom = mapViewer.ZoomFactor;

            foreach (ToolStripMenuItem menuItem in zoomMenu.DropDown.Items) {
                float zoomlevel = (float) menuItem.Tag;
                float ratio = zoomlevel / currentZoom;
                menuItem.Checked = (ratio >= 0.95F && ratio <= 1.05F);        // If we're at about this zoom ratio, check the menu item.
            }

            if (mapDisplay != null) {
                // Update Map intensity menu items -- check the correct one.
                double currentIntensity = mapDisplay.MapIntensity;

                foreach (ToolStripMenuItem menuItem in mapIntensityMenu.DropDown.Items) {
                    double intensityAmount = (double) menuItem.Tag;
                    double ratio = intensityAmount / currentIntensity;
                    menuItem.Checked = (ratio >= 0.99F && ratio <= 1.01F);        // If we're at this intensity, check the menu item.
                }

                // Update map quality menu items
                normalQualityMenu.Checked = !mapDisplay.AntiAlias;
                highQualityMenu.Checked = mapDisplay.AntiAlias;
            }

            // Update View All Controls menu item.
            allControlsMenu.Checked = controller.ShowAllControls;
        }

        // Has the selection description changed?
        bool HasSelectionDescChanged(TextPart[] newSelectionDesc)
        {
            if (selectionDesc == null || newSelectionDesc == null)
                return (selectionDesc != newSelectionDesc);

            if (selectionDesc.Length != newSelectionDesc.Length)
                return true;

            for (int i = 0; i < selectionDesc.Length; ++i) {
                if (selectionDesc[i].format != newSelectionDesc[i].format ||
                    selectionDesc[i].text != newSelectionDesc[i].text)
                    return true;
            }

            return false;
        }

        // Update the text description in the selection panel
        void UpdateSelectionPanel()
        {
            const int HEADERGAP = 4;    // number of pixels extra space before a header
            const int INDENT = 12;    // number of pixels to index non-header lines

            TextPart[] description = controller.GetSelectionDescription();

            if (HasSelectionDescChanged(description)) {
                selectionDesc = description;

                selectionPanel.SuspendLayout();

                // Remove all previous controls.
                selectionPanel.Controls.Clear();

                // Add in each of the parts of the description, in order.
                if (description != null) {
                    foreach (TextPart part in description) {
                        // Add a line break after previous control if requested.
                        if ((part.format == TextFormat.Header || part.format == TextFormat.NewLine) &&
                            selectionPanel.Controls.Count > 0) {
                            selectionPanel.SetFlowBreak(selectionPanel.Controls[selectionPanel.Controls.Count - 1], true);
                        }

                        // Add a label with the text of the object.
                        Label label = new Label();
                        label.AutoSize = true;
                        label.BackColor = Color.Transparent;
                        label.UseMnemonic = false;
                        label.Text = part.text;
                        Padding margin = label.Margin;

                        if (part.format == TextFormat.Title) {
                            // A bit bigger font.
                            label.Font = new Font(selectionPanel.Font.FontFamily, selectionPanel.Font.SizeInPoints * 1.05F, FontStyle.Bold);
                        }
                        else if (part.format == TextFormat.Header) {
                            // Add a gap before headers.
                            label.Font = new Font(selectionPanel.Font, FontStyle.Bold);
                            Padding padding = label.Padding;
                            padding.Top += HEADERGAP;
                            label.Padding = padding;
                        }
                        else if (part.format == TextFormat.NewLine) {
                            // Add an indent before non-headers.
                            margin.Left += INDENT;
                            label.Margin = margin;
                        }
                        else if (part.format == TextFormat.SameLine)
                            label.Anchor = AnchorStyles.Bottom;

                        selectionPanel.Controls.Add(label);
                    }
                }

                selectionPanel.ResumeLayout();
            }
        }

        // Get the dictionary mapping each symbol to the singular custom text for it, and give it to the description control for the popups.
        void UpdateCustomSymbolText()
        {
            Dictionary<string, List<SymbolText>> customSymbolText;
            Dictionary<string, bool> customSymbolKey;

            controller.GetCustomSymbolText(out customSymbolText, out customSymbolKey);

            string langId = controller.GetDescriptionLanguage();
            Dictionary<string, string> symbolTextDict = new Dictionary<string,string>();

            foreach (var pair in customSymbolText) {
                if (Symbol.ContainsLanguage(pair.Value, langId))
                    symbolTextDict.Add(pair.Key, Symbol.GetBestSymbolText(symbolDB, pair.Value, langId, false, "", ""));
            }

            descriptionControl.CustomSymbolText = symbolTextDict;
        }



        void Application_Idle(object sender, EventArgs e)
        {
            if (IsDisposed)
                return;

            try {
                if (this.Visible) {
                    // The application is idle. If the application state has changed, update the
                    // user interface to match.
                    UpdateMenusToolbarButtons();   // This needs updating even if other things haven't changed.
                    UpdateStatusText();

                    if (controller.HasStateChanged(ref changeNum)) {
                        UpdateWindowTitle();
                        UpdateMapFile();
                        UpdateTabs();
                        UpdateCourse();
                        UpdateTopology();
                        UpdatePrintArea();
                        UpdatePartBanner();
                        UpdateDescription();
                        UpdateSelection();
                        UpdateHighlight();
                        UpdateTopologyHighlight();
                        UpdateSelectionPanel();
                        UpdateCustomSymbolText();
                        CheckForMissingFonts();
                        CheckForNonRenderableObjects(true, false);
                    }

                    if (checkForUpdatedMapFile) {
                        checkForUpdatedMapFile = false;
                        controller.CheckForChangedMapFile();
                    }
                }
            }
            catch (Exception excep) {
                // Unlike other Winforms events, the Application_Idle event does not give the cool dialog when an exception happens (which allows
                // the user to recover. [Bug 1688896]
                Application.OnThreadException(excep);
            }
        }

        private void coursePartBanner_SelectedVariationChanged(object sender, EventArgs e)
        {
            controller.CurrentVariation = coursePartBanner.CurrentVariation;
        }

        private void coursePartBanner_SelectedPartChanged(object sender, EventArgs e)
        {
            controller.SelectPart(coursePartBanner.SelectedPart);
            UpdatePartBannerProperties();
        }

        private void coursePartBanner_PropertiesClicked(object sender, EventArgs e)
        {
            int currentPart = controller.CurrentPart;
            int numberOfParts = controller.NumberOfParts;

            if (currentPart >= 0 && numberOfParts >= 0) {
                CoursePartProperties coursePartOptionsDialog = new CoursePartProperties();
                coursePartOptionsDialog.PartOptions = controller.ActivePartOptions;
                coursePartOptionsDialog.ShowFinishCircleEnabled = (currentPart != numberOfParts - 1);

                if (coursePartOptionsDialog.ShowDialog(this) == DialogResult.OK) {
                    controller.ChangeActivePartOptions(coursePartOptionsDialog.PartOptions);
                }
            }
        }

        private void symbolBrowserMenu_Click(object sender, EventArgs e)
        {
            SymbolBrowser symbolBrowser = new SymbolBrowser();
            symbolBrowser.Initialize(symbolDB);
            symbolBrowser.ShowDialog();
            symbolBrowser.Dispose();
        }

        private void descriptionBrowserMenu_Click(object sender, EventArgs e)
        {
            DescriptionBrowser browser = new DescriptionBrowser();
            browser.Initialize(controller.GetEventDB(), symbolDB);
            browser.ShowDialog();
            browser.Dispose();
        }

        private void controlTesterMenu_Click(object sender, EventArgs e)
        {
            ControlTester controlTester = new ControlTester();
            controlTester.Initialize(controller.GetEventDB(), symbolDB);
            controlTester.ShowDialog();
            controlTester.Dispose();
        }

        private void mapTesterMenu_Click(object sender, EventArgs e)
        {
            MapTester mapTester = new MapTester();
            mapTester.ShowDialog();
            mapTester.Dispose();
        }

        private void MainFrame_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }

        private void exitMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainFrame_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Either File/Exit or the close button clicked. See if we can exit.

            bool exit = controller.TryCloseFile();
            if (!exit)
                e.Cancel = true;
        }


        private void openMenu_Click(object sender, EventArgs e)
        {
            // Try to close the current file. If that succeeds, then ask for a new file and try to open it.
            bool closeSuccess = controller.TryCloseFile();
            if (closeSuccess) {
                string newFilename = GetOpenFileName();
                if (newFilename != null) {
                    bool success = controller.LoadNewFile(newFilename);
                    if (!success) {
                        // This is bad news. The old file is gone, and we don't have a new file. Go back to initial screen is the best solution, 
                        // I guess.
                        Application.Idle -= new EventHandler(Application_Idle); 
                        this.Dispose();
                        new InitialScreen().Show();
                    }
                    else {
                        // Display the default view on the map.
                        ShowRectangle(mapDisplay.MapBounds);
                    }
                }
            }
        }


        private void newEventMenu_Click(object sender, EventArgs e)
        {
            // Try to close the current file. If that succeeds, then ask for a new file and try to open it.
            bool closeSuccess = controller.TryCloseFile();
            if (closeSuccess) {
                NewEventWizard wizard = new NewEventWizard();
                DialogResult result = wizard.ShowDialog();
                if (result == DialogResult.OK) {
                    bool success = controller.NewEvent(wizard.CreateEventInfo);
                    if (!success) {
                        // This is bad news. The old file is gone, and we don't have a new file. Go back to initial screen is the best solution, 
                        // I guess.
                        Application.Idle -= new EventHandler(Application_Idle); 
                        this.Dispose();
                        new InitialScreen().Show();
                    }
                }
            }
        }



        private void saveMenu_Click(object sender, EventArgs e)
        {
            controller.Save();
        }

        private void saveAsMenu_Click(object sender, EventArgs e)
        {
            string newFileName = GetSaveFileName(controller.FileName);
            if (newFileName != null) {
                controller.SaveAs(newFileName);
            }
        }

        // The Esc key is the shortcuy key for the cancel command, but menu items don't allow Esc
        // as a shortcut key directly. So we handle it here.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN) && keyData == Keys.Escape) {
                cancelMenu_Click(this, EventArgs.Empty);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void cancelMenu_Click(object sender, EventArgs e)
        {
            // Clear selection and cancel current mode use the same menu item.
            if (controller.CanCancelMode()) {
                controller.CancelMode();
            }
            else {
                controller.ClearSelection();
            }
        }

        private void undoMenu_Click(object sender, EventArgs e)
        {
            UndoStatus status = controller.GetUndoStatus();

            if (status.CanUndo)
                controller.Undo();
        }

        private void redoMenu_Click(object sender, EventArgs e)
        {
            UndoStatus status = controller.GetUndoStatus();

            if (status.CanRedo)
                controller.Redo();
        }

        private void deleteMenu_Click(object sender, EventArgs e)
        {
            controller.DeleteSelection();
        }

        private void deleteForkMenu_Click(object sender, EventArgs e)
        {
            controller.DeleteFork();
        }


        private void allControlsMenu_Click(object sender, EventArgs e)
        {
            controller.ShowAllControls = !controller.ShowAllControls;
            Settings.Default.ViewAllControls = controller.ShowAllControls;
            Settings.Default.Save();
        }

        private void otherCoursesMenu_Click(object sender, EventArgs e)
        {
            ViewAdditionalCourses dialog = new ViewAdditionalCourses(controller.CurrentTabName, controller.CurrentCourseId);
            dialog.EventDB = controller.GetEventDB();
            dialog.DisplayedCourses = controller.ExtraCourseDisplay;
            if (dialog.ShowDialog() == DialogResult.OK) {
                controller.ExtraCourseDisplay = dialog.DisplayedCourses;
            }
        }

        private void clearOtherCoursesMenu_Click(object sender, EventArgs e)
        {
            controller.ClearExtraCourseDisplay();
        }

        private void addControlMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);
        }

        private void addStartMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.Start, MapExchangeType.None);
        }

        private void addFinishMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.Finish, MapExchangeType.None);
        }

        private void addMapExchangeControl_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.Exchange);
        }

        private void addMapFlipControl_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.MapFlip);
        }

        private void addMapExchangeSeparate_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.MapExchange, MapExchangeType.None);
        }

        private void addVariationMenu_Click(object sender, EventArgs e)
        {
            string reason;
            if (controller.CanAddVariation(out reason) != CommandStatus.Enabled) {
                ErrorMessage(reason);
                return;                
            }

            AddForkDialog addForkDialog = new AddForkDialog();

            DialogResult result = addForkDialog.ShowDialog(this);

            if (result == DialogResult.OK) {
                controller.AddVariation(addForkDialog.Loop, addForkDialog.NumberOfBranches);
            }

            addForkDialog.Dispose();
        }

        private void zoomMenu_Click(object sender, EventArgs e)
        {
            float zoomAmount = (float) ((ToolStripMenuItem) sender).Tag;

            mapViewer.ZoomFactor = zoomAmount;
        }

        private void intensityMenu_Click(object sender, EventArgs e)
        {
            double intensityAmount = (double) ((ToolStripMenuItem) sender).Tag;
            mapDisplay.MapIntensity = (float) intensityAmount;
            Settings.Default.MapIntensity = mapDisplay.MapIntensity;
            Settings.Default.Save();
        }

        private void showPopupsMenu_Click(object sender, EventArgs e)
        {
            showToolTips = !showToolTips;
            Settings.Default.ShowPopupInfo = showToolTips;
            Settings.Default.Save();
        }

        private void showPrintAreaMenu_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowPrintArea = !Settings.Default.ShowPrintArea;
            Settings.Default.Save();
            controller.ForceChangeUpdate();
        }

        private void courseTabs_Selected(object sender, TabControlEventArgs e)
        {
            controller.SelectTab(courseTabs.SelectedIndex);
        }

        private void descriptionControl_Change(DescriptionControl sender, DescriptionControl.ChangeKind kind, int line, int box, object newValue)
        {
            controller.DescriptionChange(kind, line, box, newValue);
        }

        private void descriptionControl_SelectedIndexChange(object sender, EventArgs e)
        {
            // User changed the selected line. Update the selection manager.
            int firstLine, lastLine;
            descriptionControl.GetSelection(out firstLine, out lastLine);
            controller.SelectDescriptionLine(firstLine);
        }

        private void mapViewer_OnPointerMove(object sender, bool inViewport, PointF location)
        {
            if (inViewport) {
                controller.MouseMoved(Pane.Map, location, mapViewer.PixelSize);

                // Update the mouse cursor.
                mapViewer.Cursor = controller.GetMouseCursor(Pane.Map, location, mapViewer.PixelSize);
            }

            PointF pixelLocation = Util.PointFromPointF(mapViewer.WorldToPixel(location));
            if (pixelLocation != lastTooltipLocation)
                toolTip.Hide(mapViewer);

            UpdatePointerLabel(inViewport, location);
            UpdateStatusText();
        }

        private void mapViewerTopology_OnPointerMove(object sender, bool inViewport, PointF location)
        {
            PointF pixelLocation = Util.PointFromPointF(mapViewerTopology.WorldToPixel(location));
            if (pixelLocation != lastTooltipLocation)
                toolTip.Hide(mapViewerTopology);
        }


        private void mapViewer_OnPointerHover(object sender, bool inViewport, PointF location)
        {
            string tipText, titleText;
            if (showToolTips && controller.GetToolTip(Pane.Map, location, mapViewer.PixelSize, out tipText, out titleText)) {
                toolTip.Hide(mapViewer);
                toolTip.ToolTipTitle = titleText;
                lastTooltipLocation = Util.PointFromPointF(mapViewer.WorldToPixel(location));
                toolTip.Show(tipText, mapViewer, lastTooltipLocation.X, lastTooltipLocation.Y + 24, 7000);
            }
        }

        private void mapViewerTopology_OnPointerHover(object sender, bool inViewport, PointF location)
        {
            string tipText, titleText;
            if (showToolTips && controller.GetToolTip(Pane.Topology, location, mapViewer.PixelSize, out tipText, out titleText)) {
                toolTip.Hide(mapViewerTopology);
                toolTip.ToolTipTitle = titleText;
                lastTooltipLocation = Util.PointFromPointF(mapViewerTopology.WorldToPixel(location));
                toolTip.Show(tipText, mapViewerTopology, lastTooltipLocation.X, lastTooltipLocation.Y + 24, 7000);
            }
        }

        private void mapViewer_MouseEnter(object sender, EventArgs e)
        {
            // When the mouse enters the map, give it focus. This makes the scroll wheel work correctly.
            if (Form.ActiveForm == this)
                mapViewer.Focus();
        }

        private void mapViewer_OnViewportChange(object sender, EventArgs e)
        {
            PointF location = mapViewer.PointerLocation;
            if (controller != null) {
                controller.MouseMoved(Pane.Map, location, mapViewer.PixelSize);

                UpdateLabelsAndScrollBars();
            }
        }

        private MapViewer.DragAction mapViewer_OnMouseEvent(object sender, MouseAction action, int buttonNumber, bool[] whichButtonsDown, PointF location, PointF locationStart)
        {
            if (action != MouseAction.Move)
                toolTip.Hide(mapViewer);

            return HandleMouseEvent(Pane.Map, mapViewer, action, buttonNumber, whichButtonsDown, location, locationStart);
        }

        private MapViewer.DragAction mapViewerTopology_OnMouseEvent(object sender, MouseAction action, int buttonNumber, bool[] whichButtonsDown, PointF location, PointF locationStart)
        {
            if (action != MouseAction.Move)
                toolTip.Hide(mapViewerTopology);

            return HandleMouseEvent(Pane.Topology, mapViewerTopology, action, buttonNumber, whichButtonsDown, location, locationStart);
        }

        private MapViewer.DragAction HandleMouseEvent(Pane pane, MapViewer activePaneMapViewer, MouseAction action, int buttonNumber, bool[] whichButtonsDown, PointF location, PointF locationStart)
        {
            if (action == MouseAction.Down && buttonNumber == MapViewer.LeftMouseButton)
                return controller.LeftButtonDown(pane, location, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Down && buttonNumber == MapViewer.RightMouseButton)
                return controller.RightButtonDown(pane, location, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Up && buttonNumber == MapViewer.LeftMouseButton)
                controller.LeftButtonUp(pane, location, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Up && buttonNumber == MapViewer.RightMouseButton)
                controller.RightButtonUp(pane, location, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Click && buttonNumber == MapViewer.LeftMouseButton)
                controller.LeftButtonClick(pane, location, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Click && buttonNumber == MapViewer.RightMouseButton)
                controller.RightButtonClick(pane, location, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Drag && buttonNumber == MapViewer.LeftMouseButton)
                controller.LeftButtonDrag(pane, location, locationStart, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.Drag && buttonNumber == MapViewer.RightMouseButton)
                controller.RightButtonDrag(pane, location, locationStart, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.DragEnd && buttonNumber == MapViewer.LeftMouseButton)
                controller.LeftButtonEndDrag(pane, location, locationStart, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.DragEnd && buttonNumber == MapViewer.RightMouseButton)
                controller.RightButtonEndDrag(pane, location, locationStart, activePaneMapViewer.PixelSize);
            else if (action == MouseAction.DragCancel && buttonNumber == MapViewer.LeftMouseButton)
                controller.LeftButtonCancelDrag(pane);
            else if (action == MouseAction.DragCancel && buttonNumber == MapViewer.RightMouseButton)
                controller.RightButtonCancelDrag(pane);

            return MapViewer.DragAction.None;
        }

        private void mapViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (! e.Alt && !e.Control && !e.Shift) {
                switch (e.KeyCode) {
                    case Keys.Left:
                        mapViewer.ScrollView(mapViewer.Width / 6, 0);
                        break;
                    case Keys.Right:
                        mapViewer.ScrollView(-mapViewer.Width / 6, 0);
                        break;
                    case Keys.Up:
                        mapViewer.ScrollView(0, mapViewer.Height / 6);
                        break;
                    case Keys.Down:
                        mapViewer.ScrollView(0, -mapViewer.Height / 6);
                        break;
                }
            }
        }

        private void zoomTracker_Scroll(object sender, EventArgs e)
        {
            mapViewer.ZoomFactor = (float) Math.Pow(10.0, (((double) zoomTracker.Value / 100) * (Math.Log10(TRACKBAR_MAX) - Math.Log10(TRACKBAR_MIN))) + Math.Log10(TRACKBAR_MIN));
        }


        private void vertScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.SmallIncrement) {
                mapViewer.ScrollView(0, -mapViewer.Height / 6);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) {
                mapViewer.ScrollView(0, mapViewer.Height / 6);
            }
            else if (e.Type == ScrollEventType.LargeIncrement) {
                mapViewer.ScrollView(0, -mapViewer.Height * 5 / 6);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) {
                mapViewer.ScrollView(0, mapViewer.Height * 5 / 6);
            }
            else if (e.Type == ScrollEventType.ThumbPosition) {
                PointF center = mapViewer.CenterPoint;
                center.Y = -e.NewValue;
                mapViewer.CenterPoint = center;
            }
        }

        private void horizScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.SmallIncrement) {
                mapViewer.ScrollView(-mapViewer.Width / 6, 0);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) {
                mapViewer.ScrollView(mapViewer.Width / 6, 0);
            }
            else if (e.Type == ScrollEventType.LargeIncrement) {
                mapViewer.ScrollView(-mapViewer.Width * 5 / 6, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) {
                mapViewer.ScrollView(mapViewer.Width * 5 / 6, 0);
            }
            else if (e.Type == ScrollEventType.ThumbPosition) {
                PointF center = mapViewer.CenterPoint;
                center.X = e.NewValue;
                mapViewer.CenterPoint = center;
            }
        }

        private void deleteCourseMenu_Click(object sender, EventArgs e)
        {
            controller.DeleteCurrentCourse();
        }

        private void addCourseMenu_Click(object sender, EventArgs e)
        {
            // Initialize the dialog, use all controls print scale as the default print scale.
            DescriptionKind allControlsDescKind;
            float allControlsPrintScale;
            controller.GetAllControlsProperties(out allControlsPrintScale, out allControlsDescKind);

            AddCourse addCourseDialog = new AddCourse();
            addCourseDialog.HelpTopic = "CourseAddCourse.htm";
            addCourseDialog.InitializePrintScales(controller.MapScale);
            addCourseDialog.PrintScale = allControlsPrintScale;

            // Display the dialog
            DialogResult result = addCourseDialog.ShowDialog();

            // If the dialog completed successfully, then add the course.
            if (result == DialogResult.OK) {
                controller.NewCourse(addCourseDialog.CourseKind, addCourseDialog.CourseName, addCourseDialog.ControlLabelKind, addCourseDialog.ScoreColumn, addCourseDialog.SecondaryTitle,
                    addCourseDialog.PrintScale, addCourseDialog.Climb, addCourseDialog.Length, addCourseDialog.DescKind, addCourseDialog.FirstControlOrdinal, addCourseDialog.HideFromReports);
            }
        }

        private void InitializeCoursePropertiesDialogWithCurrentValues(AddCourse addCourseDialog)
        {
            // Get the properties of the current course.
            CourseKind courseKind;
            string courseName, secondaryTitle;
            float printScale, climb;
            float? length;
            DescriptionKind descKind;
            int firstControlOrdinal;
            ControlLabelKind labelKind;
            int scoreColumn;
            bool hideFromReports;
            controller.GetCurrentCourseProperties(out courseKind, out courseName, out labelKind, out scoreColumn, out secondaryTitle, out printScale, out climb, out length, out descKind, out firstControlOrdinal, out hideFromReports);

            // Initialize the dialog
            addCourseDialog.InitializePrintScales(controller.MapScale);
            addCourseDialog.CourseKind = courseKind;
            addCourseDialog.CourseName = courseName;
            addCourseDialog.SecondaryTitle = secondaryTitle;
            addCourseDialog.PrintScale = printScale;
            addCourseDialog.Climb = climb;
            addCourseDialog.Length = length;
            addCourseDialog.DescKind = descKind;
            addCourseDialog.FirstControlOrdinal = firstControlOrdinal;
            addCourseDialog.ControlLabelKind = labelKind;
            addCourseDialog.ScoreColumn = scoreColumn;
            addCourseDialog.HideFromReports = hideFromReports;

        }
        private void duplicateCourseMenu_Click(object sender, EventArgs e)
        {
            if (controller.CanDuplicateCurrentCourse()) {
                // Initialize the dialog
                AddCourse addCourseDialog = new AddCourse();
                List<int> descriptionsCuts;
                InitializeCoursePropertiesDialogWithCurrentValues(addCourseDialog);
                addCourseDialog.SetTitle(MiscText.DuplicateCourseTitle);
                addCourseDialog.HelpTopic = "CourseDuplicate.htm";
                addCourseDialog.CourseName = "";
                addCourseDialog.CanChangeCourseKind = false;

                // Display the dialog
                DialogResult result = addCourseDialog.ShowDialog();

                // If the dialog completed successfully, then add the course.
                if (result == DialogResult.OK) {
                    controller.DuplicateCurrentCourse(addCourseDialog.CourseName, addCourseDialog.ControlLabelKind, addCourseDialog.ScoreColumn, addCourseDialog.SecondaryTitle,
                                                      addCourseDialog.PrintScale, addCourseDialog.Climb, addCourseDialog.Length, addCourseDialog.DescKind, addCourseDialog.FirstControlOrdinal, addCourseDialog.HideFromReports);
                }

            }
        }



        private void propertiesMenu_Click(object sender, EventArgs e)
        {
            if (controller.CanChangeCourseProperties()) {
                // Initialize the dialog
                AddCourse addCourseDialog = new AddCourse();
                InitializeCoursePropertiesDialogWithCurrentValues(addCourseDialog);
                addCourseDialog.SetTitle(MiscText.CoursePropertiesTitle);
                addCourseDialog.HelpTopic = "CourseProperties.htm";

                // Display the dialog
                DialogResult result = addCourseDialog.ShowDialog();

                // If the dialog completed successfully, then change the course.
                if (result == DialogResult.OK) {
                    controller.ChangeCurrentCourseProperties(addCourseDialog.CourseKind, addCourseDialog.CourseName, addCourseDialog.ControlLabelKind, addCourseDialog.ScoreColumn, addCourseDialog.SecondaryTitle,
                        addCourseDialog.PrintScale, addCourseDialog.Climb, addCourseDialog.Length, addCourseDialog.DescKind, addCourseDialog.FirstControlOrdinal, addCourseDialog.HideFromReports);
                }
            }
            else {
                // Change properties of all controls.
                float printScale;
                DescriptionKind descKind;
                controller.GetAllControlsProperties(out printScale, out descKind);

                // Initialize the dialog
                AllControlsProperties allControlsDialog = new AllControlsProperties();
                allControlsDialog.InitializePrintScales(controller.MapScale);
                allControlsDialog.PrintScale = printScale;
                allControlsDialog.DescKind = descKind;

                // Display the dialog
                DialogResult result = allControlsDialog.ShowDialog();

                // If the dialog completed successfully, then change the course.
                if (result == DialogResult.OK) {
                    controller.ChangeAllControlsProperties(allControlsDialog.PrintScale, allControlsDialog.DescKind);
                }
            }
        }

        private void courseLoadMenu_Click(object sender, EventArgs e)
        {
            // Initialize the dialog with the current load values.
            CourseLoad courseLoadDialog = new CourseLoad();
            courseLoadDialog.SetCourseLoads(controller.GetAllCourseLoads());

            // Show the dialog.
            DialogResult result = courseLoadDialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                controller.SetAllCourseLoads(courseLoadDialog.GetCourseLoads());
            }

            courseLoadDialog.Dispose();
        }

        private void courseOrderMenu_Click(object sender, EventArgs e)
        {
            // Initialize dialog.
            ChangeCourseOrder courseOrderDialog = new ChangeCourseOrder(controller.GetAllCourseOrders());

            // Show the dialog.
            DialogResult result = courseOrderDialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                controller.SetAllCourseOrders(courseOrderDialog.GetCourseOrders());
            }

            courseOrderDialog.Dispose();
        }

        private void addTextLineMenu_Click(object sender, EventArgs e)
        {
            string defaultText;
            DescriptionLine.TextLineKind defaultLineKind;
            bool enableThisCourse;
            string objectName;

            if (controller.CanAddTextLine(out defaultText, out defaultLineKind, out objectName, out enableThisCourse)) {
                // Initialize dialog.
                AddTextLine dialog = new AddTextLine(objectName, enableThisCourse);
                dialog.TextLine = defaultText;
                dialog.TextLineKind = defaultLineKind;

                // Show the dialog.
                DialogResult result = dialog.ShowDialog(this);

                // Apply changes.
                if (result == DialogResult.OK) {
                    controller.AddTextLine(dialog.TextLine, dialog.TextLineKind);
                }

                dialog.Dispose();
            }
        }

        private void addDescriptionsMenu_Click(object sender, EventArgs e)
        {
            if (controller.CanAddDescriptions())
                controller.BeginAddDescriptionMode();
            else
                InfoMessage(MiscText.CannotAddDescriptionsToAllParts);
        }

        private void addMapIssueMenu_Click(object sender, EventArgs e)
        {
            MapIssueChoiceDialog dialog = new MapIssueChoiceDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                controller.BeginAddMapIssuePointMode(dialog.MapIssueKind);
            }
            dialog.Dispose();
        }

        private void addMandatoryCrossingMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddControlMode(ControlPointKind.CrossingPoint, MapExchangeType.None);
        }

        private void addOutOfBoundsMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.OOB, true);
        }

        private void addDangerousMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Dangerous, true);
        }

        private void addConstructionMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Construction, true);
        }


        private void addBoundaryMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Boundary, false);
        }

        private void addOptCrossingMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddPointSpecialMode(SpecialKind.OptCrossing);
        }

        private void addWaterMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddPointSpecialMode(SpecialKind.Water);
        }

        private void addFirstAidMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddPointSpecialMode(SpecialKind.FirstAid);
        }

        private void addForbiddenMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddPointSpecialMode(SpecialKind.Forbidden);
        }

        private void addRegMarkMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddPointSpecialMode(SpecialKind.RegMark);
        }

        private void addTextMenu_Click(object sender, EventArgs e)
        {
            short colorOcadId;
            float c, m, y, k;
            bool purpleOverprint;
            string fontName;
            bool fontBold, fontItalic;
            float fontHeight;
            bool fontAutoSize;
            SpecialColor fontColor;

            FindPurple.GetPurpleColor(mapDisplay, controller.GetCourseAppearance(), out colorOcadId, out c, out m, out y, out k, out purpleOverprint);

            ChangeText dialog = new ChangeText(MiscText.AddTextSpecialTitle, MiscText.AddTextSpecialExplanation, true, 
                                               CmykColor.FromCmyk(c, m, y, k), controller.ExpandText);
            dialog.HelpTopic = "EditAddText.htm";

            controller.GetAddTextDefaultProperties(out fontName, out fontBold, out fontItalic, out fontColor, out fontHeight, out fontAutoSize);
            dialog.FontName = fontName;
            dialog.FontBold = fontBold;
            dialog.FontItalic = fontItalic;
            dialog.FontColor = fontColor;
            dialog.FontSize = fontHeight;
            dialog.FontSizeAutomatic = fontAutoSize;

            if (dialog.ShowDialog(this) == DialogResult.OK) {
                controller.BeginAddTextSpecialMode(dialog.UserText, dialog.FontName, dialog.FontBold, dialog.FontItalic, dialog.FontColor, dialog.FontSizeAutomatic ? -1 : dialog.FontSize);
            }

            dialog.Dispose();
        }

        private void addImageMenu_Click(object sender, EventArgs e)
        {
            openImageDialog.FileName = null;
            DialogResult result = openImageDialog.ShowDialog();

            if (result == DialogResult.OK) {
                string fileName = openImageDialog.FileName;
                controller.BeginAddImageSpecialMode(fileName);
            }
        }

        private void addLineMenu_Click(object sender, EventArgs e)
        {
            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out c, out m, out y, out k, out purpleOverprint);

            LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.AddLineTitle, MiscText.AddLineExplanation, "EditAddLine.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

            // Get the defaults for a new line.
            SpecialColor color;
            LineKind lineKind;
            float lineWidth, gapSize, dashSize, cornerRadius;
            controller.GetLineSpecialProperties(SpecialKind.Line, false, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
            linePropertiesDialog.ShowRadius = false;
            linePropertiesDialog.ShowLineKind = true;
            linePropertiesDialog.Color = color;
            linePropertiesDialog.LineKind = lineKind;
            linePropertiesDialog.LineWidth = lineWidth;
            linePropertiesDialog.GapSize = gapSize;
            linePropertiesDialog.DashSize = dashSize;

            DialogResult result = linePropertiesDialog.ShowDialog();

            if (result == DialogResult.OK) {
                controller.BeginAddLineSpecialMode(linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize);
            }

            linePropertiesDialog.Dispose();
        }

        private void addRectangleMenu_Click(object sender, EventArgs e)
        {
            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out c, out m, out y, out k, out purpleOverprint);

            LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.AddRectangleTitle, MiscText.AddRectangleExplanation, "EditAddRectangle.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

            // Get the defaults for a new line.
            SpecialColor color;
            LineKind lineKind;
            float lineWidth, gapSize, dashSize, cornerRadius;
            controller.GetLineSpecialProperties(SpecialKind.Rectangle, false, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
            linePropertiesDialog.ShowRadius = true;
            linePropertiesDialog.ShowLineKind = false;
            linePropertiesDialog.Color = color;
            linePropertiesDialog.LineKind = LineKind.Single;
            linePropertiesDialog.LineWidth = lineWidth;
            linePropertiesDialog.GapSize = gapSize;
            linePropertiesDialog.DashSize = dashSize;
            linePropertiesDialog.CornerRadius = cornerRadius;

            DialogResult result = linePropertiesDialog.ShowDialog();

            if (result == DialogResult.OK) {
                controller.BeginAddRectangleSpecialMode(false, linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize, linePropertiesDialog.CornerRadius);
            }

            linePropertiesDialog.Dispose();
        }

        private void addEllipseMenu_Click(object sender, EventArgs e)
        {
            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out c, out m, out y, out k, out purpleOverprint);

            LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.AddEllipseTitle, MiscText.AddEllipseExplanation, "EditAddEllipse.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

            // Get the defaults for a new line.
            SpecialColor color;
            LineKind lineKind;
            float lineWidth, gapSize, dashSize, cornerRadius;
            controller.GetLineSpecialProperties(SpecialKind.Ellipse, false, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
            linePropertiesDialog.ShowRadius = false;
            linePropertiesDialog.ShowLineKind = true;
            linePropertiesDialog.Color = color;
            linePropertiesDialog.LineKind = LineKind.Single;
            linePropertiesDialog.LineWidth = lineWidth;
            linePropertiesDialog.GapSize = gapSize;
            linePropertiesDialog.DashSize = dashSize;
            linePropertiesDialog.CornerRadius = cornerRadius;

            DialogResult result = linePropertiesDialog.ShowDialog();

            if (result == DialogResult.OK) {
                controller.BeginAddRectangleSpecialMode(true, linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize, 0);
            }

            linePropertiesDialog.Dispose();
        }

        private void changeTextMenu_Click(object sender, EventArgs e)
        {
            if (controller.CanChangeText() == CommandStatus.Enabled) {
                short colorOcadId;
                float c, m, y, k;
                bool purpleOverprint;
                string fontName;
                bool fontBold, fontItalic;
                float fontHeight;
                SpecialColor fontColor;
                FindPurple.GetPurpleColor(mapDisplay, controller.GetCourseAppearance(), out colorOcadId, out c, out m, out y, out k, out purpleOverprint);

                string oldText = controller.GetChangableText();
                controller.GetChangableTextProperties(out fontName, out fontBold, out fontItalic, out fontColor, out fontHeight);
                ChangeText dialog = new ChangeText(MiscText.ChangeTextTitle, MiscText.ChangeTextSpecialExplanation, true,
                                                   CmykColor.FromCmyk(c, m, y, k), controller.ExpandText);
                dialog.HelpTopic = "ItemChangeText.htm";
                dialog.UserText = oldText;
                dialog.FontName = fontName;
                dialog.FontBold = fontBold;
                dialog.FontItalic = fontItalic;
                dialog.FontColor = fontColor;
                dialog.FontSize = (fontHeight < 0) ? 5 : fontHeight;
                dialog.FontSizeAutomatic = (fontHeight < 0);

                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    controller.ChangeText(dialog.UserText, dialog.FontName, dialog.FontBold, dialog.FontItalic, dialog.FontColor, dialog.FontSizeAutomatic ? -1 : dialog.FontSize);
                }

                dialog.Dispose();
            }
        }


        private void changeLineAppearanceMenu_Click(object sender, EventArgs e)
        {
            if (controller.CanChangeLineAppearance() == CommandStatus.Enabled) {
                CourseAppearance appearance = controller.GetCourseAppearance();

                short colorOcadId;
                float c, m, y, k;
                bool purpleOverprint;
                FindPurple.GetPurpleColor(mapDisplay, appearance, out colorOcadId, out c, out m, out y, out k, out purpleOverprint);

                LinePropertiesDialog linePropertiesDialog = new LinePropertiesDialog(MiscText.ChangeLineAppearanceTitle, MiscText.ChangeLineAppearanceExplanation, "ItemChangeLineAppearance.htm", CmykColor.FromCmyk(c, m, y, k), appearance);

                // Get the defaults for a new line.
                SpecialColor color;
                LineKind lineKind;
                bool showRadius;
                float lineWidth, gapSize, dashSize, cornerRadius;
                controller.GetChangableLineProperties(out showRadius, out color, out lineKind, out lineWidth, out gapSize, out dashSize, out cornerRadius);
                linePropertiesDialog.ShowRadius = showRadius;
                linePropertiesDialog.ShowLineKind = !showRadius;
                linePropertiesDialog.Color = color;
                linePropertiesDialog.LineKind = lineKind;
                linePropertiesDialog.LineWidth = lineWidth;
                linePropertiesDialog.GapSize = gapSize;
                linePropertiesDialog.DashSize = dashSize;
                linePropertiesDialog.CornerRadius = cornerRadius;

                DialogResult result = linePropertiesDialog.ShowDialog();

                if (result == DialogResult.OK) {
                    controller.ChangeLineAppearance(linePropertiesDialog.Color, linePropertiesDialog.LineKind, linePropertiesDialog.LineWidth, linePropertiesDialog.GapSize, linePropertiesDialog.DashSize, linePropertiesDialog.CornerRadius);
                }

                linePropertiesDialog.Dispose();
            }
        }


        private void whiteOutMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.WhiteOut, true);
        }

        private void addBendMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddBend();
        }

        private void removeBendMenu_Click(object sender, EventArgs e)
        {
            controller.BeginRemoveBend();
        }

        private void addGapMenu_Click(object sender, EventArgs e)
        {
            controller.BeginAddGap();
        }

        private void removeGapMenu_Click(object sender, EventArgs e)
        {
            controller.BeginRemoveGap();
        }

        private void rotateMenu_Click(object sender, EventArgs e)
        {
            controller.BeginRotate();
        }

        private void stretchMenu_Click(object sender, EventArgs e)
        {
            controller.BeginStretch();
        }

        private void noFlaggingMenu_Click(object sender, EventArgs e)
        {
            controller.SetLegFlagging(FlaggingKind.None);
        }

        private void entireFlaggingMenu_Click(object sender, EventArgs e)
        {
            controller.SetLegFlagging(FlaggingKind.All);
        }

        private void beginFlaggingMenu_Click(object sender, EventArgs e)
        {
            controller.SetLegFlagging(FlaggingKind.Begin);
        }

        private void endFlaggingMenu_Click(object sender, EventArgs e)
        {
            controller.SetLegFlagging(FlaggingKind.End);
        }

        private void changeDisplayedCoursesMenu_Click(object sender, EventArgs e)
        {
            CourseDesignator[] displayedCourses;
            bool showAllControls;

            if (controller.CanChangeDisplayedCourses(out displayedCourses, out showAllControls) == CommandStatus.Enabled) {
                ChangeSpecialCourses changeCoursesDialog = new ChangeSpecialCourses();
                changeCoursesDialog.EventDB = controller.GetEventDB();
                changeCoursesDialog.ShowAllControls = showAllControls; changeCoursesDialog.DisplayedCourses = displayedCourses;


                DialogResult result = changeCoursesDialog.ShowDialog(this);
                if (result == DialogResult.OK) {
                    controller.ChangeDisplayedCourses(changeCoursesDialog.DisplayedCourses);
                }
            }
        }

        // Show help of the given kind.
        private void ShowHelp(HelpNavigator navigator, object parameter)
        {
            if (helpFileUrl == null) {
                string helpFileName = Util.GetFileInAppDirectory(HELP_FILE_NAME);
                if (File.Exists(helpFileName))
                    helpFileUrl = new Uri(helpFileName);
                else {
                    ErrorMessage(string.Format(MiscText.HelpFileNotFound, helpFileName));
                    return;
                }
            }

            if (helpFileUrl != null)
                Help.ShowHelp(this, helpFileUrl.ToString(), navigator, parameter);
        }

        private void helpContentsMenu_Click(object sender, EventArgs e)
        {
            ShowHelp(HelpNavigator.TableOfContents, null);
        }

        private bool TranslatedWebSiteExists()
        {
            string url = MiscText.TranslatedHelpWebSite;
            return (url.Length > 0 && url[0] == 'h');
        }

        private void helpTranslatedMenu_Click(object sender, EventArgs e)
        {
            Util.GoToWebPage(MiscText.TranslatedHelpWebSite);
        }

        private void helpIndexMenu_Click(object sender, EventArgs e)
        {
            ShowHelp(HelpNavigator.Index, null);
        }

        private void aboutMenu_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void helpMenu_DropDownOpening(object sender, EventArgs e)
        {
            // The debug and translate menu show up only if Ctrl + Shift also pressed.
            Debug.WriteLine(Control.ModifierKeys);
            debugMenu.Visible = translateMenu.Visible = ((Control.ModifierKeys & (Keys.Control | Keys.Shift)) == (Keys.Control | Keys.Shift)) || ((Control.ModifierKeys & (Keys.Control | Keys.Alt)) == (Keys.Control | Keys.Alt));
        }

        // Change the viewport to show the given rectangle.
        private void ShowRectangle(RectangleF bounds)
        {
            if (bounds.IsEmpty) {
                // Empty -- just move the center point
                mapViewer.CenterPoint = new PointF((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
            }
            else {
                // real rectangle -- make it the new viewport.
                mapViewer.Viewport = bounds;
            }
        }

        private void entireCourseMenu_Click(object sender, EventArgs e)
        {
            // Show the entire course.
            RectangleF courseBounds = controller.GetCourseBounds();
            ShowRectangle(courseBounds);
        }

        private void entireMapMenu_Click(object sender, EventArgs e)
        {
            // Show the entire map.
            RectangleF mapBounds = mapDisplay.MapBounds;
            ShowRectangle(mapBounds);
        }

        private void highQualityMenu_Click(object sender, EventArgs e)
        {
            SetQuality(true);
        }

        private void normalQualityMenu_Click(object sender, EventArgs e)
        {
            SetQuality(false);
        }

        private void SetQuality(bool highQuality)
        {
            mapDisplay.AntiAlias = highQuality;
            Settings.Default.MapHighQuality = highQuality;
            Settings.Default.Save();
        }

        private void changeCodesMenu_Click(object sender, EventArgs e)
        {
            // Initialize the dialog with the current codes.
            ChangeAllCodes changeCodesDialog = new ChangeAllCodes();
            changeCodesDialog.SetEventDB(controller.GetEventDB());
            changeCodesDialog.Codes = controller.GetAllControlCodes();

            // Show the dialog to allow people to change the codes.
            DialogResult result = changeCodesDialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                controller.SetAllControlCodes(changeCodesDialog.Codes);
            }

            changeCodesDialog.Dispose();
        }

        private void autoNumberingMenu_Click(object sender, EventArgs e)
        {
            // Get initial values.
            int firstCode;
            bool disallowInvertibleCodes;

            controller.GetAutoNumbering(out firstCode, out disallowInvertibleCodes);

            // Initialize dialog.
            AutoNumbering autoNumberingDialog = new AutoNumbering();
            autoNumberingDialog.FirstCode = firstCode;
            autoNumberingDialog.DisallowInvertibleCodes = disallowInvertibleCodes;
            autoNumberingDialog.RenumberExisting = false;

            // Show the dialog.
            DialogResult result = autoNumberingDialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                controller.AutoNumbering(autoNumberingDialog.FirstCode, autoNumberingDialog.DisallowInvertibleCodes, autoNumberingDialog.RenumberExisting);
            }

            autoNumberingDialog.Dispose();
        }

        private void punchPatternsMenu_Click(object sender, EventArgs e)
        {
            // Get all the punch patterns and the punch card layout.
            Dictionary<string, PunchPattern> allPatterns = controller.GetAllPunchPatterns();
            PunchcardFormat punchcardFormat = controller.GetPunchcardFormat();

            // Initialize the dialog.
            PunchPatternDialog dialog = new PunchPatternDialog();
            dialog.AllPunchPatterns = allPatterns;
            dialog.PunchcardFormat = punchcardFormat;

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply the changes.
            if (result == DialogResult.OK) {
                if (!dialog.PunchcardFormat.Equals(punchcardFormat))
                    controller.SetPunchcardFormat(dialog.PunchcardFormat);
                controller.SetAllPunchPatterns(dialog.AllPunchPatterns);
            }

            dialog.Dispose();
        }

        private void customizeDescriptionsMenu_Click(object sender, EventArgs e)
        {
            Dictionary<string, List<SymbolText>> customSymbolText;
            Dictionary<string, bool> customSymbolKey;

            // Initialize the dialog
            CustomSymbolText dialog = new CustomSymbolText(symbolDB, false);
            controller.GetCustomSymbolText(out customSymbolText, out customSymbolKey);
            dialog.SetCustomSymbolDictionaries(customSymbolText, customSymbolKey);
            dialog.LangId = controller.GetDescriptionLanguage();

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply the changes
            if (result == DialogResult.OK) {
                // dialog changes the dictionaries, so we don't need to retrieve tham.
                controller.SetCustomSymbolText(customSymbolText, customSymbolKey, dialog.LangId);
                if (dialog.UseAsDefaultLanguage)
                    controller.DefaultDescriptionLanguage = dialog.LangId;
            }

            dialog.Dispose();
        }

        private void customizeCourseAppearanceMenu_Click(object sender, EventArgs e)
        {
            // Initialize the dialog
            CourseAppearanceDialog dialog = new CourseAppearanceDialog();

            // Get the correct default purple color to use.
            float c, m, y, k;
            bool purpleOverprint;
            short ocadId;
            FindPurple.GetPurpleColor(mapDisplay, null, out ocadId, out c, out m, out y, out k, out purpleOverprint);
            dialog.SetDefaultPurple(c, m, y, k);
            dialog.UsesOcadMap = (mapDisplay.MapType == MapType.OCAD);
            dialog.SetMapLayers(controller.GetUnderlyingMapColors());

            // Set the course appearance into the dialog
            CourseAppearance appearance = controller.GetCourseAppearance();
            if (dialog.UsesOcadMap && appearance.purpleColorBlend != PurpleColorBlend.UpperLowerPurple) {
                // Set the default lower purple layer anyway, so that it is chosen by default when the user changes the blend.
                appearance.mapLayerForLowerPurple = controller.GetDefaultLowerPurpleLayer();
            }
            dialog.CourseAppearance = appearance;

            // Show the dialog.
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                controller.SetCourseAppearance(dialog.CourseAppearance);
            }

            dialog.Dispose();
        }

        private void removeUnusedControlsMenu_Click(object sender, EventArgs e)
        {
            List<KeyValuePair<Id<ControlPoint>,string>> unusedControls = controller.GetUnusedControls();

            if (unusedControls.Count == 0) {
                // No controls to delete. Tell the user.
                InfoMessage(MiscText.NoUnusedControls);
            }
            else {
                // Put up the dialog and do it.
                UnusedControls dialog = new UnusedControls();
                dialog.SetControlsToDelete(controller.GetUnusedControls());

                if (dialog.ShowDialog() == DialogResult.OK) {
                    controller.RemoveControls(dialog.GetControlsToDelete());
                }

                dialog.Dispose();
            }
        }

        private void moveAllControlsMenu_Click(object sender, EventArgs e)
        {
            // Part 1: Determine which action we are doing.
            MoveAllControls moveAllControlsDialog = new MoveAllControls();
            if (moveAllControlsDialog.ShowDialog() == DialogResult.Cancel) {
                moveAllControlsDialog.Dispose();
                return;
            }

            MoveAllControlsAction action = moveAllControlsDialog.Action;
            moveAllControlsDialog.Dispose();

            // Part 2: Prompt use to move controls
            controller.BeginMoveAllControls();

            SelectLocationsForMove selectLocationsForMoveDialog = new SelectLocationsForMove(controller, action);
            Point location = this.Location;
            location.Offset(10, 130);
            selectLocationsForMoveDialog.Location = location;
            selectLocationsForMoveDialog.Show(this);

            // Dialog dismisses/disposes itself and invokes controller.
        }

        private void printDescriptionsMenu_Click(object sender, EventArgs e)
        {
            // Initialize dialog
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            PrintDescriptions printDescDialog = new PrintDescriptions(controller.GetEventDB(), false);
            printDescDialog.controller = controller;
            printDescDialog.PrintSettings = descPrintSettings;

            // show the dialog, on success, print.
            if (printDescDialog.ShowDialog(this) == DialogResult.OK) {
                // Save the settings for the next invocation of the dialog.
                descPrintSettings = printDescDialog.PrintSettings;
                controller.PrintDescriptions(descPrintSettings, false);
            }

            // And the dialog is done.
            printDescDialog.Dispose();
        }

        private void createDescriptionPdfMenu_Click(object sender, EventArgs e)
        {
            // Initialize dialog
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            PrintDescriptions printDescDialog = new PrintDescriptions(controller.GetEventDB(), true);
            printDescDialog.controller = controller;
            printDescDialog.PrintSettings = descPrintSettings;

            // show the dialog, on success, print.
            if (printDescDialog.ShowDialog(this) == DialogResult.OK) {
                // Figure out filename
                SaveFileDialog savePdfDialog = new SaveFileDialog();
                savePdfDialog.Filter = MiscText.PdfFilter;
                savePdfDialog.FilterIndex = 1;
                savePdfDialog.DefaultExt = "pdf";
                savePdfDialog.OverwritePrompt = true;
                savePdfDialog.InitialDirectory = Path.GetDirectoryName(controller.FileName);

                if (savePdfDialog.ShowDialog(this) == DialogResult.OK) {
                    // Save the settings for the next invocation of the dialog.
                    descPrintSettings = printDescDialog.PrintSettings;
                    controller.CreateDescriptionsPdf(descPrintSettings, savePdfDialog.FileName);
                }
            }

            // And the dialog is done.
            printDescDialog.Dispose();
        }


        private void printPunchCardsMenu_Click(object sender, EventArgs e)
        {
            // Initialize dialog
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            PrintPunches printPunchesDialog = new PrintPunches(controller.GetEventDB(), false);
            printPunchesDialog.controller = controller;
            printPunchesDialog.PrintSettings = punchPrintSettings;
            printPunchesDialog.PrintSettings.Count = 1;

            // show the dialog, on success, print.
            if (printPunchesDialog.ShowDialog(this) == DialogResult.OK) {
                // Save the settings for the next invocation of the dialog.
                punchPrintSettings = printPunchesDialog.PrintSettings;
                controller.PrintPunches(punchPrintSettings, false);
            }

            // And the dialog is done.
            printPunchesDialog.Dispose();
        }

        private void createPunchcardPdfMenu_Click(object sender, EventArgs e)
        {
            // Initialize dialog
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            PrintPunches printPunchesDialog = new PrintPunches(controller.GetEventDB(), true);
            printPunchesDialog.controller = controller;
            printPunchesDialog.PrintSettings = punchPrintSettings;
            printPunchesDialog.PrintSettings.Count = 1;

            // show the dialog, on success, print.
            if (printPunchesDialog.ShowDialog(this) == DialogResult.OK) {
                // Figure out filename
                SaveFileDialog savePdfDialog = new SaveFileDialog();
                savePdfDialog.Filter = MiscText.PdfFilter;
                savePdfDialog.FilterIndex = 1;
                savePdfDialog.DefaultExt = "pdf";
                savePdfDialog.OverwritePrompt = true;
                savePdfDialog.InitialDirectory = Path.GetDirectoryName(controller.FileName);

                if (savePdfDialog.ShowDialog(this) == DialogResult.OK) {
                    // Save the settings for the next invocation of the dialog.
                    punchPrintSettings = printPunchesDialog.PrintSettings;
                    controller.CreatePunchesPdf(punchPrintSettings, savePdfDialog.FileName);
                }
            }

            // And the dialog is done.
            printPunchesDialog.Dispose();
        }

        private void printCoursesMenu_Click(object sender, EventArgs e)
        {
            if (!CheckForNonRenderableObjects(false, true))
                return;

            // Initialize dialog
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            PrintCourses printCoursesDialog = new PrintCourses(controller.GetEventDB(), controller.AnyMultipart());
            printCoursesDialog.controller = controller;
            printCoursesDialog.PrintSettings = coursePrintSettings;

#if XPS_PRINTING
            if (controller.MustRasterizePrinting) {
                // Force rasterization.
                coursePrintSettings.UseXpsPrinting = false;
                printCoursesDialog.PrintSettings = coursePrintSettings;
                printCoursesDialog.EnableRasterizeChoice = false;
            }
#endif // XPS_PRINTING

            printCoursesDialog.PrintSettings.Count = 1;

            // show the dialog, on success, print.
            if (printCoursesDialog.ShowDialog(this) == DialogResult.OK) {
                // Save the settings for the next invocation of the dialog.
                coursePrintSettings = printCoursesDialog.PrintSettings;
                controller.PrintCourses(coursePrintSettings, false);
            }

            // And the dialog is done.
            printCoursesDialog.Dispose();
        }

        private void createCoursePdfMenu_Click(object sender, EventArgs e)
        {
            if (! CheckForNonRenderableObjects(false, true))
                return;

            bool isPdfMap = controller.MapType == MapType.PDF;

            CoursePdfSettings settings;
            if (coursePdfSettings != null)
                settings = coursePdfSettings.Clone();
            else {
                // Default settings: creating in file directory
                settings = new CoursePdfSettings();

                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = Path.GetDirectoryName(controller.FileName);
            }

            if (isPdfMap) {
                // If the map file is a PDF, then created PDF must use that paper size, zero margins, and crop courses to that size.
                settings.CropLargePrintArea = true;
                RectangleF bounds = controller.MapDisplay.MapBounds;
            }

            // Initialize dialog
            CreatePdfCourses createPdfDialog = new CreatePdfCourses(controller.GetEventDB(), controller.AnyMultipart());
            createPdfDialog.controller = controller;
            createPdfDialog.PdfSettings = settings;
            if (isPdfMap) {
                createPdfDialog.EnableChangeCropping = false;
            }

            // show the dialog, on success, print.
            while (createPdfDialog.ShowDialog(this) == DialogResult.OK) {
                List<string> overwritingFiles = controller.OverwritingPdfFiles(createPdfDialog.PdfSettings);
                if (overwritingFiles.Count > 0) {
                    OverwritingOcadFilesDialog overwriteDialog = new OverwritingOcadFilesDialog();
                    overwriteDialog.Filenames = overwritingFiles;
                    if (overwriteDialog.ShowDialog(this) == DialogResult.Cancel)
                        continue;
                }

                // Save the settings for the next invocation of the dialog.
                coursePdfSettings = createPdfDialog.PdfSettings;
                controller.CreateCoursePdfs(coursePdfSettings);

                break;
            }

            // And the dialog is done.
            createPdfDialog.Dispose();
        }

        private void createGpxMenu_Click(object sender, EventArgs e)
        {
            // First check and give immediate message if we can't do coordinate mapping.
            string message;
            if (!controller.CanExportGpxOrKml(out message)) {
                ErrorMessage(message);
                return;
            }

            GpxCreationSettings settings;
            if (gpxCreationSettingsPrevious != null)
                settings = gpxCreationSettingsPrevious.Clone();
            else {
                // Default settings
                settings = new GpxCreationSettings();
            }

            // Initialize the dialog.
            CreateGpx createGpxDialog = new CreateGpx(controller.GetEventDB());
            createGpxDialog.CreationSettings = settings;

            // show the dialog; on success, create the files.
            if (createGpxDialog.ShowDialog(this) == DialogResult.OK) {
                // Show common save dialog to choose output file name.
               // The default output for the XML is the same as the event file name, with xml extension.
                string gpxFileName = Path.ChangeExtension(controller.FileName, ".gpx");

                saveGpxFileDialog.FileName = gpxFileName;
                DialogResult result = saveGpxFileDialog.ShowDialog();

                if (result == DialogResult.OK) {
                    gpxFileName = saveGpxFileDialog.FileName;

                    // Save settings persisted between invocations of this dialog.
                    gpxCreationSettingsPrevious = createGpxDialog.CreationSettings;
                    controller.ExportGpx(gpxFileName, createGpxDialog.CreationSettings);
                }
            }

            // And the dialog is done.
            createGpxDialog.Dispose();
        }

        // Warn user about non-renderable objects. Return false if shouldn't continue
        private bool CheckForNonRenderableObjects(bool onlyOnce, bool showCancelAndContinue)
        {
            // Check for objects that aren't renderable, and warn. If user choses cancel, then cancel.
            string[] nonRenderableObjects = controller.NonrenderableObjects(onlyOnce);

            if (nonRenderableObjects != null && nonRenderableObjects.Length > 0) {
                NonPrintableObjects dialog = new NonPrintableObjects(showCancelAndContinue);
                dialog.MapName = Path.GetFileName(controller.MapFileName);
                dialog.BadObjectList = nonRenderableObjects;

                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.Cancel)
                    return false;
            }

            return true;
        }

        private void SetPrintArea(PrintAreaKind printAreaKind)
        {
            SetPrintAreaDialog dialog = new SetPrintAreaDialog(this, controller, printAreaKind);

            Point location = this.Location;
            location.Offset(10, 100);
            dialog.Location = location;

            dialog.Show(this);

            // Make sure the existing print area is fully visible, but hide the gray display.
            RectangleF rectangleCurrent = controller.GetCurrentPrintAreaRectangle(printAreaKind);
            if (!mapViewer.Viewport.Contains(rectangleCurrent)) {
                rectangleCurrent.Inflate(rectangleCurrent.Width * 0.05F, rectangleCurrent.Height * 0.05F);
                ShowRectangle(rectangleCurrent);
            }
            HidePrintArea = true;
            
            controller.BeginSetPrintArea(printAreaKind, dialog);
            dialog.PrintArea = controller.GetCurrentPrintArea(printAreaKind);
        }

        private void setPrintAreaMenu_DropDownOpening(object sender, EventArgs e)
        {
            bool multiPart = (controller.NumberOfParts > 1);
            UpdateMenuItem(printAreaThisPartMenu, multiPart ? CommandStatus.Enabled : CommandStatus.Hidden);
        }

        private void printAreaThisPartMenu_Click(object sender, EventArgs e)
        {
            SetPrintArea(PrintAreaKind.OnePart);
        }

        private void printAreaThisCourseMenu_Click(object sender, EventArgs e)
        {
            SetPrintArea(PrintAreaKind.OneCourse);
        }

        private void printAreaAllCoursesMenu_Click(object sender, EventArgs e)
        {
            SetPrintArea(PrintAreaKind.AllCourses);
        }

        private void changeMapFileMenu_Click(object sender, EventArgs e)
        {
            // Initialize dialog.
            ChangeMapFile dialog = new ChangeMapFile();
            dialog.MapFile = controller.MapFileName;
            if (controller.MapType == MapType.Bitmap) {
                dialog.MapScale = controller.MapScale;   // Note: these must be set AFTER the MapFile property
                dialog.Dpi = controller.MapDpi;
            }
            else if (controller.MapType == MapType.PDF) {
                dialog.MapScale = controller.MapScale;
            }

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply new map file.
            if (result == DialogResult.OK) {
                controller.ChangeMapFile(dialog.MapType, dialog.MapFile, dialog.MapScale, dialog.Dpi);
            }
        }

        // Find a new map file. This is like ChangeMapFile, but this UI is somewhat different -- we just show the
        // Open File dialog at first, and if we use it to select an OK OCAD file, then we close immediately too.
        public bool FindMissingMapFile(string missingMapFile)
        {
            // Initialize dialog.
            ChangeMapFile dialog = new ChangeMapFile();
            dialog.MapFile = missingMapFile;
            if (controller.MapType == MapType.Bitmap) {
                dialog.MapScale = controller.MapScale;   // Note: these must be set AFTER the MapFile property
                dialog.Dpi = controller.MapDpi;
            }
            else if (controller.MapType == MapType.PDF) {
                dialog.MapScale = controller.MapScale;
            }

            // Show the dialog.
            DialogResult result = dialog.ShowOpenFileDialogOnly(this.IsHandleCreated ? this : null);

            // Apply new map file.
            if (result == DialogResult.OK) {
                controller.ChangeMapFile(dialog.MapType, dialog.MapFile, dialog.MapScale, dialog.Dpi);
                return true;
            }
            else
                return false;
        }


        private void createOcadFilesMenu_Click(object sender, EventArgs e)
        {
            bool success = false;

            MapFileFormatKind restrictToKind;  // restrict to outputting this kind of map.
            if (mapDisplay.MapType == MapType.OCAD) {
                restrictToKind = mapDisplay.MapVersion.kind;
            }
            else {
                restrictToKind = MapFileFormatKind.None;
            }

            // Get the settings for the dialog. If we've previously show the dialog, then
            // use the previous settings. Note that the previous settings are wiped out when a new map file
            // is loaded.

            OcadCreationSettings settings;
            if (ocadCreationSettingsPrevious != null) 
            {
                settings = ocadCreationSettingsPrevious.Clone();
                if (restrictToKind != MapFileFormatKind.None & restrictToKind != ocadCreationSettingsPrevious.fileFormat.kind) {
                    settings.fileFormat = mapDisplay.MapVersion;
                }
            }
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new OcadCreationSettings();

                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = Path.GetDirectoryName(controller.FileName);
                if (mapDisplay.MapType == MapType.OCAD) {
                    settings.fileFormat = mapDisplay.MapVersion;
                }
                else {
                    settings.fileFormat = new MapFileFormat(MapFileFormatKind.OCAD, 8);  // TODO: Maybe change the default to OpenMapper?
                }
            }

            // Get the correct purple color to use.
            FindPurple.GetPurpleColor(mapDisplay, controller.GetCourseAppearance(), out settings.colorOcadId, out settings.cyan, out settings.magenta, out settings.yellow, out settings.black, out settings.purpleOverprint);

            // Initialize the dialog.
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            CreateOcadFiles createOcadFilesDialog = new CreateOcadFiles(controller.GetEventDB(), restrictToKind, controller.CreateOcadFilesText(false));
            createOcadFilesDialog.OcadCreationSettings = settings;

            // show the dialog; on success, create the files.
            while (createOcadFilesDialog.ShowDialog(this) == DialogResult.OK) {
                // Warn about files that will be overwritten.
                List<string> overwritingFiles = controller.OverwritingOcadFiles(createOcadFilesDialog.OcadCreationSettings);
                if (overwritingFiles.Count > 0) {
                    OverwritingOcadFilesDialog overwriteDialog = new OverwritingOcadFilesDialog();
                    overwriteDialog.Filenames = overwritingFiles;
                    if (overwriteDialog.ShowDialog(this) == DialogResult.Cancel)
                        continue;
                }

                // Give any other warning messages.
                List<string> warnings = controller.OcadFilesWarnings(createOcadFilesDialog.OcadCreationSettings);
                foreach (string warning in warnings) {
                    WarningMessage(warning);
                }

                // Save settings persisted between invocations of this dialog.
                ocadCreationSettingsPrevious = createOcadFilesDialog.OcadCreationSettings;
                success = controller.CreateOcadFiles(createOcadFilesDialog.OcadCreationSettings);

                // PP keeps bitmaps in memory and locks them. Tell the user to close PP.
                if (mapDisplay.MapType == MapType.Bitmap)
                    InfoMessage(MiscText.ClosePPBeforeLoadingOCAD);

                break;
            }

            // And the dialog is done.
            createOcadFilesDialog.Dispose();

            // The Windows Store version doesn't install Roboto fonts into the system. So we may need to tell the user to install them.
            // Check if they need to be installed, ask the user, and if they say yes, install the fonts.
            if (success) {
                if (controller.ShouldInstallRobotoFonts()) {
                    if (YesNoQuestion(MiscText.AskInstallRobotoFonts, true)) {
                        bool installSucceeded = controller.InstallRobotoFonts();
                        if (!installSucceeded)
                            ErrorMessage(MiscText.RobotoFontsInstallFailed);
                    }
                }
            }
        }

        private void createKmlFilesMenu_Click(object sender, EventArgs e)
        {
            // First check and give immediate message if we can't do coordinate mapping.
            string message;
            if (!controller.CanExportGpxOrKml(out message)) {
                ErrorMessage(message);
                return;
            }

            // Get the settings for the dialog. If we've previously show the dialog, then
            // use the previous settings. Note that the previous settings are wiped out when a new map file
            // is loaded.

            ExportKmlSettings settings;
            if (exportKmlSettingsPrevious != null) {
                settings = exportKmlSettingsPrevious.Clone();
            }
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new ExportKmlSettings();

                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = Path.GetDirectoryName(controller.FileName);
            }

            // Initialize the dialog.
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            CreateKmlFiles createKmlFilesDialog = new CreateKmlFiles(controller.GetEventDB());
            createKmlFilesDialog.ExportKmlSettings = settings;

            // show the dialog; on success, create the files.
            while (createKmlFilesDialog.ShowDialog(this) == DialogResult.OK) {
                // Warn about files that will be overwritten.
                List<string> overwritingFiles = controller.OverwritingKmlFiles(createKmlFilesDialog.ExportKmlSettings);
                if (overwritingFiles.Count > 0) {
                    OverwritingOcadFilesDialog overwriteDialog = new OverwritingOcadFilesDialog();
                    overwriteDialog.Filenames = overwritingFiles;
                    if (overwriteDialog.ShowDialog(this) == DialogResult.Cancel)
                        continue;
                }

                // Save settings persisted between invocations of this dialog.
                exportKmlSettingsPrevious = createKmlFilesDialog.ExportKmlSettings;
                controller.CreateKmlFiles(createKmlFilesDialog.ExportKmlSettings);

                break;
            }

            // And the dialog is done.
            createKmlFilesDialog.Dispose();
        }

        private void createImageFilesMenu_Click(object sender, EventArgs e)
        {
            // Get the settings for the dialog. If we've previously show the dialog, then
            // use the previous settings. Note that the previous settings are wiped out when a new map file
            // is loaded.

            BitmapCreationSettings settings;
            if (bitmapCreationSettingsPrevious != null) {
                settings = bitmapCreationSettingsPrevious.Clone();
            }
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new BitmapCreationSettings();

                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = Path.GetDirectoryName(controller.FileName);
                settings.Dpi = 200;
                settings.ColorModel = ColorModel.CMYK;
                settings.ExportedBitmapKind = BitmapCreationSettings.BitmapKind.Png;
            }

            // Initialize the dialog.
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            CreateImageFiles createImageFilesDialog = new CreateImageFiles(controller.GetEventDB());
            if (!controller.BitmapFilesCanCreateWorldFile()) {
                createImageFilesDialog.WorldFileEnabled = false;
                settings.WorldFile = false;
            }
            createImageFilesDialog.BitmapCreationSettings = settings;

            // show the dialog; on success, create the files.
            while (createImageFilesDialog.ShowDialog(this) == DialogResult.OK) {
                // Warn about files that will be overwritten.
                List<string> overwritingFiles = controller.OverwritingBitmapFiles(createImageFilesDialog.BitmapCreationSettings);
                if (overwritingFiles.Count > 0) {
                    OverwritingOcadFilesDialog overwriteDialog = new OverwritingOcadFilesDialog();
                    overwriteDialog.Filenames = overwritingFiles;
                    if (overwriteDialog.ShowDialog(this) == DialogResult.Cancel)
                        continue;
                }

                // Save settings persisted between invocations of this dialog.
                bitmapCreationSettingsPrevious = createImageFilesDialog.BitmapCreationSettings;
                controller.CreateBitmapFiles(createImageFilesDialog.BitmapCreationSettings);

                break;
            }

            // And the dialog is done.
            createImageFilesDialog.Dispose();
        }

        private void createRouteGadgetFilesMenu_Click(object sender, EventArgs e)
        {
            // Get the settings for the dialog. If we've previously show the dialog, then
            // use the previous settings. Note that the previous settings are wiped out when a new map file
            // is loaded.
            RouteGadgetCreationSettings settings;
            if (routeGadgetCreationSettingsPrevious != null)
                settings = routeGadgetCreationSettingsPrevious.Clone();
            else {
                // Default settings: creating in file directory, use format of the current map file.
                settings = new RouteGadgetCreationSettings();

                settings.fileDirectory = true;
                settings.mapDirectory = false;
                settings.outputDirectory = Path.GetDirectoryName(controller.FileName);
                settings.fileBaseName = Path.GetFileNameWithoutExtension(controller.FileName);
            }

            // Initialize the dialog.
            // CONSIDER: shouldn't have GetEventDB here! Do something different.
            CreateRouteGadgetFiles createRouteGadgetFilesDialog = new CreateRouteGadgetFiles(controller.GetEventDB());
            createRouteGadgetFilesDialog.RouteGadgetCreationSettings = settings;

            // show the dialog; on success, create the files.
            while (createRouteGadgetFilesDialog.ShowDialog(this) == DialogResult.OK) {
                List<string> overwritingFiles = controller.OverwritingRouteGadgetFiles(createRouteGadgetFilesDialog.RouteGadgetCreationSettings);
                if (overwritingFiles.Count > 0) {
                    OverwritingOcadFilesDialog overwriteDialog = new OverwritingOcadFilesDialog();
                    overwriteDialog.Filenames = overwritingFiles;
                    if (overwriteDialog.ShowDialog(this) == DialogResult.Cancel)
                        continue;
                }

                // Save settings persisted between invocations of this dialog.
                routeGadgetCreationSettingsPrevious = createRouteGadgetFilesDialog.RouteGadgetCreationSettings;
                controller.CreateRouteGadgetFiles(createRouteGadgetFilesDialog.RouteGadgetCreationSettings);

                break;
            }

            // And the dialog is done.
            createRouteGadgetFilesDialog.Dispose();
        }

        private void publishToLiveloxMenu_Click(object sender, EventArgs e)
        {
            LiveloxPublishSettings settings;
            if (liveloxPublishSettingsPrevious != null)
            {
                settings = liveloxPublishSettingsPrevious.Clone();
            }
            else
            {
                settings = new LiveloxPublishSettings();
            }

            var publishToLiveloxDialog = new PublishToLiveloxDialog(controller, symbolDB, settings);
            publishToLiveloxDialog.InitializeImportableEvent(this, call =>
            {
                // must invoke on UI thread
                this.InvokeOnUiThread(() => {
                    controller.EndProgressDialog();
                    if (call.Success)
                    {
                        publishToLiveloxDialog.ShowDialog(this);
                        liveloxPublishSettingsPrevious = publishToLiveloxDialog.PublishSettings;
                    }
                    else
                    {
                        MessageBox.Show(this, call.Exception?.Message, MiscText.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    publishToLiveloxDialog.Dispose();
                });
            });
        }

        private void createXmlMenu_Click(object sender, EventArgs e)
        {
            // The default output for the XML is the same as the event file name, with xml extension.
            string xmlFileName = Path.ChangeExtension(controller.FileName, ".xml");

            saveXmlFileDialog.FileName = xmlFileName;
            DialogResult result = saveXmlFileDialog.ShowDialog();

            if (result == DialogResult.OK) {
                int version = 2;
                if (saveXmlFileDialog.FilterIndex == 2)
                    version = 3;
                controller.ExportXml(saveXmlFileDialog.FileName, mapDisplay.MapBounds, version);
            }
        }


        private void courseSelectorTesterMenu_Click(object sender, EventArgs e)
        {
            new CourseSelectorTestForm(controller.GetEventDB()).ShowDialog(this);
        }

        private void dumpOCADFileMenu_Click(object sender, EventArgs e)
        {
            OpenFileDialog openOcadFileDialog = new OpenFileDialog();
            openOcadFileDialog.Filter = "OCAD files|*.ocd|All files|*.*";
            openOcadFileDialog.FilterIndex = 1;
            openOcadFileDialog.DefaultExt = "ocd";

            DialogResult result = openOcadFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
                return;
            string ocadFile = openOcadFileDialog.FileName;

            SaveFileDialog saveDumpFileDialog = new SaveFileDialog();
            saveDumpFileDialog.Filter = "Test file|*.txt";
            saveDumpFileDialog.FilterIndex = 1;
            saveDumpFileDialog.DefaultExt = "txt";

            result = saveDumpFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
                return;
            string dumpFile = saveDumpFileDialog.FileName;

            using (TextWriter writer = new StreamWriter(dumpFile)) {
                PurplePen.MapModel.DebugCode.OcadDump dumper = new PurplePen.MapModel.DebugCode.OcadDump();
                dumper.DumpFile(ocadFile, writer);
            }
        }

        private void supportWebSiteMenu_Click(object sender, EventArgs e)
        {
            Util.GoToWebPage("http://purple-pen.org#support");
        }

        private void mainWebSiteToolMenu_Click(object sender, EventArgs e)
        {
            Util.GoToWebPage("http://purple-pen.org");
        }

        private void donateWebSiteMenu_Click(object sender, EventArgs e)
        {
            Util.GoToWebPage("http://purple-pen.org#donate");
        }

        private void courseSummaryMenu_Click(object sender, EventArgs e)
        {
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateCourseSummaryReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(Util.RemoveHotkeyPrefix(courseSummaryMenu.Text), "", testReport, "ReportsCourseSummary.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
        }

        private void controlCrossrefMenu_Click(object sender, EventArgs e)
        {
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateCrossReferenceReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(Util.RemoveHotkeyPrefix(controlCrossrefMenu.Text), "", testReport, "ReportsControlCrossReference.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
        }

        private void controlAndLegLoadMenu_Click(object sender, EventArgs e)
        {
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateLoadReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(Util.RemoveHotkeyPrefix(controlAndLegLoadMenu.Text), "", testReport, "ReportsControlAndLegLoad.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
        }

        private void legLengthsMenu_Click(object sender, EventArgs e)
        {
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateLegLengthReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(Util.RemoveHotkeyPrefix(legLengthsMenu.Text), "", testReport, "ReportsLegLengths.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
        }

        private void eventAuditMenu_Click(object sender, EventArgs e)
        {
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateEventAuditReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm(Util.RemoveHotkeyPrefix(eventAuditMenu.Text), "", testReport, "ReportsEventAudit.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
        }

        private void courseVariationReportMenu_Click(object sender, EventArgs e)
        {
            RelaySettings relaySettings = controller.GetRelayParameters();
            bool hideVariationsOnMap = controller.GetHideVariationsOnMap();
            TeamVariationsForm reportForm = new TeamVariationsForm();
            reportForm.FirstTeamNumber = relaySettings.firstTeamNumber;
            reportForm.NumberOfTeams = relaySettings.relayTeams;
            reportForm.NumberOfLegs = relaySettings.relayLegs;
            reportForm.FixedBranchAssignments = relaySettings.relayBranchAssignments;
            reportForm.HideVariationsOnMap = hideVariationsOnMap;
            reportForm.DefaultExportFileName = controller.GetDefaultVariationExportFileName();

            SetVariationReportBody(reportForm);

            reportForm.CalculateVariationsPressed += (reportSender, reportEventArgs) => {
                SetVariationReportBody(reportForm);
            };

            reportForm.AssignLegsPressed += (reportSender, reportEventArgs) => {
                ShowAssignLegs(reportForm);
            };

            reportForm.ExportFilePressed += (reportSender, reportEventArgs) => {
                ExportVariationReport(reportForm, reportEventArgs.FileType, reportEventArgs.FileName);
            };

            reportForm.ShowDialog(this);

            if (relaySettings.firstTeamNumber != reportForm.FirstTeamNumber ||
                relaySettings.relayTeams != reportForm.NumberOfTeams ||
                relaySettings.relayLegs != reportForm.NumberOfLegs ||
                hideVariationsOnMap != reportForm.HideVariationsOnMap || 
                !object.Equals(relaySettings.relayBranchAssignments, reportForm.FixedBranchAssignments)) 
            {
                controller.SetRelayParameters(reportForm.RelaySettings, reportForm.HideVariationsOnMap);
            }

            reportForm.Dispose();
        }

        private void ShowAssignLegs(TeamVariationsForm reportForm)
        {
            LegAssignmentsDialog dialog = new LegAssignmentsDialog(controller.GetLegAssignmentCodes());
            dialog.FixedBranchAssignments = reportForm.FixedBranchAssignments;
            dialog.Validate += (sender, e) => {
                e.ErrorMessage = controller.ValidateFixedBranchAssignments(reportForm.NumberOfLegs, dialog.FixedBranchAssignments);
            };

            dialog.ShowDialog(reportForm);
            reportForm.FixedBranchAssignments = dialog.FixedBranchAssignments;
        }

        void SetVariationReportBody(TeamVariationsForm form)
        {
            if (form.NumberOfTeams == 0) {
                form.SetBody(new Reports().CreateRelayVariationNotCreated());
            }
            else {
                VariationReportData variationReportData = controller.GetVariationReportData(form.RelaySettings);
                form.SetBody(new Reports().CreateRelayVariationReport(variationReportData));
            }
        }

        void ExportVariationReport(TeamVariationsForm form, TeamVariationsForm.ExportFileType exportFileType, string exportFileName)
        {
            VariationReportData variationReportData = controller.GetVariationReportData(form.RelaySettings);
            controller.ExportRelayVariationsReport(form.RelaySettings, exportFileType, exportFileName);
        }

        private void MainFrame_Shown(object sender, EventArgs e)
        {
            // Begin check for new version in the background.
            Updater.Controller = this.controller;
            Updater.CheckForUpdates();
        }

        private void dotGridTesterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DotGridTester().ShowDialog(this);
        }

        // Update the size of controls based on the DPI.
        private void UpdateControlsForDpi()
        {
            locationDisplay.Width = LogicalToDeviceUnits(120);
            splitContainer.SplitterDistance = LogicalToDeviceUnits(256);
            splitDescription.SplitterDistance = splitDescription.Height - LogicalToDeviceUnits(150);
        }

        private void MainFrame_Load(object sender, EventArgs e)
        {
            UpdateControlsForDpi();
        }

        private void MainFrame_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            UpdateControlsForDpi();
        }

        private void zoomAmountLabel_Click(object sender, EventArgs e)
        {

        }

        private void reportTesterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reports reportGenerator = new Reports();

            string testReport = reportGenerator.CreateTestReport(controller.GetEventDB());

            ReportForm reportForm = new ReportForm("Test Report", "", testReport, "PurplePenWindow.htm");
            reportForm.ShowDialog(this);
            reportForm.Dispose();
        }

        private void fontMetricsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowFontMetrics fontMetricsDialog = new ShowFontMetrics(new GDIPlus_TextMetrics());

            fontMetricsDialog.ShowDialog(this);
            fontMetricsDialog.Dispose();
        }

        private void missingTranslationsMenuItem_Click(object sender, EventArgs e)
        {
            UntranslatedSymbolTexts untranslatedSymbolTexts = new UntranslatedSymbolTexts();
            string report = untranslatedSymbolTexts.ReportOnUntranslatedSymbolTexts(symbolDB);

            DebugTextForm debugTextForm = new DebugTextForm("Missing Translations", report);
            debugTextForm.ShowDialog(this);
            debugTextForm.Dispose();
        }



        private void programLanguageMenu_Click(object sender, EventArgs e)
        {
            SetUILanguage dialog = new SetUILanguage();

            dialog.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

            if (dialog.ShowDialog() == DialogResult.OK && dialog.Culture != null) {
                System.Threading.Thread.CurrentThread.CurrentUICulture = dialog.Culture;
                Settings.Default.UILanguage = dialog.Culture.Name;
                Settings.Default.Save();

                controller.ForceChangeUpdate();     // make the controller update state.

                ReloadMainFrameStrings();
                UpdateLabelsAndScrollBars();
                coursePartBanner.UpdateDropdown();
                Application_Idle(this, EventArgs.Empty);     // force update of everything.
                --changeNum;

                if (controller.GetDescriptionLanguage() != dialog.Culture.Name && controller.HasDescriptionLanguage(dialog.Culture.Name)) {
                    // The current description language does not match the new program language. Offer to change it to match.
                    if (YesNoQuestion(string.Format(MiscText.ChangeDescriptionLanguage,
                                                                        CultureInfo.GetCultureInfo(controller.GetDescriptionLanguage()).NativeName,
                                                                        CultureInfo.GetCultureInfo(dialog.Culture.Name).NativeName),
                                                 true)) 
                    {
                        controller.SetDescriptionLanguage(dialog.Culture.Name);
                        controller.DefaultDescriptionLanguage = dialog.Culture.Name;
                    }
                }
            }

            dialog.Dispose();
        }

        // Update all the strings in the main frame.
        private void ReloadMainFrameStrings()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainFrame));

            UpdateComponentText(resources, this, "$this");

            resources = new ComponentResourceManager(typeof(CoursePartBanner));

            UpdateComponentText(resources, this.coursePartBanner, "$this");
        }

        private void UpdateComponentText(ComponentResourceManager resources, object control, string componentName)
        {
            UpdateComponentProperty(resources, control, componentName, "Text");
            UpdateComponentProperty(resources, control, componentName, "ToolTipText");

            if (control is Control && ((Control)control).Controls != null) {
                foreach (Control subControl in ((Control) control).Controls)
                    UpdateComponentText(resources, subControl, subControl.Name);
            }

            if (control is ToolStrip) {
                foreach (ToolStripItem subItem in ((ToolStrip)control).Items)
                    UpdateComponentText(resources, subItem, subItem.Name);
            }

            if (control is ToolStripDropDownItem) {
                ToolStripDropDown dropdown = ((ToolStripDropDownItem) control).DropDown;
                if (dropdown != null)
                    UpdateComponentText(resources, dropdown, dropdown.Name);
            }
        }

        private void UpdateComponentProperty(ComponentResourceManager resources, object control, string componentName, string propertyName)
        {
            string newString = resources.GetString(componentName + "." + propertyName);
            if (newString != null) {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { newString });
            }
        }

        private void addDescriptionLanguageMenu_Click(object sender, EventArgs e)
        {
            DebugUI.NewLanguage newLanguageDialog = new NewLanguage(symbolDB);

            if (newLanguageDialog.ShowDialog(this) == DialogResult.OK) {
                SymbolLanguage symLanguage = new SymbolLanguage(newLanguageDialog.LanguageName, newLanguageDialog.LangId, newLanguageDialog.PluralNouns, 
                    newLanguageDialog.PluralModifiers, newLanguageDialog.GenderModifiers, 
                    newLanguageDialog.GenderModifiers ? newLanguageDialog.Genders.Split(new string[] {",", " "}, StringSplitOptions.RemoveEmptyEntries) : new string[0],
                    newLanguageDialog.CaseModifiers,
                    newLanguageDialog.CaseModifiers ? newLanguageDialog.Cases.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries) : new string[0]);
                controller.AddDescriptionLanguage(symLanguage, newLanguageDialog.CopyFromLangId);
                controller.SetDescriptionLanguage(symLanguage.LangId);
            }
        }

        private void addTranslatedTextsMenu_Click(object sender, EventArgs e)
        {
            // Initialize the dialog
            CustomSymbolText dialog = new CustomSymbolText(symbolDB, true);
            dialog.LangId = controller.GetDescriptionLanguage();

            // Show the dialog.
            DialogResult result = dialog.ShowDialog(this);

            // Apply the changes
            if (result == DialogResult.OK) {
                controller.AddDescriptionTexts(dialog.CustomSymbolTexts, dialog.SymbolNames);
                controller.SetDescriptionLanguage(dialog.LangId);
            }

            dialog.Dispose();
        }

        private void MainFrame_Activated(object sender, EventArgs e)
        {
            // Check whether the map file has changed.
            if (mapDisplay != null && Visible) {
                checkForUpdatedMapFile = true;
            }

        }

        private void mergeSymbolsMenu_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = ".xml";
            if (openFile.ShowDialog() == DialogResult.OK) {
                string filename = openFile.FileName;
                string langId = Microsoft.VisualBasic.Interaction.InputBox("Language code to import", "Merge Symbols.xml", null, 0, 0);
                controller.MergeSymbolsXml(filename, langId);
            }

            openFile.Dispose();
        }

        private void crashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int x = 0;
            int y = 5 / x;
        }

        private OperationInProgress operationInProgressDialog = null;

        public void ShowProgressDialog(bool knownDuration, Action onCancelPressed = null)
        {
            operationInProgressDialog = new OperationInProgress();
            operationInProgressDialog.IndefiniteDuration = !knownDuration;
            if (onCancelPressed != null)
            {
                operationInProgressDialog.OnCancelPressed += (sender, args) =>
                {
                    EndProgressDialog();
                    onCancelPressed();
                };
            }
            operationInProgressDialog.Show(this);
            this.Enabled = false;
            Application.DoEvents();
        }

        public bool UpdateProgressDialog(string info, double fractionDone)
        {
            var dialog = operationInProgressDialog;
            if (dialog != null) {
                dialog.StatusText = info;
                if (!dialog.IndefiniteDuration)
                    dialog.SetProgress(fractionDone);
                Application.DoEvents();
                return dialog.CancelPressed;
            }
            else {
                return true;
            }
        }

        public void EndProgressDialog()
        {
            if (operationInProgressDialog != null) {
                this.Enabled = true;
                operationInProgressDialog.Hide();
                operationInProgressDialog.Dispose();
                operationInProgressDialog = null;
            }
        }

        private void radioButtonDescriptionsTopology_CheckedChanged(object sender, EventArgs e)
        {
            descriptionControl.Visible = radioButtonDescriptions.Checked;
            panelTopology.Visible = radioButtonTopology.Checked;
        }

        public void ShowTopologyView()
        {
            descriptionControl.Visible = radioButtonDescriptions.Checked = false;
            panelTopology.Visible = radioButtonTopology.Checked = true;
        }

        private void mapViewerTopology_Resize(object sender, EventArgs e)
        {
            if (controller != null) {
                UpdateTopology();
            }
        }

        private void topologyScrollBar_ValueChanged(object sender, EventArgs e)
        {
            mapViewerTopology.VScrollValue = topologyScrollBar.Value;
        }

        private void mapViewerTopology_OnViewportChange(object sender, EventArgs e)
        {
            UpdateTopologyScrollBars();
        }

        private void descriptionStd2004Menu_Click(object sender, EventArgs e)
        {
            controller.ChangeDescriptionStandard("2004");
        } 
        

        private void descriptionStd2018Menu_Click(object sender, EventArgs e)
        {
            controller.ChangeDescriptionStandard("2018");
        }

        private void mapStd2000Menu_Click(object sender, EventArgs e)
        {
            controller.ChangeMapStandard("2000");
        }

        private void mapStd2017Menu_Click(object sender, EventArgs e)
        {
            controller.ChangeMapStandard("2017");
        }

        private void publishCourses_Click(object sender, EventArgs e)
        {
            using (var dlg = new PublishCoursesDialog(controller.GetEventDB()))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    publishCourses(dlg.SelectedCourses, dlg.DataExchangeFolderPath);
                }
            }
        }

        private void publishCourses(Id<Course>[] courseIds, string printerDataExchangeFolderPath)
        {
            BitmapCreationSettings settings = new BitmapCreationSettings();
            settings.CourseIds = courseIds;
            settings.AllCourses = false;
            settings.VariationChoicesPerCourse = settings.CourseIds.ToDictionary(
                id => id,
                id => new VariationChoices { Kind = VariationChoices.VariationChoicesKind.AllVariations }
            );
            settings.fileDirectory = false;
            settings.mapDirectory = false;
            settings.outputDirectory = @"..\Tulostus\Järjestelmä";
            settings.ExportedBitmapKind = BitmapCreationSettings.BitmapKind.Jpeg;
            settings.Dpi = 600;
            settings.ColorModel = ColorModel.RGB;
            settings.Quality = 95;
            settings.AutoRotate = true;
            controller.CreateBitmapFiles(settings);

            try
            {
                controller.CreateBitmapFiles(settings);
                InfoMessage(MiscText.PublishSucceeded);
            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format(MiscText.PublishFailed, ex.Message));
            }
        }

        private void splitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.AddCuttingLine();
        }

        private void mapStdSpr2019Menu_Click(object sender, EventArgs e)
        {
            controller.ChangeMapStandard("Spr2019");
        }
    }
}
