using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlashProw
{
    public partial class FormText : Form
    {
        private FormAdd main;
        public FormText(FormAdd fa)
        {
            main = fa;
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            main.added = false;
            main.desc = "";
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text != null) && (textBox1.Text.Trim().Length > 0))
            {
                main.added = true;
                main.desc = textBox1.Text.Trim();
                Close();
            }
            else
            {
                MessageBox.Show("Введите описание устройства!");
            }
        }
    }
}
