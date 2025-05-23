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
using System.IO;

using PurplePen.MapModel;
using PurplePen.MapView;

using TestingUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace PurplePen.Tests
{
    [TestClass]
    public class SetPrintAreaTests: TestFixtureBase
    {
        TestUI ui;
        Controller controller;
        EventDB eventDB;

        public void Setup(string filename)
        {
            ui = TestUI.Create();
            controller = ui.controller;
            eventDB = controller.GetEventDB();

            string fileName = TestUtil.GetTestFile(filename);

            bool success = controller.LoadInitialFile(fileName, true);
            Assert.IsTrue(success);
        }

        // Change a hightlighted rectangle from an expected rectangle to another.
        public void ChangeRectangle(RectangleF initialRect, RectangleF finalRect)
        {
            RectangleF currentRect = initialRect;

            // 1. Move the entire one to the left or right to get left edge right.
            float delta = finalRect.Left - initialRect.Left;
            PointF ptClick = new PointF((initialRect.Left + initialRect.Right) / 2, (initialRect.Top + initialRect.Bottom) / 2);
            
            // Click in center and drag to left/right.
            MapViewer.DragAction dragAction = controller.LeftButtonDown(Pane.Map, ptClick, 0.3F);
            Assert.AreEqual(MapViewer.DragAction.ImmediateDrag, dragAction);
            Assert.AreEqual(StatusBarText.DraggingObject, controller.StatusText);
            Cursor cursor = controller.GetMouseCursor(Pane.Map, ptClick, 0.3F);
            Assert.AreSame(Cursors.SizeAll, cursor);
            controller.LeftButtonEndDrag(Pane.Map, new PointF(ptClick.X + delta, ptClick.Y), ptClick, 0.3F);
            currentRect.Offset(delta, 0);

            // 2. Move the bottom edge up or down.
            delta = finalRect.Top - currentRect.Top;
            ptClick = new PointF((currentRect.Right + currentRect.Left) / 2, currentRect.Top);
            dragAction = controller.LeftButtonDown(Pane.Map, ptClick, 0.3F);
            Assert.AreEqual(MapViewer.DragAction.ImmediateDrag, dragAction);
            Assert.AreEqual(StatusBarText.SizingRectangle, controller.StatusText);
            cursor = controller.GetMouseCursor(Pane.Map, ptClick, 0.3F);
            Assert.AreSame(Cursors.SizeNS, cursor);
            controller.LeftButtonEndDrag(Pane.Map, new PointF(ptClick.X, ptClick.Y + delta), ptClick, 0.3F);
            currentRect.Height = currentRect.Height - delta;
            currentRect.Y = currentRect.Y + delta;

            // 3. Move the upper right corner around.
            float deltaX = finalRect.Right - currentRect.Right;
            float deltaY = finalRect.Bottom - currentRect.Bottom;
            ptClick = new PointF(currentRect.Right, currentRect.Bottom);
            dragAction = controller.LeftButtonDown(Pane.Map, ptClick, 0.3F);
            Assert.AreEqual(MapViewer.DragAction.ImmediateDrag, dragAction);
            Assert.AreEqual(StatusBarText.SizingRectangle, controller.StatusText);
            cursor = controller.GetMouseCursor(Pane.Map, ptClick, 0.3F);
            Assert.AreSame(Cursors.SizeNESW, cursor);
            controller.LeftButtonEndDrag(Pane.Map, new PointF(ptClick.X + deltaX, ptClick.Y + deltaY), ptClick, 0.3F);
            currentRect = RectangleF.FromLTRB(currentRect.Left, currentRect.Top, currentRect.Right + deltaX, currentRect.Bottom + deltaY);

            TestUtil.AssertEqualRect(currentRect, finalRect, 0.01, "rectangle moving algorithm");
        }

        void SetPrintArea(int tabIndex, RectangleF expectedCurrent, RectangleF newPrintArea, PrintAreaKind printAreaKind)
        {
            Dictionary<int, RectangleF> printAreas = new Dictionary<int, RectangleF>();

            // Remember print areas for all tabs.
            for (int tab = 0; tab < controller.GetTabNames().Length; ++tab) {
                controller.SelectTab(tab);
                printAreas.Add(tab, controller.GetCurrentPrintAreaRectangle(PrintAreaKind.OneCourse));
            }

            controller.SelectTab(tabIndex);

            RectangleF rectangleCurrent = controller.GetCurrentPrintAreaRectangle(printAreaKind);

            TestUtil.AssertEqualRect(expectedCurrent, rectangleCurrent, 0.1, "initial print rectangle");

            controller.BeginSetPrintArea(printAreaKind, null);

            ChangeRectangle(expectedCurrent, newPrintArea);

            controller.EndSetPrintArea(printAreaKind, new PrintArea(false, false, new RectangleF()));

            rectangleCurrent = controller.GetCurrentPrintAreaRectangle(printAreaKind);
            TestUtil.AssertEqualRect(newPrintArea, rectangleCurrent, 0.1, "final print rectangle");

            // Check all other tabs.
            for (int tab = 0; tab < controller.GetTabNames().Length; ++tab) {
                controller.SelectTab(tab);
                RectangleF rect = controller.GetCurrentPrintAreaRectangle(PrintAreaKind.OneCourse);
                if (printAreaKind == PrintAreaKind.AllCourses || tab == tabIndex)
                    TestUtil.AssertEqualRect(newPrintArea, rect, 0.1, "final print rectangle");
                else 
                    TestUtil.AssertEqualRect(printAreas[tab], rect, 0.1, "original print rectangle  for that tab");
            }
        }

        [TestMethod]
        public void SetPrintArea1()
        {
            Setup("modes\\printarea.ppen");
            RectangleF currentPrintArea = new RectangleF(-77.68744F, -142.4035F, 215.9F, 279.4F);
            RectangleF newPrintArea = RectangleF.FromLTRB(-5F, -20F, 70F, 30F);

            SetPrintArea(1, currentPrintArea, newPrintArea, PrintAreaKind.OneCourse);

            currentPrintArea = newPrintArea;
            newPrintArea = RectangleF.FromLTRB(-25F, -27.5F, 100F, 35F);

            SetPrintArea(1, currentPrintArea, newPrintArea, PrintAreaKind.OneCourse);
        }
	
        [TestMethod]
        public void SetPrintArea2()
        {
            Setup("modes\\printarea.ppen");
            RectangleF currentPrintArea = RectangleF.FromLTRB(32.1F, -12F, 177F, 101.1F);
            RectangleF newPrintArea = RectangleF.FromLTRB(32.1F, -20F, 70F, 30F);

            SetPrintArea(2, currentPrintArea, newPrintArea, PrintAreaKind.OneCourse);

            currentPrintArea = newPrintArea;
            newPrintArea = RectangleF.FromLTRB(-25F, -27.5F, 100F, 35F);

            SetPrintArea(2, currentPrintArea, newPrintArea, PrintAreaKind.OneCourse);
        }

        [TestMethod]
        public void SetPrintAreaAllControls()
        {
            Setup("modes\\printarea.ppen");
            RectangleF currentPrintArea = new RectangleF(-48.27457F, -133.2415F, 215.9F, 279.4F);
            RectangleF newPrintArea = RectangleF.FromLTRB(-5F, 20F, 70F, 130F);

            SetPrintArea(0, currentPrintArea, newPrintArea, PrintAreaKind.OneCourse);

            currentPrintArea = newPrintArea;
            newPrintArea = RectangleF.FromLTRB(-25F, -27.5F, 100F, 35F);

            SetPrintArea(0, currentPrintArea, newPrintArea, PrintAreaKind.OneCourse);
        }

        [TestMethod]
        public void SetPrintAreaAllCourses()
        {
            Setup("modes\\printarea.ppen");
            RectangleF currentPrintArea = new RectangleF(-48.27457F, -133.2415F, 215.9F, 279.4F);
            RectangleF newPrintArea = RectangleF.FromLTRB(-5F, -20F, 70F, 30F);

            SetPrintArea(1, currentPrintArea, newPrintArea, PrintAreaKind.AllCourses);

            currentPrintArea = newPrintArea;
            newPrintArea = RectangleF.FromLTRB(-25F, -27.5F, 100F, 35F);

            SetPrintArea(1, currentPrintArea, newPrintArea, PrintAreaKind.AllCourses);
        }

        [TestMethod] 
        public void AutoPrintArea()
        {
            RectangleF expectedPrintArea = RectangleF.FromLTRB(25.9F, 68.9F, 305.3F, 284.8F);

            Setup("modes\\printarea2.ppen");

            controller.SelectTab(1);
            RectangleF autoPrintArea = controller.GetCurrentPrintAreaRectangle(new CourseDesignator(CourseId(1)));
            TestUtil.AssertEqualRect(expectedPrintArea, autoPrintArea, 0.1, "print rectangle");

            controller.SelectTab(2);
            autoPrintArea = controller.GetCurrentPrintAreaRectangle(new CourseDesignator(CourseId(2)));
            TestUtil.AssertEqualRect(expectedPrintArea, autoPrintArea, 0.1, "print rectangle");

            controller.SelectTab(0);
            autoPrintArea = controller.GetCurrentPrintAreaRectangle(new CourseDesignator(CourseId(0)));
            TestUtil.AssertEqualRect(expectedPrintArea, autoPrintArea, 0.1, "print rectangle");
        }

        void DumpMapFile(string mapFileName, string outputDump)
        {
            using (TextWriter writer = new StreamWriter(outputDump, false, System.Text.Encoding.UTF8)) {
                PurplePen.MapModel.DebugCode.OcadDump dump = new PurplePen.MapModel.DebugCode.OcadDump(TestUtil.GetTestFileDirectory());
                dump.DumpFile(mapFileName, writer);
            }
        }

        void CheckDump(string ocadFile, string expectedDumpFile)
        {
            string directory = Path.GetDirectoryName(ocadFile);
            string basename = Path.GetFileNameWithoutExtension(ocadFile);
            string dumpNewFileName = directory + @"\" + basename + @"_newdump.txt";

            DumpMapFile(ocadFile, dumpNewFileName);
            TestUtil.CompareTextFileBaseline(dumpNewFileName, expectedDumpFile);
            File.Delete(dumpNewFileName);
        }

        // Create some courses, write them, and check against a dump.
        void CreateOcadFiles(OcadCreationSettings settings, string[] expectedOcadFiles, params RectangleF[] expectedPrintRectangles)
        {
            for (int i = 0; i < expectedOcadFiles.Length; ++i) {
                File.Delete(expectedOcadFiles[i]);
            }

            bool success = controller.CreateOcadFiles(settings);
            Assert.IsTrue(success);

            for (int i = 0; i < expectedOcadFiles.Length; ++i) {
                Map newMap = new Map(new GDIPlus_TextMetrics(), null);
                using (newMap.Write())
                    InputOutput.ReadFile(expectedOcadFiles[i], newMap);
                using (newMap.Read())
                    TestUtil.AssertEqualRect(expectedPrintRectangles[i], newMap.PrintArea, 0.02F, "ocad imported print area");
            }
        }

        void ExportPrintAreaToOcad(int ocadVersion)
        {
            Setup("modes\\printarea.ppen");

            SetPrintArea(1, new RectangleF(-77.68744F, -142.4035F, 215.9F, 279.4F), RectangleF.FromLTRB(-5F, -20F, 70F, 30F), PrintAreaKind.OneCourse);
            SetPrintArea(2, RectangleF.FromLTRB(32.1F, -12F, 177F, 101.1F), RectangleF.FromLTRB(-51.5F, 0F, 170.06F, 39.8F), PrintAreaKind.OneCourse);
            SetPrintArea(0, new RectangleF(-48.27457F, -133.2415F, 215.9F, 279.4F), RectangleF.FromLTRB(-250F, -110F, -170F, -10F), PrintAreaKind.OneCourse);

            OcadCreationSettings settings = new OcadCreationSettings();
            settings.mapDirectory = settings.fileDirectory = false;
            settings.outputDirectory = TestUtil.GetTestFile("modes\\ocad_print_area");

            settings.CourseIds = new Id<Course>[4] { CourseId(1), CourseId(2), CourseId(4), Id<Course>.None };
            settings.fileFormat = new MapFileFormat(MapFileFormatKind.OCAD, ocadVersion);
            settings.cyan = 0.15F;
            settings.magenta = 0.9F;
            settings.yellow = 0;
            settings.black = 0.25F;

            CreateOcadFiles(settings, 
                new string[4] { TestUtil.GetTestFile("modes\\ocad_print_area\\Course 1.ocd"),
                                         TestUtil.GetTestFile("modes\\ocad_print_area\\Course 2.ocd"),
                                         TestUtil.GetTestFile("modes\\ocad_print_area\\Course 4B.ocd"),
                                         TestUtil.GetTestFile("modes\\ocad_print_area\\All Controls.ocd") },
                new RectangleF[4] { RectangleF.FromLTRB(-5.01F, -20F, 69.99F, 30F),
                                                                               RectangleF.FromLTRB(-51.5F, 0F, 170.06F, 39.8F),
                                                                               RectangleF.FromLTRB(0F, 20F, 177F, 100F),
                                                                               RectangleF.FromLTRB(-250F, -110F, -170F, -10F)});
        }

        [TestMethod]
        public void ExportPrintAreaToOcad6()
        {
            ExportPrintAreaToOcad(6);
        }

        [TestMethod]
        public void ExportPrintAreaToOcad9()
        {
            ExportPrintAreaToOcad(9);
        }
	

    }
}

#endif //TEST
