﻿/* Copyright (c) 2006-2008, Peter Golde
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
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using PurplePen.MapModel;

namespace PurplePen
{
    using System.Linq;
    using PurplePen.Graphics2D;

    // The topology formatter transforms a CourseView into the layout that shows the topology of the course.
    class TopologyFormatter
    {
        EventDB eventDB;
        SymbolDB symbolDB;
        CourseLayout courseLayout;
        CourseLayer courseLayerAllVariationsAndParts;
        CourseLayer courseLayerSpecificVariation;
        List<CourseView.ControlView> controlViewsAllVariationsAndParts;
        List<CourseView.ControlView> controlViewsSpecificVariation;
        Dictionary<Id<CourseControl>, char> variationMap;
        Id<CourseControl>[] courseControlIdsSpecificVariation;
        Id<CourseControl> courseControlIdSelection1, courseControlIdSelection2;
        CourseAppearance appearance;
        float courseObjRatio;

        ControlPosition[] controlPositions;

        const float heightUnit = 10;
        const float widthUnit = 15;

        class ControlPosition
        {
            public float x, y;     // starting location, in basic height/width units.
            public ForkPosition[] forkStart;  // If this is a fork, location where fork goes to. Makes drawing empty forks possible.
            public float loopBottom; // bottom of loop, if this is start of a loop.
        }

        class ForkPosition
        {
            public float x, y;
            public bool loopStart;
            public bool loopFallThru;
            public char variationCode;

            public ForkPosition(float x, float y, bool loopStart, bool loopFallThru, char variationCode)
            {
                this.x = x; this.y = y; this.loopStart = loopStart; this.loopFallThru = loopFallThru;
                this.variationCode = variationCode;
            }
        }

        // Format the given CourseView into a bunch of course objects, and add it to the given course Layout
        public RectangleF FormatCourseToLayout(SymbolDB symbolDB, CourseView courseViewAllVariations, CourseView specificVariation, CourseLayout courseLayout, 
                                               Id<CourseControl> ccSelection1, Id<CourseControl> ccSelection2, CourseLayer layerAllVariations, CourseLayer layerSpecificVariation)
        {
            this.eventDB = courseViewAllVariations.EventDB;
            this.symbolDB = symbolDB;
            this.courseLayout = courseLayout;
            this.courseLayerAllVariationsAndParts = layerAllVariations;
            this.courseLayerSpecificVariation = layerSpecificVariation;
            this.controlViewsAllVariationsAndParts = courseViewAllVariations.ControlViews;
            this.controlViewsSpecificVariation = specificVariation.ControlViews;
            this.controlPositions = new ControlPosition[controlViewsAllVariationsAndParts.Count];
            this.courseControlIdsSpecificVariation = QueryEvent.EnumCourseControlIds(eventDB, specificVariation.CourseDesignator).ToArray();
            this.courseControlIdSelection1 = ccSelection1;
            this.courseControlIdSelection2 = ccSelection2;
            this.variationMap = QueryEvent.GetVariantCodeMapping(eventDB, courseViewAllVariations.CourseDesignator);

            SizeF totalAbstractSize = AssignControlPositions(0, controlViewsAllVariationsAndParts.Count, 0, 0);

            // Now create objects now that the positions have been created.
            courseObjRatio = 1.0F;
            appearance = new CourseAppearance();

            for (int index = 0; index < controlViewsAllVariationsAndParts.Count; ++index) {
                CreateObjectsForControlView(controlViewsAllVariationsAndParts[index], controlPositions[index]);
            }

            PointF bottomCenter = LocationFromAbstractPosition(0, 0);
            SizeF size = SizeFromAbstractSize(totalAbstractSize);
            RectangleF rect = new RectangleF(bottomCenter.X - size.Width / 2, bottomCenter.Y - size.Height, size.Width, size.Height);
            rect.Inflate(widthUnit, heightUnit);
            return rect;
        }

        private void CreateObjectsForControlView(CourseView.ControlView controlView, ControlPosition controlPosition)
        {
            CreateControlNumber(controlView, controlPosition);
            
            if (controlView.legTo != null) {
                for (int i = 0; i < controlView.legTo.Length; ++i) {
                    ForkPosition forkStart;
                    if (controlPosition.forkStart != null)
                        forkStart = controlPosition.forkStart[i];
                    else
                        forkStart = null;

                    CreateLegBetweenControls(controlView, controlPosition, controlViewsAllVariationsAndParts[controlView.legTo[i]], controlPositions[controlView.legTo[i]], i, forkStart);
                }
            }
            
        }

        // Is the given control view in the specific variation.
        private bool ControlViewInSpecificVariation(CourseView.ControlView controlView)
        {
            if (controlViewsSpecificVariation == controlViewsAllVariationsAndParts)
                return true;

            foreach (Id<CourseControl> idCourseControl in controlView.courseControlIds) {
                if (courseControlIdsSpecificVariation.Contains(idCourseControl))
                    return true;
            }

            return false;
        }

        // Is the given control leg in the specific variation.
        private bool LegInSpecificVariation(Id<CourseControl> start, Id<CourseControl> end)
        {
            if (controlViewsSpecificVariation == controlViewsAllVariationsAndParts)
                return true;

            for (int i = 0; i < courseControlIdsSpecificVariation.Length - 1; ++i) {
                if (courseControlIdsSpecificVariation[i] == start) {
                    if (courseControlIdsSpecificVariation[i+1] == end)
                        return true;
                    CourseControl endCourseControl = eventDB.GetCourseControl(end);
                    if (endCourseControl.splitCourseControls != null && endCourseControl.splitCourseControls.Contains(courseControlIdsSpecificVariation[i+1]))
                        return true;
                }
            }

            return false;
        }

        private void CreateControlNumber(CourseView.ControlView controlView, ControlPosition controlPosition)
        {
            Id<ControlPoint> controlId = controlView.controlId;
            Id<CourseControl> courseControlId = controlView.courseControlIds[0];
            ControlPoint control = eventDB.GetControl(controlId);
            PointF location = LocationFromAbstractPosition(controlPosition.x, controlPosition.y);

            CourseLayer layer;

            if (ControlViewInSpecificVariation(controlView))
                layer = courseLayerSpecificVariation;
            else
                layer = courseLayerAllVariationsAndParts;

            CourseObj courseObj;

            switch (control.kind) {
                case ControlPointKind.Start:
                case ControlPointKind.MapExchange:
                    // Triangle looks best if we displace it down a bit (0.8 looks right).
                    courseObj = new StartCourseObj(controlId, courseControlId, courseObjRatio * 0.75F, appearance, 0, new PointF(location.X, location.Y - 0.8F), CrossHairOptions.NoCrossHair);
                    break;

                case ControlPointKind.Finish:
                    courseObj = new FinishCourseObj(controlId, courseControlId, courseObjRatio * 0.75F, appearance, null, location, CrossHairOptions.NoCrossHair);
                    break;

                case ControlPointKind.Normal:
                    courseObj = new ControlNumberCourseObj(controlId, courseControlId, courseObjRatio, appearance, control.code, location);
                    break;

                case ControlPointKind.CrossingPoint:
                    courseObj = new CrossingCourseObj(controlId, courseControlId, Id<Special>.None, courseObjRatio * 1.5F, appearance, 0, 0, location);
                    break;

                case ControlPointKind.MapIssue:
                    courseObj = new MapIssueCourseObj(controlId, courseControlId, courseObjRatio * 1.5F, appearance, -90, new PointF(location.X - 0.8F, location.Y), MapIssueCourseObj.RenderStyle.WithTail);
                    break;

                default:
                    Debug.Fail("bad control kind");
                    return;
            }

            courseObj.layer = layer;
            courseLayout.AddCourseObject(courseObj);
        }

        private void CreateLegBetweenControls(CourseView.ControlView controlView1, ControlPosition controlPosition1, CourseView.ControlView controlView2, ControlPosition controlPosition2, int splitLegIndex, ForkPosition forkStart)
        {
            List<DropTarget> dropTargets;
            SymPath path = PathBetweenControls(controlPosition1, controlPosition2, forkStart, out dropTargets);
            CourseObj courseObj = new TopologyLegCourseObj(controlView1.controlId, controlView1.courseControlIds[splitLegIndex], controlView2.courseControlIds[0], courseObjRatio, appearance, path);
            CourseLayer layer;

            if (LegInSpecificVariation(controlView1.courseControlIds[splitLegIndex], controlView2.courseControlIds[0]))
                layer = courseLayerSpecificVariation;
            else
                layer = courseLayerAllVariationsAndParts;

            courseObj.layer = layer;
            courseLayout.AddCourseObject(courseObj);

            // No drop targets between map issue and start.
            if (!(eventDB.GetControl(controlView1.controlId).kind == ControlPointKind.MapIssue &&
                  eventDB.GetControl(controlView2.controlId).kind == ControlPointKind.Start)) 
            {
                // Add the drop targets
                foreach (DropTarget dropTarget in dropTargets) {
                    courseObj = new TopologyDropTargetCourseObj(controlView1.controlId, controlView1.courseControlIds[splitLegIndex], controlView2.courseControlIds[0], courseObjRatio, appearance,
                        LocationFromAbstractPosition(dropTarget.abstractX, dropTarget.abstractY), dropTarget.insertionLoc);

                    // Along the selected leg, show the drop targets in light gray. Along other legs, drop targets are invisible.
                    if (controlView1.courseControlIds[splitLegIndex] == courseControlIdSelection1 && controlView2.courseControlIds.Contains(courseControlIdSelection2))
                        courseObj.layer = CourseLayer.AllVariations;
                    else
                        courseObj.layer = CourseLayer.InvisibleObjects;
                    courseLayout.AddCourseObject(courseObj);
                }
            }

            if (forkStart != null && forkStart.variationCode != '\0') {
                // There is a variation fork.
                courseObj = CreateVariationCode(controlView1, controlPosition1, splitLegIndex, forkStart);
                courseLayout.AddCourseObject(courseObj);
            }
        }

        // Determine the path between controls, and the drop targets.
        SymPath PathBetweenControls(ControlPosition controlPosition1, ControlPosition controlPosition2, ForkPosition forkStart, out List<DropTarget> dropTargets)
        {
            float xStart = controlPosition1.x;
            float yStart = controlPosition1.y;
            float xEnd = controlPosition2.x;
            float yEnd = controlPosition2.y;

            dropTargets = new List<DropTarget>();

            if (forkStart != null) {
                if (forkStart.loopFallThru) {
                    dropTargets.Add(new DropTarget(xEnd, yEnd - 0.5F, LegInsertionLoc.Normal));
                }
                else if (forkStart.loopStart) {
                    dropTargets.Add(new DropTarget(forkStart.x, forkStart.y - 0.5F, LegInsertionLoc.Normal));
                }
                else {
                    dropTargets.Add(new DropTarget(xStart, yStart + 0.5F, LegInsertionLoc.PreSplit));
                    dropTargets.Add(new DropTarget(forkStart.x, forkStart.y - 0.5F, LegInsertionLoc.Normal));
                    if (forkStart.x != xEnd) {
                        // Empty fork.
                        dropTargets.Add(new DropTarget(xEnd, yEnd - 0.5F, LegInsertionLoc.PostJoin));
                    }
                }
            }
            else if (xEnd != xStart) {
                // Below start control (use when a join is going)
                dropTargets.Add(new DropTarget(xStart, yStart + 0.5F, LegInsertionLoc.Normal));
                if (yEnd > yStart) {
                    // Right before join point.
                    dropTargets.Add(new DropTarget(xEnd, yEnd - 0.5F, LegInsertionLoc.PostJoin));
                }
            }
            else if (yEnd > yStart + 1.25) {
                // Straight down, but must have join at end.
                dropTargets.Add(new DropTarget(xStart, yStart + 0.5F, LegInsertionLoc.Normal));
                dropTargets.Add(new DropTarget(xEnd, yEnd - 0.5F, LegInsertionLoc.PostJoin));
            }
            else {
                // Above end control (other cases).
                dropTargets.Add(new DropTarget(xEnd, yEnd - 0.5F, LegInsertionLoc.Normal));
            }

            bool startHorizontal = false;
            if (forkStart != null)
                startHorizontal = forkStart.loopStart;

            if (forkStart != null && forkStart.x != controlPosition2.x) {
                // The fork start in a different horizontal position than it ends. This is probably due to a fork with no controls on it.
                // Create the path in two pieces.
                float xMiddle = forkStart.x;
                float yMiddle = forkStart.y;
                SymPath path1 = PathFromStartToEnd(xStart, yStart, xMiddle, yMiddle, startHorizontal, 0);
                SymPath path3 = PathFromStartToEnd(xMiddle, yMiddle, xEnd, yEnd, false, controlPosition2.loopBottom);
                SymPath path2 = new SymPath(new[] { path1.LastPoint, path3.FirstPoint });
                return SymPath.Join(SymPath.Join(path1, path2, PointKind.Normal), path3, PointKind.Normal);
            }
            else {
                return PathFromStartToEnd(xStart, yStart, xEnd, yEnd, startHorizontal, controlPosition2.loopBottom);
            }
        }

        SymPath PathFromStartToEnd(float xStart, float yStart, float xEnd, float yEnd, bool startHorizontal, float yLoopBottom)
        {
            const float yUp = 0.45F;  // Horizontal line above end
            const float xCorner = 0.15F, yCorner = xCorner * widthUnit / heightUnit;
            const float xBez = 0.075F, yBez = xBez * widthUnit / heightUnit;

            float xDir = Math.Sign(xEnd - xStart);
            float yDir = (yEnd <= yStart) ? -1 : 1; 

            yEnd -= 0.3F * yDir;
            if (startHorizontal) {
                xStart += 0.4F * xDir;
            }
            else {
                yStart += 0.3F;
            }

            SymPath path;
            if (xStart == xEnd) {
                path = new SymPath(new[] { LocationFromAbstractPosition(xStart, yStart), LocationFromAbstractPosition(xEnd, yEnd) });
            }
            else if (startHorizontal) {
                float yHoriz = yStart;
                path = new SymPath(new[] { 
                                            LocationFromAbstractPosition(xStart, yHoriz), 
                                            /* horizontal line */
                                            LocationFromAbstractPosition(xEnd - xCorner * xDir, yHoriz),
                                            LocationFromAbstractPosition(xEnd - xBez * xDir, yHoriz), 
                                            /* corner: LocationFromAbstractPosition(xEnd, yHoriz), */
                                            LocationFromAbstractPosition(xEnd, yHoriz + yBez * yDir),
                                            LocationFromAbstractPosition(xEnd, yHoriz + yCorner * yDir), 
                                            /* vertical line */
                                            LocationFromAbstractPosition(xEnd, yEnd) },
                                   new[] { PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal, PointKind.Normal });

            }
            else {
                float yHoriz; 

                if (yDir < 0) {
                    yHoriz = yLoopBottom + yUp;
                    xEnd -= 0.3F * xDir;
                }
                else {
                    yHoriz = yEnd - yUp; 
                }

                path = new SymPath(new[] { LocationFromAbstractPosition(xStart, yStart),
                                            /* vertical line */
                                            LocationFromAbstractPosition(xStart, yHoriz - yCorner),
                                            LocationFromAbstractPosition(xStart, yHoriz - yBez), 
                                            /* corner: LocationFromAbstractPosition(xStart, yHoriz), */
                                            LocationFromAbstractPosition(xStart + xBez * xDir, yHoriz),
                                            LocationFromAbstractPosition(xStart + xCorner * xDir, yHoriz), 
                                            /* horizontal line */
                                            LocationFromAbstractPosition(xEnd - xCorner * xDir, yHoriz),
                                            LocationFromAbstractPosition(xEnd - xBez * xDir, yHoriz), 
                                            /* corner: LocationFromAbstractPosition(xEnd, yHoriz), */
                                            LocationFromAbstractPosition(xEnd, yHoriz + yBez * yDir),
                                            LocationFromAbstractPosition(xEnd, yHoriz + yCorner * yDir), 
                                            /* vertical line */
                                            LocationFromAbstractPosition(xEnd, yEnd) },
                                   new[] { PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal, PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal, PointKind.Normal });


            }

            return path;
        }

        private CourseObj CreateVariationCode(CourseView.ControlView controlView1, ControlPosition controlPosition1, int splitLegIndex, ForkPosition forkStart)
        {
            // Delta between position of fork start and position of code.
            float deltaX = (forkStart.x < controlPosition1.x) ? -0.4F : 0.4F;
            float deltaY = -0.4F;

            float x = forkStart.x + deltaX;
            float y = forkStart.y + deltaY;

            string text = "(" + forkStart.variationCode + ")";
            CourseObj courseObj = new VariationCodeCourseObj(controlView1.controlId, controlView1.courseControlIds[splitLegIndex], courseObjRatio, appearance, text, LocationFromAbstractPosition(x, y));
            courseObj.layer = CourseLayer.AllVariations;
            return courseObj;
        }


        PointF LocationFromAbstractPosition(float x, float y)
        {
            return new PointF(x * widthUnit, - y * heightUnit);
        }

        SizeF SizeFromAbstractSize(SizeF abstractSize)
        {
            return new SizeF(abstractSize.Width * widthUnit, abstractSize.Height * heightUnit);
        }



        // Assign positions/sizes from startIndex upto (not including) endIndex, starting at given position.
        // Return total size used.
        private SizeF AssignControlPositions(int startIndex, int endIndex, float startX, float startY)
        {
            int index = startIndex;
            float x = startX, y = startY;
            float totalWidth = 1, totalHeight = 0;   // Always at least a width of 1.
            while (index != endIndex) {
                int numForks = (controlViewsAllVariationsAndParts[index].legTo == null) ? 0 : controlViewsAllVariationsAndParts[index].legTo.Length;

                // Simple case, no splitting.
                controlPositions[index] = new ControlPosition() {
                    x = x,
                    y = y,
                };
                totalWidth = Math.Max(totalWidth, 1);
                totalHeight += 1;
                y += 1;

                if (numForks > 1) {
                    bool loop = (controlViewsAllVariationsAndParts[index].joinIndex == index);

                    // fork or loop subsequent. Two passes -- first determine totalWidth and maxHeight;
                    float totalForkWidth = 0, maxForkHeight = 1;
                    SizeF[] forkSize = new SizeF[numForks];
                    ForkPosition[] forkStart = new ForkPosition[numForks];

                    int startFork = 0;
                    if (loop) {
                        startFork = 1; 
                        totalForkWidth = 1;
                    }

                    // Get size of each fork.
                    for (int i = startFork; i < numForks; ++i) {
                        forkSize[i] = AssignControlPositions(controlViewsAllVariationsAndParts[index].legTo[i], controlViewsAllVariationsAndParts[index].joinIndex, 0, 0);
                        totalForkWidth += forkSize[i].Width;
                        maxForkHeight = Math.Max(maxForkHeight, forkSize[i].Height);
                    }

                    // Get position of each fork.
                    if (loop) {
                        float forkY = y;
                        float forkX = x;
                        forkStart[0] = new ForkPosition(forkX, forkY, false, true, '\0');
                        int halfForks = (numForks + 1) / 2;

                        totalForkWidth = 0;

                        forkX = x - 0.5F;
                        for (int i = startFork; i < halfForks; ++i) {
                            forkX -= forkSize[i].Width;
                        }

                        totalForkWidth = Math.Max(totalForkWidth, (x-forkX) * 2);

                        for (int i = startFork; i < halfForks; ++i) {
                            forkX += forkSize[i].Width / 2;
                            forkStart[i] = new ForkPosition(forkX, forkY, loop, false, variationMap[controlViewsAllVariationsAndParts[index].courseControlIds[i]]);
                            forkX += forkSize[i].Width / 2;
                        }

                        forkX = x + 0.5F;

                        for (int i = halfForks; i < numForks; ++i) {
                            forkX += forkSize[i].Width / 2;
                            forkStart[i] = new ForkPosition(forkX, forkY, loop, false, variationMap[controlViewsAllVariationsAndParts[index].courseControlIds[i]]);
                            forkX += forkSize[i].Width / 2;
                        }

                        totalForkWidth = Math.Max(totalForkWidth, (forkX-x) * 2);

                        controlPositions[index].loopBottom = y + maxForkHeight - 0.5F;
                    }
                    else { 
                        float forkY = y + 0.5F;
                        float forkX = x - totalForkWidth / 2;
                        for (int i = startFork; i < numForks; ++i) {
                            forkX += forkSize[i].Width / 2;
                            forkStart[i] = new ForkPosition(forkX, forkY, loop, false, variationMap[controlViewsAllVariationsAndParts[index].courseControlIds[i]]);
                            forkX += forkSize[i].Width / 2;
                        }
                    }

                    controlPositions[index].forkStart = forkStart;

                    // Assign control positions for each fork again, now that we know the start position.
                    for (int i = startFork; i < numForks; ++i) {
                        AssignControlPositions(controlViewsAllVariationsAndParts[index].legTo[i], controlViewsAllVariationsAndParts[index].joinIndex, forkStart[i].x, forkStart[i].y);
                    }

                    float height = maxForkHeight + 1;
                    totalHeight += height;
                    y += height;
                    totalWidth = Math.Max(totalWidth, totalForkWidth);

                    if (index == controlViewsAllVariationsAndParts[index].joinIndex)
                        index = controlViewsAllVariationsAndParts[index].legTo[0];
                    else
                        index = controlViewsAllVariationsAndParts[index].joinIndex;
                }
                else {
                    if (controlViewsAllVariationsAndParts[index].legTo != null && controlViewsAllVariationsAndParts[index].legTo.Length > 0)
                        index = controlViewsAllVariationsAndParts[index].legTo[0];
                    else
                        break; // no more controls.
                }
            }

            return new SizeF(totalWidth, totalHeight);
        }

        class DropTarget
        {
            public float abstractX, abstractY;
            public LegInsertionLoc insertionLoc;

            public DropTarget(float abstractX, float abstractY, LegInsertionLoc insertionLoc)
            {
                this.abstractX = abstractX;
                this.abstractY = abstractY;
                this.insertionLoc = insertionLoc;
            }
        }

    }
}
