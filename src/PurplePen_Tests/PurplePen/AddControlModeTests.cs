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

#if TEST
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using PurplePen.MapView;
using PurplePen.MapModel;

using TestingUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PurplePen.Tests
{
    [TestClass]
    public class AddControlModeTests: TestFixtureBase
    {
        TestUI ui;
        Controller controller;
        EventDB eventDB;


        [TestInitialize]
        public void Setup()
        {
            ui = TestUI.Create();
            controller = ui.controller;
            eventDB = controller.GetEventDB();

            string fileName = TestUtil.GetTestFile("modes\\marymoor.coursescribe");

            bool success = controller.LoadInitialFile(fileName, true);
            Assert.IsTrue(success);
        }

        // Add a control to the all controls collection.
        [TestMethod]
        public void AddControlAllControls()
        {
            bool isTooltip;
            string tipText, titleText;
            CourseObj[] highlights;

            // Should be no control #60 now.
            Assert.IsFalse(QueryEvent.IsCodeInUse(eventDB, "60"));

            // Select All Controls.
            controller.SelectTab(0);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            ControlCourseObj obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);

            // Move the mouse somewhere (mouse buttons are up).
            ui.MouseMoved(31, -11, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(30.3F, -10.3F), obj.location);

            // No tooltip.
            isTooltip = controller.GetToolTip(Pane.Map, new PointF(30.3F, -10.3F), 0.3F, out tipText, out titleText);
            Assert.IsFalse(isTooltip);
            Assert.AreEqual("", titleText);
            Assert.AreEqual("", tipText);

            // No tooltip on a control, either
            isTooltip = controller.GetToolTip(Pane.Map, new PointF(52.9F, -28.5F), 0.3F, out tipText, out titleText);
            Assert.IsFalse(isTooltip);
            Assert.AreEqual("", titleText);
            Assert.AreEqual("", tipText);

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(27, -18), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(27, -18), 0.1F);

            // There should be a new control #60, with the given location.
            // Is should be selected.
            Assert.IsTrue(QueryEvent.IsCodeInUse(eventDB, "60"));
            Id<ControlPoint> newControlId = QueryEvent.FindCode(eventDB, "60");
            Assert.AreEqual(new PointF(26.3F, -17.3F), eventDB.GetControl(newControlId).location);

            // The control should be highlighted.
            CheckHighlightedLines(controller, 28, 28);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(newControlId.id, highlights[0].controlId.id);
            Assert.AreEqual(newControlId.id, highlights[1].controlId.id);
        }

        // Add a control to a course. Adds a newly created control point.
        [TestMethod]
        public void AddControlCourse1()
        {
            bool isTooltip;
            string tipText, titleText;
            CourseObj[] highlights;

            // Should be no control #60 now.
            Assert.IsFalse(QueryEvent.IsCodeInUse(eventDB, "60"));

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            ControlCourseObj obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.2,32.19)--N(19.61,36.83)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(24.16,35.58)--N(38.24,19.52)",
                                        highlights[2].ToString());
            Assert.AreEqual(StatusBarText.AddingControl, controller.StatusText);
            
            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Move the mouse over another control.
            ui.MouseMoved(21, 40, 0.1F);

            // There should be a highlight exactly on control #48.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(21.5F, 40.2F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.07,32.52)--N(18.94,39)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(23.29,38.01)--N(38.31,19.59)",
                                         highlights[2].ToString());
            Assert.AreEqual(string.Format(StatusBarText.AddingExistingControl, "48"), controller.StatusText);

            // Tooltip about control #48
            isTooltip = controller.GetToolTip(Pane.Map, new PointF(21.5F, 40.2F), 0.3F, out tipText, out titleText);
            Assert.IsTrue(isTooltip);
            Assert.AreEqual("Control 48", titleText);
            Assert.AreEqual("Used in: Course 4B, Course 4G, Course 5", tipText);

            // No tooltip on a leg, though
            isTooltip = controller.GetToolTip(Pane.Map, new PointF(66.9F, 18.9F), 0.3F, out tipText, out titleText);
            Assert.IsFalse(isTooltip);
            Assert.AreEqual("", titleText);
            Assert.AreEqual("", tipText);

            // Move the mouse over a control on the course next to current control (mouse buttons are up). Shouldn't select that control, but near it.
            ui.MouseMoved(40, 15.5F, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(39.3F, 16.2F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.12,30.25)--N(36.69,17.27)",
                                        highlights[1].ToString());
            Assert.AreEqual(StatusBarText.AddingControl, controller.StatusText);

            // Mouse down somewhere.
            MapViewer.DragAction action = ui.LeftButtonDown(29, 30, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(29, 30), 0.1F);

            // There should be a new control #60, with the given location.
            // Is should be selected.
            Assert.IsTrue(QueryEvent.IsCodeInUse(eventDB, "60"));
            Id<ControlPoint> newControlId = QueryEvent.FindCode(eventDB, "60");
            Assert.AreEqual(new PointF(28.3F, 30.7F), eventDB.GetControl(newControlId).location);

            // The control should be on this course, right before course control 307.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), newControlId));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), newControlId)[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl == CourseControlId(307));
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(306)).nextCourseControl == newCourseControlId);

            // The control should be highlighted.
            CheckHighlightedLines(controller, 8, 8);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(newControlId.id, highlights[0].controlId.id);
            Assert.AreEqual(newControlId.id, highlights[1].controlId.id);
            Assert.AreEqual(StatusBarText.DragObject, controller.StatusText);
        }

        // Add a control to a course. Adds an existing control point.
        [TestMethod]
        public void AddControlCourse2()
        {
            CourseObj[] highlights;

            // Should be no control #60 now.
            Assert.IsFalse(QueryEvent.IsCodeInUse(eventDB, "60"));

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);

            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Mouse down on control #48.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(21, 40), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(21, 40), 0.1F);

            // There should not be a new control #60.
            Assert.IsFalse(QueryEvent.IsCodeInUse(eventDB, "60"));

            // The control should be on this course, right before course control 307.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), ControlId(48)));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), ControlId(48))[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl == CourseControlId(307));
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(306)).nextCourseControl == newCourseControlId);

            // The control should be highlighted.
            CheckHighlightedLines(controller, 8, 8);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(48, highlights[0].controlId.id);
            Assert.AreEqual(48, highlights[1].controlId.id);
        }

        // Add a control to a course. to make a butterfly course.
        [TestMethod]
        public void AddControlCourseButterfly()
        {
            CourseObj[] highlights;

            // Should be no control #60 now.
            Assert.IsFalse(QueryEvent.IsCodeInUse(eventDB, "60"));

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            ControlCourseObj obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.2,32.19)--N(19.61,36.83)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(24.16,35.58)--N(38.24,19.52)",
                                        highlights[2].ToString());
            Assert.AreEqual(StatusBarText.AddingControl, controller.StatusText);

            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Move the mouse over another control on the course
            ui.MouseMoved(28, 8, 0.1F);

            // There should be a highlight exactly on control #41.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(28F, 6.2F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(4.52,29.34)--N(25.99,8.18)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(30.07,8.12)--N(38.03,15.48)",
                                         highlights[2].ToString());
            Assert.AreEqual(string.Format(StatusBarText.AddingExistingControl, "41"), controller.StatusText);

            // Mouse down somewhere.
            MapViewer.DragAction action = ui.LeftButtonDown(28, 8, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(28, 8), 0.1F);

            // There should not be a new control #60.
            Assert.IsFalse(QueryEvent.IsCodeInUse(eventDB, "60"));

            // The control should be on this course, right before course control 307.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), ControlId(41)));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), ControlId(41))[1];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl == CourseControlId(307));
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(306)).nextCourseControl == newCourseControlId);

            // The control should be highlighted.
            CheckHighlightedLines(controller, 8, 8);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(41, highlights[0].controlId.id);
            Assert.AreEqual(41, highlights[1].controlId.id);
        }

        // Checkes cancelling and status text for adding a control.
        [TestMethod]
        public void CancelAddControl()
        {
            CourseObj[] highlights;

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = ui.LeftButtonDown(0.9F, 30.5F, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);
            
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Normal, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            ControlCourseObj obj = (ControlCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.2,32.19)--N(19.61,36.83)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(24.16,35.58)--N(38.24,19.52)",
                                        highlights[2].ToString());
            Assert.AreEqual(StatusBarText.AddingControl, controller.StatusText);

            // Cancel the mode
            Assert.IsTrue(controller.CanCancelMode());
            ui.MouseMoved(29, 41, 0.1f);
            controller.CancelMode();
            Assert.AreEqual(StatusBarText.DefaultStatus, controller.StatusText);

            // Highlight should return.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);
        }

        // Add a start to the all controls collection.
        [TestMethod]
        public void AddStartAllControls()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(0);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a start.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Start, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            PointCourseObj obj = (PointCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.IsInstanceOfType(   obj,    typeof(StartCourseObj));

            // Move the mouse somewhere (mouse buttons are up).
            ui.MouseMoved(31, -11, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (PointCourseObj) highlights[0];
            Assert.AreEqual(new PointF(30.3F, -10.3F), obj.location);
            Assert.IsInstanceOfType(   obj,   typeof(StartCourseObj));

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(27, -18), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(27, -18), 0.1F);

            // There should be a new start control, with the given location.
            // Is should be selected.
            // The control should be highlighted.
            CheckHighlightedLines(controller, 3, 3);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Id<ControlPoint> newControlId = highlights[0].controlId;
            Assert.AreEqual(new PointF(26.3F, -17.3F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(ControlPointKind.Start, eventDB.GetControl(newControlId).kind);
        }

        // Add a control to a course. Adds a newly created control point.
        [TestMethod]
        public void AddStartCourse1()
        {
            CourseObj[] highlights;

            // Create a new start control, so we have an extra.
            controller.GetUndoMgr().BeginCommand(667, "Add start");
            ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Start, null, new PointF(72.2F, 6.1F), 50);
            controller.GetUndoMgr().EndCommand(667);

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Start, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            StartCourseObj obj = (StartCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.AreEqual(StatusBarText.AddingStart, controller.StatusText);

            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Move the mouse over another control.
            ui.MouseMoved(69.8F, 5, 0.1F);

            // There should be a highlight exactly on the start.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (StartCourseObj) highlights[0];
            Assert.AreEqual(new PointF(72.2F, 6.1F), obj.location);
            Assert.AreEqual(string.Format(StatusBarText.AddingExistingStart), controller.StatusText);

            // Move the mouse over a control on the course (mouse buttons are up). Shouldn't select that control, but near it.
            ui.MouseMoved(56, 25, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (StartCourseObj) highlights[0];
            Assert.AreEqual(new PointF(55.3F, 25.7F), obj.location);
            Assert.AreEqual(StatusBarText.AddingStart, controller.StatusText);

            // Mouse down somewhere.
            MapViewer.DragAction action = ui.LeftButtonDown(29, 30, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(29, 30), 0.1F);

            // There should be a new start control, with the given location.
            // It should be selected.
            CheckHighlightedLines(controller, 2, 2);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Id<ControlPoint> newControlId = highlights[0].controlId;
            Assert.AreEqual(ControlPointKind.Start, eventDB.GetControl(newControlId).kind);
            Assert.AreEqual(new PointF(28.3F, 30.7F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(StatusBarText.DragObject, controller.StatusText);

            // The control should be on this course, at the start.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), newControlId));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), newControlId)[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl == CourseControlId(302));
            Assert.IsTrue(eventDB.GetCourse(CourseId(3)).firstCourseControl == newCourseControlId);
        }

        // Add a start to a course. Adds an existing start
        [TestMethod]
        public void AddStartCourse2()
        {
            CourseObj[] highlights;

            // Create a new start control, so we have an extra.
            controller.GetUndoMgr().BeginCommand(667, "Add start");
            Id<ControlPoint> newControlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Start, null, new PointF(72.2F, 6.1F), 50);
            controller.GetUndoMgr().EndCommand(667);

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            controller.BeginAddControlMode(ControlPointKind.Start, MapExchangeType.None);

            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Mouse down on new start.
            MapViewer.DragAction action = ui.LeftButtonDown(69.8F, 5, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(69.8F, 5), 0.1F);

            CheckHighlightedLines(controller, 2, 2);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Assert.AreEqual(newControlId, highlights[0].controlId);
            Assert.AreEqual(new PointF(72.2F, 6.1F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(StatusBarText.DragObject, controller.StatusText);

            // The control should be on this course, at the start.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), newControlId));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), newControlId)[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl == CourseControlId(302));
            Assert.IsTrue(eventDB.GetCourse(CourseId(3)).firstCourseControl == newCourseControlId);
        }

        // Add a finish to the all controls collection.
        [TestMethod]
        public void AddFinishAllControls()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(0);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[])controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a finish.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Finish, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            PointCourseObj obj = (PointCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.IsInstanceOfType(   obj,   typeof(FinishCourseObj));

            // Move the mouse somewhere (mouse buttons are up).
            ui.MouseMoved(31, -11, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (PointCourseObj) highlights[0];
            Assert.AreEqual(new PointF(30.3F, -10.3F), obj.location);
            Assert.IsInstanceOfType(   obj,   typeof(FinishCourseObj));

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(27, -18), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(27, -18), 0.1F);

            // There should be a new finish control, with the given location.
            // Is should be selected.
            // The control should be highlighted.
            CheckHighlightedLines(controller, 40, 40);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Id<ControlPoint> newControlId = highlights[0].controlId;
            Assert.AreEqual(new PointF(26.3F, -17.3F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(ControlPointKind.Finish, eventDB.GetControl(newControlId).kind);
        }

        // Add a control to a course. Adds a newly created control point.
        [TestMethod]
        public void AddFinishCourse1()
        {
            CourseObj[] highlights;

            // Create a new finish control, so we have an extra.
            controller.GetUndoMgr().BeginCommand(667, "Add finish");
            Id<ControlPoint> controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Finish, null, new PointF(72.2F, 6.1F), 50);
            ChangeEvent.ChangeDescriptionSymbol(eventDB, controlId, 0, "14.1");  
            controller.GetUndoMgr().EndCommand(667);

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.Finish, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            FinishCourseObj obj = (FinishCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.AreEqual(StatusBarText.AddingFinish, controller.StatusText);

            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Move the mouse over another control.
            ui.MouseMoved(69.8F, 5, 0.1F);

            // There should be a highlight exactly on the finish.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (FinishCourseObj) highlights[0];
            Assert.AreEqual(new PointF(72.2F, 6.1F), obj.location);
            Assert.AreEqual(string.Format(StatusBarText.AddingExistingFinish), controller.StatusText);

            // Move the mouse over a control on the course (mouse buttons are up). Shouldn't select that control, but near it.
            ui.MouseMoved(56, 25, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (FinishCourseObj) highlights[0];
            Assert.AreEqual(new PointF(55.3F, 25.7F), obj.location);
            Assert.AreEqual(StatusBarText.AddingFinish, controller.StatusText);

            // Mouse down somewhere.
            MapViewer.DragAction action = ui.LeftButtonDown(29, 30, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(29, 30), 0.1F);

            // There should be a new finish control, with the given location.
            // It should be selected.
            CheckHighlightedLines(controller, 16, 16);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Id<ControlPoint> newControlId = highlights[0].controlId;
            Assert.AreEqual(ControlPointKind.Finish, eventDB.GetControl(newControlId).kind);
            Assert.AreEqual(new PointF(28.3F, 30.7F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(StatusBarText.DragObject, controller.StatusText);

            // The control should be on this course, at the finish.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), newControlId));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), newControlId)[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl.IsNone);
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(314)).nextCourseControl == newCourseControlId);
        }

        // Add a finish to a course. Adds an existing finish
        [TestMethod]
        public void AddFinishCourse2()
        {
            CourseObj[] highlights;

            // Create a new finish control, so we have an extra.
            controller.GetUndoMgr().BeginCommand(667, "Add finish");
            Id<ControlPoint> newControlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Finish, null, new PointF(72.2F, 6.1F), 50);
            ChangeEvent.ChangeDescriptionSymbol(eventDB, newControlId, 0, "14.1");
            controller.GetUndoMgr().EndCommand(667);

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a control.
            controller.BeginAddControlMode(ControlPointKind.Finish, MapExchangeType.None);

            // All controls should be displayed.
            Assert.IsTrue(ControllerTests.IsAllControlsLayer(controller.GetCourseLayout()));

            // Mouse down on new finish.
            MapViewer.DragAction action = ui.LeftButtonDown(69.8F, 5, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(69.8F, 5), 0.1F);

            CheckHighlightedLines(controller, 16, 16);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Assert.AreEqual(newControlId, highlights[0].controlId);
            Assert.AreEqual(new PointF(72.2F, 6.1F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(StatusBarText.DragObject, controller.StatusText);

            // The control should be on this course, at the finish.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), newControlId));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), newControlId)[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl.IsNone);
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(314)).nextCourseControl == newCourseControlId);
        }


        // Add a point special to the all controls collection.
        [TestMethod]
        public void AddPointSpecialAllControls()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(0);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a point special.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddPointSpecialMode(SpecialKind.Water);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            WaterCourseObj obj = (WaterCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);

            // Move the mouse somewhere (mouse buttons are up).
            ui.MouseMoved(31, -11, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (WaterCourseObj) highlights[0];
            Assert.AreEqual(new PointF(30.3F, -10.3F), obj.location);

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(27, -18), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(27, -18), 0.1F);

            // There should be a water special, with the given location.
            // Is should be selected.
            int countWaters = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Water) {
                    ++countWaters;
                    Assert.AreEqual(new PointF(26.3F, -17.3F), special.locations[0]);
                }
            }
            Assert.AreEqual(1, countWaters);
            
            // The special should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            obj = (WaterCourseObj) highlights[0];
            Assert.AreEqual(new PointF(26.3F, -17.3F), obj.location);
        }

        // Add a control description to one course..
        [TestMethod]
        public void AddControlDescription()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(3);

            // Begin adding a description.
            controller.BeginAddDescriptionMode();

            // Check the status text.
            Assert.AreEqual(StatusBarText.AddingDescription, controller.StatusText);

            // Click the mouse and drag.
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(23, 37), 0.1F));
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(23, 37), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.ImmediateDrag, action);
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(23, 37), 0.1F));

            controller.LeftButtonDrag(Pane.Map, new PointF(34, 12), new PointF(23, 37), 0.1F);

            // Check the highlights.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            DescriptionCourseObj obj = (DescriptionCourseObj) highlights[0];
            Assert.AreEqual(23, obj.rect.Left);
            Assert.AreEqual(37, obj.rect.Bottom);
            Assert.AreEqual(34.84F, obj.rect.Right, 0.01F);
            Assert.AreEqual(12, obj.rect.Top);

            // Finish the drag.
            controller.LeftButtonEndDrag(Pane.Map, new PointF(36, 11), new PointF(23, 37), 0.1F);

            // There should be a description, with the given location.
            // Is should be selected.
            int countDescriptions = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Descriptions) {
                    ++countDescriptions;
                    Assert.AreEqual(23, special.locations[0].X);
                    Assert.AreEqual(37, special.locations[0].Y);
                    Assert.AreEqual(24.5235F, special.locations[1].X, 0.01F);
                    Assert.AreEqual(37, special.locations[1].Y);
                    Assert.IsFalse(special.allCourses);
                    Assert.AreEqual(1, special.courses.Length);
                    Assert.AreEqual(Designator(3), special.courses[0]);
                }
            }
            Assert.AreEqual(1, countDescriptions);

            // The descriiption should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Assert.IsInstanceOfType(   highlights[0],   typeof(DescriptionCourseObj));
        }

        // Add a control description to all controls.
        [TestMethod]
        public void AddControlDescription2()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(0);

            // Begin adding a description.
            controller.BeginAddDescriptionMode();

            // Check the status text.
            Assert.AreEqual(StatusBarText.AddingDescription, controller.StatusText);

            // Click the mouse and drag.
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(10, -70), 0.1F));
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(10, -70), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.ImmediateDrag, action);
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(10, -70), 0.1F));

            controller.LeftButtonDrag(Pane.Map, new PointF(130, -100), new PointF(10, -70), 0.1F);

            // Check the highlights.
            highlights = (CourseObj[])controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            DescriptionCourseObj obj = (DescriptionCourseObj)highlights[0];
            Assert.AreEqual(10, obj.rect.Left);
            Assert.AreEqual(-70, obj.rect.Bottom);
            Assert.AreEqual(130F, obj.rect.Right, 0.01F);
            Assert.AreEqual(-105.75, obj.rect.Top, 0.01F);

            // Finish the drag.
            controller.LeftButtonEndDrag(Pane.Map, new PointF(130, -100), new PointF(10, -70), 0.1F);

            // There should be a description, with the given location.
            // Is should be selected.
            int countDescriptions = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Descriptions) {
                    ++countDescriptions;
                    Assert.AreEqual(10, special.locations[0].X);
                    Assert.AreEqual(-70, special.locations[0].Y);
                    Assert.AreEqual(13.575F, special.locations[1].X, 0.01F);
                    Assert.AreEqual(-70, special.locations[1].Y);
                    Assert.AreEqual(4, special.fragments[0].numColumns);
                    Assert.IsFalse(special.allCourses);
                    Assert.AreEqual(1, special.courses.Length);
                    Assert.AreEqual(CourseDesignator.AllControls, special.courses[0]);
                }
            }
            Assert.AreEqual(1, countDescriptions);

            // The descriiption should be highlighted.
            highlights = (CourseObj[])controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Assert.IsInstanceOfType(highlights[0], typeof(DescriptionCourseObj));
        }


        // Add a mandatory crossing point to a course. Adds a newly created crossing point.
        [TestMethod]
        public void AddMandatoryCrossingPoint()
        {
            CourseObj[] highlights;

            // Select a course.
            controller.SelectTab(3);

            // Select control #47
            var dragAction = controller.LeftButtonDown(Pane.Map, new PointF(0.9F, 30.5F), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, dragAction);
            controller.LeftButtonClick(Pane.Map, new PointF(0.9F, 30.5F), 0.3F);

            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(2, highlights.Length);
            Assert.AreEqual(47, highlights[0].controlId.id);
            Assert.AreEqual(47, highlights[1].controlId.id);

            // Begin adding a crossing point.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddControlMode(ControlPointKind.CrossingPoint, MapExchangeType.None);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            CrossingCourseObj obj = (CrossingCourseObj) highlights[0];
            Assert.AreEqual(new PointF(22.3F, 37.7F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.2,32.19)--N(19.92,36.93)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(23.95,35.82)--N(38.24,19.52)",
                                        highlights[2].ToString());
            Assert.AreEqual(StatusBarText.AddingCrossingPoint, controller.StatusText);

            // Move the mouse over a control on the course (mouse buttons are up). Shouldn't select that control, but near it.
            ui.MouseMoved(56, 25, 0.1F);

            // There should be a highlight near the mouse.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(3, highlights.Length);
            obj = (CrossingCourseObj) highlights[0];
            Assert.AreEqual(new PointF(55.3F, 25.7F), obj.location);
            Assert.AreEqual("Leg:            control:47  scale:1  path:N(5.32,31.02)--N(52.81,25.96)",
                                        highlights[1].ToString());
            Assert.AreEqual("Leg:            scale:1  path:N(53.11,24.5)--N(42.58,18.75)",
                                        highlights[2].ToString());
            Assert.AreEqual(StatusBarText.AddingCrossingPoint, controller.StatusText);

            // Mouse down somewhere.
            MapViewer.DragAction action = ui.LeftButtonDown(29, 30, 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            controller.LeftButtonClick(Pane.Map, new PointF(29, 30), 0.1F);

            // There should be a new mandatory, with the given location.
            // Is should be selected.
            // The control should be highlighted.
            CheckHighlightedLines(controller, 8, 8);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Id<ControlPoint> newControlId = highlights[0].controlId;
            Assert.AreEqual(StatusBarText.DragObject, controller.StatusText);

            Assert.AreEqual(new PointF(28.3F, 30.7F), eventDB.GetControl(newControlId).location);
            Assert.AreEqual(ControlPointKind.CrossingPoint, eventDB.GetControl(newControlId).kind);

            // The control should be on this course, right before course control 307.
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(3), newControlId));
            Id<CourseControl> newCourseControlId = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(3), newControlId)[0];
            Assert.IsTrue(eventDB.GetCourseControl(newCourseControlId).nextCourseControl == CourseControlId(307));
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(306)).nextCourseControl == newCourseControlId);
        }


        // Add a area special to the all controls collection.
        [TestMethod]
        public void AddAreaSpecialAllControls()
        {
            CourseObj[] highlights;
            PointF[] locations = { new PointF(28, -18), new PointF(34, 7), new PointF(14, 19), new PointF(12, -19) };

            // Select All Controls.
            controller.SelectTab(0);

            // Begin adding an area special. No highlight yet.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Dangerous, true);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, locations[0], 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);

            // No highlight yet!
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Drag to each of the other locations in turn.
            for (int i = 1; i <= 3; ++i) {
                controller.LeftButtonEndDrag(Pane.Map, locations[i], new PointF(17, -8), 0.1F);
                // Check the highlight.
                highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
                Assert.AreEqual(1, highlights.Length);
                Assert.IsInstanceOfType(   highlights[0],   typeof(LineCourseObj));
                SymPath path = ((LineCourseObj)highlights[0]).path;
                PointF[] pts = path.Points;
                for (int j = 0; j <= i; ++j) {
                    Assert.AreEqual(locations[j], pts[j]);
                }

                action = controller.LeftButtonDown(Pane.Map, new PointF(17, -8), 0.1F);
                Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            }

            // Now click the mouse to end the path.
            controller.LeftButtonClick(Pane.Map, new PointF(17, -8), 0.1F);

            // There should be a dangerous special, with the given location.
            // Is should be selected.
            int countDangerous = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Dangerous) {
                    ++countDangerous;
                    Assert.AreEqual(locations.Length, special.locations.Length);
                    for (int j = 0; j < locations.Length; ++j ) {
                        Assert.AreEqual(locations[j], special.locations[j]);
                    }
                }
            }
            Assert.AreEqual(1, countDangerous);

            // The special should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            DangerousCourseObj obj = (DangerousCourseObj) highlights[0];
        }

        // Add a area special by cloing the polygon instead of clicking to end.
        [TestMethod]
        public void AddAreaSpecialClosed()
        {
            CourseObj[] highlights;
            PointF[] locations = { new PointF(28, -18), new PointF(34, 7), new PointF(14, 19), new PointF(12, -19) };

            // Select All Controls.
            controller.SelectTab(0);

            // Begin adding an area special. No highlight yet.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Dangerous, true);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, locations[0], 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);

            // No highlight yet!
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Drag to each of the other locations in turn.
            for (int i = 1; i <= 3; ++i) {
                controller.LeftButtonEndDrag(Pane.Map, locations[i], new PointF(17, -8), 0.1F);
                // Check the highlight.
                highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
                Assert.AreEqual(1, highlights.Length);
                Assert.IsInstanceOfType(   highlights[0],   typeof(LineCourseObj));
                SymPath path = ((LineCourseObj) highlights[0]).path;
                PointF[] pts = path.Points;
                for (int j = 0; j <= i; ++j) {
                    Assert.AreEqual(locations[j], pts[j]);
                }

                action = controller.LeftButtonDown(Pane.Map, new PointF(17, -8), 0.1F);
                Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            }

            // Now drag to close the beginning to end it..
            controller.LeftButtonEndDrag(Pane.Map, new PointF(28.2F, -18.1F), new PointF(17, -8), 0.1F);

            // There should be a dangerous special, with the given location.
            // Is should be selected.
            int countDangerous = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Dangerous) {
                    ++countDangerous;
                    Assert.AreEqual(locations.Length, special.locations.Length);
                    for (int j = 0; j < locations.Length; ++j) {
                        Assert.AreEqual(locations[j], special.locations[j]);
                    }
                }
            }
            Assert.AreEqual(1, countDangerous);

            // The special should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            DangerousCourseObj obj = (DangerousCourseObj) highlights[0];
        }

        // Add a line special by closing the polygon
        [TestMethod]
        public void AddLineSpecialClosed()
        {
            CourseObj[] highlights;
            PointF[] locations = { new PointF(28, -18), new PointF(34, 7), new PointF(14, 19), new PointF(12, -19) };

            // Select a course.
            controller.SelectTab(3);

            // Begin adding an area special. No highlight yet.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Boundary, false);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, locations[0], 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);

            // No highlight yet!
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Drag to each of the other locations in turn.
            for (int i = 1; i <= 3; ++i) {
                controller.LeftButtonEndDrag(Pane.Map, locations[i], new PointF(17, -8), 0.1F);
                // Check the highlight.
                highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
                Assert.AreEqual(1, highlights.Length);
                Assert.IsInstanceOfType(    highlights[0],    typeof(LineCourseObj));
                SymPath path = ((LineCourseObj)highlights[0]).path;
                PointF[] pts = path.Points;
                for (int j = 0; j <= i; ++j) {
                    Assert.AreEqual(locations[j], pts[j]);
                }

                action = controller.LeftButtonDown(Pane.Map, new PointF(17, -8), 0.1F);
                Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            }

            // Now drag to close the beginning to end it..
            controller.LeftButtonEndDrag(Pane.Map, new PointF(28.2F, -18.1F), new PointF(17, -8), 0.1F);

            // There should be a dangerous special, with the given location.
            // Is should be selected.
            int countBoundary = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Boundary) {
                    ++countBoundary;
                    Assert.AreEqual(locations.Length + 1, special.locations.Length);
                    for (int j = 0; j < locations.Length; ++j ) {
                        Assert.AreEqual(locations[j], special.locations[j]);
                    }
                    Assert.AreEqual(locations[0], special.locations[special.locations.Length - 1]);
                }
            }
            Assert.AreEqual(1, countBoundary);

            // The special should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            BoundaryCourseObj obj = (BoundaryCourseObj) highlights[0];
        }

        // Add a line special 
        [TestMethod]
        public void AddLineSpecial()
        {
            CourseObj[] highlights;
            PointF[] locations = { new PointF(28, -18), new PointF(34, 7), new PointF(14, 19), new PointF(12, -19) };

            // Select a course.
            controller.SelectTab(3);

            // Begin adding an area special. No highlight yet.
            ui.MouseMoved(23, 37, 0.1f);
            controller.BeginAddLineOrAreaSpecialMode(SpecialKind.Boundary, false);
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Mouse down somewhere.
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, locations[0], 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);

            // No highlight yet!
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.IsNull(highlights);
            Assert.AreEqual(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(32, 37), 0.1F));

            // Drag to each of the other locations in turn.
            for (int i = 1; i <= 3; ++i) {
                controller.LeftButtonEndDrag(Pane.Map, locations[i], new PointF(17, -8), 0.1F);
                // Check the highlight.
                highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
                Assert.AreEqual(1, highlights.Length);
                Assert.IsInstanceOfType(    highlights[0],    typeof(LineCourseObj));
                SymPath path = ((LineCourseObj) highlights[0]).path;
                PointF[] pts = path.Points;
                for (int j = 0; j <= i; ++j) {
                    Assert.AreEqual(locations[j], pts[j]);
                }

                action = controller.LeftButtonDown(Pane.Map, new PointF(17, -8), 0.1F);
                Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            }

            // Now click the mouse to end the path.
            controller.LeftButtonClick(Pane.Map, new PointF(17, -8), 0.1F);

            // There should be a dangerous special, with the given location.
            // Is should be selected.
            int countBoundary = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Boundary) {
                    ++countBoundary;
                    Assert.AreEqual(locations.Length, special.locations.Length);
                    for (int j = 0; j < locations.Length; ++j) {
                        Assert.AreEqual(locations[j], special.locations[j]);
                    }
                }
            }
            Assert.AreEqual(1, countBoundary);

            // The special should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            BoundaryCourseObj obj = (BoundaryCourseObj) highlights[0];
        }

        // Add a text space to one course..
        [TestMethod]
        public void AddTextSpecial()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(3);

            // Begin adding a description.
            controller.BeginAddTextSpecialMode("Course: $(CourseName)", "Arial", true, false, SpecialColor.UpperPurple, -1);

            // Check the status text.
            Assert.AreEqual(StatusBarText.AddingText, controller.StatusText);

            // Click the mouse and drag.
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(23, 37), 0.1F));
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(23, 37), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(23, 37), 0.1F));

            controller.LeftButtonDrag(Pane.Map, new PointF(74, 12), new PointF(23, 37), 0.1F);

            // Check the highlights.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            BasicTextCourseObj obj = (BasicTextCourseObj) highlights[0];
            Assert.AreEqual(23F, obj.GetHighlightBounds().Left, 0.01F);
            Assert.AreEqual(37F, obj.GetHighlightBounds().Bottom, 0.01F);
            Assert.AreEqual(74F, obj.GetHighlightBounds().Right, 0.01F);
            Assert.AreEqual(12F, obj.GetHighlightBounds().Top, 0.01F);
            Assert.AreEqual("Course: Course 3", obj.text);
            Assert.AreEqual("Arial", obj.fontName);
            Assert.AreEqual(FontStyle.Bold, obj.fontStyle);

            // Finish the drag.
            controller.LeftButtonEndDrag(Pane.Map, new PointF(76, 11), new PointF(23, 37), 0.1F);

            // There should be a text special, with the given location.
            // Is should be selected.
            int countTextSpecials = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Text) {
                    ++countTextSpecials;
                    Assert.AreEqual(23F, special.locations[0].X, 0.01F);
                    Assert.AreEqual(37F, special.locations[0].Y, 0.01F);
                    Assert.AreEqual(76F, special.locations[1].X, 0.01F);
                    Assert.AreEqual(11F, special.locations[1].Y, 0.01F);
                    Assert.IsTrue(special.allCourses);
                    Assert.AreEqual("Course: $(CourseName)", special.text);
                    Assert.AreEqual("Arial", special.fontName);
                    Assert.IsTrue(special.fontBold);
                    Assert.IsFalse(special.fontItalic);
                }
            }
            Assert.AreEqual(1, countTextSpecials);

            // The text special should be highlighted.
            highlights = (CourseObj[]) controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Assert.IsInstanceOfType(highlights[0], typeof(BasicTextCourseObj));
        }

        [TestMethod]
        public void AddImageSpecial()
        {
            CourseObj[] highlights;

            // Select All Controls.
            controller.SelectTab(3);

            // Begin adding a description.
            controller.BeginAddImageSpecialMode(TestUtil.GetTestFile("coursesymbols\\mrsneeze.jpg"));

            // Check the status text.
            Assert.AreEqual(StatusBarText.AddingRectangle, controller.StatusText);

            // Click the mouse and drag.
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(23, 37), 0.1F));
            MapViewer.DragAction action = controller.LeftButtonDown(Pane.Map, new PointF(23, 37), 0.1F);
            Assert.AreEqual(MapViewer.DragAction.DelayedDrag, action);
            Assert.AreSame(Cursors.Cross, controller.GetMouseCursor(Pane.Map, new PointF(23, 37), 0.1F));

            controller.LeftButtonDrag(Pane.Map, new PointF(74, 12), new PointF(23, 37), 0.1F);

            // Check the highlights.
            highlights = (CourseObj[])controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            ImageCourseObj obj = (ImageCourseObj)highlights[0];
            Assert.AreEqual(23.1F, obj.GetHighlightBounds().Left, 0.01F);
            Assert.AreEqual(37.154F, obj.GetHighlightBounds().Bottom, 0.01F);
            Assert.AreEqual(39.39579F, obj.GetHighlightBounds().Right, 0.01F);
            Assert.AreEqual(12F, obj.GetHighlightBounds().Top, 0.01F);
            Assert.AreEqual("mrsneeze.jpg", obj.imageName);
            Assert.IsNotNull(obj.imageBitmap);

            // Finish the drag.
            controller.LeftButtonEndDrag(Pane.Map, new PointF(76, 11), new PointF(23, 37), 0.1F);

            // There should be a image special, with the given location.
            // Is should be selected.
            int countImageSpecials = 0;
            EventDB eventDB = controller.GetEventDB();
            foreach (Special special in eventDB.AllSpecials) {
                if (special.kind == SpecialKind.Image) {
                    ++countImageSpecials;
                    Assert.AreEqual(23.1F, special.locations[0].X, 0.01F);
                    Assert.AreEqual(37.154F, special.locations[0].Y, 0.01F);
                    Assert.AreEqual(40.04362F, special.locations[1].X, 0.01F);
                    Assert.AreEqual(11F, special.locations[1].Y, 0.01F);
                    Assert.IsTrue(special.allCourses);
                    Assert.AreEqual("mrsneeze.jpg", special.text);
                }
            }
            Assert.AreEqual(1, countImageSpecials);

            // The text special should be highlighted.
            highlights = (CourseObj[])controller.GetHighlights(Pane.Map);
            Assert.AreEqual(1, highlights.Length);
            Assert.IsInstanceOfType(highlights[0], typeof(ImageCourseObj));
        }


    }

}

#endif //TEST
