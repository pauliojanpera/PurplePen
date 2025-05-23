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
using System.Drawing;
using System.Drawing.Printing;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestingUtils;

namespace PurplePen.Tests
{
    [TestClass]
    public class PunchPrintingTests: TestFixtureBase
    {
        TestUI ui;
        Controller controller;

        [TestInitialize]
        public void Setup()
        {
            ui = TestUI.Create();
            controller = ui.controller;
        }

        private void PunchPrintingTest(string basename, PunchPrintSettings punchPrintSettings)
        {
            // Get the pages of the printing.
            PunchPrinting punchPrinter = new PunchPrinting(controller.GetEventDB(), controller, punchPrintSettings);
            Bitmap[] bitmaps = punchPrinter.PrintBitmaps();

            // Check all the pages against the baseline.
            for (int page = 0; page < bitmaps.Length; ++page) {
                Bitmap bm = bitmaps[page];
                string baseFileName = basename + "_page" + (page + 1).ToString();
                TestUtil.CheckBitmapsBase(bm, baseFileName);
            }
        }

        private void PunchPdfTest(string basename, PunchPrintSettings punchPrintSettings)
        {
            // Get the pages of the printing.
            PunchPrinting punchPrinter = new PunchPrinting(controller.GetEventDB(), controller, punchPrintSettings);
            string pdfFileName = TestUtil.GetTestFile(basename + ".pdf");
            punchPrinter.PrintToPdf(pdfFileName, false);
            CheckPdfDump(pdfFileName, TestUtil.GetTestFile(basename + "_baseline_page%d.png"));
        }

        private void CheckPdfDump(string pdfFile, string expectedPng)
        {
            PdfMapFile mapFile = new PdfMapFile(pdfFile);
            string pngFile = Path.Combine(Path.GetDirectoryName(pdfFile), Path.GetFileNameWithoutExtension(pdfFile) + "_page%d_temp.png");
            mapFile.BeginUncachedConversion(pngFile, 200); // Convert 200 DPI.
            while (mapFile.Status == PdfMapFile.ConversionStatus.Working)
                System.Threading.Thread.Sleep(10);
            Assert.AreEqual(PdfMapFile.ConversionStatus.Success, mapFile.Status);

            int pageNum = 1;
            for (; ; ) {
                string pngExpectedPage = expectedPng.Replace("%d", pageNum.ToString());
                pngExpectedPage = TestUtil.GetBitnessSpecificFileName(pngExpectedPage, true);
                bool expectedPageExists = File.Exists(pngExpectedPage);
                string pngActualPage = pngFile.Replace("%d", pageNum.ToString());
                bool actualPageExists = File.Exists(pngActualPage);

                Assert.AreEqual(expectedPageExists, actualPageExists);
                if (expectedPageExists) {
                    using (Bitmap bmNew = (Bitmap)Image.FromFile(pngActualPage)) {
                        TestUtil.CompareBitmapBaseline(bmNew, pngExpectedPage);
                    }
                }
                else {
                    break;
                }

                if (!expectedPng.Contains("%d"))
                    break;

                pageNum++;
            }
        }



        [TestMethod]
        public void PrintPunches1()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("punchcards\\sample1.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.BoxSize = 18;

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(1), CourseId(2), CourseId(3), CourseId(4), CourseId(0) };
            PunchPrintingTest("punchcards\\desc1", punchPrintSettings);
        }

        [TestMethod]
        public void PrintPunches2()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("punchcards\\sample1.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.PageSettings.Landscape = true;
            punchPrintSettings.PageSettings.Margins = new Margins(50, 50, 200, 200);
            punchPrintSettings.BoxSize = 9;

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(0), CourseId(1), CourseId(2), CourseId(3), CourseId(4), CourseId(5), CourseId(6), CourseId(7) };
            PunchPrintingTest("punchcards\\desc2", punchPrintSettings);
        }

        [TestMethod]
        public void PrintPunchesRelay1()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("controller\\variations.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.BoxSize = 18;

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(2), CourseId(0) };
            PunchPrintingTest("punchcards\\relay1", punchPrintSettings);
        }

        [TestMethod]
        public void PrintPunchesRelay2()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("controller\\variations.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.BoxSize = 18;
            punchPrintSettings.VariationChoicesPerCourse[CourseId(2)] =
                new VariationChoices() {
                    Kind = VariationChoices.VariationChoicesKind.ChosenTeams,
                    FirstTeam = 2,
                    LastTeam = 3
                };

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(2) };
            PunchPrintingTest("punchcards\\relay2", punchPrintSettings);
        }

        [TestMethod]
        public void PrintPunchesRelay3()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("controller\\variations.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.BoxSize = 18;
            punchPrintSettings.VariationChoicesPerCourse[CourseId(2)] =
                new VariationChoices() {
                    Kind = VariationChoices.VariationChoicesKind.Combined
                };

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(2) };
            PunchPrintingTest("punchcards\\relay3", punchPrintSettings);
        }

        [TestMethod]
        public void PrintPunchesPdf1()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("punchcards\\sample1.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.BoxSize = 18;

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(1), CourseId(2), CourseId(3), CourseId(4), CourseId(0) };
            PunchPdfTest("punchcards\\descpdf1", punchPrintSettings);
        }

        [TestMethod]
        public void PrintPunchesPdf2()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("punchcards\\sample1.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();
            punchPrintSettings.PageSettings.Landscape = true;
            punchPrintSettings.PageSettings.Margins = new Margins(50, 50, 200, 200);
            punchPrintSettings.BoxSize = 9;

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(0), CourseId(1), CourseId(2), CourseId(3), CourseId(4), CourseId(5), CourseId(6), CourseId(7) };
            PunchPdfTest("punchcards\\descpdf2", punchPrintSettings);
        }



        [TestMethod]
        public void PrintingException()
        {
            controller.LoadInitialFile(TestUtil.GetTestFile("punchcards\\sample1.ppen"), true);
            PunchPrintSettings punchPrintSettings = new PunchPrintSettings();

            punchPrintSettings.CourseIds = new Id<Course>[] { CourseId(1), CourseId(2), CourseId(3) };
            punchPrintSettings.PageSettings.PrinterSettings.PrinterName = "foobar";

            bool success = controller.PrintPunches(punchPrintSettings, false);

            Assert.IsFalse(success);
            string expected =
@"ERROR: 'Cannot print 'Marymoor WIOL 2' for the following reason:

Settings to access printer 'foobar' are not valid.'
";

            Assert.AreEqual(expected, ui.output.ToString());
        }


    }
}


#endif //TEST
