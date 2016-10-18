namespace Shipwreck.Subprocessing.Example
{
    partial class ServiceForm
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
            this.duplexButton = new System.Windows.Forms.Button();
            this.rightOperand = new System.Windows.Forms.NumericUpDown();
            this.leftOperand = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.rightOperand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftOperand)).BeginInit();
            this.SuspendLayout();
            // 
            // duplexButton
            // 
            this.duplexButton.Location = new System.Drawing.Point(158, 9);
            this.duplexButton.Name = "duplexButton";
            this.duplexButton.Size = new System.Drawing.Size(111, 23);
            this.duplexButton.TabIndex = 0;
            this.duplexButton.Text = "Start Duplex";
            this.duplexButton.UseVisualStyleBackColor = true;
            this.duplexButton.Click += new System.EventHandler(this.duplexButton_Click);
            // 
            // rightOperand
            // 
            this.rightOperand.Location = new System.Drawing.Point(85, 12);
            this.rightOperand.Name = "rightOperand";
            this.rightOperand.Size = new System.Drawing.Size(67, 19);
            this.rightOperand.TabIndex = 4;
            this.rightOperand.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.rightOperand.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // leftOperand
            // 
            this.leftOperand.Location = new System.Drawing.Point(12, 12);
            this.leftOperand.Name = "leftOperand";
            this.leftOperand.Size = new System.Drawing.Size(67, 19);
            this.leftOperand.TabIndex = 3;
            this.leftOperand.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.leftOperand.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // HostForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 481);
            this.Controls.Add(this.rightOperand);
            this.Controls.Add(this.leftOperand);
            this.Controls.Add(this.duplexButton);
            this.Name = "HostForm";
            this.Text = "HostForm";
            this.Load += new System.EventHandler(this.HostForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.rightOperand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftOperand)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button duplexButton;
        private System.Windows.Forms.NumericUpDown rightOperand;
        private System.Windows.Forms.NumericUpDown leftOperand;
    }
}