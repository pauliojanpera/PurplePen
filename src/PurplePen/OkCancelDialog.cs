﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PurplePen
{
    public partial class OkCancelDialog: PurplePen.BaseDialog
    {
        public OkCancelDialog()
        {
            InitializeComponent();
        }

        // Override this -- the OK Button was pressed. Return true to close dialog,
        // false to prevent closing.
        protected virtual bool OkButtonClicked()
        {
            return true;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (OkButtonClicked())
                DialogResult = DialogResult.OK;
        }

        private void OkCancelDialog_Load(object sender, EventArgs e)
        {
            if (okButton.Bounds.Right > cancelButton.Bounds.Left - 6) {
                okButton.Left = okButton.Left - (okButton.Bounds.Right - cancelButton.Bounds.Left + 6);
            }
        }
    }
}
