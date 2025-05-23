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
    partial class AddCourse
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddCourse));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.firstControlLabel = new System.Windows.Forms.Label();
            this.descriptionAppearanceLabel = new System.Windows.Forms.Label();
            this.mapScaleLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.oneToPrefixLabel = new System.Windows.Forms.Label();
            this.scaleCombo = new System.Windows.Forms.ComboBox();
            this.descKindCombo = new System.Windows.Forms.ComboBox();
            this.firstControlUpDown = new System.Windows.Forms.NumericUpDown();
            this.labelKindLabel = new System.Windows.Forms.Label();
            this.labelKindCombo = new System.Windows.Forms.ComboBox();
            this.scoreColumnLabel = new System.Windows.Forms.Label();
            this.scoreColumnCombo = new System.Windows.Forms.ComboBox();
            this.secondaryTitleGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.secondaryTitleTextBox = new System.Windows.Forms.TextBox();
            this.secondaryTitleDescription = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lengthLabel = new System.Windows.Forms.Label();
            this.climbLabel = new System.Windows.Forms.Label();
            this.courseTypeLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.courseNameLabel = new System.Windows.Forms.Label();
            this.courseKindCombo = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.climbTextBox = new System.Windows.Forms.TextBox();
            this.metersSuffix = new System.Windows.Forms.Label();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.lengthTextBox = new System.Windows.Forms.TextBox();
            this.kmSuffix = new System.Windows.Forms.Label();
            this.hideFromReportsCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.firstControlUpDown)).BeginInit();
            this.secondaryTitleGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.firstControlLabel, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.descriptionAppearanceLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.mapScaleLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.descKindCombo, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.firstControlUpDown, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.labelKindLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelKindCombo, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.scoreColumnLabel, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.scoreColumnCombo, 1, 4);
            this.tableLayoutPanel2.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // firstControlLabel
            // 
            resources.ApplyResources(this.firstControlLabel, "firstControlLabel");
            this.firstControlLabel.Name = "firstControlLabel";
            // 
            // descriptionAppearanceLabel
            // 
            resources.ApplyResources(this.descriptionAppearanceLabel, "descriptionAppearanceLabel");
            this.descriptionAppearanceLabel.Name = "descriptionAppearanceLabel";
            // 
            // mapScaleLabel
            // 
            resources.ApplyResources(this.mapScaleLabel, "mapScaleLabel");
            this.mapScaleLabel.Name = "mapScaleLabel";
            // 
            // flowLayoutPanel2
            // 
            resources.ApplyResources(this.flowLayoutPanel2, "flowLayoutPanel2");
            this.flowLayoutPanel2.Controls.Add(this.oneToPrefixLabel);
            this.flowLayoutPanel2.Controls.Add(this.scaleCombo);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            // 
            // oneToPrefixLabel
            // 
            resources.ApplyResources(this.oneToPrefixLabel, "oneToPrefixLabel");
            this.oneToPrefixLabel.Name = "oneToPrefixLabel";
            // 
            // scaleCombo
            // 
            this.scaleCombo.FormattingEnabled = true;
            resources.ApplyResources(this.scaleCombo, "scaleCombo");
            this.scaleCombo.Name = "scaleCombo";
            // 
            // descKindCombo
            // 
            resources.ApplyResources(this.descKindCombo, "descKindCombo");
            this.descKindCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.descKindCombo.FormattingEnabled = true;
            this.descKindCombo.Items.AddRange(new object[] {
            resources.GetString("descKindCombo.Items"),
            resources.GetString("descKindCombo.Items1"),
            resources.GetString("descKindCombo.Items2")});
            this.descKindCombo.Name = "descKindCombo";
            // 
            // firstControlUpDown
            // 
            resources.ApplyResources(this.firstControlUpDown, "firstControlUpDown");
            this.firstControlUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.firstControlUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.firstControlUpDown.Name = "firstControlUpDown";
            this.firstControlUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelKindLabel
            // 
            resources.ApplyResources(this.labelKindLabel, "labelKindLabel");
            this.labelKindLabel.Name = "labelKindLabel";
            // 
            // labelKindCombo
            // 
            resources.ApplyResources(this.labelKindCombo, "labelKindCombo");
            this.labelKindCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.labelKindCombo.FormattingEnabled = true;
            this.labelKindCombo.Items.AddRange(new object[] {
            resources.GetString("labelKindCombo.Items"),
            resources.GetString("labelKindCombo.Items1"),
            resources.GetString("labelKindCombo.Items2"),
            resources.GetString("labelKindCombo.Items3"),
            resources.GetString("labelKindCombo.Items4"),
            resources.GetString("labelKindCombo.Items5")});
            this.labelKindCombo.Name = "labelKindCombo";
            this.labelKindCombo.SelectionChangeCommitted += new System.EventHandler(this.labelKindCombo_SelectionChangeCommitted);
            // 
            // scoreColumnLabel
            // 
            resources.ApplyResources(this.scoreColumnLabel, "scoreColumnLabel");
            this.scoreColumnLabel.Name = "scoreColumnLabel";
            // 
            // scoreColumnCombo
            // 
            resources.ApplyResources(this.scoreColumnCombo, "scoreColumnCombo");
            this.scoreColumnCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scoreColumnCombo.FormattingEnabled = true;
            this.scoreColumnCombo.Items.AddRange(new object[] {
            resources.GetString("scoreColumnCombo.Items"),
            resources.GetString("scoreColumnCombo.Items1"),
            resources.GetString("scoreColumnCombo.Items2"),
            resources.GetString("scoreColumnCombo.Items3")});
            this.scoreColumnCombo.Name = "scoreColumnCombo";
            this.scoreColumnCombo.SelectionChangeCommitted += new System.EventHandler(this.scoreColumnCombo_SelectionChangeCommitted);
            // 
            // secondaryTitleGroup
            // 
            this.secondaryTitleGroup.Controls.Add(this.tableLayoutPanel3);
            resources.ApplyResources(this.secondaryTitleGroup, "secondaryTitleGroup");
            this.secondaryTitleGroup.Name = "secondaryTitleGroup";
            this.secondaryTitleGroup.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.secondaryTitleTextBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.secondaryTitleDescription, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // secondaryTitleTextBox
            // 
            this.secondaryTitleTextBox.AcceptsReturn = true;
            resources.ApplyResources(this.secondaryTitleTextBox, "secondaryTitleTextBox");
            this.secondaryTitleTextBox.Name = "secondaryTitleTextBox";
            // 
            // secondaryTitleDescription
            // 
            resources.ApplyResources(this.secondaryTitleDescription, "secondaryTitleDescription");
            this.secondaryTitleDescription.Name = "secondaryTitleDescription";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.lengthLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.climbLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.courseTypeLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.nameTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.courseNameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.courseKindCombo, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.hideFromReportsCheckBox, 0, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // lengthLabel
            // 
            resources.ApplyResources(this.lengthLabel, "lengthLabel");
            this.lengthLabel.Name = "lengthLabel";
            // 
            // climbLabel
            // 
            resources.ApplyResources(this.climbLabel, "climbLabel");
            this.climbLabel.Name = "climbLabel";
            // 
            // courseTypeLabel
            // 
            resources.ApplyResources(this.courseTypeLabel, "courseTypeLabel");
            this.courseTypeLabel.Name = "courseTypeLabel";
            // 
            // nameTextBox
            // 
            resources.ApplyResources(this.nameTextBox, "nameTextBox");
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // courseNameLabel
            // 
            resources.ApplyResources(this.courseNameLabel, "courseNameLabel");
            this.courseNameLabel.Name = "courseNameLabel";
            // 
            // courseKindCombo
            // 
            resources.ApplyResources(this.courseKindCombo, "courseKindCombo");
            this.courseKindCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.courseKindCombo.FormattingEnabled = true;
            this.courseKindCombo.Items.AddRange(new object[] {
            resources.GetString("courseKindCombo.Items"),
            resources.GetString("courseKindCombo.Items1")});
            this.courseKindCombo.Name = "courseKindCombo";
            this.courseKindCombo.SelectionChangeCommitted += new System.EventHandler(this.courseKindCombo_SelectionChangeCommitted);
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.climbTextBox);
            this.flowLayoutPanel1.Controls.Add(this.metersSuffix);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // climbTextBox
            // 
            resources.ApplyResources(this.climbTextBox, "climbTextBox");
            this.climbTextBox.Name = "climbTextBox";
            // 
            // metersSuffix
            // 
            resources.ApplyResources(this.metersSuffix, "metersSuffix");
            this.metersSuffix.Name = "metersSuffix";
            // 
            // flowLayoutPanel3
            // 
            resources.ApplyResources(this.flowLayoutPanel3, "flowLayoutPanel3");
            this.flowLayoutPanel3.Controls.Add(this.lengthTextBox);
            this.flowLayoutPanel3.Controls.Add(this.kmSuffix);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            // 
            // lengthTextBox
            // 
            resources.ApplyResources(this.lengthTextBox, "lengthTextBox");
            this.lengthTextBox.Name = "lengthTextBox";
            // 
            // kmSuffix
            // 
            resources.ApplyResources(this.kmSuffix, "kmSuffix");
            this.kmSuffix.Name = "kmSuffix";
            // 
            // hideFromReportsCheckBox
            // 
            resources.ApplyResources(this.hideFromReportsCheckBox, "hideFromReportsCheckBox");
            this.tableLayoutPanel1.SetColumnSpan(this.hideFromReportsCheckBox, 2);
            this.hideFromReportsCheckBox.Name = "hideFromReportsCheckBox";
            this.hideFromReportsCheckBox.UseVisualStyleBackColor = true;
            // 
            // AddCourse
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.secondaryTitleGroup);
            this.Controls.Add(this.groupBox1);
            this.Name = "AddCourse";
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.secondaryTitleGroup, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.firstControlUpDown)).EndInit();
            this.secondaryTitleGroup.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox secondaryTitleGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label courseTypeLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label courseNameLabel;
        private System.Windows.Forms.Label climbLabel;
        private System.Windows.Forms.ComboBox courseKindCombo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TextBox climbTextBox;
        private System.Windows.Forms.Label metersSuffix;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label descriptionAppearanceLabel;
        private System.Windows.Forms.Label mapScaleLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label oneToPrefixLabel;
        private System.Windows.Forms.ComboBox scaleCombo;
        private System.Windows.Forms.ComboBox descKindCombo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox secondaryTitleTextBox;
        private System.Windows.Forms.Label secondaryTitleDescription;
        private System.Windows.Forms.NumericUpDown firstControlUpDown;
        private System.Windows.Forms.Label firstControlLabel;
        private System.Windows.Forms.Label labelKindLabel;
        private System.Windows.Forms.ComboBox labelKindCombo;
        private System.Windows.Forms.Label scoreColumnLabel;
        private System.Windows.Forms.ComboBox scoreColumnCombo;
        private System.Windows.Forms.Label lengthLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.TextBox lengthTextBox;
        private System.Windows.Forms.Label kmSuffix;
        private System.Windows.Forms.CheckBox hideFromReportsCheckBox;
    }
}
