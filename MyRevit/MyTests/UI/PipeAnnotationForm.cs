using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyRevit.MyTests.UI
{
    public partial class PipeAnnotationForm : Form
    {
        public PipeAnnotationForm()
        {
            InitializeComponent();

            rb_OnPipe.Checked = true;
            rb_OnLineEdge.Checked = true;
        }
    }
}
