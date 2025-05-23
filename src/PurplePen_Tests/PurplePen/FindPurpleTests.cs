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
using System.Linq;


namespace PurplePen.Tests
{
    [TestClass]
    public class FindPurpleTests: TestFixtureBase
    {
        TestUI ui;
        Controller controller;

        public void Setup(string filename)
        {
            ui = TestUI.Create();
            controller = ui.controller;
            bool success = controller.LoadInitialFile(TestUtil.GetTestFile(filename), true);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void IsPurple()
        {
            Assert.IsTrue(FindPurple.IsPurple(0, 1, 0, 0));
            Assert.IsTrue(FindPurple.IsPurple(0.43F, 0.78F, 0.22F, 0));
            Assert.IsFalse(FindPurple.IsPurple(0.95F, 0.30F, 0, 0));
            Assert.IsFalse(FindPurple.IsPurple(0, 1F, 0, 0.9F));
            Assert.IsFalse(FindPurple.IsPurple(0, 0F, 0, 0));
            Assert.IsTrue(FindPurple.IsPurple(0.35F, 0.85F, 0, 0));
            Assert.IsFalse(FindPurple.IsPurple(0.18F, 0.43F, 0, 0));
        }

        [TestMethod]
        public void IsSolidGreen()
        {
            Assert.IsTrue(FindPurple.IsSolidGreen(0.76F, 0, 0.91F, 0));
            Assert.IsTrue(FindPurple.IsSolidGreen(0.91F, 0, 0.83F, 0));
            Assert.IsFalse(FindPurple.IsSolidGreen(0.46F, 0, 0.55F, 0));
        }



        [TestMethod]
        public void FindPurpleByValue()
        {
            Map map = new Map(new GDIPlus_TextMetrics(), null);
            using (map.Write()) {
                map.AddColor("Purple 50%", 14, 0, 0.5F, 0, 0, false);
                map.AddColor("Purplicious", 11, 0.2F, 1F, 0.1F, 0.08F, false);
                map.AddColor("Blue", 12, 0.95F, 0.35F, 0, 0, false);
                map.AddColor("Black", 88, 0, 0, 0, 1F, false);
            }

            short ocadId;
            float c, m, y, k;
            List<SymColor> colorList;
            using (map.Read())
                colorList = new List<SymColor>(map.AllColors);

            Assert.IsTrue(FindPurple.FindPurpleColor(colorList, out ocadId, out c, out m, out y, out k));
            Assert.AreEqual(11, ocadId);
            Assert.AreEqual(0.2F, c);
            Assert.AreEqual(1F, m);
            Assert.AreEqual(0.1F, y);
            Assert.AreEqual(0.08F, k);
        }

        [TestMethod]
        public void NoPurple()
        {
            Map map = new Map(new GDIPlus_TextMetrics(), null);
            using (map.Write()) {
                map.AddColor("Yellow", 11, 0.0F, 0.25F, 0.79F, 0.08F, false);
                map.AddColor("Blue", 12, 0.95F, 0.35F, 0, 0, false);
                map.AddColor("Black", 88, 0, 0, 0, 1F, false);
            }

            short ocadId;
            float c, m, y, k;
            List<SymColor> colorList;
            using (map.Read())
                colorList = new List<SymColor>(map.AllColors);

            Assert.IsFalse(FindPurple.FindPurpleColor(colorList, out ocadId, out c, out m, out y, out k));
        }

        [TestMethod]
        public void SampleMap()
        {
            Setup("findpurple\\Sample Event.ppen");
            List<SymColor> colors = controller.MapDisplay.GetMapColors();

            int lowerPurpleId = FindPurple.FindBestLowerPurpleLayer(colors);
            SymColor color = colors.Single(sc => sc.OcadId == lowerPurpleId);
            Assert.AreEqual(114, lowerPurpleId);
            Assert.AreEqual("Green 60%", color.Name);
        }



        [TestMethod]
        public void OCAD_ISOM()
        {
            Setup("findpurple\\OCAD_ISOM_Blank.ppen");
            List<SymColor> colors = controller.MapDisplay.GetMapColors();

            int lowerPurpleId = FindPurple.FindBestLowerPurpleLayer(colors);
            SymColor color = colors.Single(sc => sc.OcadId == lowerPurpleId);
            Assert.AreEqual(52, lowerPurpleId);
            Assert.AreEqual("Lower purple for course planning", color.Name);
        }

        [TestMethod]
        public void OCAD_ISSprOM()
        {
            Setup("findpurple\\OCAD_ISSprOM_Blank.ppen");
            List<SymColor> colors = controller.MapDisplay.GetMapColors();

            int lowerPurpleId = FindPurple.FindBestLowerPurpleLayer(colors);
            SymColor color = colors.Single(sc => sc.OcadId == lowerPurpleId);
            Assert.AreEqual(52, lowerPurpleId);
            Assert.AreEqual("Lower purple for course planning", color.Name);
        }

        [TestMethod]
        public void OOM_ISOM()
        {
            Setup("findpurple\\OOM_ISOM_Blank.ppen");
            List<SymColor> colors = controller.MapDisplay.GetMapColors();

            int lowerPurpleId = FindPurple.FindBestLowerPurpleLayer(colors);
            Assert.AreEqual(7, lowerPurpleId);
            SymColor color = colors.Single(sc => sc.OcadId == lowerPurpleId);
            Assert.AreEqual("Purple for track symbols", color.Name);
        }

        [TestMethod]
        public void OOM_ISSprOM()
        {
            Setup("findpurple\\OOM_ISSprOM_Blank.ppen");
            List<SymColor> colors = controller.MapDisplay.GetMapColors();

            int lowerPurpleId = FindPurple.FindBestLowerPurpleLayer(colors);
            Assert.AreEqual(5, lowerPurpleId);
            SymColor color = colors.Single(sc => sc.OcadId == lowerPurpleId);
            Assert.AreEqual("Purple for track symbols", color.Name);
        }

    }
}

#endif //TEST
