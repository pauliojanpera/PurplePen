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

namespace PurplePen
{
    partial class SetPrintAreaDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            mainFrame.HidePrintArea = false;

            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetPrintAreaDialog));
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.setPrintAreaLabel = new System.Windows.Forms.Label();
            this.groupBoxPaperSize = new System.Windows.Forms.GroupBox();
            this.paperSizeControl = new PurplePen.PaperSizeControl();
            this.checkBoxAutomatic = new System.Windows.Forms.CheckBox();
            this.checkBoxFixSizeToPaper = new System.Windows.Forms.CheckBox();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBoxPaperSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // setPrintAreaLabel
            // 
            resources.ApplyResources(this.setPrintAreaLabel, "setPrintAreaLabel");
            this.setPrintAreaLabel.Name = "setPrintAreaLabel";
            // 
            // groupBoxPaperSize
            // 
            resources.ApplyResources(this.groupBoxPaperSize, "groupBoxPaperSize");
            this.groupBoxPaperSize.Controls.Add(this.paperSizeControl);
            this.groupBoxPaperSize.Name = "groupBoxPaperSize";
            this.groupBoxPaperSize.TabStop = false;
            // 
            // paperSizeControl
            // 
            resources.ApplyResources(this.paperSizeControl, "paperSizeControl");
            this.paperSizeControl.Landscape = false;
            this.paperSizeControl.MarginSize = 0;
            this.paperSizeControl.Name = "paperSizeControl";
            this.paperSizeControl.PaperSize = ((System.Drawing.Printing.PaperSize)(resources.GetObject("paperSizeControl.PaperSize")));
            this.paperSizeControl.Changed += new System.EventHandler(this.paperSizeControl_Changed);
            // 
            // checkBoxAutomatic
            // 
            resources.ApplyResources(this.checkBoxAutomatic, "checkBoxAutomatic");
            this.checkBoxAutomatic.Name = "checkBoxAutomatic";
            this.checkBoxAutomatic.UseVisualStyleBackColor = true;
            this.checkBoxAutomatic.CheckedChanged += new System.EventHandler(this.checkBoxAutomatic_CheckedChanged);
            // 
            // checkBoxFixSizeToPaper
            // 
            resources.ApplyResources(this.checkBoxFixSizeToPaper, "checkBoxFixSizeToPaper");
            this.checkBoxFixSizeToPaper.Name = "checkBoxFixSizeToPaper";
            this.checkBoxFixSizeToPaper.UseVisualStyleBackColor = true;
            this.checkBoxFixSizeToPaper.CheckedChanged += new System.EventHandler(this.checkBoxFixSizeToPaper_CheckedChanged);
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 500;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // SetPrintAreaDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.checkBoxFixSizeToPaper);
            this.Controls.Add(this.checkBoxAutomatic);
            this.Controls.Add(this.groupBoxPaperSize);
            this.Controls.Add(this.setPrintAreaLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SetPrintAreaDialog";
            this.groupBoxPaperSize.ResumeLayout(false);
            this.groupBoxPaperSize.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label setPrintAreaLabel;
        private System.Windows.Forms.GroupBox groupBoxPaperSize;
        private PaperSizeControl paperSizeControl;
        private System.Windows.Forms.CheckBox checkBoxAutomatic;
        private System.Windows.Forms.CheckBox checkBoxFixSizeToPaper;
        private System.Windows.Forms.Timer updateTimer;
    }
}
