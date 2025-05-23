﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TranslateTool
{
    public partial class SynchronizePOs : Form
    {
        public SynchronizePOs() {
            InitializeComponent();
        }

        public string ResXDirectory {
            get {
                return textBoxResXDirectory.Text;
            }
        }

        public string PODirectory {
            get {
                return textBoxPODirectory.Text;
            }
        }

        private void buttonSelectResXDirectory_Click(object sender, EventArgs e) {
            if (textBoxResXDirectory.Text.Length > 0)
                folderBrowserDialog.SelectedPath = textBoxResXDirectory.Text;
            else {
                Uri uri = new Uri(typeof(OpenDirectory).Assembly.CodeBase);
                folderBrowserDialog.SelectedPath = Path.GetFullPath(Path.GetDirectoryName(uri.LocalPath) + @"\..\..\..\..");
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxResXDirectory.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonSelectPODirectory_Click(object sender, EventArgs e) {
            if (textBoxPODirectory.Text.Length > 0)
                folderBrowserDialog.SelectedPath = textBoxPODirectory.Text;
            else {
                Uri uri = new Uri(typeof(OpenDirectory).Assembly.CodeBase);
                folderBrowserDialog.SelectedPath = Path.GetFullPath(Path.GetDirectoryName(uri.LocalPath) + @"\..\..\..\..");
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxPODirectory.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
