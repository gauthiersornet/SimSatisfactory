
namespace SimSatisfactory
{
    partial class FrmCréer
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
            this.btAnnuler = new System.Windows.Forms.Button();
            this.btValid = new System.Windows.Forms.Button();
            this.lstBxPropositions = new System.Windows.Forms.ListBox();
            this.txtRecherche = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btAnnuler
            // 
            this.btAnnuler.Location = new System.Drawing.Point(407, 292);
            this.btAnnuler.Name = "btAnnuler";
            this.btAnnuler.Size = new System.Drawing.Size(95, 30);
            this.btAnnuler.TabIndex = 6;
            this.btAnnuler.Text = "Annuler";
            this.btAnnuler.UseVisualStyleBackColor = true;
            this.btAnnuler.Click += new System.EventHandler(this.btAnnuler_Click);
            // 
            // btValid
            // 
            this.btValid.Location = new System.Drawing.Point(297, 292);
            this.btValid.Name = "btValid";
            this.btValid.Size = new System.Drawing.Size(95, 30);
            this.btValid.TabIndex = 5;
            this.btValid.Text = "Valider";
            this.btValid.UseVisualStyleBackColor = true;
            this.btValid.Click += new System.EventHandler(this.btValid_Click);
            // 
            // lstBxPropositions
            // 
            this.lstBxPropositions.FormattingEnabled = true;
            this.lstBxPropositions.ItemHeight = 16;
            this.lstBxPropositions.Location = new System.Drawing.Point(24, 40);
            this.lstBxPropositions.Name = "lstBxPropositions";
            this.lstBxPropositions.Size = new System.Drawing.Size(749, 244);
            this.lstBxPropositions.TabIndex = 7;
            this.lstBxPropositions.DoubleClick += new System.EventHandler(this.lstBxPropositions_DoubleClick);
            this.lstBxPropositions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstBxPropositions_KeyDown);
            // 
            // txtRecherche
            // 
            this.txtRecherche.Location = new System.Drawing.Point(24, 12);
            this.txtRecherche.Name = "txtRecherche";
            this.txtRecherche.Size = new System.Drawing.Size(749, 22);
            this.txtRecherche.TabIndex = 0;
            this.txtRecherche.TextChanged += new System.EventHandler(this.txtRecherche_TextChanged);
            this.txtRecherche.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtRecherche_KeyDown);
            // 
            // FrmCréer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 334);
            this.Controls.Add(this.txtRecherche);
            this.Controls.Add(this.lstBxPropositions);
            this.Controls.Add(this.btAnnuler);
            this.Controls.Add(this.btValid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmCréer";
            this.Text = "Créer";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmCréer_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btAnnuler;
        private System.Windows.Forms.Button btValid;
        private System.Windows.Forms.ListBox lstBxPropositions;
        private System.Windows.Forms.TextBox txtRecherche;
    }
}