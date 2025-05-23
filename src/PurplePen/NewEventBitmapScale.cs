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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PurplePen
{
    public partial class NewEventBitmapScale: UserControl, NewEventWizard.IWizardPage
    {
        NewEventWizard containingWizard;
        public float dpi;
        public MapType mapType;

        public NewEventBitmapScale()
        {
            InitializeComponent();
        }

        public bool CanProceed
        {
            get {
                float mapScale = 0;
                bool result = (mapType == MapType.PDF || float.TryParse(dpiTextBox.Text, out dpi)) && 
                              float.TryParse(scaleTextBox.Text, out mapScale);
                if (result)
                    containingWizard.MapScale = mapScale;
                return result;
            }
        }

        public string Title
        {
            get { return labelTitle.Text; }
        }

        private void NewEventBitmapScale_Load(object sender, EventArgs e)
        {
            containingWizard = (NewEventWizard) Parent;
            mapType = containingWizard.MapType;

            if (mapType == MapType.Bitmap) {
                Bitmap bitmap = (Bitmap)Image.FromFile(containingWizard.MapFileName);

                // GIF format doesn't have built-in resolution, so don't default it.
                if (bitmap.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                    dpiTextBox.Text = "";
                else
                    dpiTextBox.Text = bitmap.HorizontalResolution.ToString();

                pdfScaleLabel.Visible = false;
                bitmapScaleLabel.Visible = dpiTextBox.Visible = resolutionLabel.Visible = dpiLabel.Visible = true;
                bitmap.Dispose();
            }
            else {
                pdfScaleLabel.Visible = true;
                bitmapScaleLabel.Visible = dpiTextBox.Visible = resolutionLabel.Visible = dpiLabel.Visible = false;
            }

            scaleTextBox.Text = "15000";
        }
    }
}
