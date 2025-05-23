﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PurplePen
{
    public partial class OverwritingOcadFilesDialog: BaseDialog
    {
        public OverwritingOcadFilesDialog()
        {
            InitializeComponent();
        }

        public List<string> Filenames
        {
            set
            {
                foreach (string s in value)
                    listBoxFiles.Items.Add(s);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }
    }
}
