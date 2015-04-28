namespace VirtualCockpit
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.AAP_CDUPWRlabel = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.AAP_EGIPWRlabel = new System.Windows.Forms.Label();
            this.AAP_PAGElabel = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.AAP_STEERlabel = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.AAP_STEERPTlabel = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "CDU Power";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AAP_CDUPWRlabel
            // 
            this.AAP_CDUPWRlabel.AutoSize = true;
            this.AAP_CDUPWRlabel.Location = new System.Drawing.Point(94, 18);
            this.AAP_CDUPWRlabel.Name = "AAP_CDUPWRlabel";
            this.AAP_CDUPWRlabel.Size = new System.Drawing.Size(35, 13);
            this.AAP_CDUPWRlabel.TabIndex = 1;
            this.AAP_CDUPWRlabel.Text = "label1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(13, 42);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "EGI Power";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // AAP_EGIPWRlabel
            // 
            this.AAP_EGIPWRlabel.AutoSize = true;
            this.AAP_EGIPWRlabel.Location = new System.Drawing.Point(94, 47);
            this.AAP_EGIPWRlabel.Name = "AAP_EGIPWRlabel";
            this.AAP_EGIPWRlabel.Size = new System.Drawing.Size(35, 13);
            this.AAP_EGIPWRlabel.TabIndex = 3;
            this.AAP_EGIPWRlabel.Text = "label2";
            // 
            // AAP_PAGElabel
            // 
            this.AAP_PAGElabel.AutoSize = true;
            this.AAP_PAGElabel.Location = new System.Drawing.Point(94, 76);
            this.AAP_PAGElabel.Name = "AAP_PAGElabel";
            this.AAP_PAGElabel.Size = new System.Drawing.Size(35, 13);
            this.AAP_PAGElabel.TabIndex = 5;
            this.AAP_PAGElabel.Text = "label3";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(13, 71);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Page";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // AAP_STEERlabel
            // 
            this.AAP_STEERlabel.AutoSize = true;
            this.AAP_STEERlabel.Location = new System.Drawing.Point(94, 105);
            this.AAP_STEERlabel.Name = "AAP_STEERlabel";
            this.AAP_STEERlabel.Size = new System.Drawing.Size(35, 13);
            this.AAP_STEERlabel.TabIndex = 7;
            this.AAP_STEERlabel.Text = "label4";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(13, 100);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Steer";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // AAP_STEERPTlabel
            // 
            this.AAP_STEERPTlabel.AutoSize = true;
            this.AAP_STEERPTlabel.Location = new System.Drawing.Point(94, 134);
            this.AAP_STEERPTlabel.Name = "AAP_STEERPTlabel";
            this.AAP_STEERPTlabel.Size = new System.Drawing.Size(35, 13);
            this.AAP_STEERPTlabel.TabIndex = 9;
            this.AAP_STEERPTlabel.Text = "label5";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(13, 129);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Steerpt";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.AAP_STEERPTlabel);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.AAP_STEERlabel);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.AAP_PAGElabel);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.AAP_EGIPWRlabel);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.AAP_CDUPWRlabel);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label AAP_CDUPWRlabel;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label AAP_EGIPWRlabel;
        private System.Windows.Forms.Label AAP_PAGElabel;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label AAP_STEERlabel;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label AAP_STEERPTlabel;
        private System.Windows.Forms.Button button5;
    }
}

