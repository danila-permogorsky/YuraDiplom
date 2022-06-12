using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FlashProw
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            GetDevs();
        }

        private void GetDevs()
        {
            List<Dev_Machine> dml = RegRead.GetDevs();

            dataGridView1.Rows.Clear();
            for (int i = 0; i<dml.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = i;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = dml[i].GetViewId();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = dml[i].GetName();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = dml[i].GetDate();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = (dml[i].GetTypeDev()) ? "Да" : "Нет";
            }

            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows[0].Selected = true;
                dataGridView1.Sort(Date, ListSortDirection.Descending);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetDevs();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            FormAdd fa = new FormAdd();
            fa.ShowDialog();
            GetDevs();
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            FormBase fb = new FormBase();
            fb.ShowDialog();
            GetDevs();
        }
    }
}
