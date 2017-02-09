namespace SixDofViewer
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.X = new System.Windows.Forms.Label();
            this.Y = new System.Windows.Forms.Label();
            this.First = new System.Windows.Forms.Label();
            this.Second = new System.Windows.Forms.Label();
            this.Z = new System.Windows.Forms.Label();
            this.Third = new System.Windows.Forms.Label();
            this.Body = new System.Windows.Forms.Label();
            this.Color = new System.Windows.Forms.Label();
            this.Residual = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // X
            // 
            this.X.AutoSize = true;
            this.X.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.X.ForeColor = System.Drawing.Color.White;
            this.X.Location = new System.Drawing.Point(12, 175);
            this.X.Name = "X";
            this.X.Size = new System.Drawing.Size(109, 108);
            this.X.TabIndex = 0;
            this.X.Text = "X";
            this.X.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Y
            // 
            this.Y.AutoSize = true;
            this.Y.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Y.ForeColor = System.Drawing.Color.White;
            this.Y.Location = new System.Drawing.Point(17, 310);
            this.Y.Name = "Y";
            this.Y.Size = new System.Drawing.Size(109, 108);
            this.Y.TabIndex = 1;
            this.Y.Text = "Y";
            this.Y.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Roll
            // 
            this.First.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.First.AutoSize = true;
            this.First.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.First.ForeColor = System.Drawing.Color.White;
            this.First.Location = new System.Drawing.Point(697, 175);
            this.First.Name = "First";
            this.First.Size = new System.Drawing.Size(209, 108);
            this.First.TabIndex = 2;
            this.First.Text = "First";
            // 
            // Pitch
            // 
            this.Second.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Second.AutoSize = true;
            this.Second.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Second.ForeColor = System.Drawing.Color.White;
            this.Second.Location = new System.Drawing.Point(697, 310);
            this.Second.Name = "Second";
            this.Second.Size = new System.Drawing.Size(258, 108);
            this.Second.TabIndex = 3;
            this.Second.Text = "Second";
            // 
            // Z
            // 
            this.Z.AutoSize = true;
            this.Z.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Z.ForeColor = System.Drawing.Color.White;
            this.Z.Location = new System.Drawing.Point(17, 445);
            this.Z.Name = "Z";
            this.Z.Size = new System.Drawing.Size(104, 108);
            this.Z.TabIndex = 4;
            this.Z.Text = "Z";
            this.Z.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Yaw
            // 
            this.Third.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Third.AutoSize = true;
            this.Third.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Third.ForeColor = System.Drawing.Color.White;
            this.Third.Location = new System.Drawing.Point(697, 445);
            this.Third.Name = "Third";
            this.Third.Size = new System.Drawing.Size(231, 108);
            this.Third.TabIndex = 5;
            this.Third.Text = "Third";
            // 
            // Body
            // 
            this.Body.AutoSize = true;
            this.Body.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Body.ForeColor = System.Drawing.Color.White;
            this.Body.Location = new System.Drawing.Point(12, 40);
            this.Body.Name = "Body";
            this.Body.Size = new System.Drawing.Size(263, 108);
            this.Body.TabIndex = 6;
            this.Body.Text = "Body";
            this.Body.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Color
            // 
            this.Color.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Color.AutoSize = true;
            this.Color.BackColor = System.Drawing.Color.Red;
            this.Color.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Color.ForeColor = System.Drawing.Color.Red;
            this.Color.Location = new System.Drawing.Point(1305, 40);
            this.Color.Name = "Color";
            this.Color.Size = new System.Drawing.Size(102, 55);
            this.Color.TabIndex = 7;
            this.Color.Text = "      ";
            // 
            // Residual
            // 
            this.Residual.AutoSize = true;
            this.Residual.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Residual.ForeColor = System.Drawing.Color.White;
            this.Residual.Location = new System.Drawing.Point(12, 580);
            this.Residual.Name = "Residual";
            this.Residual.Size = new System.Drawing.Size(416, 108);
            this.Residual.TabIndex = 8;
            this.Residual.Text = "Residual";
            this.Residual.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1445, 715);
            this.Controls.Add(this.Residual);
            this.Controls.Add(this.Color);
            this.Controls.Add(this.Body);
            this.Controls.Add(this.Third);
            this.Controls.Add(this.Z);
            this.Controls.Add(this.Second);
            this.Controls.Add(this.First);
            this.Controls.Add(this.Y);
            this.Controls.Add(this.X);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Qualisys SixDof Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label X;
        private System.Windows.Forms.Label Y;
        private System.Windows.Forms.Label First;
        private System.Windows.Forms.Label Second;
        private System.Windows.Forms.Label Z;
        private System.Windows.Forms.Label Third;
        private System.Windows.Forms.Label Body;
        private System.Windows.Forms.Label Color;
        private System.Windows.Forms.Label Residual;
    }
}

