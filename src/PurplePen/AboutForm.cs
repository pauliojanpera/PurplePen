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

namespace PurplePen
{
    public partial class AboutForm: BaseDialog
    {
        public AboutForm()
        {
            InitializeComponent();

            this.versionLabel.Text = string.Format(MiscText.VersionLabel, Util.PrettyVersionString(VersionNumber.Current));
            this.bitnessLabel.Text = Environment.Is64BitProcess ? "64-bit" : "32-bit";
#if MSSTORE
            this.bitnessLabel.Text += " (Windows Store)";
#else
            this.bitnessLabel.Text += " (Standalone Setup)";
#endif
        }

        private void licenseButton_Click(object sender, EventArgs e)
        {
            new LicenseForm().ShowDialog();
        }

        private void logoPanel_Paint(object sender, PaintEventArgs e)
        {
            GraphicsHelper.DrawPurplePenLogo(e.Graphics, logoPanel);
        }

        private void creditsButton_Click(object sender, EventArgs e)
        {
            Util.ShowHelpTopic(this, "Credits.htm");
        }

        private void copyrightLabel_Click(object sender, EventArgs e)
        {

        }

        private void freeLabel_Click(object sender, EventArgs e)
        {

        }

        private void bitnessLabel_Click(object sender, EventArgs e)
        {

        }
    }
}