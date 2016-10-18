namespace Shipwreck.Subprocessing.Example
{
    partial class DuplexClientForm
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
            this.leftOperand = new System.Windows.Forms.NumericUpDown();
            this.rightOperand = new System.Windows.Forms.NumericUpDown();
            this.addButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.subtractButton = new System.Windows.Forms.Button();
            this.multiplyButton = new System.Windows.Forms.Button();
            this.divideButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.leftOperand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightOperand)).BeginInit();
            this.SuspendLayout();
            // 
            // leftOperand
            // 
            this.leftOperand.Location = new System.Drawing.Point(12, 12);
            this.leftOperand.Name = "leftOperand";
            this.leftOperand.Size = new System.Drawing.Size(67, 19);
            this.leftOperand.TabIndex = 0;
            this.leftOperand.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.leftOperand.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // rightOperand
            // 
            this.rightOperand.Location = new System.Drawing.Point(151, 12);
            this.rightOperand.Name = "rightOperand";
            this.rightOperand.Size = new System.Drawing.Size(67, 19);
            this.rightOperand.TabIndex = 2;
            this.rightOperand.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.rightOperand.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(85, 13);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(27, 19);
            this.addButton.TabIndex = 3;
            this.addButton.Text = "+";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(224, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 45);
            this.label1.TabIndex = 4;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // subtractButton
            // 
            this.subtractButton.Location = new System.Drawing.Point(118, 13);
            this.subtractButton.Name = "subtractButton";
            this.subtractButton.Size = new System.Drawing.Size(27, 19);
            this.subtractButton.TabIndex = 8;
            this.subtractButton.Text = "-";
            this.subtractButton.UseVisualStyleBackColor = true;
            this.subtractButton.Click += new System.EventHandler(this.subtractButton_Click);
            // 
            // multiplyButton
            // 
            this.multiplyButton.Location = new System.Drawing.Point(85, 38);
            this.multiplyButton.Name = "multiplyButton";
            this.multiplyButton.Size = new System.Drawing.Size(27, 19);
            this.multiplyButton.TabIndex = 13;
            this.multiplyButton.Text = "*";
            this.multiplyButton.UseVisualStyleBackColor = true;
            this.multiplyButton.Click += new System.EventHandler(this.multiplyButton_Click);
            // 
            // divideButton
            // 
            this.divideButton.Location = new System.Drawing.Point(118, 38);
            this.divideButton.Name = "divideButton";
            this.divideButton.Size = new System.Drawing.Size(27, 19);
            this.divideButton.TabIndex = 18;
            this.divideButton.Text = "/";
            this.divideButton.UseVisualStyleBackColor = true;
            this.divideButton.Click += new System.EventHandler(this.divideButton_Click);
            // 
            // DuplexClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 73);
            this.Controls.Add(this.divideButton);
            this.Controls.Add(this.multiplyButton);
            this.Controls.Add(this.subtractButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.rightOperand);
            this.Controls.Add(this.leftOperand);
            this.Name = "DuplexClientForm";
            this.Text = "DuplexClientForm";
            ((System.ComponentModel.ISupportInitialize)(this.leftOperand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightOperand)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown leftOperand;
        private System.Windows.Forms.NumericUpDown rightOperand;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button subtractButton;
        private System.Windows.Forms.Button multiplyButton;
        private System.Windows.Forms.Button divideButton;
    }
}