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
    partial class InitialScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InitialScreen));
            this.okButton = new System.Windows.Forms.Button();
            this.openLastRadioButton = new System.Windows.Forms.RadioButton();
            this.openSampleRadioButton = new System.Windows.Forms.RadioButton();
            this.backgroundPanel = new System.Windows.Forms.Panel();
            this.createNewRadioButton = new System.Windows.Forms.RadioButton();
            this.openExistingRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.donationLink = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // openLastRadioButton
            // 
            resources.ApplyResources(this.openLastRadioButton, "openLastRadioButton");
            this.openLastRadioButton.Checked = true;
            this.openLastRadioButton.Name = "openLastRadioButton";
            this.openLastRadioButton.TabStop = true;
            // 
            // openSampleRadioButton
            // 
            resources.ApplyResources(this.openSampleRadioButton, "openSampleRadioButton");
            this.openSampleRadioButton.Name = "openSampleRadioButton";
            // 
            // backgroundPanel
            // 
            resources.ApplyResources(this.backgroundPanel, "backgroundPanel");
            this.backgroundPanel.BackColor = System.Drawing.Color.White;
            this.backgroundPanel.Name = "backgroundPanel";
            this.backgroundPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.backgroundPanel_Paint);
            // 
            // createNewRadioButton
            // 
            resources.ApplyResources(this.createNewRadioButton, "createNewRadioButton");
            this.createNewRadioButton.Name = "createNewRadioButton";
            // 
            // openExistingRadioButton
            // 
            resources.ApplyResources(this.openExistingRadioButton, "openExistingRadioButton");
            this.openExistingRadioButton.Name = "openExistingRadioButton";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(210)))));
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.donationLink);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // donationLink
            // 
            resources.ApplyResources(this.donationLink, "donationLink");
            this.donationLink.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.donationLink.Name = "donationLink";
            this.donationLink.TabStop = true;
            this.donationLink.UseCompatibleTextRendering = true;
            this.donationLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donationLink_Click);
            this.donationLink.Click += new System.EventHandler(this.donationLink_Click);
            // 
            // InitialScreen
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.openLastRadioButton);
            this.Controls.Add(this.openSampleRadioButton);
            this.Controls.Add(this.backgroundPanel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.createNewRadioButton);
            this.Controls.Add(this.openExistingRadioButton);
            this.HelpButton = false;
            this.Name = "InitialScreen";
            this.ShowIcon = true;
            this.ShowInTaskbar = true;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InitialScreen_FormClosed);
            this.Shown += new System.EventHandler(this.InitialScreen_Shown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton openExistingRadioButton;
        private System.Windows.Forms.RadioButton createNewRadioButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Panel backgroundPanel;
        private System.Windows.Forms.RadioButton openSampleRadioButton;
        private System.Windows.Forms.RadioButton openLastRadioButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel donationLink;
    }
}
