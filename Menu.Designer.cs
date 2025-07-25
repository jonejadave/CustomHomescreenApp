namespace CustomHomescreen
{
    partial class Menu
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Menu));
            btnSelect = new Button();
            picPreview = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            // 
            // btnSelect
            // 
            btnSelect.Location = new Point(12, 12);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(154, 23);
            btnSelect.TabIndex = 0;
            btnSelect.Text = "select MP4 file";
            btnSelect.UseVisualStyleBackColor = true;
            btnSelect.Click += btnSelect_Click;
            // 
            // picPreview
            // 
            picPreview.BackColor = SystemColors.Window;
            picPreview.Location = new Point(12, 50);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(320, 180);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 1;
            picPreview.TabStop = false;
            // 
            // Menu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(348, 280);
            Controls.Add(picPreview);
            Controls.Add(btnSelect);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Menu";
            Text = "VALORANT Homescreen+";
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnSelect;
        private PictureBox picPreview;
    }
}
