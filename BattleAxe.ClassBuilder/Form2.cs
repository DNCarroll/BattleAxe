using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleAxe.Class {
    public partial class Form2 : Form {
        public Form2() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(ConnectionString.Text)) {
                try {
                    using (var conn = new System.Data.SqlClient.SqlConnection(ConnectionString.Text)) {
                        conn.Open();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show($"Failed to open a connection with provided Connection String{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                    return;
                }
            }
            else {
                MessageBox.Show("Connection String required.");
                return;
            }
            if (string.IsNullOrEmpty(CommandText.Text)) {
                MessageBox.Show("Command Text required.");
                return;
            }
            if (string.IsNullOrEmpty(NameSpace.Text)) {
                MessageBox.Show("namespace required.");
                return;
            }
            if (string.IsNullOrEmpty(ClassName.Text)) {
                MessageBox.Show("Class Name required.");
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
