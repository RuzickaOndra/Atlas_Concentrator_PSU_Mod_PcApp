
namespace HV_Power_Supply_GUI_ver._1
{
    partial class Form_Setting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Save = new System.Windows.Forms.Button();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.textBox_nm = new System.Windows.Forms.TextBox();
            this.textBox_gw = new System.Windows.Forms.TextBox();
            this.button_exit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button_save_reset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(12, 157);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(75, 35);
            this.btn_Save.TabIndex = 0;
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // textBox_ip
            // 
            this.textBox_ip.Location = new System.Drawing.Point(101, 28);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.Size = new System.Drawing.Size(121, 20);
            this.textBox_ip.TabIndex = 1;
            // 
            // textBox_nm
            // 
            this.textBox_nm.Location = new System.Drawing.Point(101, 54);
            this.textBox_nm.Name = "textBox_nm";
            this.textBox_nm.Size = new System.Drawing.Size(121, 20);
            this.textBox_nm.TabIndex = 2;
            // 
            // textBox_gw
            // 
            this.textBox_gw.Location = new System.Drawing.Point(101, 80);
            this.textBox_gw.Name = "textBox_gw";
            this.textBox_gw.Size = new System.Drawing.Size(121, 20);
            this.textBox_gw.TabIndex = 3;
            // 
            // button_exit
            // 
            this.button_exit.Location = new System.Drawing.Point(205, 157);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(75, 35);
            this.button_exit.TabIndex = 4;
            this.button_exit.Text = "Cancel";
            this.button_exit.UseVisualStyleBackColor = true;
            this.button_exit.Click += new System.EventHandler(this.button_exit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "IP Adress:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Net Mask:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "GateWay:";
            // 
            // button_save_reset
            // 
            this.button_save_reset.Location = new System.Drawing.Point(93, 157);
            this.button_save_reset.Name = "button_save_reset";
            this.button_save_reset.Size = new System.Drawing.Size(75, 35);
            this.button_save_reset.TabIndex = 8;
            this.button_save_reset.Text = "Save and Reset";
            this.button_save_reset.UseVisualStyleBackColor = true;
            this.button_save_reset.Click += new System.EventHandler(this.button_save_reset_Click);
            // 
            // Form_Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 204);
            this.Controls.Add(this.button_save_reset);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.textBox_gw);
            this.Controls.Add(this.textBox_nm);
            this.Controls.Add(this.textBox_ip);
            this.Controls.Add(this.btn_Save);
            this.MaximumSize = new System.Drawing.Size(308, 243);
            this.MinimumSize = new System.Drawing.Size(308, 243);
            this.Name = "Form_Setting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_Setting";
            this.Load += new System.EventHandler(this.Form_Setting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.TextBox textBox_ip;
        private System.Windows.Forms.TextBox textBox_nm;
        private System.Windows.Forms.TextBox textBox_gw;
        private System.Windows.Forms.Button button_exit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_save_reset;
    }
}