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
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestingUtils;
using PurplePen.Graphics2D;

namespace PurplePen.Tests
{
    using PurplePen.MapModel;
    using System.Drawing.Drawing2D;
    using System.Windows.Media;
    using Geometry = PurplePen.Graphics2D.Geometry;

    [TestClass]
    public class ChangeEventTests : TestFixtureBase
    {
        UndoMgr undomgr;
        EventDB eventDB;

        private void Setup(string filename)
        {
            undomgr = new UndoMgr(10);
            eventDB = new EventDB(undomgr);

            eventDB.Load(TestUtil.GetTestFile(filename));
            eventDB.Validate();
        }

        [TestMethod]
        public void ChangeDescriptionSymbol()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(108, "Change symbol");

            ChangeEvent.ChangeDescriptionSymbol(eventDB, ControlId(19), 4, "11.12");
            ChangeEvent.ChangeDescriptionSymbol(eventDB, ControlId(20), 3, "10.2");
            ChangeEvent.ChangeDescriptionSymbol(eventDB, ControlId(1), 2, null);
            ChangeEvent.ChangeDescriptionSymbol(eventDB, ControlId(6), 0, "14.1");
            ChangeEvent.ChangeDescriptionSymbol(eventDB, ControlId(2), 3, null);

            undomgr.EndCommand(108);

            ControlPoint control;

            control = eventDB.GetControl(ControlId(19));
            Assert.AreEqual("11.12", control.symbolIds[4]);
            control = eventDB.GetControl(ControlId(20));
            Assert.AreEqual("10.2", control.symbolIds[3]);
            Assert.IsNull(control.columnFText);
            control = eventDB.GetControl(ControlId(1));
            Assert.IsNull(control.symbolIds[2]);
            control = eventDB.GetControl(ControlId(6));
            Assert.AreEqual("14.1", control.symbolIds[0]);
            control = eventDB.GetControl(ControlId(2));
            Assert.IsNull(control.symbolIds[3]);
            Assert.IsNull(control.columnFText);

            eventDB.Validate();

            undomgr.Undo();
            control = eventDB.GetControl(ControlId(19));
            Assert.IsNull(control.symbolIds[4]);
            control = eventDB.GetControl(ControlId(20));
            Assert.IsNull(control.symbolIds[3]);
            Assert.AreEqual("6x7", control.columnFText);
            control = eventDB.GetControl(ControlId(1));
            Assert.AreEqual("8.5", control.symbolIds[2]);
            control = eventDB.GetControl(ControlId(6));
            Assert.AreEqual("14.2", control.symbolIds[0]);
            control = eventDB.GetControl(ControlId(2));
            Assert.IsNull(control.symbolIds[3]);
            Assert.AreEqual("2m", control.columnFText);


        }

        [TestMethod]
        public void ChangeColumnFText()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(109, "Change symbol");

            ChangeEvent.ChangeColumnFText(eventDB, ControlId(2), "3/4");
            ChangeEvent.ChangeColumnFText(eventDB, ControlId(4), "9");
            ChangeEvent.ChangeColumnFText(eventDB, ControlId(9), null);
            ChangeEvent.ChangeColumnFText(eventDB, ControlId(18), "");
            ChangeEvent.ChangeColumnFText(eventDB, ControlId(13), null);
            ChangeEvent.ChangeColumnFText(eventDB, ControlId(12), "7.5");

            undomgr.EndCommand(109);

            ControlPoint control;

            control = eventDB.GetControl(ControlId(2));
            Assert.AreEqual("3/4", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(4));
            Assert.AreEqual("9", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(9));
            Assert.IsNull(control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(18));
            Assert.IsNull(control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(13));
            Assert.IsNull(control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(12));
            Assert.AreEqual("7.5", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);

            eventDB.Validate();

            undomgr.Undo();
            control = eventDB.GetControl(ControlId(2));
            Assert.AreEqual("2m", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(4));
            Assert.IsNull(control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(9));
            Assert.AreEqual("4x4|5x6", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(18));
            Assert.AreEqual("4", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(13));
            Assert.IsNull(control.columnFText);
            Assert.AreEqual("10.2", control.symbolIds[3]);
            control = eventDB.GetControl(ControlId(12));
            Assert.AreEqual("0.5/2.5", control.columnFText);
            Assert.IsNull(control.symbolIds[3]);



        }

        [TestMethod]
        public void ChangeCode()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(110, "Change code");

            ChangeEvent.ChangeCode(eventDB, ControlId(11), "XY");

            undomgr.EndCommand(110);

            ControlPoint control;
            control = eventDB.GetControl(ControlId(11));
            Assert.AreEqual("XY", control.code);

            eventDB.Validate();

            undomgr.Undo();
            control = eventDB.GetControl(ControlId(11));
            Assert.AreEqual("191", control.code);
        }

        [TestMethod]
        public void ChangeAllControlsLocation()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(117, "Change all controls location");

            ChangeEvent.ChangeAllControlsCodeLocation(eventDB, ControlId(11), true, 95.0F);

            undomgr.EndCommand(117);

            ControlPoint control;
            control = eventDB.GetControl(ControlId(11));
            Assert.AreEqual(95.0F, control.codeLocationAngle);
            Assert.AreEqual(true, control.customCodeLocation);

            eventDB.Validate();

            undomgr.Undo();
            control = eventDB.GetControl(ControlId(11));
            Assert.AreEqual(false, control.customCodeLocation);
        }

        [TestMethod]
        public void ChangeControl()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(116, "Move number");
            ChangeEvent.ChangeNumberLocation(eventDB, CourseControlId(203), true, new PointF(45, 0));
            undomgr.EndCommand(116);
            undomgr.BeginCommand(111, "Change control");
            ChangeEvent.ChangeControl(eventDB, CourseControlId(203), ControlId(13));
            undomgr.EndCommand(111);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(203));
            Assert.AreEqual(ControlId(13), courseControl.control);
            Assert.IsFalse(courseControl.customNumberPlacement);
            Assert.AreEqual(CourseControlId(204), courseControl.nextCourseControl);
            Assert.AreEqual(CourseControlId(203), eventDB.GetCourseControl(CourseControlId(202)).nextCourseControl);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(203));
            Assert.AreEqual(ControlId(9), courseControl.control);
            Assert.IsTrue(courseControl.customNumberPlacement);
            Assert.AreEqual(CourseControlId(204), courseControl.nextCourseControl);
            Assert.AreEqual(CourseControlId(203), eventDB.GetCourseControl(CourseControlId(202)).nextCourseControl);
        }

        [TestMethod]
        public void ChangeControlVariation()
        {
            Setup("changeevent\\variations.ppen");

            undomgr.BeginCommand(116, "Move number");
            ChangeEvent.ChangeNumberLocation(eventDB, CourseControlId(4), true, new PointF(45, 0));
            undomgr.EndCommand(116);
            undomgr.BeginCommand(111, "Change control");
            ChangeEvent.ChangeControl(eventDB, CourseControlId(4), ControlId(25));
            undomgr.EndCommand(111);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(ControlId(25), courseControl.control);
            Assert.IsFalse(courseControl.customNumberPlacement);
            Assert.AreEqual(CourseControlId(5), courseControl.nextCourseControl);
            Assert.AreEqual(CourseControlId(4), eventDB.GetCourseControl(CourseControlId(3)).nextCourseControl);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(ControlId(4), courseControl.control);
            Assert.IsTrue(courseControl.customNumberPlacement);
            Assert.AreEqual(CourseControlId(5), courseControl.nextCourseControl);
            Assert.AreEqual(CourseControlId(4), eventDB.GetCourseControl(CourseControlId(3)).nextCourseControl);
        }

        [TestMethod]
        public void ChangeControlExchange()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(119, "Add exchange");
            ChangeEvent.ChangeControlExchange(eventDB, CourseControlId(204), MapExchangeType.Exchange);
            undomgr.EndCommand(119);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(204));
            Assert.IsTrue(courseControl.exchange);
            Assert.IsFalse(courseControl.exchangeIsFlip);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(204));
            Assert.IsFalse(courseControl.exchange);
            Assert.IsFalse(courseControl.exchangeIsFlip);
        }

        [TestMethod]
        public void ChangeControlMapFlip()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(129, "Add exchange");
            ChangeEvent.ChangeControlExchange(eventDB, CourseControlId(204), MapExchangeType.MapFlip);
            undomgr.EndCommand(129);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(204));
            Assert.IsTrue(courseControl.exchange);
            Assert.IsTrue(courseControl.exchangeIsFlip);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(204));
            Assert.IsFalse(courseControl.exchange);
            Assert.IsFalse(courseControl.exchangeIsFlip);
        }

        [TestMethod]
        public void ChangeControlExchangeVariation()
        {
            Setup("changeevent\\variations.ppen");

            undomgr.BeginCommand(119, "Add exchange");
            ChangeEvent.ChangeControlExchange(eventDB, CourseControlId(4), MapExchangeType.Exchange);
            undomgr.EndCommand(119);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.IsTrue(courseControl.exchange);
            Assert.IsFalse(courseControl.exchangeIsFlip);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.IsFalse(courseControl.exchange);
            Assert.IsFalse(courseControl.exchangeIsFlip);
        }

        [TestMethod]
        public void ChangeTextLine()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, CourseControlId(203), "hello", false);
            undomgr.EndCommand(118);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(203));
            Assert.AreEqual("hello", courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(203));
            Assert.IsNull(courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, CourseControlId(203), "goodbye", true);
            undomgr.EndCommand(118);

            courseControl = eventDB.GetCourseControl(CourseControlId(203));
            Assert.AreEqual("goodbye", courseControl.descTextBefore);
            Assert.IsNull(courseControl.descTextAfter);

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, CourseControlId(203), "", true);
            undomgr.EndCommand(118);

            courseControl = eventDB.GetCourseControl(CourseControlId(203));
            Assert.IsNull(courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);
        }

        [TestMethod]
        public void ChangeTextLine2()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, ControlId(11), "hello", false);
            undomgr.EndCommand(118);

            eventDB.Validate();

            ControlPoint control = eventDB.GetControl(ControlId(11));
            Assert.AreEqual("hello", control.descTextAfter);
            Assert.IsNull(control.descTextBefore);

            undomgr.Undo();
            eventDB.Validate();

            control = eventDB.GetControl(ControlId(11));
            Assert.IsNull(control.descTextAfter);
            Assert.IsNull(control.descTextBefore);

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, ControlId(11), "goodbye", true);
            undomgr.EndCommand(118);

            control = eventDB.GetControl(ControlId(11));
            Assert.AreEqual("goodbye", control.descTextBefore);
            Assert.IsNull(control.descTextAfter);

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, ControlId(11), "", true);
            undomgr.EndCommand(118);

            control = eventDB.GetControl(ControlId(11));
            Assert.IsNull(control.descTextAfter);
            Assert.IsNull(control.descTextBefore);
        }

        [TestMethod]
        public void ChangeTextLine3()
        {
            Setup("changeevent\\variations.ppen");

            undomgr.BeginCommand(118, "Change text line");
            ChangeEvent.ChangeTextLine(eventDB, CourseControlId(2), "hello", false);
            undomgr.EndCommand(118);

            eventDB.Validate();

            CourseControl courseControl = eventDB.GetCourseControl(CourseControlId(2));
            Assert.AreEqual("hello", courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);

            courseControl = eventDB.GetCourseControl(CourseControlId(24));
            Assert.AreEqual("hello", courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);

            undomgr.Undo();
            eventDB.Validate();

            courseControl = eventDB.GetCourseControl(CourseControlId(2));
            Assert.IsNull(courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);

            courseControl = eventDB.GetCourseControl(CourseControlId(24));
            Assert.IsNull(courseControl.descTextAfter);
            Assert.IsNull(courseControl.descTextBefore);
        }

        [TestMethod]
        public void ChangeScore()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(110, "Change points");

            ChangeEvent.ChangeScore(eventDB, CourseControlId(4), 25);

            undomgr.EndCommand(110);

            CourseControl courseControl;
            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(25, courseControl.points);

            eventDB.Validate();

            undomgr.Undo();
            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(20, courseControl.points);
        }

        [TestMethod]
        public void ChangeNumberLocation()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(7110, "Change number Location");
            ChangeEvent.ChangeNumberLocation(eventDB, CourseControlId(4), true, new PointF(15.9F, -8.43F));
            undomgr.EndCommand(7110);

            CourseControl courseControl;
            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(true, courseControl.customNumberPlacement);
            Assert.AreEqual(-4F, courseControl.numberDeltaX);
            Assert.AreEqual(-8F, courseControl.numberDeltaY);
            eventDB.Validate();

            undomgr.Undo();
            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(false, courseControl.customNumberPlacement);

            undomgr.Redo();
            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(true, courseControl.customNumberPlacement);
            Assert.AreEqual(-4F, courseControl.numberDeltaX);
            Assert.AreEqual(-8F, courseControl.numberDeltaY);
            eventDB.Validate();

            // Try the default location.
            undomgr.BeginCommand(7110, "Change number Location");
            ChangeEvent.ChangeNumberLocation(eventDB, CourseControlId(4), false, new PointF(0, 0));
            undomgr.EndCommand(7110);

            courseControl = eventDB.GetCourseControl(CourseControlId(4));
            Assert.AreEqual(false, courseControl.customNumberPlacement);
        }


        [TestMethod]
        public void ChangeEventTitle()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(111, "Change event title");

            ChangeEvent.ChangeEventTitle(eventDB, "New title");

            undomgr.EndCommand(111);

            Assert.AreEqual("New title", eventDB.GetEvent().title);

            undomgr.Undo();
            Assert.AreEqual("Sample Event 1", eventDB.GetEvent().title);
        }

        [TestMethod]
        public void ChangeMapScale()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(145, "Change map scale");

            ChangeEvent.ChangeMapScale(eventDB, 7124);

            undomgr.EndCommand(145);

            Assert.AreEqual(7124, eventDB.GetEvent().mapScale);

            undomgr.Undo();
            Assert.AreEqual(15000, eventDB.GetEvent().mapScale);
        }

        [TestMethod]
        public void ChangeDescriptionLanguage()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(66145, "Change description language");

            ChangeEvent.ChangeDescriptionLanguage(eventDB, "de");

            undomgr.EndCommand(66145);

            Assert.AreEqual("de", eventDB.GetEvent().descriptionLangId);

            undomgr.Undo();
            Assert.AreEqual("en", eventDB.GetEvent().descriptionLangId);
        }

        [TestMethod]
        public void ChangeAutoNumbering()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(147, "Change auto numbering");

            ChangeEvent.ChangeAutoNumbering(eventDB, 77, false);

            undomgr.EndCommand(147);

            Assert.AreEqual(77, eventDB.GetEvent().firstControlCode);
            Assert.AreEqual(false, eventDB.GetEvent().disallowInvertibleCodes);

            undomgr.Undo();
            Assert.AreEqual(31, eventDB.GetEvent().firstControlCode);
            Assert.AreEqual(true, eventDB.GetEvent().disallowInvertibleCodes);
        }



        [TestMethod]
        public void ChangeCourseName()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(112, "Change course name");

            ChangeEvent.ChangeCourseName(eventDB, CourseId(1), "Banana");
            ChangeEvent.ChangeCourseName(eventDB, CourseId(4), "Orange");

            undomgr.EndCommand(112);

            Assert.AreEqual("Banana", eventDB.GetCourse(CourseId(1)).name);
            Assert.AreEqual("Orange", eventDB.GetCourse(CourseId(4)).name);

            undomgr.Undo();
            Assert.AreEqual("White", eventDB.GetCourse(CourseId(1)).name);
            Assert.AreEqual("SampleCourse4", eventDB.GetCourse(CourseId(4)).name);
        }

        [TestMethod]
        public void ChangeCourseClimb()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(117, "Change course climb");

            ChangeEvent.ChangeCourseClimb(eventDB, CourseId(4), -1F);
            ChangeEvent.ChangeCourseClimb(eventDB, CourseId(6), 173.4F);

            undomgr.EndCommand(117);

            Assert.AreEqual(-1, eventDB.GetCourse(CourseId(4)).climb);
            Assert.AreEqual(173.4F, eventDB.GetCourse(CourseId(6)).climb);

            undomgr.Undo();
            Assert.AreEqual(173, eventDB.GetCourse(CourseId(4)).climb);
            Assert.AreEqual(-1, eventDB.GetCourse(CourseId(6)).climb);
        }

        [TestMethod]
        public void ChangeCourseSecondaryTitle()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(113, "Change secondary title");

            ChangeEvent.ChangeCourseSecondaryTitle(eventDB, CourseId(1), "Banana");
            ChangeEvent.ChangeCourseSecondaryTitle(eventDB, CourseId(4), "Orange");
            ChangeEvent.ChangeCourseSecondaryTitle(eventDB, CourseId(5), "");

            undomgr.EndCommand(113);

            Assert.AreEqual("Banana", eventDB.GetCourse(CourseId(1)).secondaryTitle);
            Assert.AreEqual("Orange", eventDB.GetCourse(CourseId(4)).secondaryTitle);
            Assert.IsNull(eventDB.GetCourse(CourseId(5)).secondaryTitle);

            undomgr.Undo();
            Assert.AreEqual("White is right", eventDB.GetCourse(CourseId(1)).secondaryTitle);
            Assert.IsNull(eventDB.GetCourse(CourseId(4)).secondaryTitle);
            Assert.AreEqual("Score more!", eventDB.GetCourse(CourseId(5)).secondaryTitle);
        }

        [TestMethod]
        public void ChangeCourseProperties()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(713, "Change secondary title");
            ChangeEvent.ChangeCourseProperties(eventDB, CourseId(1), CourseKind.Score, "Xavier", ControlLabelKind.SequenceAndCode, 1, null, 5000, 55, 7.6F, DescriptionKind.Text, 10, true);
            undomgr.EndCommand(713);

            Course course = eventDB.GetCourse(CourseId(1));
            Assert.AreEqual(CourseKind.Score, course.kind);
            Assert.AreEqual("Xavier", course.name);
            Assert.AreEqual(null, course.secondaryTitle);
            Assert.AreEqual(5000, course.printScale);
            Assert.AreEqual(55, course.climb);
            Assert.AreEqual(ControlLabelKind.SequenceAndCode, course.labelKind);
            Assert.AreEqual(1, course.scoreColumn);
            Assert.AreEqual(DescriptionKind.Text, course.descKind);
            Assert.AreEqual(10, course.firstControlOrdinal);
            Assert.IsTrue(course.hideFromReports);

            undomgr.Undo();

            course = eventDB.GetCourse(CourseId(1));
            Assert.AreEqual(CourseKind.Normal, course.kind);
            Assert.AreEqual("White", course.name);
            Assert.AreEqual("White is right", course.secondaryTitle);
            Assert.AreEqual(15000, course.printScale);
            Assert.AreEqual(66, course.climb);
            Assert.AreEqual(ControlLabelKind.Sequence, course.labelKind);
            Assert.AreEqual(-1, course.scoreColumn);
            Assert.AreEqual(DescriptionKind.SymbolsAndText, course.descKind);
            Assert.AreEqual(1, course.firstControlOrdinal);
            Assert.IsFalse(course.hideFromReports);
        }

        void TestDuplicateCourse(Id<Course> oldCourseId)
        {
            Id<Course> newCourseId;
            undomgr.BeginCommand(4713, "Duplicate course");
            newCourseId = ChangeEvent.DuplicateCourse(eventDB, oldCourseId, "Duplicate");
            undomgr.EndCommand(4713);

            Course courseOld = eventDB.GetCourse(oldCourseId), courseNew = eventDB.GetCourse(newCourseId);
            Assert.AreEqual(courseOld.climb, courseNew.climb);
            Assert.AreEqual(courseOld.descKind, courseNew.descKind);
            Assert.AreEqual(courseOld.kind, courseNew.kind);
            Assert.AreEqual(courseOld.labelKind, courseNew.labelKind);
            Assert.AreEqual(courseOld.load, courseNew.load);
            Assert.AreEqual(courseOld.printArea, courseNew.printArea);
            Assert.AreEqual(courseOld.printScale, courseNew.printScale);
            Assert.AreEqual(courseOld.scoreColumn, courseNew.scoreColumn);
            Assert.AreEqual(courseOld.secondaryTitle, courseNew.secondaryTitle);
            Assert.AreEqual(courseOld.sortOrder + 1, courseNew.sortOrder);
            Assert.AreEqual(courseOld.hideFromReports, courseNew.hideFromReports);

            Id<CourseControl>[] oldCourseControlIds = QueryEvent.EnumCourseControlIds(eventDB, new CourseDesignator(oldCourseId)).ToArray();
            Id<CourseControl>[] newCourseControlIds = QueryEvent.EnumCourseControlIds(eventDB, new CourseDesignator(newCourseId)).ToArray();
            Assert.AreEqual(oldCourseControlIds.Length, newCourseControlIds.Length);

            for (int i = 0; i < oldCourseControlIds.Length; ++i) {
                Assert.AreNotEqual(oldCourseControlIds[i], newCourseControlIds[i]);
                CourseControl oldCC = eventDB.GetCourseControl(oldCourseControlIds[i]);
                CourseControl newCC = eventDB.GetCourseControl(newCourseControlIds[i]);
                Assert.AreEqual(oldCC.customNumberPlacement, newCC.customNumberPlacement);
                Assert.AreEqual(oldCC.control, newCC.control);
                Assert.AreEqual(oldCC.descTextAfter, newCC.descTextAfter);
                Assert.AreEqual(oldCC.descTextBefore, newCC.descTextBefore);
                Assert.AreEqual(oldCC.exchange, newCC.exchange);
                Assert.AreEqual(oldCC.numberDeltaX, newCC.numberDeltaX);
                Assert.AreEqual(oldCC.numberDeltaY, newCC.numberDeltaY);
                Assert.AreEqual(oldCC.points, newCC.points);
                Assert.AreEqual(oldCC.split, newCC.split);
                Assert.AreEqual(oldCC.splitEnd, newCC.splitEnd);
            }

            eventDB.Validate();

            undomgr.Undo();

            Assert.IsFalse(eventDB.AllCourseIds.Contains(newCourseId));

            eventDB.Validate();
        }

        void DuplicateCourseTestAllCourses(string filename)
        {
            Setup(filename);

            foreach (var courseId in eventDB.AllCourseIds.ToArray()) {
                // Duplicate course doesnt work if a split.
                bool hasSplit = (from cc in QueryEvent.EnumCourseControlIds(eventDB, new CourseDesignator(courseId))
                                 where eventDB.GetCourseControl(cc).split
                                 select cc).Any();

                if (!hasSplit)
                    TestDuplicateCourse(courseId);
            }

        }

        [TestMethod]
        public void DuplicateCourse()
        {
            DuplicateCourseTestAllCourses("changeevent\\mapexchange1.ppen");
            DuplicateCourseTestAllCourses("changeevent\\marymoor4.coursescribe");
            DuplicateCourseTestAllCourses("changeevent\\sampleevent1.coursescribe");
            DuplicateCourseTestAllCourses("changeevent\\sampleevent3.ppen");
            DuplicateCourseTestAllCourses("changeevent\\sampleevent7.coursescribe");
            DuplicateCourseTestAllCourses("changeevent\\sampleevent12.ppen");
            DuplicateCourseTestAllCourses("changeevent\\SpecialLegs.coursescribe");
            DuplicateCourseTestAllCourses("changeevent\\sampleevent7.coursescribe");
            DuplicateCourseTestAllCourses("changeevent\\variations.ppen");
        }

        [TestMethod]
        public void ChangeCourseLoad()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(953, "Change course load");
            ChangeEvent.ChangeCourseLoad(eventDB, CourseId(1), 72);
            undomgr.EndCommand(953);

            Course course = eventDB.GetCourse(CourseId(1));
            Assert.AreEqual(72, course.load);

            undomgr.Undo();

            course = eventDB.GetCourse(CourseId(1));
            Assert.AreEqual(-1, course.load);
        }

        [TestMethod]
        public void ChangeCourseSortOrder()
        {
            Setup("changeevent\\marymoor4.coursescribe");

            undomgr.BeginCommand(959, "Change course order");
            ChangeEvent.ChangeCourseSortOrder(eventDB, CourseId(1), 19);
            undomgr.EndCommand(959);

            Course course = eventDB.GetCourse(CourseId(1));
            Assert.AreEqual(19, course.sortOrder);

            undomgr.Undo();

            course = eventDB.GetCourse(CourseId(1));
            Assert.AreEqual(1, course.sortOrder);
        }

        [TestMethod]
        public void ChangeControlLocation()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(812, "Move control");

            ChangeEvent.ChangeControlLocation(eventDB, ControlId(2), new PointF(20, 15));
            ChangeEvent.ChangeControlLocation(eventDB, ControlId(1), new PointF(-5, 0));
            ChangeEvent.ChangeControlLocation(eventDB, ControlId(5), new PointF(0, 0));

            undomgr.EndCommand(812);
            eventDB.Validate();

            Assert.AreEqual(20, eventDB.GetControl(ControlId(2)).location.X);
            Assert.AreEqual(15, eventDB.GetControl(ControlId(2)).location.Y);
            Assert.AreEqual(-5, eventDB.GetControl(ControlId(1)).location.X);
            Assert.AreEqual(0, eventDB.GetControl(ControlId(1)).location.Y);
            Assert.AreEqual(0, eventDB.GetControl(ControlId(5)).location.X);
            Assert.AreEqual(0, eventDB.GetControl(ControlId(5)).location.Y);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(10, eventDB.GetControl(ControlId(2)).location.X);
            Assert.AreEqual(10, eventDB.GetControl(ControlId(2)).location.Y);
            Assert.AreEqual(5, eventDB.GetControl(ControlId(1)).location.X);
            Assert.AreEqual(0, eventDB.GetControl(ControlId(1)).location.Y);
            Assert.AreEqual(35.4F, eventDB.GetControl(ControlId(5)).location.X);
            Assert.AreEqual(-22.5F, eventDB.GetControl(ControlId(5)).location.Y);

        }

        [TestMethod]
        public void MoveLegGap()
        {
            LegGap[] legGaps;

            Setup(@"changeevent\gappedlegs.coursescribe");

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);
            Assert.AreEqual(25, legGaps[1].distanceFromStart);
            Assert.AreEqual(9, legGaps[1].length);

            undomgr.BeginCommand(991, "Move gap");
            ChangeEvent.MoveLegGap(eventDB, ControlId(1), ControlId(2), new PointF(68.42F, -19.52F), new PointF(69.1F, -14.6F));
            undomgr.EndCommand(991);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);
            Assert.AreEqual(25, legGaps[1].distanceFromStart);
            Assert.AreEqual(4.03F, legGaps[1].length, 0.01F);
        }

        [TestMethod]
        public void MoveLegWithGaps()
        {
            LegGap[] legGaps;

            Setup(@"changeevent\gappedlegs.coursescribe");

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);
            Assert.AreEqual(25, legGaps[1].distanceFromStart);
            Assert.AreEqual(9, legGaps[1].length);

            undomgr.BeginCommand(991, "Move control");
            ChangeEvent.ChangeControlLocation(eventDB, ControlId(1), new PointF(77, 35));
            undomgr.EndCommand(991);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(28.19F, legGaps[0].distanceFromStart, 0.01F);
            Assert.AreEqual(3.5F, legGaps[0].length, 0.01F);
            Assert.AreEqual(46.19F, legGaps[1].distanceFromStart, 0.01F);
            Assert.AreEqual(9F, legGaps[1].length, 0.01F);
        }

        [TestMethod]
        public void MoveBendWithGaps()
        {
            LegGap[] legGaps;

            Setup(@"changeevent\gappedlegs.coursescribe");

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);
            Assert.AreEqual(25, legGaps[1].distanceFromStart);
            Assert.AreEqual(9, legGaps[1].length);

            undomgr.BeginCommand(991, "Move bend");
            ChangeEvent.MoveLegBend(eventDB, ControlId(1), ControlId(2), new PointF(68, -22), new PointF(74, -17));
            undomgr.EndCommand(991);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(6.9F, legGaps[0].distanceFromStart, 0.01);
            Assert.AreEqual(3.45F, legGaps[0].length, 0.01);
            Assert.AreEqual(24.66F, legGaps[1].distanceFromStart, 0.01);
            Assert.AreEqual(12.23F, legGaps[1].length, 0.01);
        }

        [TestMethod]
        public void DeleteBendWithGaps()
        {
            LegGap[] legGaps;

            Setup(@"changeevent\gappedlegs.coursescribe");

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);
            Assert.AreEqual(25, legGaps[1].distanceFromStart);
            Assert.AreEqual(9, legGaps[1].length);

            undomgr.BeginCommand(991, "Delete bend");
            ChangeEvent.RemoveLegBend(eventDB, ControlId(1), ControlId(2), new PointF(68, -22));
            undomgr.EndCommand(991);

            // Should now have no bends and no gaps.
            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.IsNull(legGaps);
            Assert.IsTrue(QueryEvent.FindLeg(eventDB, ControlId(1), ControlId(2)).IsNone);
        }

        [TestMethod]
        public void AddBendWithGaps()
        {
            LegGap[] legGaps;

            Setup(@"changeevent\gappedlegs.coursescribe");

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);
            Assert.AreEqual(25, legGaps[1].distanceFromStart);
            Assert.AreEqual(9, legGaps[1].length);

            undomgr.BeginCommand(991, "Add bend");
            ChangeEvent.AddLegBend(eventDB, ControlId(1), ControlId(2), new PointF(73, -6));
            undomgr.EndCommand(991);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(2, legGaps.Length);
            Assert.AreEqual(6.96F, legGaps[0].distanceFromStart, 0.01);
            Assert.AreEqual(3.48F, legGaps[0].length, 0.01);
            Assert.AreEqual(25.38F, legGaps[1].distanceFromStart, 0.01);
            Assert.AreEqual(8.92F, legGaps[1].length, 0.01);
        }

        [TestMethod]
        public void RemoveLegGap()
        {
            LegGap[] legGaps;

            Setup(@"changeevent\gappedlegs.coursescribe");

            undomgr.BeginCommand(999, "remove gap");
            ChangeEvent.RemoveLegGap(eventDB, ControlId(1), ControlId(2), new PointF(70, -13));
            undomgr.EndCommand(999);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(1, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);

            // This shouldn't do anything.
            undomgr.BeginCommand(999, "remove gap");
            ChangeEvent.RemoveLegGap(eventDB, ControlId(1), ControlId(2), new PointF(47, -35));
            undomgr.EndCommand(999);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.AreEqual(1, legGaps.Length);
            Assert.AreEqual(7, legGaps[0].distanceFromStart);
            Assert.AreEqual(3.5F, legGaps[0].length);

            // This shouldn't do anything.
            undomgr.BeginCommand(999, "remove gap");
            ChangeEvent.RemoveLegGap(eventDB, ControlId(1), ControlId(2), new PointF(73, 6));
            undomgr.EndCommand(999);

            legGaps = QueryEvent.GetLegGaps(eventDB, ControlId(1), ControlId(2));
            Assert.IsNull(legGaps);
        }


        [TestMethod]
        public void ChangeControlOrientation()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual(94.5F, eventDB.GetControl(ControlId(3)).orientation);

            undomgr.BeginCommand(812, "Rotate crossing point");

            ChangeEvent.ChangeControlOrientation(eventDB, ControlId(3), 247.4F);

            undomgr.EndCommand(812);
            eventDB.Validate();

            Assert.AreEqual(247.4F, eventDB.GetControl(ControlId(3)).orientation);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(94.5F, eventDB.GetControl(ControlId(3)).orientation);
        }

        [TestMethod]
        public void ChangeControlStretch()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual(0, eventDB.GetControl(ControlId(3)).stretch);

            undomgr.BeginCommand(812, "Stretch crossing point");

            ChangeEvent.ChangeControlStretch(eventDB, ControlId(3), 2.33F);

            undomgr.EndCommand(812);
            eventDB.Validate();

            Assert.AreEqual(2.33F, eventDB.GetControl(ControlId(3)).stretch);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(0, eventDB.GetControl(ControlId(3)).stretch);
        }

        [TestMethod]
        public void ChangeControlGaps()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(819, "add gap");

            ChangeEvent.ChangeControlGaps(eventDB, ControlId(4), 10000F, CircleGap.ComputeCircleGaps(0x3FF1F00F));
            ChangeEvent.ChangeControlGaps(eventDB, ControlId(4), 15000F, CircleGap.ComputeCircleGaps(0xF0FFFFFF));
            ChangeEvent.ChangeControlGaps(eventDB, ControlId(4), 5000F, CircleGap.ComputeCircleGaps(0xFFFFFFFF));

            ChangeEvent.ChangeControlGaps(eventDB, ControlId(2), 12000F, CircleGap.ComputeCircleGaps(0xFFFFFFFF));
            ChangeEvent.ChangeControlGaps(eventDB, ControlId(2), 10000F, CircleGap.ComputeCircleGaps(0xF00FFFFF));

            ChangeEvent.ChangeControlGaps(eventDB, ControlId(3), 10000F, CircleGap.ComputeCircleGaps(0xFFFF4FFF));
            ChangeEvent.ChangeControlGaps(eventDB, ControlId(3), 10000F, CircleGap.ComputeCircleGaps(0xFFFFFFFF));

            undomgr.EndCommand(819);
            eventDB.Validate();

            CollectionAssert.AreEqual(CircleGap.ComputeCircleGaps(0x3FF1F00FU), eventDB.GetControl(ControlId(4)).gaps[10000]);
            CollectionAssert.AreEqual(CircleGap.ComputeCircleGaps(0xF0FFFFFFU), eventDB.GetControl(ControlId(4)).gaps[15000]);
            Assert.IsFalse(eventDB.GetControl(ControlId(4)).gaps.ContainsKey(5000));

            CollectionAssert.AreEqual(CircleGap.ComputeCircleGaps(0xF00FFFFFU), eventDB.GetControl(ControlId(2)).gaps[10000]);
            Assert.IsFalse(eventDB.GetControl(ControlId(2)).gaps.ContainsKey(12000));

            Assert.IsFalse(eventDB.GetControl(ControlId(3)).gaps.ContainsKey(10000));

            undomgr.Undo();
            eventDB.Validate();

            CollectionAssert.AreEqual(CircleGap.ComputeCircleGaps(0xFFFFFFDFU), eventDB.GetControl(ControlId(4)).gaps[15000]);
            Assert.IsNull(eventDB.GetControl(ControlId(2)).gaps);
            Assert.IsNull(eventDB.GetControl(ControlId(3)).gaps);
        }

        [TestMethod]
        public void AddGapPoints()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(818, "change gap");
            ChangeEvent.ChangeControlGaps(eventDB, ControlId(4), 10000F, new CircleGap[] { new CircleGap(-20, 60) });
            undomgr.EndCommand(818);

            undomgr.BeginCommand(819, "add gap");
            ChangeEvent.AddGap(eventDB, 10000F, ControlId(4), new PointF(18, 25), new PointF(-10, 16));
            undomgr.EndCommand(819);

            eventDB.Validate();

            CollectionAssert.AreEqual(new CircleGap[] { new CircleGap(-20, 60), new CircleGap(93.2245255F, 138.544769F) }, eventDB.GetControl(ControlId(4)).gaps[10000]);

            undomgr.Undo();
            eventDB.Validate();

            CollectionAssert.AreEqual(new CircleGap[] { new CircleGap(-20, 60) }, eventDB.GetControl(ControlId(4)).gaps[10000]);
        }

        [TestMethod]
        public void RemoveCourseControl()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.IsTrue(eventDB.IsControlPresent(ControlId(3)));

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(4), CourseControlId(14));
            undomgr.EndCommand(912);
            eventDB.Validate();

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(13));
            Assert.AreEqual(15, cc.nextCourseControl.id);
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(14)));
            Assert.IsTrue(eventDB.IsControlPresent(ControlId(3)));

            undomgr.Undo();
            eventDB.Validate();

            cc = eventDB.GetCourseControl(CourseControlId(13));
            Assert.AreEqual(14, cc.nextCourseControl.id);
            cc = eventDB.GetCourseControl(CourseControlId(14));
            Assert.AreEqual(15, cc.nextCourseControl.id);
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(14)));
            Assert.IsTrue(eventDB.IsControlPresent(ControlId(3)));
        }

        [TestMethod]
        public void RemoveCourseControlVariation()
        {
            Setup("changeevent\\variations.ppen");

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(10));
            undomgr.EndCommand(912);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(10)));

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(12));
            Assert.AreEqual(11, cc.nextCourseControl.id);
            cc = eventDB.GetCourseControl(CourseControlId(2));
            Assert.AreEqual(CourseControlId(11), cc.splitEnd);
            cc = eventDB.GetCourseControl(CourseControlId(24));
            Assert.AreEqual(CourseControlId(11), cc.splitEnd);

            undomgr.Undo();
            eventDB.Validate();

            ///////////////////////////////

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(11));
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(10));
            undomgr.EndCommand(912);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(11)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(10)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(12)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(24)));

            cc = eventDB.GetCourseControl(CourseControlId(2));
            Assert.AreEqual(3, cc.nextCourseControl.id);
            Assert.IsFalse(cc.split);
            Assert.IsNull(cc.splitCourseControls);
            Assert.IsTrue(cc.splitEnd.IsNone);

            undomgr.Undo();
            eventDB.Validate();

            ///////////////////////////////

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(4));
            undomgr.EndCommand(912);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(4)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(16)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(15)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(17)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(18)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(25)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(26)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(27)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(19)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(20)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(21)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(22)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(23)));

            cc = eventDB.GetCourseControl(CourseControlId(3));
            Assert.AreEqual(5, cc.nextCourseControl.id);
            Assert.IsFalse(cc.split);
            Assert.IsNull(cc.splitCourseControls);
            Assert.IsTrue(cc.splitEnd.IsNone);

            undomgr.Undo();
            eventDB.Validate();

            ///////////////////////////////

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(25));
            undomgr.EndCommand(912);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(16)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(15)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(25)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(4)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(17)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(18)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(26)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(27)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(19)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(20)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(21)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(22)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(23)));

            cc = eventDB.GetCourseControl(CourseControlId(3));
            Assert.AreEqual(4, cc.nextCourseControl.id);
            Assert.IsFalse(cc.split);
            Assert.IsNull(cc.splitCourseControls);
            Assert.IsTrue(cc.splitEnd.IsNone);

            undomgr.Undo();
            eventDB.Validate();

            ///////////////////////////////

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(24));
            undomgr.EndCommand(912);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(24)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(12)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(2)));

            cc = eventDB.GetCourseControl(CourseControlId(2));
            Assert.AreEqual(3, cc.nextCourseControl.id);
            Assert.IsFalse(cc.split);
            Assert.IsNull(cc.splitCourseControls);
            Assert.IsTrue(cc.splitEnd.IsNone);

            undomgr.Undo();
            eventDB.Validate();

            ///////////////////////////////

            undomgr.BeginCommand(912, "Remove Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(1), CourseControlId(9));
            undomgr.EndCommand(912);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(9)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(28)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(13)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(29)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(14)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(12)));

            cc = eventDB.GetCourseControl(CourseControlId(8));
            Assert.AreEqual(28, cc.nextCourseControl.id);
            Assert.IsFalse(cc.split);
            Assert.IsNull(cc.splitCourseControls);
            Assert.IsTrue(cc.splitEnd.IsNone);

            cc = eventDB.GetCourseControl(CourseControlId(28));
            Assert.AreEqual(13, cc.nextCourseControl.id);
            Assert.IsTrue(cc.split);
            Assert.AreEqual(2, cc.splitCourseControls.Length);
            Assert.AreEqual(10, cc.splitEnd.id);

            undomgr.Undo();
            eventDB.Validate();

        }

        [TestMethod]
        public void RemoveLastCourseControl()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(913, "Remove Last Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(4), CourseControlId(19));
            undomgr.EndCommand(913);
            eventDB.Validate();

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(18));
            Assert.AreEqual(0, cc.nextCourseControl.id);
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(19)));

            undomgr.Undo();
            eventDB.Validate();

            cc = eventDB.GetCourseControl(CourseControlId(18));
            Assert.AreEqual(19, cc.nextCourseControl.id);
            cc = eventDB.GetCourseControl(CourseControlId(19));
            Assert.AreEqual(0, cc.nextCourseControl.id);
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(19)));
        }

        [TestMethod]
        public void RemoveFirstCourseControl()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(914, "Remove First Course Control");
            ChangeEvent.RemoveCourseControl(eventDB, CourseId(4), CourseControlId(11));
            undomgr.EndCommand(914);
            eventDB.Validate();

            Course c = eventDB.GetCourse(CourseId(4));
            Assert.AreEqual(12, c.firstCourseControl.id);
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(11)));

            undomgr.Undo();
            eventDB.Validate();

            c = eventDB.GetCourse(CourseId(4));
            Assert.AreEqual(11, c.firstCourseControl.id);
            CourseControl cc = eventDB.GetCourseControl(CourseControlId(11));
            Assert.AreEqual(12, cc.nextCourseControl.id);
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(11)));
        }

        [TestMethod]
        public void RemoveUnusedControl()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.IsTrue(eventDB.IsControlPresent(ControlId(23)));

            undomgr.BeginCommand(915, "Remove Unused Control");
            ChangeEvent.RemoveControl(eventDB, ControlId(23));
            undomgr.EndCommand(915);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsControlPresent(ControlId(23)));

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsControlPresent(ControlId(23)));
        }

        [TestMethod]
        public void RemoveUsedControl()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.IsTrue(eventDB.IsControlPresent(ControlId(3)));

            undomgr.BeginCommand(921, "Remove Control");
            ChangeEvent.RemoveControl(eventDB, ControlId(3));
            undomgr.EndCommand(921);
            eventDB.Validate();

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(13));
            Assert.AreEqual(15, cc.nextCourseControl.id);
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(14)));
            cc = eventDB.GetCourseControl(CourseControlId(53));
            Assert.AreEqual(55, cc.nextCourseControl.id);
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(54)));
            cc = eventDB.GetCourseControl(CourseControlId(103));
            Assert.AreEqual(105, cc.nextCourseControl.id);
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(104)));

            Assert.IsFalse(eventDB.IsControlPresent(ControlId(3)));

            undomgr.Undo();
            eventDB.Validate();

            cc = eventDB.GetCourseControl(CourseControlId(13));
            Assert.AreEqual(14, cc.nextCourseControl.id);
            cc = eventDB.GetCourseControl(CourseControlId(14));
            Assert.AreEqual(15, cc.nextCourseControl.id);
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(14)));
            cc = eventDB.GetCourseControl(CourseControlId(53));
            Assert.AreEqual(54, cc.nextCourseControl.id);
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(54)));
            cc = eventDB.GetCourseControl(CourseControlId(103));
            Assert.AreEqual(104, cc.nextCourseControl.id);
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(104)));
            Assert.IsTrue(eventDB.IsControlPresent(ControlId(3)));

        }

        [TestMethod]
        public void RemoveControlInLeg()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            // Create a leg
            undomgr.BeginCommand(9214, "Add Leg gaps");
            ChangeEvent.ChangeLegGaps(eventDB, ControlId(9), ControlId(8), new LegGap[] { new LegGap(3F, 1.2F) });
            undomgr.EndCommand(9214);

            Id<Leg> leg = QueryEvent.FindLeg(eventDB, ControlId(9), ControlId(8));
            Assert.IsTrue(leg.IsNotNone);
            Assert.IsTrue(eventDB.IsLegPresent(leg));

            // Remove control id 8.
            undomgr.BeginCommand(9215, "Remove control");
            ChangeEvent.RemoveControl(eventDB, ControlId(9));
            undomgr.EndCommand(9215);

            // Check consistency.
            eventDB.Validate();

            // The leg should be gone.
            Assert.IsFalse(eventDB.IsLegPresent(leg));

            // Move control 9.
            undomgr.BeginCommand(9216, "Move control");
            ChangeEvent.ChangeControlLocation(eventDB, ControlId(8), new PointF(20, 0));
            undomgr.EndCommand(9216);
        }

        [TestMethod]
        public void RemoveControlInVariation()
        {
            Setup("changeevent\\variations.ppen");

            // Remove control id 8.
            undomgr.BeginCommand(9215, "Remove control");
            ChangeEvent.RemoveControl(eventDB, ControlId(4));
            undomgr.EndCommand(9215);

            // Check consistency.
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(4)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(16)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(15)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(17)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(18)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(25)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(26)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(27)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(19)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(20)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(21)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(22)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(23)));

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(3));
            Assert.AreEqual(5, cc.nextCourseControl.id);
            Assert.IsFalse(cc.split);
            Assert.IsNull(cc.splitCourseControls);
            Assert.IsTrue(cc.splitEnd.IsNone);

            undomgr.Undo();
            eventDB.Validate();
        }

        [TestMethod]
        public void AddNewControlPoint()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;

            undomgr.BeginCommand(955, "Add Control");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Normal, "345", new PointF(13, 17.4F), 0);
            undomgr.EndCommand(955);
            eventDB.Validate();

            Assert.IsTrue(controlId.IsNotNone);
            Assert.IsTrue(eventDB.IsControlPresent(controlId));
            Assert.AreEqual(ControlPointKind.Normal, eventDB.GetControl(controlId).kind);
            Assert.AreEqual("345", eventDB.GetControl(controlId).code);
            Assert.AreEqual(new PointF(13, 17.4F), eventDB.GetControl(controlId).location);
            Assert.AreEqual(0F, eventDB.GetControl(controlId).orientation);

            undomgr.BeginCommand(956, "Add Start");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Start, null, new PointF(7, 13), 0);
            undomgr.EndCommand(956);
            eventDB.Validate();

            Assert.IsTrue(controlId.IsNotNone);
            Assert.IsTrue(eventDB.IsControlPresent(controlId));
            Assert.AreEqual(ControlPointKind.Start, eventDB.GetControl(controlId).kind);
            Assert.AreEqual(null, eventDB.GetControl(controlId).code);
            Assert.AreEqual(new PointF(7, 13), eventDB.GetControl(controlId).location);
            Assert.AreEqual(0F, eventDB.GetControl(controlId).orientation);

            undomgr.BeginCommand(957, "Add CrossingPoint");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.CrossingPoint, null, new PointF(6, 6), 123.3F);
            undomgr.EndCommand(957);
            eventDB.Validate();

            Assert.IsTrue(controlId.IsNotNone);
            Assert.IsTrue(eventDB.IsControlPresent(controlId));
            Assert.AreEqual(ControlPointKind.CrossingPoint, eventDB.GetControl(controlId).kind);
            Assert.AreEqual(null, eventDB.GetControl(controlId).code);
            Assert.AreEqual(new PointF(6, 6), eventDB.GetControl(controlId).location);
            Assert.AreEqual(123.3F, eventDB.GetControl(controlId).orientation);
        }

        [TestMethod]
        public void AddCourseControl1()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;

            controlId = QueryEvent.FindCode(eventDB, "290");
            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));

            undomgr.BeginCommand(957, "Add Control");
            courseControlId = ChangeEvent.AddCourseControl(eventDB, controlId, CourseId(6), CourseControlId(204), CourseControlId(205), LegInsertionLoc.Normal);
            undomgr.EndCommand(957);
            eventDB.Validate();

            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            Id<CourseControl>[] courseControls = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(6), controlId);
            Assert.AreEqual(1, courseControls.Length);
            Assert.IsTrue(courseControls[0] == courseControlId);

            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(204)).nextCourseControl == courseControlId);
            Assert.IsTrue(eventDB.GetCourseControl(courseControlId).nextCourseControl == CourseControlId(205));

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(204)).nextCourseControl == CourseControlId(205));
        }

        [TestMethod]
        public void AddCourseControl3()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;
            LegInsertionLoc legInsertionLoc = LegInsertionLoc.Normal;

            controlId = QueryEvent.FindCode(eventDB, "290");
            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));

            undomgr.BeginCommand(958, "Add Control");
            Id<CourseControl> courseControl1 = Id<CourseControl>.None, courseControl2 = Id<CourseControl>.None;
            QueryEvent.FindControlInsertionPoint(eventDB, Designator(6), ref courseControl1, ref courseControl2, ref legInsertionLoc);
            courseControlId = ChangeEvent.AddCourseControl(eventDB, controlId, CourseId(6), courseControl1, courseControl2, legInsertionLoc);
            undomgr.EndCommand(958);
            eventDB.Validate();

            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            Id<CourseControl>[] courseControls = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(6), controlId);
            Assert.AreEqual(1, courseControls.Length);
            Assert.IsTrue(courseControls[0] == courseControlId);

            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(212)).nextCourseControl == courseControlId);
            Assert.IsTrue(eventDB.GetCourseControl(courseControlId).nextCourseControl == CourseControlId(213));

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            Assert.IsTrue(eventDB.GetCourseControl(CourseControlId(212)).nextCourseControl == CourseControlId(213));
        }

        [TestMethod]
        public void AddCourseControl4()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;

            controlId = QueryEvent.FindCode(eventDB, "290");
            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));

            undomgr.BeginCommand(959, "Add Control");
            courseControlId = ChangeEvent.AddCourseControl(eventDB, controlId, CourseId(2), Id<CourseControl>.None, Id<CourseControl>.None, LegInsertionLoc.Normal);
            undomgr.EndCommand(959);
            eventDB.Validate();

            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));
            Id<CourseControl>[] courseControls = QueryEvent.GetCourseControlsInCourse(eventDB, Designator(2), controlId);
            Assert.AreEqual(1, courseControls.Length);
            Assert.IsTrue(courseControls[0] == courseControlId);

            Assert.IsTrue(eventDB.GetCourse(CourseId(2)).firstCourseControl == courseControlId);
            Assert.IsTrue(eventDB.GetCourseControl(courseControlId).nextCourseControl.IsNone);

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));
            Assert.IsTrue(eventDB.GetCourse(CourseId(2)).firstCourseControl.IsNone);
        }

        [TestMethod]
        public void AddStartToCourse1()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;

            undomgr.BeginCommand(961, "Add Control Point");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Start, null, new PointF(29, 27.4F), 95);
            undomgr.EndCommand(961);
            undomgr.BeginCommand(960, "Add Start");
            courseControlId = ChangeEvent.AddStartOrMapIssueToCourse(eventDB, controlId, CourseId(6), false);
            undomgr.EndCommand(960);
            eventDB.Validate();

            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            Course course = eventDB.GetCourse(CourseId(6));
            Id<CourseControl> first = course.firstCourseControl;
            Assert.AreEqual(controlId, eventDB.GetCourseControl(course.firstCourseControl).control);
            Assert.AreEqual(202, eventDB.GetCourseControl(course.firstCourseControl).nextCourseControl.id);

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            course = eventDB.GetCourse(CourseId(6));
            first = course.firstCourseControl;
            Assert.AreEqual(ControlId(1), eventDB.GetCourseControl(course.firstCourseControl).control);
            Assert.AreEqual(202, eventDB.GetCourseControl(course.firstCourseControl).nextCourseControl.id);
        }

        [TestMethod]
        public void AddStartToCourse2()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;
            Id<Course> noStartCourseId;

            // Create a could with a single non-start control.
            undomgr.BeginCommand(1982, "Create Course");
            noStartCourseId = eventDB.AddCourse(new Course(CourseKind.Normal, "My Course", 15000, 10));
            ChangeEvent.AddCourseControl(eventDB, ControlId(2), noStartCourseId, Id<CourseControl>.None, Id<CourseControl>.None, LegInsertionLoc.Normal);
            undomgr.EndCommand(1982);

            // Add a new start, plus ask to add to other courses without a start.
            undomgr.BeginCommand(961, "Add Control Point");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Start, null, new PointF(29, 27.4F), 95);
            undomgr.EndCommand(961);
            undomgr.BeginCommand(960, "Add Start");
            courseControlId = ChangeEvent.AddStartOrMapIssueToCourse(eventDB, controlId, CourseId(6), true);
            undomgr.EndCommand(960);
            eventDB.Validate();

            // Should have added the start to the empty course and course 2
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, new CourseDesignator(noStartCourseId), controlId));

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));
            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, new CourseDesignator(noStartCourseId), controlId));
        }

        [TestMethod]
        public void AddFinishToCourse1()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;

            undomgr.BeginCommand(961, "Add Control Point");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Finish, null, new PointF(29, 27.4F), 0);
            undomgr.EndCommand(961);
            undomgr.BeginCommand(960, "Add Finish");
            courseControlId = ChangeEvent.AddFinishToCourse(eventDB, controlId, CourseId(6), false);
            undomgr.EndCommand(960);
            eventDB.Validate();

            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            Id<CourseControl> last = QueryEvent.LastCourseControl(eventDB, CourseId(6), false);
            Id<CourseControl> lastExceptFinish = QueryEvent.LastCourseControl(eventDB, CourseId(6), true);
            Assert.AreEqual(courseControlId, last);
            Assert.AreEqual(controlId, eventDB.GetCourseControl(last).control);
            Assert.AreEqual(212, lastExceptFinish.id);
            Assert.AreEqual(last, eventDB.GetCourseControl(lastExceptFinish).nextCourseControl);
            Assert.IsTrue(eventDB.GetCourseControl(courseControlId).nextCourseControl.IsNone);

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), controlId));
            last = QueryEvent.LastCourseControl(eventDB, CourseId(6), false);
            lastExceptFinish = QueryEvent.LastCourseControl(eventDB, CourseId(6), true);
            Assert.AreNotEqual(controlId, eventDB.GetCourseControl(last).control);
            Assert.AreEqual(212, lastExceptFinish.id);
            Assert.AreEqual(213, last.id);
        }

        [TestMethod]
        public void AddFinishToCourse2()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Id<ControlPoint> controlId;
            Id<CourseControl> courseControlId;
            Id<Course> noFinishCourseId;

            // Create a course with a single non-Finish control.
            undomgr.BeginCommand(1982, "Create Course");
            noFinishCourseId = eventDB.AddCourse(new Course(CourseKind.Normal, "My Course", 15000, 10));
            ChangeEvent.AddCourseControl(eventDB, ControlId(2), noFinishCourseId, Id<CourseControl>.None, Id<CourseControl>.None, LegInsertionLoc.Normal);
            undomgr.EndCommand(1982);

            // Add a new start, plus ask to add to other courses without a start.
            undomgr.BeginCommand(961, "Add Control Point");
            controlId = ChangeEvent.AddControlPoint(eventDB, ControlPointKind.Finish, null, new PointF(29, 27.4F), 95);
            undomgr.EndCommand(961);
            undomgr.BeginCommand(960, "Add Finish");
            courseControlId = ChangeEvent.AddFinishToCourse(eventDB, controlId, CourseId(6), true);
            undomgr.EndCommand(960);
            eventDB.Validate();

            // Should have added the start to the empty course and course 2
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, new CourseDesignator(noFinishCourseId), controlId));

            undomgr.Undo();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(2), controlId));
            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, new CourseDesignator(noFinishCourseId), controlId));
        }

        [TestMethod]
        public void ReplaceControlInCourse()
        {
            Setup("changeevent\\mapexchange2.ppen");

            undomgr.BeginCommand(34124, "ReplaceControlInCourse");

            ChangeEvent.ReplaceControlInCourse(eventDB, CourseId(6), ControlId(41), ControlId(81));
            undomgr.EndCommand(34124);
            eventDB.Validate();

            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(6), ControlId(81)));
            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), ControlId(41)));

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(QueryEvent.CourseUsesControl(eventDB, Designator(6), ControlId(81)));
            Assert.IsTrue(QueryEvent.CourseUsesControl(eventDB, Designator(6), ControlId(41)));

        }

        [TestMethod]
        public void DeleteCourse()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.IsTrue(eventDB.IsCoursePresent(CourseId(6)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(201)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(202)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(208)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(212)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(213)));

            undomgr.BeginCommand(1983, "DeleteCourse");
            ChangeEvent.DeleteCourse(eventDB, CourseId(6));
            undomgr.EndCommand(1983);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCoursePresent(CourseId(6)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(201)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(202)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(208)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(212)));
            Assert.IsFalse(eventDB.IsCourseControlPresent(CourseControlId(213)));

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsCoursePresent(CourseId(6)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(201)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(202)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(208)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(212)));
            Assert.IsTrue(eventDB.IsCourseControlPresent(CourseControlId(213)));
        }

        [TestMethod]
        public void DeleteCourse2()
        {
            // Make sure specials are affected properly.
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.IsTrue(eventDB.IsCoursePresent(CourseId(3)));

            undomgr.BeginCommand(1983, "DeleteCourse");
            ChangeEvent.DeleteCourse(eventDB, CourseId(3));
            undomgr.EndCommand(1983);
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCoursePresent(CourseId(3)));
            Assert.IsFalse(eventDB.IsSpecialPresent(SpecialId(3)));  // only on course 3, so should be gone.
            Special sp2 = eventDB.GetSpecial(SpecialId(2));
            Assert.AreEqual(3, sp2.courses.Length);
            Assert.AreEqual(1, sp2.courses[0].CourseId.id);
            Assert.AreEqual(true, sp2.courses[0].AllParts);
            Assert.AreEqual(2, sp2.courses[1].CourseId.id);
            Assert.AreEqual(true, sp2.courses[1].AllParts);
            Assert.AreEqual(6, sp2.courses[2].CourseId.id);
            Assert.AreEqual(true, sp2.courses[2].AllParts);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsCoursePresent(CourseId(3)));
            Assert.IsTrue(eventDB.IsSpecialPresent(SpecialId(3)));
            Special sp3 = eventDB.GetSpecial(SpecialId(3));
            Assert.AreEqual(1, sp3.courses.Length);
            Assert.AreEqual(3, sp3.courses[0].CourseId.id);
            sp2 = eventDB.GetSpecial(SpecialId(2));
            Assert.AreEqual(4, sp2.courses.Length);
            Assert.AreEqual(1, sp2.courses[0].CourseId.id);
            Assert.AreEqual(2, sp2.courses[1].CourseId.id);
            Assert.AreEqual(3, sp2.courses[2].CourseId.id);
            Assert.AreEqual(6, sp2.courses[3].CourseId.id);
        }

        [TestMethod]
        public void DeleteCourse3()
        {
            Setup("changeevent\\mapexchange1.ppen");

            undomgr.BeginCommand(13, "delete course");
            ChangeEvent.DeleteCourse(eventDB, CourseId(6));
            ChangeEvent.DeleteCourse(eventDB, CourseId(3));
            undomgr.EndCommand(13);
            eventDB.Validate();

            Special special = eventDB.GetSpecial(SpecialId(3));
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { new CourseDesignator(CourseId(2), 1) });

            special = eventDB.GetSpecial(SpecialId(1));
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { Designator(0) });

            special = eventDB.GetSpecial(SpecialId(5));
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { Designator(2), Designator(4) });

            Assert.IsFalse(eventDB.IsSpecialPresent(SpecialId(6)));

            undomgr.Undo();
            eventDB.Validate();
        }

        [TestMethod]
        public void CreateCourse()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(77, "Remove extra start/finish");
            ChangeEvent.RemoveControl(eventDB, ControlId(23));
            ChangeEvent.RemoveControl(eventDB, ControlId(24));
            undomgr.EndCommand(77);

            undomgr.BeginCommand(183, "Create Course");
            Id<Course> courseId1 = ChangeEvent.CreateCourse(eventDB, CourseKind.Normal, "My New Course", ControlLabelKind.SequenceAndCode, -1, "Secondary Title", 15000, 25, null, DescriptionKind.Symbols, 1, addStartAndFinish: true, hideFromReports: false); 
            Id<Course> courseId2 = ChangeEvent.CreateCourse(eventDB, CourseKind.Score, "My New Course 2", ControlLabelKind.Sequence, 1, null, 7600, -1, null, DescriptionKind.Text, 1, addStartAndFinish: true, hideFromReports: true);
            Id<Course> courseId3 = ChangeEvent.CreateCourse(eventDB, CourseKind.Normal, "My New Course 3", ControlLabelKind.Sequence, -1, null, 7500, 101, 7500F, DescriptionKind.SymbolsAndText, 7, addStartAndFinish: false, hideFromReports: false);
            undomgr.EndCommand(183);
            eventDB.Validate();

            Course course;

            course = eventDB.GetCourse(courseId1);
            Assert.AreEqual(CourseKind.Normal, course.kind);
            Assert.AreEqual("My New Course", course.name);
            Assert.AreEqual("Secondary Title", course.secondaryTitle);
            Assert.AreEqual(DescriptionKind.Symbols, course.descKind);
            Assert.AreEqual(15000F, course.printScale);
            Assert.AreEqual(25F, course.climb);
            Assert.AreEqual(null, course.overrideCourseLength);
            Assert.AreEqual(1, course.firstControlOrdinal);
            Assert.AreEqual(ControlLabelKind.SequenceAndCode, course.labelKind);
            Assert.AreEqual(-1, course.scoreColumn);
            Assert.IsFalse(course.hideFromReports);
            Assert.AreEqual(1, eventDB.GetCourseControl(course.firstCourseControl).control.id);
            Assert.AreEqual(6, eventDB.GetCourseControl(eventDB.GetCourseControl(course.firstCourseControl).nextCourseControl).control.id);

            course = eventDB.GetCourse(courseId2);
            Assert.AreEqual(CourseKind.Score, course.kind);
            Assert.AreEqual("My New Course 2", course.name);
            Assert.AreEqual(null, course.secondaryTitle);
            Assert.AreEqual(DescriptionKind.Text, course.descKind);
            Assert.AreEqual(7600F, course.printScale);
            Assert.IsTrue(course.climb < 0);
            Assert.AreEqual(null, course.overrideCourseLength);
            Assert.AreEqual(1, course.firstControlOrdinal);
            Assert.AreEqual(ControlLabelKind.Sequence, course.labelKind);
            Assert.AreEqual(1, course.scoreColumn);
            Assert.IsTrue(course.hideFromReports);
            Assert.AreEqual(1, eventDB.GetCourseControl(course.firstCourseControl).control.id);
            Assert.AreEqual(6, eventDB.GetCourseControl(eventDB.GetCourseControl(course.firstCourseControl).nextCourseControl).control.id);

            course = eventDB.GetCourse(courseId3);
            Assert.AreEqual(CourseKind.Normal, course.kind);
            Assert.AreEqual("My New Course 3", course.name);
            Assert.AreEqual(null, course.secondaryTitle);
            Assert.AreEqual(DescriptionKind.SymbolsAndText, course.descKind);
            Assert.AreEqual(7500F, course.printScale);
            Assert.AreEqual(101F, course.climb);
            Assert.AreEqual(7500F, course.overrideCourseLength);
            Assert.AreEqual(7, course.firstControlOrdinal);
            Assert.AreEqual(ControlLabelKind.Sequence, course.labelKind);
            Assert.AreEqual(-1, course.scoreColumn);
            Assert.IsFalse(course.hideFromReports);
            Assert.IsTrue(course.firstCourseControl.IsNone);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsCoursePresent(courseId1));
            Assert.IsFalse(eventDB.IsCoursePresent(courseId2));
            Assert.IsFalse(eventDB.IsCoursePresent(courseId3));
        }

        [TestMethod]
        public void ChangeSpecialLocation()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(11, "move special");
            ChangeEvent.ChangeSpecialLocations(eventDB, SpecialId(1), new PointF[1] { new PointF(12.1F, -38.1F) });
            ChangeEvent.ChangeSpecialLocations(eventDB, SpecialId(4), new PointF[5] { new PointF(5, 3), new PointF(1, 2), new PointF(6, 4), new PointF(11, 1), new PointF(5, 3) });
            undomgr.EndCommand(11);
            eventDB.Validate();

            Assert.AreEqual(1, eventDB.GetSpecial(SpecialId(1)).locations.Length);
            Assert.AreEqual(new PointF(12.1F, -38.1F), eventDB.GetSpecial(SpecialId(1)).locations[0]);
            Assert.AreEqual(5, eventDB.GetSpecial(SpecialId(4)).locations.Length);
            Assert.AreEqual(new PointF(5, 3), eventDB.GetSpecial(SpecialId(4)).locations[0]);
            Assert.AreEqual(new PointF(1, 2), eventDB.GetSpecial(SpecialId(4)).locations[1]);
            Assert.AreEqual(new PointF(6, 4), eventDB.GetSpecial(SpecialId(4)).locations[2]);
            Assert.AreEqual(new PointF(11, 1), eventDB.GetSpecial(SpecialId(4)).locations[3]);
            Assert.AreEqual(new PointF(5, 3), eventDB.GetSpecial(SpecialId(4)).locations[4]);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(1, eventDB.GetSpecial(SpecialId(1)).locations.Length);
            Assert.AreEqual(new PointF(14.5F, 31.2F), eventDB.GetSpecial(SpecialId(1)).locations[0]);
            Assert.AreEqual(4, eventDB.GetSpecial(SpecialId(4)).locations.Length);
            Assert.AreEqual(new PointF(3, 7), eventDB.GetSpecial(SpecialId(4)).locations[0]);
        }

        [TestMethod]
        public void ChangeSpecialOrientation()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual(45F, eventDB.GetSpecial(SpecialId(2)).orientation);

            undomgr.BeginCommand(11, "rotate special");
            ChangeEvent.ChangeSpecialOrientation(eventDB, SpecialId(2), 237.4F);
            undomgr.EndCommand(11);
            eventDB.Validate();

            Assert.AreEqual(237.4F, eventDB.GetSpecial(SpecialId(2)).orientation);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(45F, eventDB.GetSpecial(SpecialId(2)).orientation);
        }

        [TestMethod]
        public void ChangeSpecialStretch()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual(0F, eventDB.GetSpecial(SpecialId(2)).stretch);

            undomgr.BeginCommand(1154, "stretch special");
            ChangeEvent.ChangeSpecialStretch(eventDB, SpecialId(2), 2.3F);
            undomgr.EndCommand(1154);
            eventDB.Validate();

            Assert.AreEqual(2.3F, eventDB.GetSpecial(SpecialId(2)).stretch);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(0, eventDB.GetSpecial(SpecialId(2)).stretch);
        }

        [TestMethod]
        public void ChangeSpecialText()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual("$(CourseName)", eventDB.GetSpecial(SpecialId(7)).text);

            undomgr.BeginCommand(11, "change text");
            ChangeEvent.ChangeSpecialText(eventDB, SpecialId(7), "Mr. Mr.", "Times New Roman", true, true, new SpecialColor(0.4F, 0.5F, 0.1F, 0.2F), -1);
            undomgr.EndCommand(11);
            eventDB.Validate();

            Assert.AreEqual("Mr. Mr.", eventDB.GetSpecial(SpecialId(7)).text);
            Assert.AreEqual("Times New Roman", eventDB.GetSpecial(SpecialId(7)).fontName);
            Assert.AreEqual(true, eventDB.GetSpecial(SpecialId(7)).fontBold);
            Assert.AreEqual(true, eventDB.GetSpecial(SpecialId(7)).fontItalic);
            Assert.AreEqual(new SpecialColor(0.4F, 0.5F, 0.1F, 0.2F), eventDB.GetSpecial(SpecialId(7)).color);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual("$(CourseName)", eventDB.GetSpecial(SpecialId(7)).text);
        }

        [TestMethod]
        public void ChangeSpecialLineAppearance1()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual(SpecialKind.Line, eventDB.GetSpecial(SpecialId(8)).kind);
            Assert.AreEqual(LineKind.Single, eventDB.GetSpecial(SpecialId(8)).lineKind);

            undomgr.BeginCommand(11, "change line appearance");
            ChangeEvent.ChangeSpecialLineAppearance(eventDB, SpecialId(8), new SpecialColor(0.4F, 0.5F, 0.1F, 0.2F), LineKind.Dashed, 0.77F, 1.33F, 1.22F, 3.4F);
            undomgr.EndCommand(11);
            eventDB.Validate();

            Assert.AreEqual(new SpecialColor(0.4F, 0.5F, 0.1F, 0.2F), eventDB.GetSpecial(SpecialId(8)).color);
            Assert.AreEqual(LineKind.Dashed, eventDB.GetSpecial(SpecialId(8)).lineKind);
            Assert.AreEqual(0.77F, eventDB.GetSpecial(SpecialId(8)).lineWidth);
            Assert.AreEqual(1.33F, eventDB.GetSpecial(SpecialId(8)).gapSize);
            Assert.AreEqual(1.22F, eventDB.GetSpecial(SpecialId(8)).dashSize);
            Assert.AreEqual(0, eventDB.GetSpecial(SpecialId(8)).cornerRadius);  // corner radius not changed on line

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(LineKind.Single, eventDB.GetSpecial(SpecialId(8)).lineKind);
        }

        [TestMethod]
        public void ChangeSpecialLineAppearance2()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.AreEqual(SpecialKind.Rectangle, eventDB.GetSpecial(SpecialId(9)).kind);
            Assert.AreEqual(LineKind.Single, eventDB.GetSpecial(SpecialId(9)).lineKind);

            undomgr.BeginCommand(11, "change line appearance");
            ChangeEvent.ChangeSpecialLineAppearance(eventDB, SpecialId(9), new SpecialColor(0.4F, 0.5F, 0.1F, 0.2F), LineKind.Double, 0.77F, 1.33F, 0F, 3.4F);
            undomgr.EndCommand(11);
            eventDB.Validate();

            Assert.AreEqual(new SpecialColor(0.4F, 0.5F, 0.1F, 0.2F), eventDB.GetSpecial(SpecialId(9)).color);
            Assert.AreEqual(LineKind.Double, eventDB.GetSpecial(SpecialId(9)).lineKind);
            Assert.AreEqual(0.77F, eventDB.GetSpecial(SpecialId(9)).lineWidth);
            Assert.AreEqual(1.33F, eventDB.GetSpecial(SpecialId(9)).gapSize);
            Assert.AreEqual(0F, eventDB.GetSpecial(SpecialId(9)).dashSize);
            Assert.AreEqual(3.4F, eventDB.GetSpecial(SpecialId(9)).cornerRadius);

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(LineKind.Single, eventDB.GetSpecial(SpecialId(9)).lineKind);
        }

        [TestMethod]
        public void ChangeDescriptionColumns()
        {
            Setup("changeevent\\mapexchange1.ppen");

            Assert.AreEqual(1, QueryEvent.GetDescriptionColumns(eventDB, SpecialId(1)));

            undomgr.BeginCommand(11, "change columns");
            ChangeEvent.ChangeDescriptionColumns(eventDB, SpecialId(1), 3);
            undomgr.EndCommand(11);
            eventDB.Validate();

            Assert.AreEqual(3, QueryEvent.GetDescriptionColumns(eventDB, SpecialId(1)));

            undomgr.Undo();
            eventDB.Validate();

            Assert.AreEqual(1, QueryEvent.GetDescriptionColumns(eventDB, SpecialId(1)));
        }

        [TestMethod]
        public void DeleteSpecial()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            Assert.IsTrue(eventDB.IsSpecialPresent(SpecialId(4)));

            undomgr.BeginCommand(12, "delete special");
            ChangeEvent.DeleteSpecial(eventDB, SpecialId(4));
            undomgr.EndCommand(12);

            Assert.IsFalse(eventDB.IsSpecialPresent(SpecialId(4)));

            undomgr.Undo();

            Assert.IsTrue(eventDB.IsSpecialPresent(SpecialId(4)));
        }

        [TestMethod]
        public void AddPointSpecial()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add special");
            Id<Special> newSpecialId = ChangeEvent.AddPointSpecial(eventDB, SpecialKind.OptCrossing, new PointF(16.4F, -12.3F), 45F);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(1, special.locations.Length);
            Assert.AreEqual(new PointF(16.4F, -12.3F), special.locations[0]);
            Assert.AreEqual(SpecialKind.OptCrossing, special.kind);
            Assert.AreEqual(45F, special.orientation);
            Assert.IsTrue(special.allCourses);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddAreaSpecial()
        {
            PointF[] locations = { new PointF(10, 10), new PointF(20, 40), new PointF(40, 22), new PointF(-5, 18) };
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add special");
            Id<Special> newSpecialId = ChangeEvent.AddLineAreaSpecial(eventDB, SpecialKind.OOB, locations);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(4, special.locations.Length);
            for (int i = 0; i < special.locations.Length; ++i)
                Assert.AreEqual(locations[i], special.locations[i]);
            Assert.AreEqual(SpecialKind.OOB, special.kind);
            Assert.AreEqual(0, special.orientation);
            Assert.IsTrue(special.allCourses);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddBoundarySpecial()
        {
            PointF[] locations = { new PointF(10, 10), new PointF(20, 40), new PointF(17, 66) };
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add special");
            Id<Special> newSpecialId = ChangeEvent.AddLineAreaSpecial(eventDB, SpecialKind.Boundary, locations);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(3, special.locations.Length);
            for (int i = 0; i < special.locations.Length; ++i)
                Assert.AreEqual(locations[i], special.locations[i]);
            Assert.AreEqual(SpecialKind.Boundary, special.kind);
            Assert.AreEqual(0, special.orientation);
            Assert.IsTrue(special.allCourses);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddLineSpecial()
        {
            PointF[] locations = { new PointF(10, 10), new PointF(20, 40), new PointF(17, 66) };
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add special");
            Id<Special> newSpecialId = ChangeEvent.AddLineSpecial(eventDB, locations, SpecialColor.Black, LineKind.Dashed, 1.2F, 2.3F, 3.7F);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(3, special.locations.Length);
            for (int i = 0; i < special.locations.Length; ++i)
                Assert.AreEqual(locations[i], special.locations[i]);
            Assert.AreEqual(SpecialKind.Line, special.kind);
            Assert.AreEqual(0, special.orientation);
            Assert.IsTrue(special.allCourses);
            Assert.AreEqual(SpecialColor.Black, special.color);
            Assert.AreEqual(LineKind.Dashed, special.lineKind);
            Assert.AreEqual(1.2F, special.lineWidth);
            Assert.AreEqual(2.3F, special.gapSize);
            Assert.AreEqual(3.7F, special.dashSize);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddRectangleSpecial()
        {
            RectangleF rect = new RectangleF(20, 40, 17, 66);
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add special");
            Id<Special> newSpecialId = ChangeEvent.AddRectangleSpecial(eventDB, rect, false, new SpecialColor(0.4F, 0.5F, 0.2F, 0.1F), LineKind.Dashed, 1.2F, 2.3F, 3.7F, 3.3F);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(2, special.locations.Length);
            Assert.AreEqual(rect, Geometry.RectFromPoints(special.locations[0].X, special.locations[0].Y, special.locations[1].X, special.locations[1].Y));
            Assert.AreEqual(SpecialKind.Rectangle, special.kind);
            Assert.AreEqual(0, special.orientation);
            Assert.IsTrue(special.allCourses);
            Assert.AreEqual(new SpecialColor(0.4F, 0.5F, 0.2F, 0.1F), special.color);
            Assert.AreEqual(LineKind.Dashed, special.lineKind);
            Assert.AreEqual(1.2F, special.lineWidth);
            Assert.AreEqual(2.3F, special.gapSize);
            Assert.AreEqual(3.7F, special.dashSize);
            Assert.AreEqual(3.3F, special.cornerRadius);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddDescription1()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add description");
            Id<Special> newSpecialId = ChangeEvent.AddDescription(eventDB, true, null, new PointF(30, 40), 4.5F, 2);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(2, special.locations.Length);
            Assert.AreEqual(new PointF(30, 40), special.locations[0]);
            Assert.AreEqual(new PointF(34.5F, 40F), special.locations[1]);
            Assert.AreEqual(SpecialKind.Descriptions, special.kind);
            Assert.AreEqual(0F, special.orientation);
            Assert.AreEqual(2, special.fragments[0].numColumns);
            Assert.IsTrue(special.allCourses);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddDescription2()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add description");
            Id<Special> newSpecialId = ChangeEvent.AddDescription(eventDB, false, new CourseDesignator[] { Designator(1), Designator(3) }, new PointF(0, -25.5F), 5F, 1);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(2, special.locations.Length);
            Assert.AreEqual(new PointF(0, -25.5F), special.locations[0]);
            Assert.AreEqual(new PointF(5, -25.5F), special.locations[1]);
            Assert.AreEqual(SpecialKind.Descriptions, special.kind);
            Assert.AreEqual(0F, special.orientation);
            Assert.AreEqual(1, special.fragments[0].numColumns);
            Assert.IsFalse(special.allCourses);
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { Designator(1), Designator(3) });

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }


        [TestMethod]
        public void AddDescription3()
        {
            Setup("changeevent\\mapexchange1.ppen");

            undomgr.BeginCommand(13, "add description");
            Id<Special> newSpecialId = ChangeEvent.AddDescription(eventDB, false, new CourseDesignator[] { new CourseDesignator(CourseId(2), 1), Designator(6) }, new PointF(0, -25.5F), 5F, 1);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { new CourseDesignator(CourseId(2), 1), Designator(6) });

            special = eventDB.GetSpecial(SpecialId(5));
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { new CourseDesignator(CourseId(2), 0), Designator(4) });

            Assert.IsFalse(eventDB.IsSpecialPresent(SpecialId(6)));

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddDescription4()
        {
            Setup("changeevent\\mapexchange1.ppen");

            undomgr.BeginCommand(13, "add description");
            Id<Special> newSpecialId = ChangeEvent.AddDescription(eventDB, false, new CourseDesignator[] { new CourseDesignator(CourseId(6), 3), Designator(0) }, new PointF(0, -25.5F), 5F, 1);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { new CourseDesignator(CourseId(6), 3), Designator(0) });

            special = eventDB.GetSpecial(SpecialId(5));
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { Designator(2), Designator(4) });

            special = eventDB.GetSpecial(SpecialId(6));
            TestUtil.TestEnumerableAnyOrder(special.courses, new CourseDesignator[] { new CourseDesignator(CourseId(6), 1) });

            Assert.IsFalse(eventDB.IsSpecialPresent(SpecialId(1)));

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }

        [TestMethod]
        public void AddTextSpecial()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(13, "add text special");
            Id<Special> newSpecialId = ChangeEvent.AddTextSpecial(eventDB, new RectangleF(10, 20, 30, 40), "hello $(CourseName)", "Arial", true, true, new SpecialColor(0.5F, 0.8F, 0.2F, 0.3F), 4.5F);
            undomgr.EndCommand(13);
            eventDB.Validate();

            Assert.IsTrue(eventDB.IsSpecialPresent(newSpecialId));
            Special special = eventDB.GetSpecial(newSpecialId);
            Assert.AreEqual(2, special.locations.Length);
            Assert.AreEqual(new PointF(10, 60), special.locations[0]);
            Assert.AreEqual(new PointF(40, 20), special.locations[1]);
            Assert.AreEqual(SpecialKind.Text, special.kind);
            Assert.AreEqual(0, special.orientation);
            Assert.AreEqual("Arial", special.fontName);
            Assert.AreEqual("hello $(CourseName)", special.text);
            Assert.AreEqual(new SpecialColor(0.5F, 0.8F, 0.2F, 0.3F), special.color);
            Assert.IsTrue(special.fontBold);
            Assert.IsTrue(special.fontItalic);
            Assert.IsTrue(special.allCourses);
            Assert.IsTrue(special.color.Equals(new SpecialColor(0.5F, 0.8F, 0.2F, 0.3F)));
            Assert.AreEqual(4.5F, special.fontHeight);

            undomgr.Undo();
            eventDB.Validate();

            Assert.IsFalse(eventDB.IsSpecialPresent(newSpecialId));
        }


        [TestMethod]
        public void ChangeFlagging()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs.coursescribe");

            undomgr.BeginCommand(4112, "Change flagging");
            ChangeEvent.ChangeFlagging(eventDB, ControlId(5), ControlId(6), FlaggingKind.All);
            undomgr.EndCommand(4112);
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(5), ControlId(6));
            Assert.IsFalse(legId.IsNone);
            Assert.AreNotEqual("14.1", eventDB.GetControl(ControlId(6)).symbolIds[0]);
            Assert.AreEqual(FlaggingKind.All, QueryEvent.GetLegFlagging(eventDB, ControlId(5), ControlId(6)));

            undomgr.BeginCommand(4112, "Change flagging");
            ChangeEvent.ChangeFlagging(eventDB, ControlId(1), ControlId(2), FlaggingKind.All);
            undomgr.EndCommand(4112);
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(1), ControlId(2));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(FlaggingKind.All, leg.flagging);
            Assert.AreEqual(FlaggingKind.All, QueryEvent.GetLegFlagging(eventDB, ControlId(1), ControlId(2)));

            undomgr.BeginCommand(4112, "Change flagging");
            ChangeEvent.ChangeFlagging(eventDB, ControlId(2), ControlId(3), FlaggingKind.Begin);
            undomgr.EndCommand(4112);
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(2), ControlId(3));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(FlaggingKind.Begin, leg.flagging);
            Assert.AreEqual(FlaggingKind.Begin, QueryEvent.GetLegFlagging(eventDB, ControlId(2), ControlId(3)));
            Assert.AreEqual(22.95F, leg.bends[0].X);
            Assert.AreEqual(-7.45F, leg.bends[0].Y);

            undomgr.BeginCommand(4112, "Change flagging");
            ChangeEvent.ChangeFlagging(eventDB, ControlId(1), ControlId(2), FlaggingKind.None);
            undomgr.EndCommand(4112);
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(1), ControlId(2));
            Assert.IsTrue(legId.IsNone);
            Assert.AreEqual(FlaggingKind.None, QueryEvent.GetLegFlagging(eventDB, ControlId(1), ControlId(2)));

            undomgr.BeginCommand(4112, "Change flagging");
            ChangeEvent.ChangeFlagging(eventDB, ControlId(5), ControlId(6), FlaggingKind.None);
            undomgr.EndCommand(4112);
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(5), ControlId(6));
            Assert.AreEqual("14.3", eventDB.GetControl(ControlId(6)).symbolIds[0]);
            Assert.AreEqual(FlaggingKind.None, QueryEvent.GetLegFlagging(eventDB, ControlId(5), ControlId(6)));

            undomgr.BeginCommand(4112, "Change flagging");
            ChangeEvent.ChangeFlagging(eventDB, ControlId(5), ControlId(6), FlaggingKind.End);
            undomgr.EndCommand(4112);
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(5), ControlId(6));
            Assert.AreNotEqual("14.2", eventDB.GetControl(ControlId(6)).symbolIds[0]);
            Assert.AreEqual(FlaggingKind.End, QueryEvent.GetLegFlagging(eventDB, ControlId(5), ControlId(6)));

        }

        [TestMethod]
        public void ChangeDisplayedCourses()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(4117, "Change displayed courses");
            ChangeEvent.ChangeDisplayedCourses(eventDB, SpecialId(3), new CourseDesignator[] { Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            ChangeEvent.ChangeDisplayedCourses(eventDB, SpecialId(4), new CourseDesignator[] { Designator(1), Designator(2) });
            ChangeEvent.ChangeDisplayedCourses(eventDB, SpecialId(5), new CourseDesignator[] { Designator(0) });
            ChangeEvent.ChangeDisplayedCourses(eventDB, SpecialId(6), new CourseDesignator[] { Designator(0), Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            undomgr.EndCommand(4117);

            eventDB.Validate();

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(3), true), new CourseDesignator[] { Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            Assert.IsFalse(eventDB.GetSpecial(SpecialId(3)).allCourses);

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(4), true), new CourseDesignator[] { Designator(1), Designator(2) });
            Assert.IsFalse(eventDB.GetSpecial(SpecialId(4)).allCourses);

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(5), true), new CourseDesignator[] { Designator(0) });
            Assert.IsFalse(eventDB.GetSpecial(SpecialId(5)).allCourses);

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(6), true), new CourseDesignator[] { Designator(0), Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            Assert.IsTrue(eventDB.GetSpecial(SpecialId(6)).allCourses);

            undomgr.Undo();
            eventDB.Validate();

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(3), true), new CourseDesignator[] { Designator(3) });
            Assert.IsFalse(eventDB.GetSpecial(SpecialId(3)).allCourses);

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(4), true), new CourseDesignator[] { Designator(0), Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            Assert.IsTrue(eventDB.GetSpecial(SpecialId(4)).allCourses);

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(5), true), new CourseDesignator[] { Designator(5) });
            Assert.IsFalse(eventDB.GetSpecial(SpecialId(5)).allCourses);

        }


        [TestMethod]
        public void ChangeDisplayedCourse2()
        {
            Setup("changeevent\\mapexchange1.ppen");

            undomgr.BeginCommand(4117, "Change displayed courses");
            ChangeEvent.ChangeDisplayedCourses(eventDB, SpecialId(5), new CourseDesignator[] { Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            undomgr.EndCommand(4117);

            eventDB.Validate();

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(5), true), new CourseDesignator[] { Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            Assert.IsFalse(eventDB.GetSpecial(SpecialId(5)).allCourses);

            undomgr.Undo();
            eventDB.Validate();

            undomgr.BeginCommand(4117, "Change displayed courses");
            ChangeEvent.ChangeDisplayedCourses(eventDB, SpecialId(6), new CourseDesignator[] { Designator(0), Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            undomgr.EndCommand(4117);

            eventDB.Validate();

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, SpecialId(6), true), new CourseDesignator[] { Designator(0), Designator(1), Designator(2), Designator(3), Designator(4), Designator(5), Designator(6) });
            Assert.IsTrue(eventDB.GetSpecial(SpecialId(6)).allCourses);

            undomgr.Undo();
            eventDB.Validate();
        }


        [TestMethod]
        public void SingleDescriptionPerCourse()
        {
            Id<Special> desc1, desc2, desc3;
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(7126, "Add descriptions");

            desc1 = ChangeEvent.AddDescription(eventDB, true, null, new PointF(0, 0), 5, 1);
            desc2 = ChangeEvent.AddDescription(eventDB, false, new CourseDesignator[] { Designator(3), Designator(5) }, new PointF(0, 0), 5, 1);
            desc3 = ChangeEvent.AddDescription(eventDB, false, new CourseDesignator[] { Designator(5), Designator(1) }, new PointF(0, 0), 5, 1);

            undomgr.EndCommand(7126);

            eventDB.Validate();
            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, desc1, true), new CourseDesignator[] { Designator(0), Designator(2), Designator(4), Designator(6) });
            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, desc2, true), new CourseDesignator[] { Designator(3) });
            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, desc3, true), new CourseDesignator[] { Designator(1), Designator(5) });

            undomgr.BeginCommand(7126, "Change descriptions");

            ChangeEvent.ChangeDisplayedCourses(eventDB, desc1, new CourseDesignator[] { Designator(3), Designator(6) });

            undomgr.EndCommand(7126);

            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, desc1, true), new CourseDesignator[] { Designator(3), Designator(6) });
            Assert.IsFalse(eventDB.IsSpecialPresent(desc2));
            TestUtil.TestEnumerableAnyOrder(QueryEvent.GetSpecialDisplayedCourses(eventDB, desc3, true), new CourseDesignator[] { Designator(1), Designator(5) });
        }


        [TestMethod]
        public void MoveLegBend()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs2.coursescribe");

            undomgr.BeginCommand(4116, "Change legs");
            ChangeEvent.MoveLegBend(eventDB, ControlId(4), ControlId(5), new PointF(35, 41), new PointF(54, 13));
            ChangeEvent.MoveLegBend(eventDB, ControlId(3), ControlId(4), new PointF(12, 20), new PointF(17, 21));
            undomgr.EndCommand(4116);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(4), ControlId(5));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(2, leg.bends.Length);
            Assert.AreEqual(new PointF(54, 13), leg.bends[0]);
            Assert.AreEqual(new PointF(50, 30), leg.bends[1]);

            legId = QueryEvent.FindLeg(eventDB, ControlId(3), ControlId(4));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(1, leg.bends.Length);
            Assert.AreEqual(new PointF(17, 21), leg.bends[0]);
            Assert.AreEqual(new PointF(17, 21), leg.flagStartStop);

            undomgr.Undo();
            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(4), ControlId(5));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(2, leg.bends.Length);
            Assert.AreEqual(new PointF(35, 41), leg.bends[0]);
            Assert.AreEqual(new PointF(50, 30), leg.bends[1]);

            legId = QueryEvent.FindLeg(eventDB, ControlId(3), ControlId(4));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(1, leg.bends.Length);
            Assert.AreEqual(new PointF(12, 20), leg.bends[0]);
            Assert.AreEqual(new PointF(12, 20), leg.flagStartStop);
        }

        [TestMethod]
        public void AddLegBend()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs2.coursescribe");

            undomgr.BeginCommand(4129, "Change legs");
            ChangeEvent.AddLegBend(eventDB, ControlId(4), ControlId(5), new PointF(31, 41));
            ChangeEvent.AddLegBend(eventDB, ControlId(5), ControlId(6), new PointF(106, 20));
            ChangeEvent.AddLegBend(eventDB, ControlId(5), ControlId(6), new PointF(88, 33));
            ChangeEvent.AddLegBend(eventDB, ControlId(1), ControlId(2), new PointF(49, 0));
            undomgr.EndCommand(4129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(4), ControlId(5));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(3, leg.bends.Length);
            Assert.AreEqual(new PointF(31, 41), leg.bends[0]);
            Assert.AreEqual(new PointF(35, 41), leg.bends[1]);
            Assert.AreEqual(new PointF(50, 30), leg.bends[2]);

            legId = QueryEvent.FindLeg(eventDB, ControlId(5), ControlId(6));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(4, leg.bends.Length);
            Assert.AreEqual(new PointF(83, 43), leg.bends[0]);
            Assert.AreEqual(new PointF(88, 33), leg.bends[1]);
            Assert.AreEqual(new PointF(103, 30), leg.bends[2]);
            Assert.AreEqual(new PointF(106, 20), leg.bends[3]);

            legId = QueryEvent.FindLeg(eventDB, ControlId(1), ControlId(2));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(1, leg.bends.Length);
            Assert.AreEqual(new PointF(49, 0), leg.bends[0]);
        }

        [TestMethod]
        public void ChangeLegGaps()
        {
            Id<Leg> legId;

            Setup("changeevent\\gappedlegs.coursescribe");

            undomgr.BeginCommand(1129, "Change bends");
            ChangeEvent.ChangeLegGaps(eventDB, ControlId(1), ControlId(2), null);
            undomgr.EndCommand(1129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(1), ControlId(2));
            Assert.IsNull(eventDB.GetLeg(legId).gaps);

            undomgr.BeginCommand(1129, "Change bends");
            ChangeEvent.ChangeLegGaps(eventDB, ControlId(2), ControlId(3), new LegGap[] { new LegGap(5.4F, 1.1F) });
            undomgr.EndCommand(1129);

            legId = QueryEvent.FindLeg(eventDB, ControlId(2), ControlId(3));
            LegGap[] newGaps = eventDB.GetLeg(legId).gaps;
            Assert.AreEqual(1, newGaps.Length);
            Assert.AreEqual(5.4F, newGaps[0].distanceFromStart);
            Assert.AreEqual(1.1F, newGaps[0].length);

            undomgr.BeginCommand(1129, "Change bends");
            ChangeEvent.ChangeLegGaps(eventDB, ControlId(3), ControlId(4), new LegGap[] { new LegGap(5.4F, 1.1F) });
            undomgr.EndCommand(1129);

            legId = QueryEvent.FindLeg(eventDB, ControlId(3), ControlId(4));
            newGaps = eventDB.GetLeg(legId).gaps;
            Assert.AreEqual(1, newGaps.Length);
            Assert.AreEqual(5.4F, newGaps[0].distanceFromStart);
            Assert.AreEqual(1.1F, newGaps[0].length);
        }

        [TestMethod]
        public void AddSpecialCorner()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(7112, "Add corner");
            ChangeEvent.AddSpecialCorner(eventDB, SpecialId(4), new PointF(-5, 5));
            ChangeEvent.AddSpecialCorner(eventDB, SpecialId(4), new PointF(-4, -2));
            ChangeEvent.AddSpecialCorner(eventDB, SpecialId(4), new PointF(12, -1));
            undomgr.EndCommand(7112);

            eventDB.Validate();

            Special special = eventDB.GetSpecial(SpecialId(4));
            PointF[] locations = special.locations;

            Assert.AreEqual(7, locations.Length);
            Assert.AreEqual(new PointF(3, 7), locations[0]);
            Assert.AreEqual(new PointF(11, 2), locations[1]);
            Assert.AreEqual(new PointF(12, -1), locations[2]);
            Assert.AreEqual(new PointF(0, -7), locations[3]);
            Assert.AreEqual(new PointF(-4, -2), locations[4]);
            Assert.AreEqual(new PointF(-12, -3), locations[5]);
            Assert.AreEqual(new PointF(-5, 5), locations[6]);

            undomgr.Undo();
            eventDB.Validate();

            special = eventDB.GetSpecial(SpecialId(4));
            locations = special.locations;

            Assert.AreEqual(4, locations.Length);
            Assert.AreEqual(new PointF(3, 7), locations[0]);
            Assert.AreEqual(new PointF(11, 2), locations[1]);
            Assert.AreEqual(new PointF(0, -7), locations[2]);
            Assert.AreEqual(new PointF(-12, -3), locations[3]);

        }

        [TestMethod]
        public void RemoveSpecialCorner()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            undomgr.BeginCommand(7119, "Remove corner");
            ChangeEvent.RemoveSpecialCorner(eventDB, SpecialId(4), new PointF(3, 7));
            undomgr.EndCommand(7119);

            eventDB.Validate();

            Special special = eventDB.GetSpecial(SpecialId(4));
            PointF[] locations = special.locations;

            Assert.AreEqual(3, locations.Length);
            Assert.AreEqual(new PointF(11, 2), locations[0]);
            Assert.AreEqual(new PointF(0, -7), locations[1]);
            Assert.AreEqual(new PointF(-12, -3), locations[2]);

            undomgr.Undo();
            eventDB.Validate();

            special = eventDB.GetSpecial(SpecialId(4));
            locations = special.locations;

            Assert.AreEqual(4, locations.Length);
            Assert.AreEqual(new PointF(3, 7), locations[0]);
            Assert.AreEqual(new PointF(11, 2), locations[1]);
            Assert.AreEqual(new PointF(0, -7), locations[2]);
            Assert.AreEqual(new PointF(-12, -3), locations[3]);

            undomgr.BeginCommand(7119, "Remove corner");
            ChangeEvent.RemoveSpecialCorner(eventDB, SpecialId(4), new PointF(-12, -3));
            undomgr.EndCommand(7119);

            eventDB.Validate();

            special = eventDB.GetSpecial(SpecialId(4));
            locations = special.locations;

            Assert.AreEqual(3, locations.Length);
            Assert.AreEqual(new PointF(3, 7), locations[0]);
            Assert.AreEqual(new PointF(11, 2), locations[1]);
            Assert.AreEqual(new PointF(0, -7), locations[2]);

        }

        [TestMethod]
        public void RemoveLegBend1()
        {
            Id<Leg> legId;

            Setup("changeevent\\speciallegs3.coursescribe");

            undomgr.BeginCommand(4129, "Remove bends");
            ChangeEvent.RemoveLegBend(eventDB, ControlId(2), ControlId(3), new PointF(20, 0));
            ChangeEvent.RemoveLegBend(eventDB, ControlId(2), ControlId(3), new PointF(18, 8));
            undomgr.EndCommand(4129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(2), ControlId(3));
            Assert.IsTrue(legId.IsNone);
        }

        [TestMethod]
        public void RemoveLegBend2()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs3.coursescribe");

            undomgr.BeginCommand(4129, "Remove bends");
            ChangeEvent.RemoveLegBend(eventDB, ControlId(2), ControlId(3), new PointF(20, 0));
            undomgr.EndCommand(4129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(2), ControlId(3));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(1, leg.bends.Length);
            Assert.AreEqual(new PointF(18, 8), leg.bends[0]);
        }

        [TestMethod]
        public void RemoveLegBend3()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs3.coursescribe");

            undomgr.BeginCommand(4129, "Remove bends");
            ChangeEvent.RemoveLegBend(eventDB, ControlId(3), ControlId(4), new PointF(12, 20));
            undomgr.EndCommand(4129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(3), ControlId(4));
            leg = eventDB.GetLeg(legId);
            Assert.IsNull(leg.bends);
            Assert.AreEqual(FlaggingKind.All, leg.flagging);
        }


        [TestMethod]
        public void RemoveLegBend4()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs3.coursescribe");

            undomgr.BeginCommand(4129, "Remove bends");
            ChangeEvent.RemoveLegBend(eventDB, ControlId(4), ControlId(5), new PointF(55, 35));
            undomgr.EndCommand(4129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(4), ControlId(5));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(FlaggingKind.Begin, leg.flagging);
            Assert.AreEqual(new PointF(50, 30), leg.flagStartStop);
            Assert.AreEqual(1, leg.bends.Length);
            Assert.AreEqual(new PointF(50, 30), leg.bends[0]);
        }


        [TestMethod]
        public void RemoveLegBend5()
        {
            Id<Leg> legId;
            Leg leg;

            Setup("changeevent\\speciallegs3.coursescribe");

            undomgr.BeginCommand(4129, "Remove bends");
            ChangeEvent.RemoveLegBend(eventDB, ControlId(5), ControlId(6), new PointF(83, 43));
            undomgr.EndCommand(4129);

            eventDB.Validate();

            legId = QueryEvent.FindLeg(eventDB, ControlId(5), ControlId(6));
            leg = eventDB.GetLeg(legId);
            Assert.AreEqual(FlaggingKind.Begin, leg.flagging);
            Assert.AreEqual(new PointF(103, 30), leg.flagStartStop);
            Assert.AreEqual(1, leg.bends.Length);
            Assert.AreEqual(new PointF(103, 30), leg.bends[0]);
        }

        [TestMethod]
        public void AddGap()
        {
            CircleGap[] result = ChangeEvent.AddGap(null, Math.Atan2(1, 1.1));
            CollectionAssert.AreEqual(new CircleGap[] { new CircleGap(27.27369F, 57.27369F) }, result);
            result = ChangeEvent.AddGap(null, Math.Atan2(0.05, 1));
            CollectionAssert.AreEqual(new CircleGap[] { new CircleGap(-12.1376038F, 17.8624058F) }, result);
            result = ChangeEvent.AddGap(null, Math.Atan2(-2, 1));
            CollectionAssert.AreEqual(new CircleGap[] { new CircleGap(281.565063F, 311.565063F) }, result);
        }

        [TestMethod]
        public void RemoveGap()
        {
            CircleGap[] result = ChangeEvent.RemoveGap(null, Math.Atan2(1, 1.1));
            Assert.IsNull(result);
            result = ChangeEvent.RemoveGap(CircleGap.ComputeCircleGaps(0xD17FF03F), Math.Atan2(-2, 1));
            CollectionAssert.AreEqual(CircleGap.ComputeCircleGaps(0xDF7FF03F), result);
            result = ChangeEvent.RemoveGap(CircleGap.ComputeCircleGaps(0x7FF7FFFC), Math.Atan2(0.05, 1));
            CollectionAssert.AreEqual(CircleGap.ComputeCircleGaps(0xFFF7FFFF), result);
        }

        [TestMethod]
        public void AutoRenumber()
        {
            UndoMgr undomgr = new UndoMgr(5);
            EventDB eventDB = new EventDB(undomgr);

            Event ev = new Event();

            Id<ControlPoint>[] ids = new Id<ControlPoint>[10];

            undomgr.BeginCommand(123, "Add controls");
            ids[0] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "36", new PointF()));   //36 
            ids[1] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "33", new PointF()));   //33
            ids[2] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "AB", new PointF()));   //40
            ids[3] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "34", new PointF()));   //34
            ids[4] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "31", new PointF()));   //35
            ids[5] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "51", new PointF()));   //39
            ids[6] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "37", new PointF()));   //37
            ids[7] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "38", new PointF()));   //38
            ids[8] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "FA", new PointF()));   //41
            ev.mapScale = 15000;
            ev.firstControlCode = 33;
            ev.disallowInvertibleCodes = false;
            eventDB.ChangeEvent(ev);

            ChangeEvent.AutoRenumberControls(eventDB);
            undomgr.EndCommand(123);

            Assert.AreEqual("36", eventDB.GetControl(ids[0]).code);
            Assert.AreEqual("33", eventDB.GetControl(ids[1]).code);
            Assert.AreEqual("40", eventDB.GetControl(ids[2]).code);
            Assert.AreEqual("34", eventDB.GetControl(ids[3]).code);
            Assert.AreEqual("35", eventDB.GetControl(ids[4]).code);
            Assert.AreEqual("39", eventDB.GetControl(ids[5]).code);
            Assert.AreEqual("37", eventDB.GetControl(ids[6]).code);
            Assert.AreEqual("38", eventDB.GetControl(ids[7]).code);
            Assert.AreEqual("41", eventDB.GetControl(ids[8]).code);
        }

        [TestMethod]
        public void AutoRenumber2()
        {
            UndoMgr undomgr = new UndoMgr(5);
            EventDB eventDB = new EventDB(undomgr);

            Event ev = new Event();

            Id<ControlPoint>[] ids = new Id<ControlPoint>[10];

            undomgr.BeginCommand(123, "Add controls");
            ids[0] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "64", new PointF()));   //64
            ids[1] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "69", new PointF()));   //69
            ids[2] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "67", new PointF()));   //67
            ids[3] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "71", new PointF()));   //71
            ids[4] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "73", new PointF()));   //65
            ids[5] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "61", new PointF()));   //62
            ids[6] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "66", new PointF()));   //63
            ids[7] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "60", new PointF()));   //60 
            ids[8] = eventDB.AddControlPoint(new ControlPoint(ControlPointKind.Normal, "70", new PointF()));   //70
            ev.mapScale = 15000;
            ev.firstControlCode = 60;
            ev.disallowInvertibleCodes = true;
            eventDB.ChangeEvent(ev);

            ChangeEvent.AutoRenumberControls(eventDB);
            undomgr.EndCommand(123);

            Assert.AreEqual("64", eventDB.GetControl(ids[0]).code);
            Assert.AreEqual("69", eventDB.GetControl(ids[1]).code);
            Assert.AreEqual("67", eventDB.GetControl(ids[2]).code);
            Assert.AreEqual("71", eventDB.GetControl(ids[3]).code);
            Assert.AreEqual("65", eventDB.GetControl(ids[4]).code);
            Assert.AreEqual("62", eventDB.GetControl(ids[5]).code);
            Assert.AreEqual("63", eventDB.GetControl(ids[6]).code);
            Assert.AreEqual("60", eventDB.GetControl(ids[7]).code);
            Assert.AreEqual("70", eventDB.GetControl(ids[8]).code);
        }

        [TestMethod]
        public void SetAllPunchPatterns()
        {
            Setup("changeevent\\speciallegs3.coursescribe");

            Dictionary<string, PunchPattern> dict = new Dictionary<string, PunchPattern>();
            PunchPattern pattern;

            pattern = new PunchPattern();
            pattern.size = 9;
            pattern.dots = new bool[9, 9];
            pattern.dots[1, 1] = pattern.dots[4, 6] = pattern.dots[8, 1] = true;
            dict["31"] = pattern;

            dict["32"] = null;

            pattern = new PunchPattern();
            pattern.size = 9;
            pattern.dots = new bool[9, 9];
            pattern.dots[6, 1] = pattern.dots[6, 4] = pattern.dots[6, 7] = true;
            dict["34"] = pattern;

            undomgr.BeginCommand(811, "Set punch patterns");
            ChangeEvent.SetAllPunchPatterns(eventDB, dict);
            undomgr.EndCommand(811);

            ControlPoint control = eventDB.GetControl(ControlId(2));
            Assert.IsNotNull(control.punches);
            Assert.AreEqual(9, control.punches.size);
            Assert.IsTrue(control.punches.dots[1, 1]);
            Assert.IsTrue(control.punches.dots[4, 6]);
            Assert.IsFalse(control.punches.dots[6, 4]);

            control = eventDB.GetControl(ControlId(3));
            Assert.IsNull(control.punches);

            control = eventDB.GetControl(ControlId(5));
            Assert.IsNotNull(control.punches);
            Assert.AreEqual(9, control.punches.size);
            Assert.IsTrue(control.punches.dots[6, 1]);
            Assert.IsTrue(control.punches.dots[6, 4]);
            Assert.IsFalse(control.punches.dots[4, 6]);

            dict.Clear();
            dict["31"] = null;

            pattern = new PunchPattern();
            pattern.size = 9;
            pattern.dots = new bool[9, 9];
            pattern.dots[1, 1] = pattern.dots[4, 4] = pattern.dots[7, 7] = true;
            dict["34"] = pattern;

            undomgr.BeginCommand(811, "Set punch patterns");
            ChangeEvent.SetAllPunchPatterns(eventDB, dict);
            undomgr.EndCommand(811);

            control = eventDB.GetControl(ControlId(2));
            Assert.IsNull(control.punches);

            control = eventDB.GetControl(ControlId(3));
            Assert.IsNull(control.punches);

            control = eventDB.GetControl(ControlId(5));
            Assert.IsNotNull(control.punches);
            Assert.AreEqual(9, control.punches.size);
            Assert.IsTrue(control.punches.dots[1, 1]);
            Assert.IsTrue(control.punches.dots[4, 4]);
            Assert.IsFalse(control.punches.dots[6, 4]);
            Assert.IsFalse(control.punches.dots[4, 6]);

        }

        [TestMethod]
        public void ChangePunchcardFormat()
        {
            Setup("changeevent\\speciallegs3.coursescribe");

            PunchcardFormat format = new PunchcardFormat();
            format.boxesAcross = 6;
            format.boxesDown = 5;
            format.leftToRight = false;
            format.topToBottom = true;

            undomgr.BeginCommand(9111, "Change punch card layout");
            ChangeEvent.ChangePunchcardFormat(eventDB, format);
            undomgr.EndCommand(9111);

            format.boxesAcross = 17;

            PunchcardFormat newFormat = eventDB.GetEvent().punchcardFormat;
            Assert.AreEqual(6, newFormat.boxesAcross);
            Assert.AreEqual(5, newFormat.boxesDown);
            Assert.IsFalse(newFormat.leftToRight);
            Assert.IsTrue(newFormat.topToBottom);
        }

        [TestMethod]
        public void ChangeCourseAppearance()
        {
            Setup("changeevent\\sampleevent3.ppen");

            CourseAppearance courseAppearance = new CourseAppearance();
            courseAppearance.lineWidth = 1.3F;
            courseAppearance.controlCircleSize = 0.9F;
            courseAppearance.numberHeight = 1.1F;
            courseAppearance.numberBold = true;
            courseAppearance.numberRoboto = false;
            courseAppearance.numberOutlineWidth = 0.33F;
            courseAppearance.useDefaultPurple = false;
            courseAppearance.purpleC = 0.4F;
            courseAppearance.purpleM = 0.5F;
            courseAppearance.purpleY = 0.6F;
            courseAppearance.purpleK = 0.74F;

            undomgr.BeginCommand(7712, "Change course appearance");
            ChangeEvent.ChangeCourseAppearance(eventDB, courseAppearance);
            undomgr.EndCommand(7712);

            eventDB.Validate();

            CourseAppearance n = eventDB.GetEvent().courseAppearance;
            Assert.AreEqual(1.3F, n.lineWidth);
            Assert.AreEqual(0.9F, n.controlCircleSize);
            Assert.AreEqual(1.1F, n.numberHeight);
            Assert.AreEqual(true, n.numberBold);
            Assert.AreEqual(false, n.numberRoboto);
            Assert.AreEqual(0.33F, n.numberOutlineWidth);
            Assert.AreEqual(false, n.useDefaultPurple);
            Assert.AreEqual(0.4F, n.purpleC);
            Assert.AreEqual(0.5F, n.purpleM);
            Assert.AreEqual(0.6F, n.purpleY);
            Assert.AreEqual(0.74F, n.purpleK);

            undomgr.Undo();
            eventDB.Validate();

            n = eventDB.GetEvent().courseAppearance;
            Assert.AreEqual(1.0F, n.lineWidth);
            Assert.AreEqual(1.0F, n.controlCircleSize);
            Assert.AreEqual(1.0F, n.numberHeight);
            Assert.AreEqual(false, n.numberBold);
            Assert.AreEqual(true, n.numberRoboto);
            Assert.AreEqual(0, n.numberOutlineWidth);
            Assert.AreEqual(true, n.useDefaultPurple);
        }

        [TestMethod]
        public void ChangeCustomSymbolText()
        {
            Setup("changeevent\\sampleevent3.ppen");

            Dictionary<string, List<SymbolText>> customSymbolText;
            Dictionary<string, bool> customSymbolKey;

            QueryEvent.GetCustomSymbolText(eventDB, out customSymbolText, out customSymbolKey);

            Assert.AreEqual("man-made object", customSymbolText["6.1"][0].Text);
            Assert.AreEqual(true, customSymbolKey["6.1"]);
            Assert.AreEqual("playground equipment", customSymbolText["6.2"][0].Text);
            Assert.AreEqual(false, customSymbolKey["6.2"]);
            Assert.AreEqual("light pole", customSymbolText["5.6"][0].Text);
            Assert.AreEqual(true, customSymbolKey["5.6"]);
            Assert.AreEqual("medical", customSymbolText["12.1"][0].Text);
            Assert.AreEqual(true, customSymbolKey["12.1"]);
            Assert.AreEqual("wet {0}", customSymbolText["8.7"][0].Text);
            Assert.AreEqual(false, customSymbolKey["8.7"]);
            Assert.IsFalse(customSymbolText.ContainsKey("5.23"));

            // Make sure that changes to the retrieved dictionaries don't affect the event.
            SymbolText text = new SymbolText(), text2 = new SymbolText();
            text.Lang = "xx"; text.Text = "hydrant";
            customSymbolText["6.2"] = new List<SymbolText>() { text };
            customSymbolKey["12.1"] = false;
            customSymbolText.Remove("8.7");
            text = new SymbolText(); text2 = new SymbolText();
            text.Lang = "en"; text.Text = "overhang";
            text2.Lang = "en"; text2.Plural = true; text2.Gender = "masculine"; text2.Text = "overhangs";
            customSymbolText.Add("5.23", new List<SymbolText>() { text, text2 });
            customSymbolKey.Add("5.23", true);

            undomgr.BeginCommand(9112, "Change custom symbol text");
            ChangeEvent.ChangeCustomSymbolText(eventDB, customSymbolText, customSymbolKey);
            undomgr.EndCommand(9112);

            customSymbolText.Clear();
            customSymbolKey.Clear();

            QueryEvent.GetCustomSymbolText(eventDB, out customSymbolText, out customSymbolKey);

            Assert.AreEqual("man-made object", customSymbolText["6.1"][0].Text);
            Assert.AreEqual(true, customSymbolKey["6.1"]);
            Assert.AreEqual("hydrant", customSymbolText["6.2"][0].Text);
            Assert.AreEqual(false, customSymbolKey["6.2"]);
            Assert.AreEqual("light pole", customSymbolText["5.6"][0].Text);
            Assert.AreEqual(true, customSymbolKey["5.6"]);
            Assert.AreEqual("medical", customSymbolText["12.1"][0].Text);
            Assert.AreEqual(false, customSymbolKey["12.1"]);
            Assert.AreEqual("overhang", customSymbolText["5.23"][0].Text);
            Assert.AreEqual("overhangs", customSymbolText["5.23"][1].Text);
            Assert.AreEqual(true, customSymbolText["5.23"][1].Plural);
            Assert.AreEqual("masculine", customSymbolText["5.23"][1].Gender);
            Assert.AreEqual(true, customSymbolKey["5.23"]);
            Assert.IsFalse(customSymbolText.ContainsKey("8.7"));

            undomgr.Undo();
            QueryEvent.GetCustomSymbolText(eventDB, out customSymbolText, out customSymbolKey);

            Assert.AreEqual("man-made object", customSymbolText["6.1"][0].Text);
            Assert.AreEqual(true, customSymbolKey["6.1"]);
            Assert.AreEqual("playground equipment", customSymbolText["6.2"][0].Text);
            Assert.AreEqual(false, customSymbolKey["6.2"]);
            Assert.AreEqual("light pole", customSymbolText["5.6"][0].Text);
            Assert.AreEqual(true, customSymbolKey["5.6"]);
            Assert.AreEqual("medical", customSymbolText["12.1"][0].Text);
            Assert.AreEqual(true, customSymbolKey["12.1"]);
            Assert.AreEqual("wet {0}", customSymbolText["8.7"][0].Text);
            Assert.AreEqual(false, customSymbolKey["8.7"]);
            Assert.IsFalse(customSymbolText.ContainsKey("5.23"));
        }


        [TestMethod]
        public void ChangePrintArea()
        {
            Setup("changeevent\\sampleevent7.coursescribe");

            undomgr.BeginCommand(9151, "Change print area");
            ChangeEvent.ChangePrintArea(eventDB, CourseDesignator.AllControls, false, new PrintArea(false, true, new RectangleF(25, 50, 110, 130)));
            undomgr.EndCommand(9151);

            PrintArea result = QueryEvent.GetPrintArea(eventDB, CourseDesignator.AllControls);
            Assert.AreEqual(25, result.printAreaRectangle.Left);
            Assert.AreEqual(50, result.printAreaRectangle.Top);
            Assert.AreEqual(110 + 25, result.printAreaRectangle.Right);
            Assert.AreEqual(130 + 50, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsTrue(result.restrictToPageSize);

            undomgr.BeginCommand(9151, "Change print area");
            ChangeEvent.ChangePrintArea(eventDB, Designator(4), false, new PrintArea(false, false, new RectangleF(35, 50, 110, 150)));
            undomgr.EndCommand(9151);

            result = QueryEvent.GetPrintArea(eventDB, Designator(4));
            Assert.AreEqual(35, result.printAreaRectangle.Left);
            Assert.AreEqual(50, result.printAreaRectangle.Top);
            Assert.AreEqual(110 + 35, result.printAreaRectangle.Right);
            Assert.AreEqual(150 + 50, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsFalse(result.restrictToPageSize);

            undomgr.BeginCommand(9151, "Change print area");
            ChangeEvent.ChangePrintArea(eventDB, Designator(4), false, new PrintArea(true, true, new RectangleF(1, 2, 3, 4)));
            undomgr.EndCommand(9151);

            result = QueryEvent.GetPrintArea(eventDB, Designator(4));
            Assert.IsTrue(result.autoPrintArea);
            Assert.IsTrue(result.restrictToPageSize);
        }

        [TestMethod]
        public void ChangePrintArea2()
        {
            Setup("changeevent\\mapexchange1.ppen");

            undomgr.BeginCommand(9151, "Change print area");
            ChangeEvent.ChangePrintArea(eventDB, CourseDesignator.AllControls, false, new PrintArea(false, true, new RectangleF(25, 50, 110, 130)));
            undomgr.EndCommand(9151);

            PrintArea result = QueryEvent.GetPrintArea(eventDB, CourseDesignator.AllControls);
            Assert.AreEqual(25, result.printAreaRectangle.Left);
            Assert.AreEqual(50, result.printAreaRectangle.Top);
            Assert.AreEqual(110 + 25, result.printAreaRectangle.Right);
            Assert.AreEqual(130 + 50, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsTrue(result.restrictToPageSize);

            undomgr.BeginCommand(9151, "Change print area");
            ChangeEvent.ChangePrintArea(eventDB, new CourseDesignator(CourseId(6), 2), false, new PrintArea(false, false, new RectangleF(35, 50, 110, 150)));
            ChangeEvent.ChangePrintArea(eventDB, new CourseDesignator(CourseId(6)), false, new PrintArea(false, true, new RectangleF(5, 10, 15, 20)));
            undomgr.EndCommand(9151);

            result = QueryEvent.GetPrintArea(eventDB, new CourseDesignator(CourseId(6), 2));
            Assert.AreEqual(35, result.printAreaRectangle.Left);
            Assert.AreEqual(50, result.printAreaRectangle.Top);
            Assert.AreEqual(35 + 110, result.printAreaRectangle.Right);
            Assert.AreEqual(50 + 150, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsFalse(result.restrictToPageSize);


            result = QueryEvent.GetPrintArea(eventDB, new CourseDesignator(CourseId(6)));
            Assert.AreEqual(5, result.printAreaRectangle.Left);
            Assert.AreEqual(10, result.printAreaRectangle.Top);
            Assert.AreEqual(5 + 15, result.printAreaRectangle.Right);
            Assert.AreEqual(10 + 20, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsTrue(result.restrictToPageSize);

            undomgr.BeginCommand(9151, "Change print area");
            ChangeEvent.ChangePrintArea(eventDB, new CourseDesignator(CourseId(6)), true, new PrintArea(false, true, new RectangleF(1, 2, 3, 4)));
            undomgr.EndCommand(9151);

            result = QueryEvent.GetPrintArea(eventDB, new CourseDesignator(CourseId(6), 2));
            Assert.AreEqual(1, result.printAreaRectangle.Left);
            Assert.AreEqual(2, result.printAreaRectangle.Top);
            Assert.AreEqual(1 + 3, result.printAreaRectangle.Right);
            Assert.AreEqual(2 + 4, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsTrue(result.restrictToPageSize);
            result = QueryEvent.GetPrintArea(eventDB, new CourseDesignator(CourseId(6)));
            Assert.AreEqual(1, result.printAreaRectangle.Left);
            Assert.AreEqual(2, result.printAreaRectangle.Top);
            Assert.AreEqual(1 + 3, result.printAreaRectangle.Right);
            Assert.AreEqual(2 + 4, result.printAreaRectangle.Bottom);
            Assert.IsFalse(result.autoPrintArea);
            Assert.IsTrue(result.restrictToPageSize);
        }

        [TestMethod]
        public void ChangePartOptions()
        {
            Setup("changeevent\\mapexchange1.ppen");

            undomgr.BeginCommand(9152, "Change part options");
            ChangeEvent.ChangePartOptions(eventDB, new CourseDesignator(CourseId(6), 1), new PartOptions() { ShowFinish = true });
            ChangeEvent.ChangePartOptions(eventDB, new CourseDesignator(CourseId(6), 0), new PartOptions() { ShowFinish = false });
            undomgr.EndCommand(9152);

            PartOptions result;
            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 0));
            Assert.AreEqual(false, result.ShowFinish);

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 1));
            Assert.AreEqual(true, result.ShowFinish);

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 2));
            Assert.AreEqual(true, result.ShowFinish);

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 3));
            Assert.AreEqual(true, result.ShowFinish);

            undomgr.Undo();

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 0));
            Assert.AreEqual(false, result.ShowFinish);

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 1));
            Assert.AreEqual(false, result.ShowFinish);

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 2));
            Assert.AreEqual(true, result.ShowFinish);

            result = QueryEvent.GetPartOptions(eventDB, new CourseDesignator(CourseId(6), 3));
            Assert.AreEqual(true, result.ShowFinish);

        }

        [TestMethod]
        public void ChangeAllControlsProperties()
        {
            Setup("changeevent\\sampleevent12.ppen");

            undomgr.BeginCommand(7713, "Change all controls properties");
            ChangeEvent.ChangeAllControlsProperties(eventDB, 6400, DescriptionKind.Text);
            undomgr.EndCommand(7713);

            Event ev = eventDB.GetEvent();
            Assert.AreEqual(6400, ev.allControlsPrintScale);
            Assert.AreEqual(DescriptionKind.Text, ev.allControlsDescKind);

            undomgr.Undo();

            ev = eventDB.GetEvent();
            Assert.AreEqual(9000, ev.allControlsPrintScale);
            Assert.AreEqual(DescriptionKind.SymbolsAndText, ev.allControlsDescKind);
        }

        [TestMethod]
        public void AddVariation1()
        {
            Setup("queryevent\\variations.ppen");

            undomgr.BeginCommand(3413, "Add Variation");

            bool result = ChangeEvent.AddVariation(eventDB, Designator(1), CourseControlId(19), false, 3);
            Assert.IsTrue(result);

            undomgr.EndCommand(3413);

            eventDB.Validate();

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(19));
            Assert.IsTrue(cc.split);
            Assert.IsFalse(cc.loop);
            Assert.AreEqual(3, cc.splitCourseControls.Length);

            undomgr.Undo();
            eventDB.Validate();
        }

        [TestMethod]
        public void AddVariation2()
        {
            Setup("queryevent\\variations.ppen");

            VariationInfo.VariationPath variationPath = new VariationInfo.VariationPath(new[] {
                CourseControlId(2),
                CourseControlId(27),
                CourseControlId(30),
                CourseControlId(26),
                CourseControlId(25),
                CourseControlId(4),
                CourseControlId(28),
            });
            VariationInfo variationInfo = (from vi in QueryEvent.GetAllVariations(eventDB, CourseId(1)) where vi.Path.Equals(variationPath) select vi).First();

            CourseDesignator designator = new CourseDesignator(CourseId(1), variationInfo);

            undomgr.BeginCommand(3413, "Add Variation");

            bool result = ChangeEvent.AddVariation(eventDB, designator, CourseControlId(3), true, 2);
            Assert.IsTrue(result);

            undomgr.EndCommand(3413);

            eventDB.Validate();

            CourseControl cc = eventDB.GetCourseControl(CourseControlId(3));
            Assert.IsTrue(cc.split);
            Assert.IsTrue(cc.loop);
            Assert.AreEqual(3, cc.splitCourseControls.Length);

            undomgr.Undo();
            eventDB.Validate();
        }

        string[] CodesInOrder(CourseDesignator designator)
        {
            return (from ccid in QueryEvent.EnumCourseControlIds(eventDB, designator)
                    select eventDB.GetControl(eventDB.GetCourseControl(ccid).control).code).ToArray();
        }

        [TestMethod]
        public void MoveControlInCourse1()
        {
            Setup("changeevent\\sampleevent1.coursescribe");

            string[] expectedBefore = { null, "305", "211", "210", "32", "302", "GO", "303", "74", "191", "189", "190", null };
            string[] expectedAfter = { null, "305", "74", "211", "210", "32", "302", "GO", "303", "191", "189", "190", null };
            CollectionAssert.AreEqual(expectedBefore, CodesInOrder(Designator(6)));

            eventDB.Validate();

            undomgr.BeginCommand(3712, "Move Control");
            ChangeEvent.MoveControlInCourse(eventDB, CourseId(6), CourseControlId(209), CourseControlId(202), CourseControlId(203), LegInsertionLoc.Normal);
            undomgr.EndCommand(3712);

            eventDB.Validate();
            CollectionAssert.AreEqual(expectedAfter, CodesInOrder(Designator(6)));

            undomgr.Undo();
            eventDB.Validate();
            CollectionAssert.AreEqual(expectedBefore, CodesInOrder(Designator(6)));
        }

        [TestMethod]
        public void MoveControlInCourseVariation()
        {
            Setup("queryevent\\variations.ppen");

            string[] expectedBefore = {
                null, "31", "52", "31", "32",
                "33", "40", "41", "33", "42",
                "43", "33", "44", "46", "45",
                "46", "47", "48", "33", "34",
                "35", "36", "37", "38", "49",
                "38", "50", "38", "39", null
            };
            string[] expectedAfter = {
                null, "31", "52", "31", "32",
                "33", "49", "40", "41", "33", "42",
                "43", "33", "44", "46", "45",
                "46", "47", "48", "33", "34",
                "35", "36", "37", "38",
                "38", "50", "38", "39", null
            }; string[] codes = CodesInOrder(Designator(1));
            CollectionAssert.AreEqual(expectedBefore, codes);

            eventDB.Validate();


            undomgr.BeginCommand(3712, "Move Control");
            ChangeEvent.MoveControlInCourse(eventDB, CourseId(1), CourseControlId(13), CourseControlId(25), CourseControlId(15), LegInsertionLoc.Normal);
            undomgr.EndCommand(3712);

            eventDB.Validate();
            CollectionAssert.AreEqual(expectedAfter, CodesInOrder(Designator(1)));

            undomgr.Undo();
            eventDB.Validate();
            CollectionAssert.AreEqual(expectedBefore, CodesInOrder(Designator(1)));

        }

        [TestMethod]
        public void MoveControlInCourseVariation2()
        {
            Setup("changeevent\\variations2.ppen");

            string[] expectedBefore = {
                null, "31", "52", "31", "32",
                "34", "35", "36", "39", null
            };
            string[] expectedAfter = {
                null, "31", "35", "52", "35", "32",
                "34", "36", "39", null
            };
            string[] codes = CodesInOrder(Designator(4));
            var ccids = QueryEvent.EnumCourseControlIds(eventDB, Designator(4)).ToArray();

            CollectionAssert.AreEqual(expectedBefore, codes);

            eventDB.Validate();


            undomgr.BeginCommand(3712, "Move Control");
            ChangeEvent.MoveControlInCourse(eventDB, CourseId(4), CourseControlId(91), CourseControlId(72), CourseControlId(73), LegInsertionLoc.PreSplit);
            undomgr.EndCommand(3712);

            eventDB.Validate();
            codes = CodesInOrder(Designator(4));
            CollectionAssert.AreEqual(expectedAfter, codes);

            undomgr.Undo();
            eventDB.Validate();
            codes = CodesInOrder(Designator(4));
            CollectionAssert.AreEqual(expectedBefore, codes);

        }

        [TestMethod]
        public void MoveControlInCourseVariation3()
        {
            Setup("changeevent\\variations2.ppen");

            string[] expectedBefore = {
                null, "31", "52", "31", "32",
                "34", "35", "36", "39", null
            };
            string[] expectedAfter = {
                null, "31", "52", "31", "32",
                "35", "36", "34", "39", null
            };
            string[] codes = CodesInOrder(Designator(4));
            var ccids = QueryEvent.EnumCourseControlIds(eventDB, Designator(4)).ToArray();

            CollectionAssert.AreEqual(expectedBefore, codes);

            eventDB.Validate();


            undomgr.BeginCommand(3712, "Move Control");
            ChangeEvent.MoveControlInCourse(eventDB, CourseId(4), CourseControlId(90), CourseControlId(92), CourseControlId(99), LegInsertionLoc.PostJoin);
            undomgr.EndCommand(3712);

            eventDB.Validate();
            codes = CodesInOrder(Designator(4));
            CollectionAssert.AreEqual(expectedAfter, codes);

            undomgr.Undo();
            eventDB.Validate();
            codes = CodesInOrder(Designator(4));
            CollectionAssert.AreEqual(expectedBefore, codes);

        }

        private void CheckGaps(CircleGap[] oldGaps, CircleGap[] newGaps, float rotation)
        {
            HashSet<int> usedNewIndices = new HashSet<int>();
            foreach (CircleGap oldGap in oldGaps) {
                bool foundMatch = false;

                for (int i = 0; i < newGaps.Length; ++i) {
                    if (usedNewIndices.Contains(i))
                        continue;

                    if (Math.Abs(Math.IEEERemainder(oldGap.startAngle + rotation, 360.0) - Math.IEEERemainder(newGaps[i].startAngle, 360.0)) < 0.001 &&
                        Math.Abs(Math.IEEERemainder(oldGap.stopAngle + rotation, 360.0) - Math.IEEERemainder(newGaps[i].stopAngle, 360.0)) < 0.001) {
                        usedNewIndices.Add(i);
                        foundMatch = true;
                        break;
                    }
                }

                Assert.IsTrue(foundMatch, string.Format("No match found for circle gap: {0}", oldGap));
            }
        }

        private void CheckChangedControlPointLocation(ControlPoint newControlPoint, ControlPoint oldControlPoint, System.Drawing.Drawing2D.Matrix matrix, float rotation, float scale)
        {
            PointF expectedLocation = Geometry.TransformPoint(oldControlPoint.location, matrix);
            Assert.AreEqual(expectedLocation.X, newControlPoint.location.X, 0.0001);
            Assert.AreEqual(expectedLocation.Y, newControlPoint.location.Y, 0.0001);
            if (oldControlPoint.kind == ControlPointKind.CrossingPoint) {
                Assert.AreEqual(Math.IEEERemainder(oldControlPoint.orientation + rotation, 360.0), Math.IEEERemainder(newControlPoint.orientation, 360.0), 0.001);
                Assert.AreEqual(oldControlPoint.stretch * scale, newControlPoint.stretch, 0.001);
            }
            if (oldControlPoint.customCodeLocation) {
                Assert.AreEqual(Math.IEEERemainder(oldControlPoint.codeLocationAngle + rotation, 360.0), Math.IEEERemainder(newControlPoint.codeLocationAngle, 360.0), 0.001);
            }
            if (oldControlPoint.gaps != null) {
                foreach (var gapPair in oldControlPoint.gaps) {
                    CircleGap[] gapsOld = gapPair.Value;
                    CircleGap[] gapsNew = newControlPoint.gaps[gapPair.Key];
                    CheckGaps(gapsOld, gapsNew, rotation);
                }
            }
        }

        private void CheckChangesCourseControlLocation(CourseControl newCourseControl, CourseControl oldCourseControl, System.Drawing.Drawing2D.Matrix matrix, float rotation, float scale)
        {
            double rotRadians = rotation * Math.PI / 180.0;
            if (oldCourseControl.customNumberPlacement) {
                Assert.AreEqual(oldCourseControl.numberDeltaX * Math.Cos(rotRadians) * scale - oldCourseControl.numberDeltaY * Math.Sin(rotRadians) * scale, newCourseControl.numberDeltaX, 0.001);
                Assert.AreEqual(oldCourseControl.numberDeltaX * Math.Sin(rotRadians) * scale + oldCourseControl.numberDeltaY * Math.Cos(rotRadians) * scale, newCourseControl.numberDeltaY, 0.001);
            }
        }

        private void CheckChangesSpecialLocation(Special newSpecial, Special oldSpecial, System.Drawing.Drawing2D.Matrix matrix, float rotation, float scale)
        {
            switch (oldSpecial.kind) {
                case SpecialKind.Text:
                case SpecialKind.Descriptions:
                case SpecialKind.Image:
                case SpecialKind.Rectangle:
                case SpecialKind.Ellipse:
                    RectangleF oldRectangle = Geometry.RectFromPoints(oldSpecial.locations[0], oldSpecial.locations[1]);
                    RectangleF newRectangle = Geometry.RectFromPoints(newSpecial.locations[0], newSpecial.locations[1]);
                    PointF expectedCenter = Geometry.TransformPoint(oldRectangle.Center(), matrix);
                    Assert.AreEqual(expectedCenter.X, newRectangle.Center().X, 0.01);
                    Assert.AreEqual(expectedCenter.Y, newRectangle.Center().Y, 0.01);
                    Assert.AreEqual(oldRectangle.Width * scale, newRectangle.Width, 0.01);
                    Assert.AreEqual(oldRectangle.Height * scale, newRectangle.Height, 0.01);
                    break;

                default:
                    PointF[] expectedLocations = Geometry.TransformPoints(oldSpecial.locations, matrix);
                    for (int i = 0; i < expectedLocations.Length; ++i) {
                        Assert.AreEqual(expectedLocations[i].X, newSpecial.locations[i].X, 0.01);
                        Assert.AreEqual(expectedLocations[i].Y, newSpecial.locations[i].Y, 0.01);
                    }
                    break;
            }

            if (oldSpecial.kind == SpecialKind.OptCrossing) {
                Assert.AreEqual(Math.IEEERemainder(oldSpecial.orientation + rotation, 360.0), Math.IEEERemainder(newSpecial.orientation, 360.0), 0.001);
                Assert.AreEqual(oldSpecial.stretch * scale, newSpecial.stretch, 0.001);
            }
        }

        private void CheckChangesLegLocation(Leg newLeg, Leg oldLeg, System.Drawing.Drawing2D.Matrix matrix, float rotation, float scale)
        {
            if (oldLeg.bends != null) {
                PointF[] expectedBends = Geometry.TransformPoints(oldLeg.bends, matrix);
                for (int i = 0; i < expectedBends.Length; ++i) {
                    Assert.AreEqual(expectedBends[i].X, newLeg.bends[i].X, 0.01);
                    Assert.AreEqual(expectedBends[i].Y, newLeg.bends[i].Y, 0.01);
                }
            }

            if (oldLeg.gaps != null) {
                for (int i = 0; i < oldLeg.gaps.Length; ++i) {
                    Assert.AreEqual(oldLeg.gaps[i].distanceFromStart * scale, newLeg.gaps[i].distanceFromStart, 0.01);
                    Assert.AreEqual(oldLeg.gaps[i].length * scale, newLeg.gaps[i].length, 0.01);
                }
            }

            if (Leg.NeedsFlaggingStartStopPosition(oldLeg.flagging)) {
                PointF expectedStartStop = Geometry.TransformPoint(oldLeg.flagStartStop, matrix);
                Assert.AreEqual(expectedStartStop.X, newLeg.flagStartStop.X, 0.01);
                Assert.AreEqual(expectedStartStop.Y, newLeg.flagStartStop.Y, 0.01);
            }
        }

        private void CheckChangesPrintArea(PrintArea newPrintArea, PrintArea oldPrintArea, System.Drawing.Drawing2D.Matrix matrix, float scale)
        {
            if (oldPrintArea.autoPrintArea) {
                Assert.IsTrue(newPrintArea.autoPrintArea);
                return;
            }

            RectangleF oldRectangle = oldPrintArea.printAreaRectangle;
            RectangleF newRectangle = newPrintArea.printAreaRectangle;

            PointF expectedCenter = Geometry.TransformPoint(oldRectangle.Center(), matrix);
            Assert.AreEqual(expectedCenter.X, newRectangle.Center().X, 0.01);
            Assert.AreEqual(expectedCenter.Y, newRectangle.Center().Y, 0.01);
            Assert.AreEqual(oldRectangle.Width * scale, newRectangle.Width, 0.02);
            Assert.AreEqual(oldRectangle.Height * scale, newRectangle.Height, 0.02);
        }


        private void TestChangeAllObjectLocations(EventDB eventDB, PointF location, float rotation, float scale)
        {
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
            matrix.Scale(scale, scale);
            matrix.RotateAt(rotation, location);

            eventDB.Validate();

            undomgr.BeginCommand(6671, "Move All Object Locations");

            Dictionary<Id<ControlPoint>, ControlPoint> controlPoints = eventDB.AllControlPointPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
            Dictionary<Id<CourseControl>, CourseControl> courseControls = eventDB.AllCourseControlPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
            Dictionary<Id<Special>, Special> specials = eventDB.AllSpecialPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
            Dictionary<Id<Leg>, Leg> legs = eventDB.AllLegPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
            Dictionary<Id<Course>, Course> courses = eventDB.AllCoursePairs.ToDictionary(pair => pair.Key, pair => pair.Value);

            ChangeEvent.ChangeAllObjectLocations(eventDB, matrix);

            foreach (var pair in eventDB.AllControlPointPairs) {
                CheckChangedControlPointLocation(pair.Value, controlPoints[pair.Key], matrix, rotation, scale);
            }

            foreach (var pair in eventDB.AllCourseControlPairs) {
                CheckChangesCourseControlLocation(pair.Value, courseControls[pair.Key], matrix, rotation, scale);
            }

            foreach (var pair in eventDB.AllSpecialPairs) {
                CheckChangesSpecialLocation(pair.Value, specials[pair.Key], matrix, rotation, scale);
            }

            foreach (var pair in eventDB.AllLegPairs) {
                CheckChangesLegLocation(pair.Value, legs[pair.Key], matrix, rotation, scale);
            }

            foreach (var pair in eventDB.AllCoursePairs) {
                if (pair.Value.printArea != null) {
                    CheckChangesPrintArea(pair.Value.printArea, courses[pair.Key].printArea, matrix, scale);
                }
                if (pair.Value.partPrintAreas != null) {
                    foreach (var partPair in pair.Value.partPrintAreas.ToArray()) {
                        CheckChangesPrintArea(partPair.Value, courses[pair.Key].partPrintAreas[partPair.Key], matrix, scale);
                    }
                }
            }

            undomgr.EndCommand(6671);

            eventDB.Validate();
            undomgr.Undo();
            eventDB.Validate();
        }

        [TestMethod]
        public void ChangeAllObjectLocations()
        {
            Setup("changeevent\\sampleevent13.ppen");

            TestChangeAllObjectLocations(eventDB, new PointF(11, -18), -205, 1);
            TestChangeAllObjectLocations(eventDB, new PointF(-5, 4), 312, 1.5F);
            TestChangeAllObjectLocations(eventDB, new PointF(6, 1), 0, 0.9F);
            TestChangeAllObjectLocations(eventDB, new PointF(-6, -99), -99, 4.5F);
        }




    }
}

#endif //TEST
