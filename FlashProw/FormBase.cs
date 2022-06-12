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
    public partial class FormBase : Form
    {

        private List<Dev_Acs> lda;

        public FormBase()
        {
            InitializeComponent();
            GetDevs();
        }

        private void GetDevs()
        {
            BaseXML xml = new BaseXML();
            lda = xml.GetDev();

            dataGridView1.Rows.Clear();
            for (int i = 0; i < lda.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = i;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = lda[i].GetId();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = lda[i].GetFName();
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = lda[i].GetName();
            }

            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows[0].Selected = true;
                BtnDel.Visible = true;
                BtnDel.Enabled = true;
            }
            else
            {
                BtnDel.Visible = false;
                BtnDel.Enabled = false;
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            int i = Convert.ToInt32(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value);
            lda[i].SetDel(true);
            BaseXML xml = new BaseXML();
            xml.DelDev(lda);
            GetDevs();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            FormAdd fa = new FormAdd();
            fa.ShowDialog();
            GetDevs();
        }
    }
}
