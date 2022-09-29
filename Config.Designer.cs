
namespace SimSatisfactory
{
    partial class FrmConfig
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
            this.chkBloquer = new System.Windows.Forms.CheckBox();
            this.txtFacteur = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btValid = new System.Windows.Forms.Button();
            this.btAnnuler = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkBloquer
            // 
            this.chkBloquer.AutoSize = true;
            this.chkBloquer.Enabled = false;
            this.chkBloquer.Location = new System.Drawing.Point(215, 49);
            this.chkBloquer.Name = "chkBloquer";
            this.chkBloquer.Size = new System.Drawing.Size(79, 21);
            this.chkBloquer.TabIndex = 0;
            this.chkBloquer.Text = "Bloquer";
            this.chkBloquer.UseVisualStyleBackColor = true;
            this.chkBloquer.Visible = false;
            // 
            // txtFacteur
            // 
            this.txtFacteur.Location = new System.Drawing.Point(91, 21);
            this.txtFacteur.Name = "txtFacteur";
            this.txtFacteur.Size = new System.Drawing.Size(203, 22);
            this.txtFacteur.TabIndex = 1;
            this.txtFacteur.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFacteur_KeyDown);
            this.txtFacteur.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtFacteur_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Facteur";
            // 
            // btValid
            // 
            this.btValid.Location = new System.Drawing.Point(58, 93);
            this.btValid.Name = "btValid";
            this.btValid.Size = new System.Drawing.Size(95, 30);
            this.btValid.TabIndex = 3;
            this.btValid.Text = "Valider";
            this.btValid.UseVisualStyleBackColor = true;
            this.btValid.Click += new System.EventHandler(this.btValid_Click);
            // 
            // btAnnuler
            // 
            this.btAnnuler.Location = new System.Drawing.Point(168, 93);
            this.btAnnuler.Name = "btAnnuler";
            this.btAnnuler.Size = new System.Drawing.Size(95, 30);
            this.btAnnuler.TabIndex = 4;
            this.btAnnuler.Text = "Annuler";
            this.btAnnuler.UseVisualStyleBackColor = true;
            this.btAnnuler.Click += new System.EventHandler(this.btAnnuler_Click);
            // 
            // FrmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 135);
            this.Controls.Add(this.btAnnuler);
            this.Controls.Add(this.btValid);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFacteur);
            this.Controls.Add(this.chkBloquer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmConfig";
            this.Text = "Configurer";
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmConfig_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkBloquer;
        private System.Windows.Forms.TextBox txtFacteur;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btValid;
        private System.Windows.Forms.Button btAnnuler;
    }
}