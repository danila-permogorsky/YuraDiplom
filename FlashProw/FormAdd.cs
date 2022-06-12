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
    public partial class FormAdd : Form
    {
        public bool added;
        public string desc;
        private List<Dev_Machine> dml;

        public FormAdd()
        {
            InitializeComponent();
            dml = RegRead.GetDevs();

            dataGridView1.Rows.Clear();
            for (int i = 0; i < dml.Count; i++)
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
                BtnAdd.Enabled = true;
                BtnAdd.Visible = true;
            }
            else
            {
                BtnAdd.Enabled = false;
                BtnAdd.Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            added = false;
            desc = "";
            FormText ft = new FormText(this);
            ft.ShowDialog();
            if (added)
            {
                BaseXML bm = new BaseXML();
                int i = Convert.ToInt32(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value);
                bm.AddDev(dml[i].GetId(), dml[i].GetName(), desc);
                Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
