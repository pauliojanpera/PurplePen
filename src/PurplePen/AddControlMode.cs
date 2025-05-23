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
using System.Windows.Forms;
using System.Diagnostics;

using PurplePen.MapView;
using PurplePen.MapModel;
using PurplePen.Graphics2D;

namespace PurplePen
{
    class AddControlMode: BaseMode
    {
        const int PIXELOFFSETX = -7;         // offset in pixels from the mouse cursor to the cross-hairs of the control to place.
        const int PIXELOFFSETY = 7;

        Controller controller;
        SelectionMgr selectionMgr;
        UndoMgr undoMgr;
        EventDB eventDB;
        SymbolDB symbolDB;
        bool allControls;                  // Are we in All Controls (true), or adding to a course (false)
        ControlPointKind controlKind;      // Kind of control we are adding.
        MapExchangeType mapExchangeType;   // If not None, controlKind == Normal and we are changing a control to an exchange point.
        float courseObjRatio;
        MapIssueKind mapIssueKind;
        CourseAppearance appearance;

        PointCourseObj highlight;    // the highlight of the control we are creating.
        CourseObj[] additionalHighlights;  // additional highlights to show also. 

        public AddControlMode(Controller controller, SelectionMgr selectionMgr, UndoMgr undoMgr, EventDB eventDB, SymbolDB symbolDB, bool allControls, ControlPointKind controlKind, MapExchangeType mapExchangeType, MapIssueKind mapIssueKind)
        {
            this.controller = controller;
            this.selectionMgr = selectionMgr;
            this.undoMgr = undoMgr;
            this.eventDB = eventDB;
            this.symbolDB = symbolDB;
            this.allControls = allControls;
            this.controlKind = controlKind;
            this.mapExchangeType = mapExchangeType;
            this.mapIssueKind = mapIssueKind;
            this.appearance = controller.GetCourseAppearance();
            this.courseObjRatio = selectionMgr.ActiveCourseView.CourseObjRatio(appearance);
        }

        public override void BeginMode()
        {
            if (!allControls) {
                // Show all the existing controls we could add (not already in the course).
                controller.SetTemporaryControlView(true, controlKind);
            }

            // Create the initial highlight.
            PointF location; 
            float pixelSize;

            if (controller.GetCurrentLocation(out location, out pixelSize)) {
                PointF highlightLocation;
                bool temp = false;
                HitTestPoint(location, pixelSize, out highlightLocation);
                SetHighlightLocation(highlightLocation, pixelSize, ref temp);
            }
        }

        public override void EndMode()
        {
            // Don't view any other controls any more.
            controller.SetTemporaryControlView(false, ControlPointKind.None);
        }

        public override string StatusText
        {
            get
            {
                Id<ControlPoint> existingControl = Id<ControlPoint>.None;
                PointF location;
                float pixelSize;
                if (controller.GetCurrentLocation(out location, out pixelSize)) {
                    PointF highlightLocation;
                    existingControl = HitTestPoint(location, pixelSize, out highlightLocation);
                }

                switch (controlKind) {
                case ControlPointKind.MapIssue:
                    return (existingControl.IsNone) ? StatusBarText.AddingMapIssue : StatusBarText.AddingExistingMapIssue;
                case ControlPointKind.Start:
                    return (existingControl.IsNone) ? StatusBarText.AddingStart : StatusBarText.AddingExistingStart;
                case ControlPointKind.Finish:
                    return (existingControl.IsNone) ? StatusBarText.AddingFinish : StatusBarText.AddingExistingFinish;
                case ControlPointKind.CrossingPoint:
                    return (existingControl.IsNone) ? StatusBarText.AddingCrossingPoint : StatusBarText.AddingExistingCrossingPoint;
                case ControlPointKind.MapExchange:
                    return (existingControl.IsNone) ? StatusBarText.AddingMapExchange : StatusBarText.AddingExistingMapExchange;
                case ControlPointKind.Normal:
                    if (existingControl.IsNone) {
                        return StatusBarText.AddingControl;
                    } else {
                        return string.Format(mapExchangeType != MapExchangeType.None && QueryEvent.CourseUsesControl(eventDB, selectionMgr.Selection.ActiveCourseDesignator, existingControl) ? 
                                                    StatusBarText.AddingMapExchangeToControl : StatusBarText.AddingExistingControl, 
                                             eventDB.GetControl(existingControl).code);
                    }
                default:
                    return "";
                }
            }
        }

        // Hit test a point to see if it is over an existing control, or will create a new control.
        Id<ControlPoint> HitTestPoint(PointF mouseLocation, float pixelSize, out PointF highlightLocation)
        {
            if (allControls) {
                // If all controls, always new control.
                highlightLocation = new PointF(mouseLocation.X + PIXELOFFSETX * pixelSize, mouseLocation.Y + PIXELOFFSETY * pixelSize);
                return Id<ControlPoint>.None;
            }
            else {
                // Are we over a control we might add?
                CourseLayout layout = controller.GetCourseLayout();
                PointCourseObj courseObj = layout.HitTest(mouseLocation, pixelSize, CourseLayer.AllControls, (co => co is PointCourseObj)) as PointCourseObj;
                if (courseObj != null) {
                    highlightLocation = courseObj.location;
                    return courseObj.controlId;
                }
                else {
                    courseObj = layout.HitTest(mouseLocation, pixelSize, CourseLayer.MainCourse, (co => co is PointCourseObj)) as PointCourseObj;
                    if (courseObj != null && 
                        courseObj.controlId.IsNotNone)
                    {
                        // Allow selecting a control in the current course for a butterfly course. But -- it must be a normal control or crossing point, and not adjacent to the control being inserted.
                        if (eventDB.GetControl(courseObj.controlId).kind == controlKind && (controlKind == ControlPointKind.Normal || controlKind == ControlPointKind.CrossingPoint)) 
                        {
                            Id<CourseControl> courseControl1, courseControl2;
                            CourseDesignator courseDesignator;
                            LegInsertionLoc legInsertionLoc;

                            GetControlInsertionPoint(courseObj.location, out courseDesignator, out courseControl1, out courseControl2, out legInsertionLoc);
                            if (eventDB.GetCourse(courseDesignator.CourseId).kind != CourseKind.Score && 
                                (mapExchangeType != MapExchangeType.None || 
                                 (courseObj.courseControlId != courseControl1 && courseObj.courseControlId != courseControl2))) {
                                highlightLocation = courseObj.location;
                                return courseObj.controlId;
                            }
                        }
                    }

                    highlightLocation = new PointF(mouseLocation.X + PIXELOFFSETX * pixelSize, mouseLocation.Y + PIXELOFFSETY * pixelSize);
                    return Id<ControlPoint>.None;
                }
            }
        }

        public override void MouseMoved(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane == Pane.Map) {
                PointF highlightLocation;
                Id<ControlPoint> controlId = HitTestPoint(location, pixelSize, out highlightLocation);
                SetHighlightLocation(highlightLocation, pixelSize, ref displayUpdateNeeded);
            }
        }

        public override IMapViewerHighlight[] GetHighlights(Pane pane)
        {
            if (pane == Pane.Map) {
                if (highlight != null) {
                    if (additionalHighlights != null && additionalHighlights.Length > 0) {
                        CourseObj[] highlights = new CourseObj[additionalHighlights.Length + 1];
                        highlights[0] = highlight;
                        Array.Copy(additionalHighlights, 0, highlights, 1, additionalHighlights.Length);
                        return highlights;
                    }
                    else {
                        return new CourseObj[] { highlight };
                    }
                }
                else
                    return null;
            }
            else {
                return null;
            }
        }

        // Get the controls the define where to insert the new control point.
        private void GetControlInsertionPoint(PointF pt, out CourseDesignator courseDesignator, 
                                              out Id<CourseControl> courseControlId1, out Id<CourseControl> courseControlId2, 
                                              out LegInsertionLoc legInsertionLoc)
        {
            SelectionMgr.SelectionInfo selection = selectionMgr.Selection;
            courseDesignator = selection.ActiveCourseDesignator;
            courseControlId1 = Id<CourseControl>.None;
            courseControlId2 = Id<CourseControl>.None;
            legInsertionLoc = LegInsertionLoc.Normal;

            if (selection.SelectionKind == SelectionMgr.SelectionKind.Control && 
                (courseDesignator.IsAllControls || QueryEvent.IsCourseControlInPart(eventDB, courseDesignator, selection.SelectedCourseControl)))
                courseControlId1 = selection.SelectedCourseControl;
            else if (selection.SelectionKind == SelectionMgr.SelectionKind.Leg) {
                courseControlId1 = selection.SelectedCourseControl;
                courseControlId2 = selection.SelectedCourseControl2;
                legInsertionLoc = selection.LegInsertionLoc;
            }
            else if (courseDesignator.IsNotAllControls) {
                // Not all control, and neight control or leg is selected. Use the closest leg.
                QueryEvent.LegInfo leg = QueryEvent.FindClosestLeg(eventDB, courseDesignator, pt);
                courseControlId1 = leg.courseControlId1;
                courseControlId2 = leg.courseControlId2;
            }

            if (courseDesignator.IsNotAllControls)
                QueryEvent.FindControlInsertionPoint(eventDB, courseDesignator, ref courseControlId1, ref courseControlId2, ref legInsertionLoc);
        }

        public override MapViewer.DragAction LeftButtonDown(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane == Pane.Map) {
                // Delay to see if click or drag.
                return MapViewer.DragAction.DelayedDrag;
            }
            else {
                return MapViewer.DragAction.None;
            }
        }

        public override void LeftButtonDrag(Pane pane, PointF location, PointF locationStart, float pixelSize, ref bool displayUpdateNeeded)
        {
            Debug.Assert(pane == Pane.Map);

            // Drag is move map.
            controller.InitiateMapDragging(locationStart, System.Windows.Forms.MouseButtons.Left);
        }

        public override void LeftButtonClick(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return;

            // Create the new control!

            // Are we creating a new control point, or using existing one?
            PointF highlightLocation;
            Id<ControlPoint> controlId = HitTestPoint(location, pixelSize, out highlightLocation);
            bool createNewControl = controlId.IsNone;
            string commandString;

            switch (controlKind) {
            case ControlPointKind.MapIssue: commandString = CommandNameText.AddMapIssue; break;
            case ControlPointKind.Start: commandString = CommandNameText.AddStart; break;
            case ControlPointKind.Finish: commandString = CommandNameText.AddFinish; break;
            case ControlPointKind.CrossingPoint: commandString = CommandNameText.AddCrossingPoint; break;
            case ControlPointKind.MapExchange: commandString = CommandNameText.AddMapExchange; break;
            default:
                if (mapExchangeType == MapExchangeType.Exchange)
                    commandString = CommandNameText.AddMapExchange;
                else if (mapExchangeType == MapExchangeType.MapFlip)
                    commandString = CommandNameText.AddMapFlip;
                else
                    commandString = CommandNameText.AddControl;
                break;
            }

            undoMgr.BeginCommand(1321, commandString);

            if (createNewControl) {
                // Creating a new control point.
                string newCode = null;
                if (controlKind == ControlPointKind.Normal)
                    newCode = QueryEvent.NextUnusedControlCode(eventDB);
                controlId = ChangeEvent.AddControlPoint(eventDB, controlKind, newCode, highlightLocation, 0, mapIssueKind);
                if (controlKind == ControlPointKind.Finish)
                    ChangeEvent.ChangeDescriptionSymbol(eventDB, controlId, 0, "14.3");   // set finish to "navigate to finish".
                else if (controlKind == ControlPointKind.CrossingPoint)
                    ChangeEvent.ChangeDescriptionSymbol(eventDB, controlId, 0, "13.3");   // set to mandatory crossing point.
                else if (controlKind == ControlPointKind.MapIssue)
                    ChangeEvent.ChangeDescriptionSymbol(eventDB, controlId, 0, "13.6");   // Map issue point.
            }

            if (allControls) {
                // select the new control.
                selectionMgr.SelectControl(controlId);
            }
            else {
                // Add the control to the current course.

                // Get where to add the control.
                CourseDesignator courseDesignator;
                Id<CourseControl> courseControl1, courseControl2;
                LegInsertionLoc legInsertionLoc;

                GetControlInsertionPoint(highlightLocation, out courseDesignator, out courseControl1, out courseControl2, out legInsertionLoc);

                // And add it.
                Id<CourseControl> courseControlId;
                if (controlKind == ControlPointKind.Start)
                    courseControlId = ChangeEvent.AddStartOrMapIssueToCourse(eventDB, controlId, courseDesignator.CourseId, true);
                else if (controlKind == ControlPointKind.MapIssue)
                    courseControlId = ChangeEvent.AddStartOrMapIssueToCourse(eventDB, controlId, courseDesignator.CourseId, true);
                else if (controlKind == ControlPointKind.Finish)
                    courseControlId = ChangeEvent.AddFinishToCourse(eventDB, controlId, courseDesignator.CourseId, true);
                else if (controlKind == ControlPointKind.MapExchange) {
                    courseControlId = ChangeEvent.AddCourseControl(eventDB, controlId, courseDesignator.CourseId, courseControl1, courseControl2, legInsertionLoc);
                    ChangeEvent.ChangeControlExchange(eventDB, courseControlId, MapExchangeType.Exchange);
                }
                else if (mapExchangeType != MapExchangeType.None && QueryEvent.CourseUsesControl(eventDB, courseDesignator, controlId)) {
                    // Selected control already on course, just add map exchange at that courseControl(s)).
                    courseControlId = Id<CourseControl>.None;
                    foreach (Id<CourseControl> courseControlBecomesExchange in QueryEvent.GetCourseControlsInCourse(eventDB, courseDesignator, controlId)) {
                        ChangeEvent.ChangeControlExchange(eventDB, courseControlBecomesExchange, mapExchangeType);
                        courseControlId = courseControlBecomesExchange;
                    }
                }
                else {
                    courseControlId = ChangeEvent.AddCourseControl(eventDB, controlId, courseDesignator.CourseId, courseControl1, courseControl2, legInsertionLoc);
                    if (mapExchangeType != MapExchangeType.None)
                        ChangeEvent.ChangeControlExchange(eventDB, courseControlId, mapExchangeType);
                }

                // select the new control.
                selectionMgr.SelectCourseControl(courseControlId);
            }

            undoMgr.EndCommand(1321);

            controller.DefaultCommandMode();
        }

        // Create the highlight, and put it at the given location.
        // Set displayUpdateNeeded to true if the highlight was just created or was moved.
        void SetHighlightLocation(PointF highlightLocation, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (highlight != null && highlight.location == highlightLocation)
                return;

            PointF unused;
            Id<ControlPoint> existingControl = HitTestPoint(highlightLocation, pixelSize, out unused);

            // Get where the control is being inserted.
            CourseDesignator courseDesignator;
            Id<CourseControl> courseControl1, courseControl2;
            LegInsertionLoc legInsertionLoc;

            GetControlInsertionPoint(highlightLocation, out courseDesignator, out courseControl1, out courseControl2, out legInsertionLoc);

            // Note, we cannot changed this existing highlight because it is needed for erasing.
            additionalHighlights = null;

            switch (controlKind) {
            case ControlPointKind.Normal:
                highlight = new ControlCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, courseObjRatio, appearance, null, highlightLocation);

                if (courseDesignator.IsNotAllControls &&
                    !(mapExchangeType != MapExchangeType.None && existingControl.IsNotNone && QueryEvent.CourseUsesControl(eventDB, courseDesignator, existingControl)) &&
                    eventDB.GetCourse(courseDesignator.CourseId).kind != CourseKind.Score) 
                {
                    // Show the legs to and from the control also as additional highlights.
                    additionalHighlights = CreateLegHighlights(eventDB, highlightLocation, Id<ControlPoint>.None, controlKind, courseControl1, courseControl2, courseObjRatio, appearance);
                }
                break;

            case ControlPointKind.MapIssue:
                highlight = new MapIssueCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, courseObjRatio, appearance, 0, highlightLocation, MapIssueCourseObj.RenderStyle.WithTail);
                break;

            case ControlPointKind.Start:
                highlight = new StartCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, courseObjRatio, appearance, 0, highlightLocation, CrossHairOptions.HighlightCrossHair);
                break;

            case ControlPointKind.MapExchange:
                highlight = new StartCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, courseObjRatio, appearance, 0, highlightLocation, CrossHairOptions.HighlightCrossHair);

                if (courseDesignator.IsNotAllControls && eventDB.GetCourse(courseDesignator.CourseId).kind != CourseKind.Score) {
                    // Show the legs to and from the control also as additional highlights.
                    additionalHighlights = CreateLegHighlights(eventDB, highlightLocation, Id<ControlPoint>.None, controlKind, courseControl1, courseControl2, courseObjRatio, appearance);
                }
                break;

            case ControlPointKind.Finish:
                highlight = new FinishCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, courseObjRatio, appearance, null, highlightLocation, CrossHairOptions.HighlightCrossHair);
                break;

            case ControlPointKind.CrossingPoint:
                highlight = new CrossingCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, Id<Special>.None, courseObjRatio, appearance, 0, 0, highlightLocation);

                if (courseDesignator.IsNotAllControls && eventDB.GetCourse(courseDesignator.CourseId).kind != CourseKind.Score) {
                    // Show the legs to and from the control also as additional highlights.
                    additionalHighlights = CreateLegHighlights(eventDB, highlightLocation, Id<ControlPoint>.None, controlKind, courseControl1, courseControl2, courseObjRatio, appearance);
                }
                break;

            default:
                throw new Exception("bad control kind");
            }

            highlight.location = highlightLocation;
            displayUpdateNeeded = true;
        }

        // Create a leg object from one point to another. Might return null. The controlIds can be None, but if they are supplied, then
        // they are used to handle bends. If either is null, the leg object is just straight. Gaps are never displayed.
        private static LegCourseObj CreateLegHighlight(EventDB eventDB, PointF pt1, ControlPointKind kind1, Id<ControlPoint> controlId1, PointF pt2, ControlPointKind kind2, Id<ControlPoint> controlId2, float courseObjRatio, CourseAppearance appearance)
        {
            LegGap[] gaps;

            SymPath path = CourseFormatter.GetLegPath(eventDB, pt1, kind1, controlId1, pt2, kind2, controlId2, float.NaN, courseObjRatio, appearance, out gaps);
            if (path != null)
                return new LegCourseObj(controlId1, Id<CourseControl>.None, Id<CourseControl>.None, courseObjRatio, appearance, path, null);     // We never display the gaps, because it looks dumb.
            else
                return null;
        }

        // Create highlights to and from a point to course controls. If controlDrag is set (optional), it is 
        // used to get the correct bends for legs.
        // Static because it is used from DragControlMode also.
        public static CourseObj[] CreateLegHighlights(EventDB eventDB, PointF newPoint, Id<ControlPoint>controlDrag, ControlPointKind controlKind, Id<CourseControl> courseControlId1, Id<CourseControl> courseControlId2, float courseObjRatio, CourseAppearance appearance)
        {
            List<CourseObj> highlights = new List<CourseObj>();

            if (courseControlId1.IsNotNone) {
                Id<ControlPoint> controlId1 = eventDB.GetCourseControl(courseControlId1).control;
                ControlPoint control1 = eventDB.GetControl(controlId1);
                LegCourseObj highlight = CreateLegHighlight(eventDB, control1.location, control1.kind, controlId1, newPoint, controlKind, controlDrag, courseObjRatio, appearance);
                if (highlight != null)
                    highlights.Add(highlight);
            }

            if (courseControlId2.IsNotNone) {
                Id<ControlPoint> controlId2 = eventDB.GetCourseControl(courseControlId2).control;
                ControlPoint control2 = eventDB.GetControl(controlId2);
                LegCourseObj highlight = CreateLegHighlight(eventDB, newPoint, controlKind, controlDrag, control2.location, control2.kind, controlId2, courseObjRatio, appearance);
                if (highlight != null)
                    highlights.Add(highlight);
            }

            return highlights.ToArray();
        }

        public override bool GetToolTip(Pane pane, PointF location, float pixelSize, out string tipText, out string titleText)
        {
            if (pane == Pane.Map) {
                PointF highlightLocation;
                Id<ControlPoint> existingControl = HitTestPoint(location, pixelSize, out highlightLocation);
                if (existingControl.IsNotNone) {
                    TextPart[] textParts = SelectionDescriber.DescribeControl(symbolDB, eventDB, existingControl);
                    base.ConvertTextPartsToToolTip(textParts, out tipText, out titleText);
                    return true;
                }
                else {
                    tipText = titleText = "";
                    return false;
                }
            }
            else {
                return base.GetToolTip(pane, location, pixelSize, out tipText, out titleText);
            }
        }
    }

    class AddPointSpecialMode: BaseMode
    {
        const int PIXELOFFSETX = -7;         // offset in pixels from the mouse cursor to the cross-hairs of the special to place.
        const int PIXELOFFSETY = 7;

        Controller controller;
        SelectionMgr selectionMgr;
        UndoMgr undoMgr;
        EventDB eventDB;
        SpecialKind specialKind;      // Kind of special we are adding.
        float courseObjRatio;
        CourseAppearance appearance;

        PointCourseObj highlight;    // the highlight we are creating.

        public AddPointSpecialMode(Controller controller, SelectionMgr selectionMgr, UndoMgr undoMgr, EventDB eventDB, SpecialKind specialKind)
        {
            this.controller = controller;
            this.selectionMgr = selectionMgr;
            this.undoMgr = undoMgr;
            this.eventDB = eventDB;
            this.specialKind = specialKind;
            this.appearance = controller.GetCourseAppearance();
            this.courseObjRatio = selectionMgr.ActiveCourseView.CourseObjRatio(appearance);
        }

        public override void BeginMode()
        {
            // Create the initial highlight.
            PointF location;
            float pixelSize;

            if (controller.GetCurrentLocation(out location, out pixelSize)) {
                bool temp = false;
                PointF highlightLocation = new PointF(location.X + PIXELOFFSETX * pixelSize, location.Y + PIXELOFFSETY * pixelSize);
                SetHighlightLocation(highlightLocation, pixelSize, ref temp);
            }
        }

        public override string StatusText
        {
            get
            {
                return StatusBarText.AddingObject;
            }
        }

        public override void MouseMoved(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return;

            PointF highlightLocation = new PointF(location.X + PIXELOFFSETX * pixelSize, location.Y + PIXELOFFSETY * pixelSize);
            SetHighlightLocation(highlightLocation, pixelSize, ref displayUpdateNeeded);
        }

        public override IMapViewerHighlight[] GetHighlights(Pane pane)
        {
            if (pane != Pane.Map)
                return null;

            if (highlight != null)
                return new CourseObj[] { highlight };
            else
                return null;
        }

        public override MapViewer.DragAction LeftButtonDown(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane == Pane.Map) {
                // Delay to see if click or drag.
                return MapViewer.DragAction.DelayedDrag;
            }
            else {
                return MapViewer.DragAction.None;
            }
        }

        public override void LeftButtonDrag(Pane pane, PointF location, PointF locationStart, float pixelSize, ref bool displayUpdateNeeded)
        {
            Debug.Assert(pane == Pane.Map);

            // Drag is move map.
            controller.InitiateMapDragging(locationStart, System.Windows.Forms.MouseButtons.Left);
        }

        public override void LeftButtonClick(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return;

            // Create the new special!

            PointF highlightLocation = new PointF(location.X + PIXELOFFSETX * pixelSize, location.Y + PIXELOFFSETY * pixelSize);

            undoMgr.BeginCommand(1322, CommandNameText.AddObject);

            // Creat the special
            Id<Special> specialId = ChangeEvent.AddPointSpecial(eventDB, specialKind, highlightLocation, 0);

            // select the new special.
            selectionMgr.SelectSpecial(specialId);
            undoMgr.EndCommand(1322);

            controller.DefaultCommandMode();
        }

        // Create the highlight, and put it at the given location.
        // Set displayUpdateNeeded to true if the highlight was just created or was moved.
        void SetHighlightLocation(PointF highlightLocation, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (highlight != null && highlight.location == highlightLocation)
                return;

            // Note, we cannot change this existing highlight because it is needed for erasing.
            switch (specialKind) {
            case SpecialKind.FirstAid:
                highlight = new FirstAidCourseObj(Id<Special>.None, courseObjRatio, appearance, highlightLocation);
                break;
            case SpecialKind.Water:
                highlight = new WaterCourseObj(Id<Special>.None, courseObjRatio, appearance, highlightLocation);
                break;
            case SpecialKind.OptCrossing:
                highlight = new CrossingCourseObj(Id<ControlPoint>.None, Id<CourseControl>.None, Id<Special>.None, courseObjRatio, appearance, 0, 0, highlightLocation);
                break;
            case SpecialKind.Forbidden:
                highlight = new ForbiddenCourseObj(Id<Special>.None, courseObjRatio, appearance, highlightLocation);
                break;
            case SpecialKind.RegMark:
                highlight = new RegMarkCourseObj(Id<Special>.None, courseObjRatio, appearance, highlightLocation);
                break;
            default:
                throw new Exception("bad special kind");
            }

            highlight.location = highlightLocation;
            displayUpdateNeeded = true;
        }
    }

    // Mode to add a line or area special.
    class AddLineAreaSpecialMode: BaseMode
    {
        const float CLOSEDISTANCE = 5F;          // pixel distance to consider closing the polygon.

        Controller controller;
        SelectionMgr selectionMgr;
        UndoMgr undoMgr;
        EventDB eventDB;
        bool isArea;                     // is it an area special?
        Func<PointF[], Id<Special>> createObject; // Function to create the object.
        float courseObjRatio;
        CourseAppearance appearance;

        List<PointF> points = new List<PointF>();      // the list of coordinates in the path we are creating.
        int numberFixedPoints = 0;                          // number of coordinates now fixed in place. 
                                                                            // The last coordinate in points may or may not be fixed depending on this value vs. points.Count.
        BoundaryCourseObj highlight;    // the highlight of the path we are creating.

        public AddLineAreaSpecialMode(Controller controller, SelectionMgr selectionMgr, UndoMgr undoMgr, EventDB eventDB, Func<PointF[], Id<Special>> createObject, bool isArea)
        {
            this.controller = controller;
            this.selectionMgr = selectionMgr;
            this.undoMgr = undoMgr;
            this.eventDB = eventDB;
            this.createObject = createObject;
            this.isArea = isArea;
            this.appearance = controller.GetCourseAppearance();
            this.courseObjRatio = selectionMgr.ActiveCourseView.CourseObjRatio(appearance);
        }

        public override string StatusText
        {
            get
            {
                return StatusBarText.AddingLineArea;
            }
        }

        public override Cursor GetMouseCursor(Pane pane, PointF location, float pixelSize)
        {
            if (pane == Pane.Map) {
                return Cursors.Cross;
            }
            else {
                return Cursors.Arrow;
            }
        }

        public override IMapViewerHighlight[] GetHighlights(Pane pane)
        {
            if (pane == Pane.Map && highlight != null)
                return new CourseObj[] { highlight };
            else
                return null;
        }

        public override MapViewer.DragAction LeftButtonDown(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return MapViewer.DragAction.None;

            if (numberFixedPoints == 0) {
                // The first point. Fix it at the location.
                AddFixedPoint(location);
            }

            displayUpdateNeeded = true;
            return MapViewer.DragAction.DelayedDrag;
        }

        public override void LeftButtonDrag(Pane pane, PointF location, PointF locationStart, float pixelSize, ref bool displayUpdateNeeded)
        {
            Debug.Assert(pane == Pane.Map);

            // In the middle of dragging. Current location isn't fixed yet.
            AddUnfixedPoint(location);
            displayUpdateNeeded = true;
        }

        public override void LeftButtonEndDrag(Pane pane, PointF location, PointF locationStart, float pixelSize, ref bool displayUpdateNeeded)
        {
            Debug.Assert(pane == Pane.Map);

            // If we ended near to the first point, we've create a polygon and creation is done.
            if (numberFixedPoints >= 3 && Geometry.Distance(location, points[0]) < pixelSize * CLOSEDISTANCE) {
                if (!isArea)
                    AddFixedPoint(points[0]);  // area symbols close automatically.

                CreateObject();

                controller.DefaultCommandMode();
                displayUpdateNeeded = true;
            }
            else {
                // Ended dragging. Current location the next location.
                AddFixedPoint(location);
                displayUpdateNeeded = true;
            }
        }

        public override void LeftButtonClick(Pane pane, PointF location, float pixelSize, ref bool displayUpdateNeeded)
        {
            if (pane != Pane.Map)
                return;

            // Left button clicked. Ends creating the item and we're done.
            CreateObject();

            controller.DefaultCommandMode();
            displayUpdateNeeded = true;
        }

        // Should there be a left button click here?

        // Add a new fixed point that never gets changed.
        private void AddFixedPoint(PointF newPoint)
        {
            AddUnfixedPoint(newPoint);
            ++numberFixedPoints;
        }

        // Add or change the final unfixed point.
        private void AddUnfixedPoint(PointF newPoint)
        {
            if (numberFixedPoints > points.Count - 1)
                points.Add(newPoint);
            else
                points[numberFixedPoints] = newPoint;

            if (points.Count >= 2)
                highlight = new BoundaryCourseObj(Id<Special>.None, courseObjRatio, appearance, new SymPath(points.ToArray()));
        }

        // Create the object with the number of fixed points there are, if there are enough. Returns true if object was created, false
        // if no enough points.
        bool CreateObject()
        {
            // Line objects need at least 2 points, area objects need at least 3.
            if (numberFixedPoints < 2 || (isArea && numberFixedPoints < 3))
                return false;

            undoMgr.BeginCommand(1327, CommandNameText.AddObject);

            // Create the special
            Id<Special> specialId = createObject(points.GetRange(0, numberFixedPoints).ToArray());

            // select the new special.
            selectionMgr.SelectSpecial(specialId);
            undoMgr.EndCommand(1327);
            return true;
        }
    }
}
