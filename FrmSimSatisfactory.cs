using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SimSatisfactory
{
    public partial class FrmSimSatisfactory : Form
    {
        static private readonly float CLICK_SEUIL = 5.0f;

        private Usine usine;
        private PointF vuePos;
        private float vueScl;
        private SizeF DimentionD2;
        private PointF SelectedP_LEFT;
        private PointF SelectedP_LEFT_Move;

        private List<Producteur> SelectedProd_LEFT;
        private Producteur.SelectionMode SelectedMod_LEFT;
        private Pièce.EPièce SelectedPcs_LEFT;

        private PointF ClickP_RIGHT;
        private PointF SelectedP_RIGHT;
        private bool CTRL;

        public FrmSimSatisfactory()
        {
            Recette.InitialiserRecettes();
            usine = null;
            vuePos = new PointF(0.0f, 100.0f);
            vueScl = 1.0f;
            CTRL = false;
            SelectedProd_LEFT = null;
            SelectedMod_LEFT = Producteur.SelectionMode.None;
            SelectedPcs_LEFT = Pièce.EPièce.vide;
            InitializeComponent();
            DimentionD2 = new SizeF(this.Width / 2.0f, this.Height / 2.0f);
            MouseWheel += new MouseEventHandler(this.FrmSimSatisfactory_MouseWheel);
            //Usine usine = Usine.Construire_Usine(null, Pièce.EPièce.tôle_aluminium);
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.minerai_fer, Pièce.EPièce.minerai_cuivre, Pièce.EPièce.minerai_calcaire));
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.lingo_fer, Pièce.EPièce.lingo_cuivre));
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.tôle_cuivre));
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.lingo_aluminium));
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.tôle_aluminium));
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.barre_combustible_uranium));
            //usine = new Usine(Usine.Construire_Usine(null, Pièce.EPièce.minerai_fer));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if(usine != null)
            {
                Graphics g = e.Graphics;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                //RectangleF vue = new RectangleF(GV.GC.P.X, GV.GC.P.Y, this.Width / GV.GC.E, this.Height / GV.GC.E);
                g.TranslateTransform(DimentionD2.Width, DimentionD2.Height);
                //g.RotateTransform(GV.GC.A);
                g.ScaleTransform(vueScl, vueScl);
                g.TranslateTransform(-vuePos.X, -vuePos.Y);

                usine.Dessiner(g);
                //g.DrawEllipse(new Pen(Color.Red), -2.0f, -2.0f, 5.0f, 5.0f);
                //if (SelectedMod_LEFT != Producteur.SelectionMode.None)
                {
                    if (SelectedMod_LEFT == Producteur.SelectionMode.LienEntrant || SelectedMod_LEFT == Producteur.SelectionMode.LienSortant)
                    {
                        if(SelectedProd_LEFT != null) SelectedProd_LEFT.First().DessinerLiens(g, SelectedP_LEFT, SelectedP_LEFT_Move);
                        Ingrédient.Dessinner(g, SelectedP_LEFT_Move, SelectedPcs_LEFT);
                    }
                    else
                    {
                        if(SelectedProd_LEFT != null) SelectedProd_LEFT.ForEach(prd => prd.DessinerAllow(g));
                        if(SelectedMod_LEFT == Producteur.SelectionMode.SelectMultiple)
                        {
                            Pen pn = new Pen(Color.Black);
                            RectangleF rect = new RectangleF();
                            rect.X = Math.Min(SelectedP_LEFT.X, SelectedP_LEFT_Move.X);
                            rect.Y = Math.Min(SelectedP_LEFT.Y, SelectedP_LEFT_Move.Y);
                            rect.Width = Math.Max(SelectedP_LEFT.X, SelectedP_LEFT_Move.X) - rect.X;
                            rect.Height = Math.Max(SelectedP_LEFT.Y, SelectedP_LEFT_Move.Y) - rect.Y;
                            g.DrawRectangle(pn, rect.X, rect.Y, rect.Width, rect.Height);
                        }
                    }
                }
            }
        }

        private void FrmSimSatisfactory_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                SelectedP_LEFT = ProjectionInvWin(e.Location);
                if(usine != null)
                {
                    var SelectedProd = usine.Selection(SelectedP_LEFT);
                    if (SelectedProd.Item1 != null)
                    {
                        if (SelectedProd_LEFT != null && SelectedProd.Item2 == Producteur.SelectionMode.Déplacement)
                        {
                            if (SelectedProd_LEFT.Contains(SelectedProd.Item1))
                            {
                                if (CTRL)
                                {
                                    SelectedProd_LEFT.Remove(SelectedProd.Item1);
                                    if (!SelectedProd_LEFT.Any()) SelectedProd_LEFT = null;
                                }
                            }
                            else if (CTRL) SelectedProd_LEFT.Add(SelectedProd.Item1);
                            else SelectedProd_LEFT = new List<Producteur>() { SelectedProd.Item1 };
                        }
                        else
                        {
                            SelectedProd_LEFT = new List<Producteur>() { SelectedProd.Item1 };
                        }
                        SelectedMod_LEFT = SelectedProd.Item2;
                        SelectedPcs_LEFT = SelectedProd.Item3;
                    }
                    else
                    {
                        SelectedMod_LEFT = Producteur.SelectionMode.SelectMultiple;
                    }
                }
            }
            if (e.Button.HasFlag(MouseButtons.Right))
            {
                ClickP_RIGHT = e.Location;
                SelectedP_RIGHT = ProjectionInvWin(e.Location);
            }
        }

        private void FrmSimSatisfactory_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Button.HasFlag(MouseButtons.Left))
            {
                if(SelectedMod_LEFT != Producteur.SelectionMode.None)
                {
                    PointF pt = ProjectionInvWin(e.Location);
                    switch(SelectedMod_LEFT)
                    {
                        case Producteur.SelectionMode.Déplacement:
                            SelectedProd_LEFT.ForEach(x => {
                                x.P.X += pt.X - SelectedP_LEFT.X;
                                x.P.Y += pt.Y - SelectedP_LEFT.Y;
                            });
                            SelectedP_LEFT = pt;
                            break;
                        case Producteur.SelectionMode.LienEntrant:
                            SelectedP_LEFT_Move = pt;
                            break;
                        case Producteur.SelectionMode.LienSortant:
                            SelectedP_LEFT_Move = pt;
                            break;
                        case Producteur.SelectionMode.SelectMultiple:
                            SelectedP_LEFT_Move = pt;
                            break;
                    }
                    this.Refresh();
                }
            }
            if (e.Button.HasFlag(MouseButtons.Right))
            {
                PointF pt = ProjectionInvWin(e.Location);
                vuePos.X -= (pt.X - SelectedP_RIGHT.X);
                vuePos.Y -= (pt.Y - SelectedP_RIGHT.Y);
                this.Refresh();
            }
        }

        private void FrmSimSatisfactory_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (usine != null && SelectedMod_LEFT != Producteur.SelectionMode.None)
                {
                    SelectedP_LEFT_Move = ProjectionInvWin(e.Location);
                    switch (SelectedMod_LEFT)
                    {
                        case Producteur.SelectionMode.Déplacement:
                            break;
                        case Producteur.SelectionMode.LienEntrant:
                            {
                                Producteur dprod = usine.SelectionSeule(SelectedP_LEFT_Move);
                                if (dprod != null)
                                {
                                    Producteur.RelierProducteur(SelectedProd_LEFT.First(), dprod, SelectedPcs_LEFT);
                                    SelectedProd_LEFT.First().Recalculer();
                                }
                            }
                            break;
                        case Producteur.SelectionMode.LienSortant:
                            {
                                Producteur dprod = usine.SelectionSeule(SelectedP_LEFT_Move);
                                if (dprod != null)
                                {
                                    Producteur.RelierProducteur(dprod, SelectedProd_LEFT.First(), SelectedPcs_LEFT);
                                    dprod.Recalculer();
                                }
                            }
                            break;
                        case Producteur.SelectionMode.SelectMultiple:
                            {
                                RectangleF rect = new RectangleF();
                                rect.X = Math.Min(SelectedP_LEFT.X, SelectedP_LEFT_Move.X);
                                rect.Y = Math.Min(SelectedP_LEFT.Y, SelectedP_LEFT_Move.Y);
                                rect.Width = Math.Max(SelectedP_LEFT.X, SelectedP_LEFT_Move.X) - rect.X;
                                rect.Height = Math.Max(SelectedP_LEFT.Y, SelectedP_LEFT_Move.Y) - rect.Y;
                                IEnumerable<Producteur> sel = usine.SelectionMulty(rect);
                                if (sel != null && sel.Any())
                                {
                                    if (SelectedProd_LEFT == null || !CTRL) SelectedProd_LEFT = sel.ToList();
                                    else SelectedProd_LEFT.AddRange(sel);
                                }
                                else if (!CTRL)
                                {
                                    SelectedProd_LEFT = null;
                                    SelectedMod_LEFT = Producteur.SelectionMode.None;
                                    SelectedPcs_LEFT = Pièce.EPièce.vide;
                                }
                            }
                            break;
                    }
                }
                if (SelectedMod_LEFT == Producteur.SelectionMode.SelectMultiple)
                {
                    SelectedMod_LEFT = Producteur.SelectionMode.None;
                    //SelectedMod_LEFT = Producteur.SelectionMode.Déplacement;
                }
                else if (SelectedMod_LEFT != Producteur.SelectionMode.Déplacement || !CTRL)
                {
                    //SelectedProd_LEFT = null;
                    SelectedMod_LEFT = Producteur.SelectionMode.None;
                    SelectedPcs_LEFT = Pièce.EPièce.vide;
                }
            }
            this.Refresh();
        }

        public PointF ProjectionInvWin(PointF p)
        {
            return ProjectionInvVue(new PointF(p.X - DimentionD2.Width, p.Y - DimentionD2.Height));
            //PointF dimProj = GC.Projection(Dimention);
            //return new PointF(P.X + (p.X / E), P.Y + (p.Y / E));
        }

        public PointF ProjectionInvVue(PointF p)
        {
            //Matrix m = new Matrix();
            //m.Rotate(A);
            return new PointF
                (
                    vuePos.X + (p.X * 1.0f/*m.Elements[0]*/ + p.Y * 0.0f/*m.Elements[1]*/) / vueScl,
                    vuePos.Y + (p.X * 0.0f/*m.Elements[2]*/ + p.Y * 1.0f/*m.Elements[3]*/) / vueScl
                );
            //return new PointF(P.X + (p.X / E), P.Y + (p.Y / E));
        }

        private void FrmSimSatisfactory_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                int delta = Math.Sign(e.Delta);
                PointF pt = ProjectionInvWin(e.Location);

                if (e.Delta < 0)
                {
                    if (vueScl > 0.13f) vueScl /= 1.2f;
                }
                else if (e.Delta > 0)
                {
                    if (vueScl < 6.19f) vueScl *= 1.2f;
                }

                PointF npt = ProjectionInvWin(e.Location);
                PointF dp = new PointF((npt.X - pt.X), (npt.Y - pt.Y));

                //PointF npt = new PointF((e.Location.X / GC.E), (e.Location.Y / GC.E));
                vuePos.X -= dp.X;
                vuePos.Y -= dp.Y;

                this.Refresh();
            }
        }

        private void FrmSimSatisfactory_SizeChanged(object sender, EventArgs e)
        {
            DimentionD2 = new SizeF(this.Width / 2.0f, this.Height / 2.0f);
        }

        private void MettreEnMain(List<Producteur> prods, PointF p)
        {
            prods.ForEach(prd => { prd.P.X += p.X; prd.P.Y += p.Y; });
            SelectedProd_LEFT = prods;
            SelectedMod_LEFT = Producteur.SelectionMode.Déplacement;
            SelectedPcs_LEFT = Pièce.EPièce.vide;
            SelectedP_LEFT = p;
        }

        private void AjouterUsine(List<Producteur> lstProd)
        {
            if (lstProd != null && lstProd.Any())
            {
                if (usine == null) usine = new Usine(lstProd);
                else
                {
                    Usine.Calculer(lstProd);
                    usine.Ajouter(lstProd);
                }
            }
        }

        private void CréationUsine(FrmCréer frmCréer, PointF pt)
        {
            if (frmCréer.ShowDialog(this) == DialogResult.Yes)
            {
                Pièce.EPièce ep = (Pièce.EPièce)Enum.Parse(typeof(Pièce.EPièce), frmCréer.valeur);
                List<Producteur> lstProd = Usine.Construire_Usine(SelectedProd_LEFT, ep);
                if (lstProd != null)
                {
                    AjouterUsine(lstProd);
                    MettreEnMain(lstProd, pt);
                    this.Refresh();
                }
                else MessageBox.Show("Impossible de créer une usine", "Impossible");
            }
        }

        private void CréerConteneur(FrmCréer frmCréer, PointF pt)
        {
            if (frmCréer.ShowDialog(this) == DialogResult.Yes)
            {
                Pièce.EPièce ep = (Pièce.EPièce)Enum.Parse(typeof(Pièce.EPièce), frmCréer.valeur);
                Producteur prod = new Producteur(new Recette(Recette.EProducteur.Conteneur, 0.0, new Ingrédient[]{ }, new Ingrédient[]{ new Ingrédient(ep, 1.0) }));
                List<Producteur> lstProd = new List<Producteur>() { prod };
                AjouterUsine(lstProd);
                MettreEnMain(lstProd, pt);
                this.Refresh();
            }
        }

        private void CréerProducteur(FrmCréer frmCréer, PointF pt)
        {
            if (frmCréer.ShowDialog(this) == DialogResult.Yes)
            {
                Recette rct = Recette.Parse(frmCréer.valeur);
                Producteur prod = new Producteur(rct);
                List<Producteur> lstProd = new List<Producteur>() { prod };
                AjouterUsine(lstProd);
                MettreEnMain(lstProd, pt);
                this.Refresh();
            }
        }

        private void NétoyerMain()
        {
            SelectedProd_LEFT = null;
            SelectedMod_LEFT = Producteur.SelectionMode.None;
            SelectedPcs_LEFT = Pièce.EPièce.vide;
            this.Refresh();
        }
		
		private static readonly (string Nom, Recette[] Recettes)[] _producteurs_recettes =
		{
			("Extracteurs", Recette.Extracteurs),
			("Centrales", Recette.Centrales),
			("Fonderies", Recette.Fonderies),
			("Fonderies_avancées", Recette.Fonderies_avancées),
			("Constructeurs", Recette.Constructeurs),
			("Assembleuses", Recette.Assembleuses),
			("Façonneuses", Recette.Façonneuses),
			("Rafineries", Recette.Rafineries),
			("Packageurs", Recette.Packageurs),
			("Mélangeurs", Recette.Mélangeurs),
			("Accélérateur", Recette.Accélérateur_de_particules)
		};

        private void FrmSimSatisfactory_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Right))
            {
                PointF dX = new PointF(e.X - ClickP_RIGHT.X, e.Y - ClickP_RIGHT.Y);
                float dCarrée = dX.X * dX.X + dX.Y * dX.Y;
                if (dCarrée < CLICK_SEUIL)
                {
                    ContextMenuStrip ctxm = new ContextMenuStrip();

                    PointF pt = ProjectionInvWin(e.Location);
                    if (usine != null)
                    {
						// Ajoutez un élément de menu "Consommation"
						ctxm.Items.Add($"ConsommationRéel : {-Math.Round(usine.ConsommationRéel, 3)}Mw");
						ctxm.Items.Add($"ConsommationMax : {-Math.Round(usine.ConsommationMax, 3)}Mw");
						
						

						// Créer un sous-menu pour les options de simulation
						var subMenuSimulation = new ToolStripMenuItem("Simulation");

						// Ajouter les éléments de menu pour les options de simulation
						var menuItemCalculer = new ToolStripMenuItem("Calculer");
						menuItemCalculer.Click += (sender, e) =>
						{
							if (usine != null)
							{
								usine.Calculer();
								this.Refresh();
							}
						};
						subMenuSimulation.DropDownItems.Add(menuItemCalculer);

						var menuItemOptimiser = new ToolStripMenuItem("Optimiser");
						menuItemOptimiser.Click += (sender, e) =>
						{
							if (usine != null)
							{
								usine.Optimiser();
								this.Refresh();
							}
						};
						subMenuSimulation.DropDownItems.Add(menuItemOptimiser);

						var menuItemOptimiserFlux = new ToolStripMenuItem("Optimiser les flux 1 step");
						menuItemOptimiserFlux.Click += (sender, e) =>
						{
							if (usine != null)
							{
								usine.OptimiserFlux(1, 1.0);
								this.Refresh();
							}
						};
						subMenuSimulation.DropDownItems.Add(menuItemOptimiserFlux);

						var menuItemOptimiserFlux10 = new ToolStripMenuItem("Optimiser les flux 10 step");
						menuItemOptimiserFlux10.Click += (sender, e) =>
						{
							if (usine != null)
							{
								usine.OptimiserFlux(10, 0.8);
								this.Refresh();
							}
						};
						subMenuSimulation.DropDownItems.Add(menuItemOptimiserFlux10);

						var menuItemOptimiserFlux100 = new ToolStripMenuItem("Optimiser les flux 100 step");
						menuItemOptimiserFlux100.Click += (sender, e) =>
						{
							if (usine != null)
							{
								usine.OptimiserFlux(100, 0.6);
								this.Refresh();
							}
						};
						subMenuSimulation.DropDownItems.Add(menuItemOptimiserFlux100);

						var menuItemOptimiserFlux1000 = new ToolStripMenuItem("Optimiser les flux 1000 step");
						menuItemOptimiserFlux1000.Click += (sender, e) =>
						{
							if (usine != null)
							{
								usine.OptimiserFlux(1000, 0.5);
								this.Refresh();
							}
						};
						subMenuSimulation.DropDownItems.Add(menuItemOptimiserFlux1000);
						
						// Ajouter l'élément de menu subMenuSimulation
						ctxm.Items.Add(subMenuSimulation);
						
                        Lien ln = usine.SelectionLien(pt);
                        if (ln != null)
                        {
							var menuItemConfigurerLien = new ToolStripMenuItem("Configurer (DBCLICK)");
							menuItemConfigurerLien.Click += (sender, e) =>
							{
								if (new FrmConfig(ln).ShowDialog(this) == DialogResult.Yes)
								{
									if(ln.ProducteurSource != null)
									{
										ln.ProducteurSource.Recalculer();
										this.Refresh();
									}
								}
							};
							ctxm.Items.Add(menuItemConfigurerLien);
							ctxm.Items.Add("-");
							var menuItemSupprimerLien = new ToolStripMenuItem("Supprimer (SUPPR)");
							menuItemSupprimerLien.Click += (sender, e) =>
							{
								Producteur.SupprimerLien(ln);
								usine.Calculer();
								NétoyerMain();
							};
							ctxm.Items.Add(menuItemSupprimerLien);
                        }
                        else
                        {
                            Producteur prod = usine.SelectionSeule(pt);
                            if(prod != null)
                            {
								var menuItemConfigurerProd = new ToolStripMenuItem("Configurer (DBCLICK)");
								menuItemConfigurerProd.Click += (sender, e) =>
								{
									if (new FrmConfig(prod).ShowDialog(this) == DialogResult.Yes)
									{
										prod.Recalculer();
										this.Refresh();
										
									}
								};
								ctxm.Items.Add(menuItemConfigurerProd);
								
								ctxm.Items.Add("-");
								
								var menuItemSupprimerProd = new ToolStripMenuItem("Supprimer (SUPPR)");
								menuItemSupprimerProd.Click += (sender, e) =>
								{
									usine.SupprimerProducteur(prod);
									usine.Calculer();
									NétoyerMain();
								};
								ctxm.Items.Add(menuItemSupprimerProd);
								
								var menuItemSupprimerProdAvSrc = new ToolStripMenuItem("Supprimer avec source (RETOUR)");
								menuItemSupprimerProdAvSrc.Click += (sender, e) =>
								{
									usine.SupprimerProducteurAvecSource(prod);
									usine.Calculer();
									NétoyerMain();
								};
								ctxm.Items.Add(menuItemSupprimerProdAvSrc);
                            }
                        }
                    }
                    //if(ctxm.MenuItems.Count == 0)
                    {
                        ctxm.Items.Add("-");
						
						// Créer un élément de menu "Créer usine"
						var menuItemCreerUsine = new ToolStripMenuItem("Créer usine");
						menuItemCreerUsine.Click += (sender, e) =>
						{
							// Créer une nouvelle instance de FrmCréer
							var cr = new FrmCréer("usine", Enum.GetNames(typeof(Pièce.EPièce)).Skip(7).ToArray());

							// Appeler la méthode CréationUsine
							CréationUsine(cr, pt);
						};

						// Ajouter l'élément de menu "Créer usine" au ContextMenuStrip
						ctxm.Items.Add(menuItemCreerUsine);

						// Créer un élément de menu "Créer conteneur"
						var menuItemCreerConteneur = new ToolStripMenuItem("Créer conteneur");
						menuItemCreerConteneur.Click += (sender, e) =>
						{
							// Créer une nouvelle instance de FrmCréer
							var cr = new FrmCréer("conteneur", Enum.GetNames(typeof(Pièce.EPièce)).Skip(1).ToArray());

							// Appeler la méthode CréerConteneur
							CréerConteneur(cr, pt);
						};

						// Ajouter l'élément de menu "Créer conteneur" au ContextMenuStrip
						ctxm.Items.Add(menuItemCreerConteneur);
						
						// Créer un élément de menu "Créer producteur"
						//var menuItemCreerProducteur = new ToolStripMenuItem("Créer producteur");

						// Créer un sous-menu pour les types de producteurs
						var subMenuProducteur = new ToolStripMenuItem("Créer producteur");
						//menuItemCreerProducteur.DropDownItems.Add(subMenuProducteur);

						// Ajouter des éléments de menu pour chaque type de producteur
						foreach (var typeProducteur in _producteurs_recettes)
						{
							var menuItem = new ToolStripMenuItem(typeProducteur.Nom);
							menuItem.Click += (sender, e) =>
							{
								// Créer une nouvelle instance de FrmCréer
								var cr = new FrmCréer(typeProducteur.Nom, typeProducteur.Recettes.Select(r => r.ToString()).ToArray());

								// Appeler la méthode CréerProducteur
								CréerProducteur(cr, pt);
							};

							subMenuProducteur.DropDownItems.Add(menuItem);
						}

						// Ajouter l'élément de menu "Créer producteur" au ContextMenuStrip
						ctxm.Items.Add(subMenuProducteur);
                    }
					
					ctxm.Items.Add("-");
					var menuItemPhotographier = new ToolStripMenuItem("Photographier");
					menuItemPhotographier.Click += (sender, e) =>
					{
						if (usine != null) usine.Photographier();
					};
					ctxm.Items.Add(menuItemPhotographier);
					var menuItemSauvegarder = new ToolStripMenuItem("Sauvegarder");
					menuItemSauvegarder.Click += (sender, e) =>
					{
						if(usine != null) usine.Sauvegarder();
					};
					ctxm.Items.Add(menuItemSauvegarder);
					ctxm.Items.Add("-");
					var menuItemToutSupprimer = new ToolStripMenuItem("Tout Supprimer");
					menuItemToutSupprimer.Click += (sender, e) =>
					{
						usine = null; NétoyerMain();
					};
					ctxm.Items.Add(menuItemToutSupprimer);
                    ctxm.Show(this, e.Location);
                }
            }
        }

        private void FrmSimSatisfactory_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void FrmSimSatisfactory_DragDrop(object sender, DragEventArgs e)
        {
            string[] lstFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (lstFiles != null && lstFiles.Length > 0)
            {
                string xmlfile = lstFiles.FirstOrDefault(f => f.ToUpper().EndsWith(".XML"));
                if (xmlfile != null)
                {
                    PointF drop_p = ProjectionInvWin(new PointF((e.X - this.Left - 5), (e.Y - this.Top - 27)));

                    XmlDocument doc = new XmlDocument();
                    try { doc.Load(xmlfile); }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        MessageBox.Show(this, ex.Message, "Fichier non trouvé", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    catch (System.Xml.XmlException ex)
                    {
                        MessageBox.Show(this, ex.Message, "Fichier XML incorrect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Usine us = new Usine(doc.ChildNodes.Item(0), drop_p);

                    if (usine == null) usine = us;
                    else usine.Fusionner(us);

                    this.Refresh();
                }
            }
        }

        private void FrmSimSatisfactory_KeyDown(object sender, KeyEventArgs e)
        {
            CTRL = e.Control;
            if(e.KeyCode == Keys.A && CTRL && usine != null)
            {
                SelectedProd_LEFT = usine.Producteurs.ToList();
                SelectedMod_LEFT = Producteur.SelectionMode.None;
                SelectedPcs_LEFT = Pièce.EPièce.vide;
                this.Refresh();
            }
        }

        private void FrmSimSatisfactory_KeyUp(object sender, KeyEventArgs e)
        {
            CTRL = e.Control;
            switch(e.KeyCode)
            {
                case Keys.Delete:
                    if(usine != null && SelectedProd_LEFT != null && SelectedProd_LEFT.Any())
                    {
                        SelectedProd_LEFT.ForEach(prd => usine.SupprimerProducteur(prd));
                        SelectedProd_LEFT = null;
                        SelectedMod_LEFT = Producteur.SelectionMode.None;
                        SelectedPcs_LEFT = Pièce.EPièce.vide;
                        usine.Calculer();
                        this.Refresh();
                    }
                    break;
                case Keys.Back:
                    if (usine != null && SelectedProd_LEFT != null && SelectedProd_LEFT.Any())
                    {
                        SelectedProd_LEFT.ForEach(prd => usine.SupprimerProducteurAvecSource(prd));
                        SelectedProd_LEFT = null;
                        SelectedMod_LEFT = Producteur.SelectionMode.None;
                        SelectedPcs_LEFT = Pièce.EPièce.vide;
                        usine.Calculer();
                        this.Refresh();
                    }
                    break;
            }
        }

        private void FrmSimSatisfactory_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PointF pt = ProjectionInvWin(e.Location);
            Lien ln = usine.SelectionLien(pt);
            if (ln != null) { if (new FrmConfig(ln).ShowDialog(this) == DialogResult.Yes) { if (ln.ProducteurSource != null) ln.ProducteurSource.Recalculer(); this.Refresh(); } }
            else
            {
                Producteur prod = usine.SelectionSeule(pt);
                if (prod != null) { if (new FrmConfig(prod).ShowDialog(this) == DialogResult.Yes) { prod.Recalculer(); this.Refresh(); } }
            }
        }
    }
}
