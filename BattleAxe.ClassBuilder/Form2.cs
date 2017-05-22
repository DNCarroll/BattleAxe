using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BattleAxe.Class {
    public partial class Form2 : Form {
        public Form2() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            if (fieldsAreValid()) {
                try {
                    using (var conn = new System.Data.SqlClient.SqlConnection(GetConnectionString())) {
                        conn.Open();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    return;
                }
                this.DialogResult = DialogResult.OK;
            }
        }
        bool fieldsAreValid() {
            List<TextBox> textBoxes = new List<TextBox>(){ Server, Catalog, CommandText, NameSpace, ClassName };
            if (!UseIntegratedSecurity.Checked) {
                textBoxes.Add(UserName);
                textBoxes.Add(Password);
            }
            foreach (var item in textBoxes) {
                if (string.IsNullOrEmpty(item.Text)) {
                    MessageBox.Show(item.Name + " required.");
                    return false;
                }
            }
            return true;
        }

        private void UseIntegratedSecurity_CheckedChanged(object sender, EventArgs e) {
            this.UserName.ReadOnly = this.UseIntegratedSecurity.Checked;
            this.Password.ReadOnly = this.UseIntegratedSecurity.Checked;
        }

        public string GetConnectionString() {
            return this.UseIntegratedSecurity.Checked ?
                $"Data Source={this.Server.Text};Initial Catalog={this.Catalog.Text};Integrated Security=true" :
                $"Data Source={this.Server.Text};Initial Catalog={this.Catalog.Text};User Id={this.UserName.Text};Password={this.Password.Text}";
        }
    }
}
