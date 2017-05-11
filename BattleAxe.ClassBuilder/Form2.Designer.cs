namespace BattleAxe.Class {
    partial class Form2 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label4 = new System.Windows.Forms.Label();
            this.CommandText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ConnectionString = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ClassName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.NameSpace = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 213);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 25);
            this.label4.TabIndex = 19;
            this.label4.Text = "Command Text";
            // 
            // CommandText
            // 
            this.CommandText.Location = new System.Drawing.Point(306, 208);
            this.CommandText.Multiline = true;
            this.CommandText.Name = "CommandText";
            this.CommandText.Size = new System.Drawing.Size(400, 177);
            this.CommandText.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 25);
            this.label3.TabIndex = 17;
            this.label3.Text = "Connection String";
            // 
            // ConnectionString
            // 
            this.ConnectionString.Location = new System.Drawing.Point(306, 150);
            this.ConnectionString.Name = "ConnectionString";
            this.ConnectionString.Size = new System.Drawing.Size(400, 31);
            this.ConnectionString.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 25);
            this.label2.TabIndex = 15;
            this.label2.Text = "Class Name";
            // 
            // ClassName
            // 
            this.ClassName.Location = new System.Drawing.Point(306, 94);
            this.ClassName.Name = "ClassName";
            this.ClassName.Size = new System.Drawing.Size(400, 31);
            this.ClassName.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 25);
            this.label1.TabIndex = 13;
            this.label1.Text = "namespace";
            // 
            // NameSpace
            // 
            this.NameSpace.Location = new System.Drawing.Point(306, 41);
            this.NameSpace.Name = "NameSpace";
            this.NameSpace.Size = new System.Drawing.Size(400, 31);
            this.NameSpace.TabIndex = 12;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button2.Location = new System.Drawing.Point(49, 421);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(169, 63);
            this.button2.TabIndex = 11;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(537, 421);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(169, 63);
            this.button1.TabIndex = 10;
            this.button1.Text = "Create";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 519);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.CommandText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ConnectionString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ClassName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NameSpace);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox CommandText;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox ConnectionString;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox ClassName;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox NameSpace;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}