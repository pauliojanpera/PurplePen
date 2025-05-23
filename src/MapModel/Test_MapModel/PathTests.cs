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
using NUnit.Framework;

namespace PurplePen.MapModel.Tests
{
    [TestFixture]
    public class PathTests
    {
        private SymPath p1, p2, p3;

        [SetUp]
        public void Initialize()
        {
            p1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(5, 5) },
                new PointKind[] { PointKind.Normal, PointKind.Normal });
            p2 = SymPath.CreateEllipsePath(new RectangleF(0, 0, 10, 10));
            PointF[] points = {
				new PointF(0,0), new PointF(10,10), new PointF(20,40), new PointF(-10, 10),
				new PointF(20,20), new PointF(25, 30), new PointF(25,30), new PointF(40,25) };
            PointKind[] kinds = {
				PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl,
				PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal };
            p3 = new SymPath(points, kinds);
        }

        [Test]
        public void DumpToString()
        {
            Assert.AreEqual("N(0,0)--N(10,10)--B(20,40)--B(-10,10)--N(20,20)--N(25,30)--N(25,30)--N(40,25)", p3.ToString());

            PointF[] points = {
				new PointF(0,0), new PointF(-10.5F,10), new PointF(20.4562F,-40.12345F), new PointF(-10.379F, 10),
				new PointF(20,20), new PointF(25, 30) };
            PointKind[] kinds = {
				PointKind.Normal, PointKind.Corner, PointKind.BezierControl, PointKind.BezierControl,
				PointKind.Dash, PointKind.Normal };
            SymPath p = new SymPath(points, kinds);

            Assert.AreEqual("N(0,0)--C(-10.5,10)--B(20.46,-40.12)--B(-10.38,10)--D(20,20)--N(25,30)", p.ToString());
        }

        [Test]
        public void DumpToString2()
        {
            PointF[] points = {
				new PointF(0,0), new PointF(10,10), new PointF(20,40), new PointF(-10, 10),
				new PointF(20,20), new PointF(25, 30), new PointF(25,30), new PointF(40,25) };
            PointKind[] kinds = {
				PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl,
				PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal };
            byte[] startStopPoints = {
                0x1, 0x1, 0x0, 0x0, 0x3, 0x0, 0x3 };
            SymPath p = new SymPath(points, kinds, startStopPoints);
            Assert.AreEqual("N(0,0)/1--N(10,10)/1--B(20,40)--B(-10,10)--N(20,20)/3--N(25,30)--N(25,30)/3--N(40,25)", p.ToString());
        }

        [Test]
        public void DumpHolesToString()
        {
            SymPath path1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(10, 5), new PointF(10, 10), new PointF(5, 10), new PointF(5, 5)},
                new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path2 = new SymPath(new PointF[] { new PointF(6,7), new PointF(6,8), new PointF(8,8), new PointF(6,7) },
                new PointKind[] { PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Corner });
            SymPath path3 = new SymPath(new PointF[] { new PointF(8.567F, 8.9F), new PointF(6, 8), new PointF(8.123F, 8), new PointF(6, 7), new PointF(8.567F, 8.9F) },
                new PointKind[] { PointKind.Corner, PointKind.Corner, PointKind.Dash, PointKind.Normal, PointKind.Normal });

            SymPathWithHoles pholes = new SymPathWithHoles(path1, new SymPath[] { path2, path3 });
            Assert.AreEqual(@"N(5,5)--N(10,5)--N(10,10)--N(5,10)--N(5,5)  HOLE 0: C(6,7)--C(6,8)--C(8,8)--C(6,7)  HOLE 1: C(8.57,8.9)--C(6,8)--D(8.12,8)--N(6,7)--N(8.57,8.9)"
                , pholes.ToString());
	
            SymPathWithHoles pholes2 = new SymPathWithHoles(path1, null);
            Assert.AreEqual(@"N(5,5)--N(10,5)--N(10,10)--N(5,10)--N(5,5)", pholes2.ToString());
        }

        [Test]
        public void FirstLastPoint()
        {
            Assert.AreEqual(new PointF(0, 0), p3.FirstPoint);
            Assert.AreEqual(new PointF(40, 25), p3.LastPoint);
        }

        [Test]
        public void SplitAtEnd()
        {
            SymPath pathInitial = new SymPath(new PointF[] { new PointF(2, 2), new PointF(2, 10), new PointF(5, 13) });
            SymPath res1, res2;

            pathInitial.Split(new PointF(2, 2), out res1, out res2);
            Assert.IsNull(res1);
            Assert.AreEqual(@"N(2,2)--N(2,10)--N(5,13)", res2.ToString());

            pathInitial.Split(new PointF(5, 13), out res1, out res2);
            Assert.IsNull(res2);
            Assert.AreEqual(@"N(2,2)--N(2,10)--N(5,13)", res1.ToString());
        }

        [Test]
        public void SplitAtEnd2()
        {
            SymPath pathInitial = new SymPath(new PointF[] { new PointF(2, 2), new PointF(2, 10), new PointF(5, 13) },
                new PointKind[] { PointKind.Normal, PointKind.Corner, PointKind.Normal },
                new byte[] { 0x1, 0x2 }, false);
            SymPath res1, res2;

            pathInitial.Split(new PointF(2, 2), out res1, out res2);
            Assert.IsNull(res1);
            Assert.AreEqual(@"N(2,2)/1--C(2,10)/2--N(5,13)", res2.ToString());

            pathInitial.Split(new PointF(5, 13), out res1, out res2);
            Assert.IsNull(res2);
            Assert.AreEqual(@"N(2,2)/1--C(2,10)/2--N(5,13)", res1.ToString());
        }

        [Test]
        public void SplitMiddle()
        {
            SymPath pathInitial = new SymPath(new PointF[] { new PointF(2, 2), new PointF(2, 10), new PointF(5, 13) });
            SymPath res1, res2;

            pathInitial.Split(new PointF(3, 11), out res1, out res2);
            Assert.AreEqual(@"N(2,2)--N(2,10)--N(3,11)", res1.ToString());
            Assert.AreEqual(@"N(3,11)--N(5,13)", res2.ToString());
        }

        [Test]
        public void SplitMiddle2()
        {
            SymPath pathInitial = new SymPath(new PointF[] { new PointF(2, 2), new PointF(2, 10), new PointF(5, 13) },
                new PointKind[] { PointKind.Normal, PointKind.Corner, PointKind.Normal },
                new byte[] { 0x1, 0x2 }, false);
            SymPath res1, res2;

            pathInitial.Split(new PointF(3, 11), out res1, out res2);
            Assert.AreEqual(@"N(2,2)/1--C(2,10)/2--N(3,11)", res1.ToString());
            Assert.AreEqual(@"N(3,11)/2--N(5,13)", res2.ToString());
        }

        [Test]
        public void SplitMiddle3()
        {
            SymPath pathInitial = new SymPath(new PointF[] { new PointF(2, 2), new PointF(2, 10), new PointF(5, 13) },
                new PointKind[] { PointKind.Normal, PointKind.Corner, PointKind.Normal },
                new byte[] { 0x1, 0x2 }, false);
            SymPath res1, res2;

            pathInitial.Split(new PointF(2,10), out res1, out res2);
            Assert.AreEqual(@"N(2,2)/1--C(2,10)", res1.ToString());
            Assert.AreEqual(@"C(2,10)/2--N(5,13)", res2.ToString());
        }

        [Test]
        public void SplitMiddle4()
        {
            SymPath pathInitial = new SymPath(new PointF[] { new PointF(2, 2), new PointF(2, 10), new PointF(3, 11), new PointF(3,12), new PointF(5, 13), new PointF(7,20) },
                new PointKind[] { PointKind.Normal, PointKind.Corner, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal, PointKind.Normal },
                new byte[] { 0x1, 0x2, 0, 0, 0x4 }, false);
            SymPath res1, res2;
            PointF splitPoint;
            pathInitial.DistanceFromPoint(new PointF(4, 11.5F), out splitPoint);

            pathInitial.Split(splitPoint, out res1, out res2);
            Assert.AreEqual(@"N(2,2)/1--C(2,10)/2--B(2.66,10.66)--B(2.88,11.31)--N(3.52,11.97)", res1.ToString());
            Assert.AreEqual(@"N(3.52,11.97)/2--B(3.86,12.31)--B(4.31,12.66)--N(5,13)/4--N(7,20)", res2.ToString());
        }




        [Test]
        public void NearestPoint()
        {
            PointF nearest;
            float distance;

            distance = p1.DistanceFromPoint(new PointF(7, 6), out nearest);
            Assert.AreEqual(2.23608F, distance, 0.007F);
            Assert.AreEqual(5, nearest.X, 0.007F);
            Assert.AreEqual(5, nearest.Y, 0.007F);

            distance = p2.DistanceFromPoint(new PointF(8, 4), out nearest);
            Assert.AreEqual(1.8344F, distance, 0.007F);
            Assert.AreEqual(9.7226F, nearest.X, 0.007F);
            Assert.AreEqual(3.3693F, nearest.Y, 0.007F);

            distance = p3.DistanceFromPoint(new PointF(-4, -3), out nearest);
            Assert.AreEqual(5F, distance, 0.007F);
            Assert.AreEqual(0, nearest.X, 0.007F);
            Assert.AreEqual(0, nearest.Y, 0.007F);

            distance = p3.DistanceFromPoint(new PointF(3, 25), out nearest);
            Assert.AreEqual(5.048F, distance, 0.007F);
            Assert.AreEqual(7.063F, nearest.X, 0.007F);
            Assert.AreEqual(22.005F, nearest.Y, 0.007F);

            distance = p3.DistanceFromPoint(new PointF(22, 37), out nearest);
            Assert.AreEqual(7.615773F, distance, 0.007F);
            Assert.AreEqual(25F, nearest.X, 0.007F);
            Assert.AreEqual(30F, nearest.Y, 0.007F);

            distance = p3.DistanceFromPoint(new PointF(30, 30), out nearest);
            Assert.AreEqual(1.58114F, distance, 0.007F);
            Assert.AreEqual(29.5F, nearest.X, 0.007F);
            Assert.AreEqual(28.5F, nearest.Y, 0.007F);

            distance = p3.DistanceFromPoint(new PointF(50, 30), out nearest);
            Assert.AreEqual(11.18034F, distance, 0.007F);
            Assert.AreEqual(40F, nearest.X, 0.007F);
            Assert.AreEqual(25F, nearest.Y, 0.007F);
        }

        [Test]
        public void LengthToPoint()
        {
            PointF pointAtLength;
            float distance;

            distance = p1.LengthToPoint(new PointF(7, 6));
            Assert.AreEqual(0, distance, 0.007F);

            distance = p2.LengthToPoint(new PointF(8, 4));
            pointAtLength = p2.PointAtLength(distance);
            Assert.AreEqual(9.7226F, pointAtLength.X, 0.001F);
            Assert.AreEqual(3.3693, pointAtLength.Y, 0.001F);

            distance = p3.LengthToPoint(new PointF(-4, -3));
            pointAtLength = p3.PointAtLength(distance);
            Assert.AreEqual(0, pointAtLength.X, 0.007F);
            Assert.AreEqual(0, pointAtLength.Y, 0.007F);

            distance = p3.LengthToPoint(new PointF(3, 25));
            pointAtLength = p3.PointAtLength(distance);
            Assert.AreEqual(7.063F, pointAtLength.X, 0.007F);
            Assert.AreEqual(22.005F, pointAtLength.Y, 0.007F);

            distance = p3.LengthToPoint(new PointF(22, 37));
            pointAtLength = p3.PointAtLength(distance);
            Assert.AreEqual(25F, pointAtLength.X, 0.007F);
            Assert.AreEqual(30F, pointAtLength.Y, 0.007F);

            distance = p3.LengthToPoint(new PointF(30, 30));
            pointAtLength = p3.PointAtLength(distance);
            Assert.AreEqual(29.5F, pointAtLength.X, 0.007F);
            Assert.AreEqual(28.5F, pointAtLength.Y, 0.007F);

            distance = p3.LengthToPoint(new PointF(50, 30));
            pointAtLength = p3.PointAtLength(distance);
            Assert.AreEqual(40F, pointAtLength.X, 0.007F);
            Assert.AreEqual(25F, pointAtLength.Y, 0.007F);
        }

        [Test]
        public void PathSegment()
        {
            PointF pt1, pt2;

            p3.DistanceFromPoint(new PointF(12, 15), out pt1);
            p3.DistanceFromPoint(new PointF(30,28), out pt2);

            SymPath result = p3.Segment(pt1, pt2);
            Assert.AreEqual("N(11.47,15.13)--B(16.35,36.26)--B(-8.05,10.65)--N(20,20)--N(25,30)--N(25,30)--N(30.1,28.3)", result.ToString());

            // Do in reverse order, should get null.
            result = p3.Segment(pt2, pt1);
            Assert.IsNull(result);
        }

        [Test]
        public void PathSegment2()
        {
            SymPath result = p3.Segment(new PointF(20,20), new PointF(0,0));
            Assert.IsNull(result);
        }

        [Test]
        public void Area()
        {
            Assert.IsTrue(p2.IsClosedCurve);

            float area = p2.Area();
            Assert.AreEqual(78.5F, area, 0.1F);

            SymPath rect = SymPath.CreateRectanglePath(new RectangleF(30000, -10000, 13, 37));
            area = rect.Area();
            Assert.AreEqual(area, 13F * 37F, 0.01F);

            SymPathWithHoles ph = new SymPathWithHoles(p2, null);
            area = ph.Area();
            Assert.AreEqual(78.5F, area, 0.1F);

            ph = new SymPathWithHoles(p2, new SymPath[] { SymPath.CreateRectanglePath(new RectangleF(3, 3, 2, 2)) });
            area = ph.Area();
            Assert.AreEqual(74.5F, area, 0.1F);
        }

        [Test]
        public void EqualPaths()
        {
            SymPath path1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path2 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path3 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal });
            SymPath path4 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Corner, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path5 = new SymPath(new PointF[] { new PointF(5, 5.1F), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path6 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, -8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path7 = new SymPath(new PointF[] { new PointF(10, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }, null, true);
            SymPath path8 = new SymPath(new PointF[] { new PointF(10, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }, null, true);
            SymPath path9 = new SymPath(new PointF[] { new PointF(10, 5), new PointF(6, 3), new PointF(7, 8), new PointF(10, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }, null, false);

            Assert.AreEqual(path1, path1);
            Assert.AreEqual(path1, path2);
            Assert.AreNotEqual(path1, path3);
            Assert.AreNotEqual(path1, path4);
            Assert.AreNotEqual(path1, path5);
            Assert.AreNotEqual(path1, path6);
            Assert.AreEqual(path7, path8);
            Assert.AreNotEqual(path7, path9);
        }

        [Test]
        public void EqualPathsWithHoles()
        {
            SymPath path1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path2 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath path3 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal });

            SymPathWithHoles ph1 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), null);
            SymPathWithHoles ph2 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), null);
            SymPathWithHoles ph3 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 4), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), null);
            SymPathWithHoles ph4 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Dash, PointKind.Normal }), null);
            SymPathWithHoles ph5 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), new SymPath[] { path1 });
            SymPathWithHoles ph6 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), new SymPath[] { path1, path2 });
            SymPathWithHoles ph7 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), new SymPath[] { path1, path2 });
            SymPathWithHoles ph8 = new SymPathWithHoles(new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), new SymPath[] { path3, path2 });

            Assert.AreEqual(ph1, ph1);
            Assert.AreEqual(ph1, ph2);
            Assert.AreNotEqual(ph1, ph3);
            Assert.AreNotEqual(ph1, ph4);
            Assert.AreNotEqual(ph1, ph5);
            Assert.AreNotEqual(ph5, ph1);
            Assert.AreEqual(ph6, ph7);
            Assert.AreNotEqual(ph5, ph7);
            Assert.AreNotEqual(ph7, ph5);
            Assert.AreNotEqual(ph8, ph7);
        }

        [Test]
        public void InitStartStopPoints()
        {
            SymPath path1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal },
                                                          new byte[] { 0, 0, 0 });
            Assert.IsNull(path1.StartStopFlags);     // all zero start/stop flags should become null.

            SymPath path2 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            Assert.IsNull(path2.StartStopFlags);     // start/stop flags should become null.

            SymPath path3 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal },
                                                          new byte[] { 0, 2, 3 });
            Assert.AreEqual(0, path3.StartStopFlags[0]);
            Assert.AreEqual(2, path3.StartStopFlags[1]);
            Assert.AreEqual(3, path3.StartStopFlags[2]);
        }

        [Test]
        public void GetSubpaths1()
        {
            SymPath path1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                                          new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
            SymPath[] result = path1.GetSubpaths(1);
            Assert.AreEqual(1, result.Length);
            Assert.AreSame(result[0], path1);
        }

        [Test]
        public void GetSubpaths2()
        {
            SymPath path1 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(5, 5) },
                                              new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal },
                                              new byte[] { 1, 3, 1 });
            SymPath[] result = path1.GetSubpaths(4);
            Assert.AreEqual(1, result.Length);
            Assert.AreSame(result[0], path1);            // entire path.

            result = path1.GetSubpaths(1);
            Assert.AreEqual(0, result.Length);   // no paths.
        }

        [Test]
        public void GetSubpaths3()
        {
            PointF[] points = new PointF[] {
				new PointF(0,0), new PointF(10,10), new PointF(20,40), new PointF(-10, 10),
				new PointF(20,20), new PointF(25, 30), new PointF(25,30), new PointF(40,25) };
            PointKind[] kinds = new PointKind[] {
				PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl,
				PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal };
            byte[] startStopPoints = new byte[] {
                0x11, 0x11, 0x2, 0x4, 
                0x13, 0x14, 0x13 };
            SymPath p = new SymPath(points, kinds, startStopPoints);
            
            SymPath[] result = p.GetSubpaths(0x10);
            Assert.AreEqual(0, result.Length);   // no paths.

            result = p.GetSubpaths(0x1);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("N(25,30)--N(25,30)", result[0].ToString());

            result = p.GetSubpaths(0x2);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("N(0,0)--N(10,10)--B(20,40)--B(-10,10)--N(20,20)", result[0].ToString());
            Assert.AreEqual("N(25,30)--N(25,30)", result[1].ToString());

            result = p.GetSubpaths(0x4);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("N(0,0)--N(10,10)--B(20,40)--B(-10,10)--N(20,20)--N(25,30)", result[0].ToString());
            Assert.AreEqual("N(25,30)--N(40,25)", result[1].ToString());
        }

        [Test]
        public void Shorten()
        {
            PointF[] points = {
				new PointF(0,0), new PointF(10,10), new PointF(20,40), new PointF(-10, 10),
				new PointF(20,20), new PointF(25, 30), new PointF(25,30), new PointF(40,25) };
            PointKind[] kinds = {
				PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl,
				PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal };
            byte[] startStopPoints = {
                0x01, 0x11, 0x2, 0x4, 
                0x13, 0x14, 0x13 };
            SymPath p = new SymPath(points, kinds, startStopPoints);

            SymPath result = p.Shorten(20, 12);
            Assert.AreEqual("N(11.59,15.65)/11--B(15.94,35.84)--B(-7.81,10.73)--N(20,20)/13--N(25,30)/14--N(25,30)/13--N(28.62,28.79)", result.ToString());
            
        }

        [Test]
        public void Join()
        {
            PointF[] points = {
				new PointF(0,0), new PointF(10,10), new PointF(20,40), new PointF(-10, 10),
				new PointF(20,20), new PointF(25, 30), new PointF(25,30), new PointF(5,5) };
            PointKind[] kinds = {
				PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl,
				PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal };
            byte[] startStopPoints = {
                0x01, 0x11, 0x0, 0x0, 
                0x13, 0x14, 0x13 };
            SymPath path1 = new SymPath(points, kinds, startStopPoints);
            SymPath path2 = new SymPath(new PointF[] { new PointF(5, 5), new PointF(6, 3), new PointF(7, 8), new PointF(12,9) },
                                              new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal },
                                              new byte[] { 1, 3, 1 });


            SymPath result = SymPath.Join(path1, path2, PointKind.Corner);
            Assert.AreEqual("N(0,0)/1--N(10,10)/11--B(20,40)--B(-10,10)--N(20,20)/13--N(25,30)/14--N(25,30)/13--C(5,5)/1--N(6,3)/3--N(7,8)/1--N(12,9)", result.ToString());
        }

        [Test]
        public void FindCornerPoints()
        {
            float[] perpAngles, subtendedAngles;
            PointF[] result;

            PointF[] points = { new PointF(20, 0), new PointF(15, 0), new PointF(10, 0), new PointF(10, 10), new PointF(10, 20) };
            PointKind[] kinds = { PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Corner };
            SymPath path1 = new SymPath(points, kinds);

            result = path1.FindCornerOrDashPoints(PointKind.Corner, false, out perpAngles, out subtendedAngles);

            Assert.That(result, Is.EquivalentTo(points));
            Assert.That(perpAngles, Is.EquivalentTo(new float[] { 90F, 90F, 45F, 0F, 0F }));
            Assert.That(subtendedAngles, Is.EquivalentTo(new float[] { 180F, 180F, 90F, 180F, 180F }));

            points = new PointF[] { new PointF(0, 0), new PointF(10, 0), new PointF(20, 1.76F), new PointF(30, 0), new PointF(40, 0) };
            kinds = new PointKind[] { PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Corner };
            path1 = new SymPath(points, kinds);

            result = path1.FindCornerOrDashPoints(PointKind.Corner, false, out perpAngles, out subtendedAngles);

            Assert.That(result, Is.EquivalentTo(points));
            Assert.That(perpAngles, Is.EquivalentTo(new float[] { -90F, -85.00909F, -90F, -94.99091F, -90F }));
            Assert.That(subtendedAngles, Is.EquivalentTo(new float[] { 180F, 170.018173F, 160.036346F, 170.018173F, 180F }));
        }

        [Test]
        public void PointsAlongLine()
        {
            PointF[] points = { new PointF(0, 0), new PointF(10, 0) };
            PointKind[] kinds = { PointKind.Normal, PointKind.Normal };
            SymPath path1 = new SymPath(points, kinds);

            float[] perpAngles;
            PointF[] result = path1.FindPointsAlongLine(new float[] { -1F, 3F, 3F, 3F, 3F, 3F }, out perpAngles);

            Assert.That(result, Is.EquivalentTo(new PointF[] { new PointF(0, 0), new PointF(2F, 0), new PointF(5F, 0), new PointF(8F, 0), new PointF(10, 0), new PointF(10,0) }));
        }

        [Test]
        public void OffsetRight()
        {
            PointF[] points = { new PointF(0, 0), new PointF(10, 0), new PointF(10,10), new PointF(0, 10), new PointF(0,1) };
            PointKind[] kinds = { PointKind.Normal, PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Normal };
            SymPath path1 = new SymPath(points, kinds);

            SymPath result1 = path1.OffsetRight(-3, 5);
            Assert.AreEqual("N(0,3)--C(7,3)--C(7,7)--C(3,7)--N(3,1)", result1.ToString());

            SymPath result2 = path1.OffsetRight(4, 5);
            Assert.AreEqual("N(0,-4)--C(14,-4)--C(14,14)--C(-4,14)--N(-4,1)", result2.ToString());
        }

        [Test]
        public void OffsetRightClosed()
        {
            PointF[] points = { new PointF(0, 0), new PointF(10, 0), new PointF(10, 10), new PointF(0, 10), new PointF(0, 0) };
            PointKind[] kinds = { PointKind.Normal, PointKind.Corner, PointKind.Corner, PointKind.Corner, PointKind.Normal };
            byte[] startStopFlags = { 1, 0, 1, 0 };
            SymPath path1 = new SymPath(points, kinds, startStopFlags);

            SymPath result1 = path1.OffsetRight(-3, 5);
            Assert.AreEqual("N(3,3)/1--C(7,3)--C(7,7)/1--C(3,7)--N(3,3)", result1.ToString());

            SymPath result2 = path1.OffsetRight(4, 5);
            Assert.AreEqual("N(-4,-4)/1--C(14,-4)--C(14,14)/1--C(-4,14)--N(-4,-4)", result2.ToString());
        }

        bool EqualPoints(PointF[] x, PointF[] y, float delta)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; ++i) {
                if (Math.Abs(x[i].X - y[i].X) > delta)
                    return false;
                if (Math.Abs(x[i].Y - y[i].Y) > delta)
                    return false;
            }

            return true;
        }

        [Test]
        public void Intersects()
        {
            PointF[] points = { new PointF(17, 17), new PointF(35, 25), new PointF(25, 50) };
            PointKind[] kinds = { PointKind.Normal, PointKind.Corner, PointKind.Normal };
            SymPath path1 = new SymPath(points, kinds);

            points = new PointF[] { new PointF(25, 10), new PointF(26, 32), new PointF(50, 50) };
            kinds = new PointKind[] { PointKind.Normal, PointKind.Corner, PointKind.Normal };
            SymPath path2 = new SymPath(points, kinds);

            points = new PointF[] { new PointF(10, 10), new PointF(24, 2), new PointF(55, 25), new PointF(36, 51) };
            kinds = new PointKind[] { PointKind.Normal, PointKind.Corner, PointKind.Corner, PointKind.Normal };
            SymPath path3 = new SymPath(points, kinds);

            PointF[] intersections;
            bool result;

            PointF[] expectedIntersections = new PointF[] { new PointF(25.48969F, 20.7731953F), new PointF(30.76923F, 35.5769234F) };
            result = path1.Intersects(path2, out intersections);
            Assert.True(result);
            Assert.True(EqualPoints(intersections, expectedIntersections, 0.001F));

            result = path2.Intersects(path1, out intersections);
            Assert.True(result);
            Assert.True(EqualPoints(intersections, expectedIntersections, 0.001F));

            result = path1.Intersects(path3, out intersections);
            Assert.False(result);

            result = path3.Intersects(path1, out intersections);
            Assert.False(result);

            expectedIntersections = new PointF[] { new PointF(41.42857F, 43.57143F) };
            result = path2.Intersects(path3, out intersections);
            Assert.True(result);
            Assert.True(EqualPoints(intersections, expectedIntersections, 0.001F));

            result = path3.Intersects(path2, out intersections);
            Assert.True(result);
            Assert.True(EqualPoints(intersections, expectedIntersections, 0.001F));
        }

        [Test]
        public void TestTangetAtPoint()
        {
            float result;

            result = p1.TangentAngleAtPoint(new PointF(5,5));
            Assert.AreEqual(0, result);

            result = p3.TangentAngleAtPoint(new PointF(0, 0));
            Assert.AreEqual(45, result);

            result = p3.TangentAngleAtPoint(new PointF(3, 3));
            Assert.AreEqual(45, result);

            result = p3.TangentAngleAtPoint(new PointF(10, 10));
            Assert.AreEqual(58.41796, result, 0.0001F);

            result = p3.TangentAngleAtPoint(new PointF(10, 10));
            Assert.AreEqual(58.41796, result, 0.0001F);

            result = p2.TangentAngleAtPoint(new PointF(0.0753633F, 4.16412F));
            Assert.AreEqual(98.6774139, result, 0.0001F);
        }
    }
}

#endif //TEST
