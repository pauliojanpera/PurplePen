﻿namespace PurplePen.DebugUI
{
    partial class NewLanguage
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
            this.label1 = new System.Windows.Forms.Label();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.pluralNounCheckBox = new System.Windows.Forms.CheckBox();
            this.genderAdjectiveCheckBox = new System.Windows.Forms.CheckBox();
            this.pluralAdjectiveCheckBox = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxLanguageName = new System.Windows.Forms.TextBox();
            this.gendersTextBox = new System.Windows.Forms.TextBox();
            this.gendersLabel = new System.Windows.Forms.Label();
            this.caseModifiersCheckBox = new System.Windows.Forms.CheckBox();
            this.casesLabel = new System.Windows.Forms.Label();
            this.casesTextBox = new System.Windows.Forms.TextBox();
            this.copyFromComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "New language: ";
            // 
            // languageComboBox
            // 
            this.languageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(139, 13);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(185, 21);
            this.languageComboBox.Sorted = true;
            this.languageComboBox.TabIndex = 1;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // pluralNounCheckBox
            // 
            this.pluralNounCheckBox.AutoSize = true;
            this.pluralNounCheckBox.Location = new System.Drawing.Point(12, 83);
            this.pluralNounCheckBox.Name = "pluralNounCheckBox";
            this.pluralNounCheckBox.Size = new System.Drawing.Size(250, 17);
            this.pluralNounCheckBox.TabIndex = 2;
            this.pluralNounCheckBox.Text = "Use plural nouns for between/junction/crossing";
            this.pluralNounCheckBox.UseVisualStyleBackColor = true;
            this.pluralNounCheckBox.CheckedChanged += new System.EventHandler(this.pluralNounCheckBox_CheckedChanged);
            // 
            // genderAdjectiveCheckBox
            // 
            this.genderAdjectiveCheckBox.AutoSize = true;
            this.genderAdjectiveCheckBox.Location = new System.Drawing.Point(12, 130);
            this.genderAdjectiveCheckBox.Name = "genderAdjectiveCheckBox";
            this.genderAdjectiveCheckBox.Size = new System.Drawing.Size(307, 17);
            this.genderAdjectiveCheckBox.TabIndex = 3;
            this.genderAdjectiveCheckBox.Text = "Adjectives change based on gender of noun being modified";
            this.genderAdjectiveCheckBox.UseVisualStyleBackColor = true;
            this.genderAdjectiveCheckBox.CheckedChanged += new System.EventHandler(this.genderAdjectiveCheckBox_CheckedChanged);
            // 
            // pluralAdjectiveCheckBox
            // 
            this.pluralAdjectiveCheckBox.AutoSize = true;
            this.pluralAdjectiveCheckBox.Location = new System.Drawing.Point(32, 107);
            this.pluralAdjectiveCheckBox.Name = "pluralAdjectiveCheckBox";
            this.pluralAdjectiveCheckBox.Size = new System.Drawing.Size(199, 17);
            this.pluralAdjectiveCheckBox.TabIndex = 4;
            this.pluralAdjectiveCheckBox.Text = "Use plural adjectives for plural nouns";
            this.pluralAdjectiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(248, 292);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(167, 291);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Native language name:";
            // 
            // textBoxLanguageName
            // 
            this.textBoxLanguageName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLanguageName.Location = new System.Drawing.Point(139, 44);
            this.textBoxLanguageName.Name = "textBoxLanguageName";
            this.textBoxLanguageName.Size = new System.Drawing.Size(185, 20);
            this.textBoxLanguageName.TabIndex = 8;
            // 
            // gendersTextBox
            // 
            this.gendersTextBox.Location = new System.Drawing.Point(85, 153);
            this.gendersTextBox.Name = "gendersTextBox";
            this.gendersTextBox.Size = new System.Drawing.Size(234, 20);
            this.gendersTextBox.TabIndex = 9;
            // 
            // gendersLabel
            // 
            this.gendersLabel.AutoSize = true;
            this.gendersLabel.Location = new System.Drawing.Point(29, 156);
            this.gendersLabel.Name = "gendersLabel";
            this.gendersLabel.Size = new System.Drawing.Size(50, 13);
            this.gendersLabel.TabIndex = 10;
            this.gendersLabel.Text = "Genders:";
            // 
            // caseModifiersCheckBox
            // 
            this.caseModifiersCheckBox.AutoSize = true;
            this.caseModifiersCheckBox.Location = new System.Drawing.Point(12, 179);
            this.caseModifiersCheckBox.Name = "caseModifiersCheckBox";
            this.caseModifiersCheckBox.Size = new System.Drawing.Size(213, 17);
            this.caseModifiersCheckBox.TabIndex = 11;
            this.caseModifiersCheckBox.Text = "Nouns change case based on modifiers";
            this.caseModifiersCheckBox.UseVisualStyleBackColor = true;
            this.caseModifiersCheckBox.CheckedChanged += new System.EventHandler(this.caseModifiersCheckBox_CheckedChanged);
            // 
            // casesLabel
            // 
            this.casesLabel.AutoSize = true;
            this.casesLabel.Location = new System.Drawing.Point(29, 203);
            this.casesLabel.Name = "casesLabel";
            this.casesLabel.Size = new System.Drawing.Size(39, 13);
            this.casesLabel.TabIndex = 12;
            this.casesLabel.Text = "Cases:";
            // 
            // casesTextBox
            // 
            this.casesTextBox.Location = new System.Drawing.Point(85, 200);
            this.casesTextBox.Name = "casesTextBox";
            this.casesTextBox.Size = new System.Drawing.Size(234, 20);
            this.casesTextBox.TabIndex = 13;
            // 
            // copyFromComboBox
            // 
            this.copyFromComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.copyFromComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.copyFromComboBox.FormattingEnabled = true;
            this.copyFromComboBox.Location = new System.Drawing.Point(134, 248);
            this.copyFromComboBox.Name = "copyFromComboBox";
            this.copyFromComboBox.Size = new System.Drawing.Size(185, 21);
            this.copyFromComboBox.Sorted = true;
            this.copyFromComboBox.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 251);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Copy initial text from:";
            // 
            // NewLanguage
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(336, 327);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.copyFromComboBox);
            this.Controls.Add(this.casesTextBox);
            this.Controls.Add(this.casesLabel);
            this.Controls.Add(this.caseModifiersCheckBox);
            this.Controls.Add(this.gendersLabel);
            this.Controls.Add(this.gendersTextBox);
            this.Controls.Add(this.textBoxLanguageName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.pluralAdjectiveCheckBox);
            this.Controls.Add(this.genderAdjectiveCheckBox);
            this.Controls.Add(this.pluralNounCheckBox);
            this.Controls.Add(this.languageComboBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewLanguage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Description Language";
            this.Load += new System.EventHandler(this.NewLanguage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.CheckBox pluralNounCheckBox;
        private System.Windows.Forms.CheckBox genderAdjectiveCheckBox;
        private System.Windows.Forms.CheckBox pluralAdjectiveCheckBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxLanguageName;
        private System.Windows.Forms.TextBox gendersTextBox;
        private System.Windows.Forms.Label gendersLabel;
        private System.Windows.Forms.CheckBox caseModifiersCheckBox;
        private System.Windows.Forms.Label casesLabel;
        private System.Windows.Forms.TextBox casesTextBox;
        private System.Windows.Forms.ComboBox copyFromComboBox;
        private System.Windows.Forms.Label label3;
    }
}