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
    partial class PrintDescriptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintDescriptions));
            this.printerGroup = new System.Windows.Forms.GroupBox();
            this.outputPanel = new System.Windows.Forms.TableLayoutPanel();
            this.marginChange = new System.Windows.Forms.Button();
            this.paperSize = new System.Windows.Forms.Label();
            this.paperSizeLabel = new System.Windows.Forms.Label();
            this.margins = new System.Windows.Forms.Label();
            this.marginsLabel = new System.Windows.Forms.Label();
            this.orientation = new System.Windows.Forms.Label();
            this.orientationLabel = new System.Windows.Forms.Label();
            this.printerName = new System.Windows.Forms.Label();
            this.printerChange = new System.Windows.Forms.Button();
            this.printerLabel = new System.Windows.Forms.Label();
            this.layoutGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.descriptionKindCombo = new System.Windows.Forms.ComboBox();
            this.descriptionTypeLabel = new System.Windows.Forms.Label();
            this.mmSuffixLabel = new System.Windows.Forms.Label();
            this.boxSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.lineSizeLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.printButton = new System.Windows.Forms.Button();
            this.previewButton = new System.Windows.Forms.Button();
            this.coursesGroupBox = new System.Windows.Forms.GroupBox();
            this.courseSelector = new PurplePen.CourseSelector();
            this.pageSetupDialog = new System.Windows.Forms.PageSetupDialog();
            this.copiesGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.descriptionsLabel = new System.Windows.Forms.Label();
            this.descriptionsUpDown = new System.Windows.Forms.NumericUpDown();
            this.copiesCombo = new System.Windows.Forms.ComboBox();
            this.whatToPrintLabel = new System.Windows.Forms.Label();
            this.printDialog = new System.Windows.Forms.PrintDialog();
            this.printerGroup.SuspendLayout();
            this.outputPanel.SuspendLayout();
            this.layoutGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxSizeUpDown)).BeginInit();
            this.coursesGroupBox.SuspendLayout();
            this.copiesGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.descriptionsUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // printerGroup
            // 
            this.printerGroup.Controls.Add(this.outputPanel);
            resources.ApplyResources(this.printerGroup, "printerGroup");
            this.printerGroup.Name = "printerGroup";
            this.printerGroup.TabStop = false;
            // 
            // outputPanel
            // 
            resources.ApplyResources(this.outputPanel, "outputPanel");
            this.outputPanel.Controls.Add(this.marginChange, 2, 2);
            this.outputPanel.Controls.Add(this.paperSize, 1, 1);
            this.outputPanel.Controls.Add(this.paperSizeLabel, 0, 1);
            this.outputPanel.Controls.Add(this.margins, 1, 3);
            this.outputPanel.Controls.Add(this.marginsLabel, 0, 3);
            this.outputPanel.Controls.Add(this.orientation, 1, 2);
            this.outputPanel.Controls.Add(this.orientationLabel, 0, 2);
            this.outputPanel.Controls.Add(this.printerName, 1, 0);
            this.outputPanel.Controls.Add(this.printerChange, 2, 0);
            this.outputPanel.Controls.Add(this.printerLabel, 0, 0);
            this.outputPanel.Name = "outputPanel";
            // 
            // marginChange
            // 
            resources.ApplyResources(this.marginChange, "marginChange");
            this.marginChange.Name = "marginChange";
            this.outputPanel.SetRowSpan(this.marginChange, 2);
            this.marginChange.UseVisualStyleBackColor = true;
            this.marginChange.Click += new System.EventHandler(this.marginChange_Click);
            // 
            // paperSize
            // 
            resources.ApplyResources(this.paperSize, "paperSize");
            this.paperSize.Name = "paperSize";
            // 
            // paperSizeLabel
            // 
            resources.ApplyResources(this.paperSizeLabel, "paperSizeLabel");
            this.paperSizeLabel.Name = "paperSizeLabel";
            // 
            // margins
            // 
            resources.ApplyResources(this.margins, "margins");
            this.margins.Name = "margins";
            // 
            // marginsLabel
            // 
            resources.ApplyResources(this.marginsLabel, "marginsLabel");
            this.marginsLabel.Name = "marginsLabel";
            // 
            // orientation
            // 
            resources.ApplyResources(this.orientation, "orientation");
            this.orientation.Name = "orientation";
            // 
            // orientationLabel
            // 
            resources.ApplyResources(this.orientationLabel, "orientationLabel");
            this.orientationLabel.Name = "orientationLabel";
            // 
            // printerName
            // 
            resources.ApplyResources(this.printerName, "printerName");
            this.printerName.Name = "printerName";
            // 
            // printerChange
            // 
            resources.ApplyResources(this.printerChange, "printerChange");
            this.printerChange.Name = "printerChange";
            this.outputPanel.SetRowSpan(this.printerChange, 2);
            this.printerChange.UseVisualStyleBackColor = true;
            this.printerChange.Click += new System.EventHandler(this.printerChange_Click);
            // 
            // printerLabel
            // 
            resources.ApplyResources(this.printerLabel, "printerLabel");
            this.printerLabel.Name = "printerLabel";
            // 
            // layoutGroup
            // 
            this.layoutGroup.Controls.Add(this.tableLayoutPanel3);
            resources.ApplyResources(this.layoutGroup, "layoutGroup");
            this.layoutGroup.Name = "layoutGroup";
            this.layoutGroup.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.descriptionKindCombo, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.descriptionTypeLabel, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.mmSuffixLabel, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.boxSizeUpDown, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.lineSizeLabel, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // descriptionKindCombo
            // 
            resources.ApplyResources(this.descriptionKindCombo, "descriptionKindCombo");
            this.tableLayoutPanel3.SetColumnSpan(this.descriptionKindCombo, 2);
            this.descriptionKindCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.descriptionKindCombo.FormattingEnabled = true;
            this.descriptionKindCombo.Items.AddRange(new object[] {
            resources.GetString("descriptionKindCombo.Items"),
            resources.GetString("descriptionKindCombo.Items1"),
            resources.GetString("descriptionKindCombo.Items2"),
            resources.GetString("descriptionKindCombo.Items3")});
            this.descriptionKindCombo.Name = "descriptionKindCombo";
            this.descriptionKindCombo.SelectedIndexChanged += new System.EventHandler(this.descriptionKindCombo_SelectedIndexChanged);
            // 
            // descriptionTypeLabel
            // 
            resources.ApplyResources(this.descriptionTypeLabel, "descriptionTypeLabel");
            this.descriptionTypeLabel.Name = "descriptionTypeLabel";
            // 
            // mmSuffixLabel
            // 
            resources.ApplyResources(this.mmSuffixLabel, "mmSuffixLabel");
            this.mmSuffixLabel.Name = "mmSuffixLabel";
            // 
            // boxSizeUpDown
            // 
            resources.ApplyResources(this.boxSizeUpDown, "boxSizeUpDown");
            this.boxSizeUpDown.DecimalPlaces = 1;
            this.boxSizeUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.boxSizeUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.boxSizeUpDown.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.boxSizeUpDown.Name = "boxSizeUpDown";
            this.boxSizeUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lineSizeLabel
            // 
            resources.ApplyResources(this.lineSizeLabel, "lineSizeLabel");
            this.lineSizeLabel.Name = "lineSizeLabel";
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // printButton
            // 
            resources.ApplyResources(this.printButton, "printButton");
            this.printButton.Name = "printButton";
            this.printButton.UseVisualStyleBackColor = true;
            this.printButton.Click += new System.EventHandler(this.printButton_Click);
            // 
            // previewButton
            // 
            resources.ApplyResources(this.previewButton, "previewButton");
            this.previewButton.Name = "previewButton";
            this.previewButton.UseVisualStyleBackColor = true;
            this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // coursesGroupBox
            // 
            this.coursesGroupBox.Controls.Add(this.courseSelector);
            resources.ApplyResources(this.coursesGroupBox, "coursesGroupBox");
            this.coursesGroupBox.Name = "coursesGroupBox";
            this.coursesGroupBox.TabStop = false;
            // 
            // courseSelector
            // 
            this.courseSelector.Filter = null;
            resources.ApplyResources(this.courseSelector, "courseSelector");
            this.courseSelector.Name = "courseSelector";
            this.courseSelector.ShowAllControls = true;
            this.courseSelector.ShowCourseParts = false;
            this.courseSelector.ShowVariationChooser = true;
            // 
            // copiesGroupBox
            // 
            this.copiesGroupBox.Controls.Add(this.tableLayoutPanel2);
            resources.ApplyResources(this.copiesGroupBox, "copiesGroupBox");
            this.copiesGroupBox.Name = "copiesGroupBox";
            this.copiesGroupBox.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.descriptionsLabel, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.descriptionsUpDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.copiesCombo, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.whatToPrintLabel, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // descriptionsLabel
            // 
            resources.ApplyResources(this.descriptionsLabel, "descriptionsLabel");
            this.descriptionsLabel.Name = "descriptionsLabel";
            // 
            // descriptionsUpDown
            // 
            resources.ApplyResources(this.descriptionsUpDown, "descriptionsUpDown");
            this.descriptionsUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.descriptionsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.descriptionsUpDown.Name = "descriptionsUpDown";
            this.descriptionsUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // copiesCombo
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.copiesCombo, 2);
            this.copiesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.copiesCombo.FormattingEnabled = true;
            this.copiesCombo.Items.AddRange(new object[] {
            resources.GetString("copiesCombo.Items"),
            resources.GetString("copiesCombo.Items1"),
            resources.GetString("copiesCombo.Items2")});
            resources.ApplyResources(this.copiesCombo, "copiesCombo");
            this.copiesCombo.Name = "copiesCombo";
            this.copiesCombo.SelectedIndexChanged += new System.EventHandler(this.copiesCombo_SelectedIndexChanged);
            // 
            // whatToPrintLabel
            // 
            resources.ApplyResources(this.whatToPrintLabel, "whatToPrintLabel");
            this.whatToPrintLabel.Name = "whatToPrintLabel";
            // 
            // printDialog
            // 
            this.printDialog.UseEXDialog = true;
            // 
            // PrintDescriptions
            // 
            this.AcceptButton = this.printButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.copiesGroupBox);
            this.Controls.Add(this.coursesGroupBox);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.printButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.layoutGroup);
            this.Controls.Add(this.printerGroup);
            this.HelpTopic = "FilePrintDescriptions.htm";
            this.Name = "PrintDescriptions";
            this.printerGroup.ResumeLayout(false);
            this.outputPanel.ResumeLayout(false);
            this.outputPanel.PerformLayout();
            this.layoutGroup.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxSizeUpDown)).EndInit();
            this.coursesGroupBox.ResumeLayout(false);
            this.copiesGroupBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.descriptionsUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox printerGroup;
        private System.Windows.Forms.GroupBox layoutGroup;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button printButton;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.GroupBox coursesGroupBox;
        private System.Windows.Forms.PageSetupDialog pageSetupDialog;
        private System.Windows.Forms.GroupBox copiesGroupBox;
        private CourseSelector courseSelector;
        private System.Windows.Forms.PrintDialog printDialog;
        private System.Windows.Forms.TableLayoutPanel outputPanel;
        private System.Windows.Forms.Button marginChange;
        private System.Windows.Forms.Label paperSize;
        private System.Windows.Forms.Label paperSizeLabel;
        private System.Windows.Forms.Label margins;
        private System.Windows.Forms.Label marginsLabel;
        private System.Windows.Forms.Label orientation;
        private System.Windows.Forms.Label orientationLabel;
        private System.Windows.Forms.Label printerName;
        private System.Windows.Forms.Label printerLabel;
        private System.Windows.Forms.Button printerChange;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label descriptionsLabel;
        private System.Windows.Forms.NumericUpDown descriptionsUpDown;
        private System.Windows.Forms.ComboBox copiesCombo;
        private System.Windows.Forms.Label whatToPrintLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.NumericUpDown boxSizeUpDown;
        private System.Windows.Forms.Label lineSizeLabel;
        private System.Windows.Forms.ComboBox descriptionKindCombo;
        private System.Windows.Forms.Label descriptionTypeLabel;
        private System.Windows.Forms.Label mmSuffixLabel;
    }
}
