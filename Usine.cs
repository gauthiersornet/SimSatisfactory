using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SimSatisfactory
{
    class Usine
    {
        static private readonly SizeF EspacementProducteurs = new SizeF(20.0f, 200.0f);

        static public List<Producteur> Créer_Producteurs(params Pièce.EPièce[] pièces)
        {
            List<Producteur> lstProducteurs = new List<Producteur>();

            if (pièces != null && pièces.Any())
            {
                //for(Pièce.EPièce ep = ressources.FirstOrDefault(); ep != Pièce.EPièce.vide; ep = ressources.FirstOrDefault())
                foreach (Pièce.EPièce ep in pièces)
                {
                    //List<Producteur> lstProd;
                    List<Recette> lstRct;
                    /*if (dicoProdUsine.TryGetValue(ep, out lstProd) && lstProd != null && lstProd.Any())
                    {
                        //lstProd.ForEach(p => { Producteur.RelierProducteur(p, prod, ep); });
                    }
                    else*/ if (Recette.EPièceSortantRecette.TryGetValue(ep, out lstRct) && lstRct != null && lstRct.Any())
                    {
                        if (lstRct.Any(r => r.Entrants.EstVide)) lstRct = lstRct.Where(r => r.Entrants.EstVide).ToList();
                        foreach (Recette rct in lstRct)
                        {
                            Producteur rctPrd = new Producteur(rct);
                            //Producteur.RelierProducteur(rctPrd, prod, ep);
                            lstProducteurs.Add(rctPrd);
                            //usine.Producteurs.Add(rctPrd);
                            //AccProductionUsine(dicoProdUsine, rctPrd);
                        }
                    }
                    //ressources.Remove(ep);
                }

                if(lstProducteurs.Count >= 2)//EspacementProducteurs
                {
                    /*float px = -(lstProducteurs.First().Taille.Width + lstProducteurs.Last().Taille.Width) - EspacementProducteurs.Width;
                    for(int i = lstProducteurs.Count - 2; i > 0; --i) px -= lstProducteurs[i].Taille.Width - EspacementProducteurs.Width;
                    px /= 2.0f;*/
                    float px = -(lstProducteurs.Sum(p => p.Taille.Width + EspacementProducteurs.Width)) / 2.0f;
                    foreach(Producteur prd in lstProducteurs)
                    {
                        float demi = (prd.Taille.Width + EspacementProducteurs.Width) / 2.0f;
                        px += demi;
                        prd.P = new PointF(px, 0.0f);
                        px += demi;
                    }
                }
            }

            return lstProducteurs;
        }

        static public List<Producteur> Construire_Usine(List<Producteur> existant, List<Producteur> lstProducteurs)
        {
            if (lstProducteurs != null && lstProducteurs.Any())
            {
                //if (usine == null) usine = new Usine();
                //lstProducteurs.ForEach(p => { if(!usine.Producteurs.Contains(p)) usine.Producteurs.Add(p); });
                // = recettes.Select(r => new Producteur(r, 0.0)).ToList();
                //usine.Producteurs.AddRange(lstProducteurs);

                PointF midPoint = new PointF();
                float hMax = 0.0f;

                //Dictionary<Pièce.EPièce, List<Producteur>> dicoProdUsine = usine?.ProdEPièceUsine ?? new Dictionary<Pièce.EPièce, List<Producteur>>();
                Dictionary<Pièce.EPièce, List<Producteur>> dicoProdUsine = Usine.CréerDicoResExistantes(existant);
                lstProducteurs.ForEach(p => { /*AccProductionUsine(dicoProdUsine, p);*/ midPoint.X += p.P.X; midPoint.Y += p.P.Y; hMax = Math.Max(hMax, p.Taille.Height); });

                midPoint.X /= lstProducteurs.Count;
                midPoint.Y /= lstProducteurs.Count;
                midPoint.Y -= hMax / 2.0f + EspacementProducteurs.Height;

                List<Producteur> resLst = new List<Producteur>();
                resLst.AddRange(lstProducteurs);

                for (int i = 0; i < 100 && lstProducteurs.Any(); ++i)
                {
                    List<Producteur> nLst = new List<Producteur>();
                    foreach (Producteur prod in lstProducteurs)
                    {
                        HashSet<Pièce.EPièce> ressources = prod.EPièceNonAppro;
                        if (ressources != null && ressources.Any())
                        {
                            //for(Pièce.EPièce ep = ressources.FirstOrDefault(); ep != Pièce.EPièce.vide; ep = ressources.FirstOrDefault())
                            foreach (Pièce.EPièce ep in ressources)
                            {
                                List<Producteur> lstProd;
                                List<Recette> lstRct;
                                dicoProdUsine.TryGetValue(ep, out lstProd);
                                /*if (lstProd != null && lstProd.Any())
                                {
                                    lstProd = lstProd.Where(p => !prod.EstSourceDe(p)).ToList();
                                }*/
                                if (lstProd != null && lstProd.Any())
                                {
                                    lstProd.ForEach(p => Producteur.RelierProducteur(p, prod, ep));
                                }
                                if (!(lstProd != null && lstProd.Any(p => !prod.EstSourceDe(p))) && Recette.EPièceSortantRecette.TryGetValue(ep, out lstRct) && lstRct != null && lstRct.Any())
                                {
                                    lstRct = lstRct.Where(r => r.TypeProducteur != Recette.EProducteur.Packageur).ToList();
                                    if (lstRct.Any(r => r.Entrants.EstVide)) lstRct = lstRct.Where(r => r.Entrants.EstVide).ToList();
                                    else if(lstRct.Any(r => r.Sortants.ingrédients.Length == 1)) lstRct = lstRct.Where(r => r.Sortants.ingrédients.Length == 1).ToList();
                                    foreach (Recette rct in lstRct)
                                    {
                                        Producteur rctPrd = new Producteur(rct);
                                        Producteur.RelierProducteur(rctPrd, prod, ep);
                                        nLst.Add(rctPrd);
                                        //usine.Producteurs.Add(rctPrd);
                                        //AccProductionUsine(dicoProdUsine, rctPrd);
                                    }
                                }
                                //ressources.Remove(ep);
                            }
                        }
                    }

                    if (nLst.Any())
                    {
                        hMax = nLst.Max(p => p.Taille.Height) / 2.0f;
                        PointF newMidPoint = new PointF();
                        midPoint.Y -= hMax;
                        /*float px = -(lstProducteurs.First().Taille.Width + lstProducteurs.Last().Taille.Width / 2.0f) - EspacementProducteurs.Width;
                        for (int j = lstProducteurs.Count - 2; j > 0; --j) px -= lstProducteurs[j].Taille.Width - EspacementProducteurs.Width;
                        px /= 2.0f;*/
                        float px = -(nLst.Sum(p => p.Taille.Width + EspacementProducteurs.Width)) / 2.0f;
                        foreach (Producteur prd in nLst)
                        {
                            float demi = (prd.Taille.Width + EspacementProducteurs.Width) / 2.0f;
                            px += demi;
                            prd.P = new PointF(midPoint.X + px, midPoint.Y);
                            newMidPoint.X += prd.P.X;
                            newMidPoint.Y += prd.P.Y;
                            px += demi;
                            AccProductionUsine(dicoProdUsine, prd);
                        }
                        resLst.AddRange(nLst);
                        midPoint.X = newMidPoint.X / nLst.Count;
                        nLst.ForEach(p => p.P.X -= midPoint.X);
                        midPoint.X = 0.0f;
                        midPoint.Y = newMidPoint.Y / nLst.Count;
                        midPoint.Y -= hMax + EspacementProducteurs.Height;
                    }
                    
                    lstProducteurs = nLst;
                }

                if(resLst.Any())
                {
                    midPoint = new PointF();
                    resLst.ForEach(p => { midPoint.X += p.P.X; midPoint.Y += p.P.Y; });
                    midPoint.X /= resLst.Count;
                    midPoint.Y /= resLst.Count;
                    resLst.ForEach(p => { p.P.X -= midPoint.X; p.P.Y -= midPoint.Y; });
                }

                return resLst;
            }
            else return null;
        }

        /*
		static public List<Producteur> Construire_Usine(List<Producteur> existant, List<Producteur> lstProducteurs)
        {
            if (lstProducteurs != null && lstProducteurs.Any())
            {
                //if (usine == null) usine = new Usine();
                //lstProducteurs.ForEach(p => { if(!usine.Producteurs.Contains(p)) usine.Producteurs.Add(p); });
                // = recettes.Select(r => new Producteur(r, 0.0)).ToList();
                //usine.Producteurs.AddRange(lstProducteurs);

                PointF midPoint = new PointF();
                float hMax = 0.0f;

                //Dictionary<Pièce.EPièce, List<Producteur>> dicoProdUsine = usine?.ProdEPièceUsine ?? new Dictionary<Pièce.EPièce, List<Producteur>>();
                Dictionary<Pièce.EPièce, List<Producteur>> dicoProdUsine = Usine.CréerDicoResExistantes(existant);
                lstProducteurs.ForEach(p => { AccProductionUsine(dicoProdUsine, p); midPoint.X += p.P.X; midPoint.Y += p.P.Y; hMax = Math.Max(hMax, p.Taille.Height); });

                midPoint.X /= lstProducteurs.Count;
                midPoint.Y /= lstProducteurs.Count;
                midPoint.Y -= hMax / 2.0f + EspacementProducteurs.Height;

                List<Producteur> resLst = new List<Producteur>();
                resLst.AddRange(lstProducteurs);

                for (int i = 0; i < 100 && lstProducteurs.Any(); ++i)
                {
                    List<Producteur> nLst = new List<Producteur>();
                    foreach (Producteur prod in lstProducteurs)
                    {
                        HashSet<Pièce.EPièce> ressources = prod.EPièceNonAppro;
                        if (ressources != null && ressources.Any())
                        {
                            //for(Pièce.EPièce ep = ressources.FirstOrDefault(); ep != Pièce.EPièce.vide; ep = ressources.FirstOrDefault())
                            foreach (Pièce.EPièce ep in ressources)
                            {
                                List<Producteur> lstProd;
                                List<Recette> lstRct;
                                dicoProdUsine.TryGetValue(ep, out lstProd);
                                //if (lstProd != null && lstProd.Any())
                                //{
                                //    lstProd = lstProd.Where(p => !prod.EstSourceDe(p)).ToList();
                                //}
                                if (lstProd != null && lstProd.Any())
                                {
                                    lstProd.ForEach(p => Producteur.RelierProducteur(p, prod, ep));
                                }
                                if (!(lstProd != null && lstProd.Any(p => !prod.EstSourceDe(p))) && Recette.EPièceSortantRecette.TryGetValue(ep, out lstRct) && lstRct != null && lstRct.Any())
                                {
                                    if (lstRct.Any(r => r.Entrants.EstVide)) lstRct = lstRct.Where(r => r.Entrants.EstVide).ToList();
                                    else lstRct = lstRct.Where(r => r.TypeProducteur != Recette.EProducteur.Packageur).ToList();
                                    foreach (Recette rct in lstRct)
                                    {
                                        Producteur rctPrd = new Producteur(rct);
                                        Producteur.RelierProducteur(rctPrd, prod, ep);
                                        nLst.Add(rctPrd);
                                        //usine.Producteurs.Add(rctPrd);
                                        AccProductionUsine(dicoProdUsine, rctPrd);
                                    }
                                }
                                //ressources.Remove(ep);
                            }
                        }
                    }

                    if (nLst.Any())
                    {
                        hMax = nLst.Max(p => p.Taille.Height) / 2.0f;
                        PointF newMidPoint = new PointF();
                        midPoint.Y -= hMax;
                        //float px = -(lstProducteurs.First().Taille.Width + lstProducteurs.Last().Taille.Width / 2.0f) - EspacementProducteurs.Width;
                        //for (int j = lstProducteurs.Count - 2; j > 0; --j) px -= lstProducteurs[j].Taille.Width - EspacementProducteurs.Width;
                        //px /= 2.0f;
                        float px = -(nLst.Sum(p => p.Taille.Width + EspacementProducteurs.Width)) / 2.0f;
                        foreach (Producteur prd in nLst)
                        {
                            float demi = (prd.Taille.Width + EspacementProducteurs.Width) / 2.0f;
                            px += demi;
                            prd.P = new PointF(midPoint.X + px, midPoint.Y);
                            newMidPoint.X += prd.P.X;
                            newMidPoint.Y += prd.P.Y;
                            px += demi;
                        }
                        resLst.AddRange(nLst);
                        midPoint.X = newMidPoint.X / nLst.Count;
                        nLst.ForEach(p => p.P.X -= midPoint.X);
                        midPoint.X = 0.0f;
                        midPoint.Y = newMidPoint.Y / nLst.Count;
                        midPoint.Y -= hMax + EspacementProducteurs.Height;
                    }
                    
                    lstProducteurs = nLst;
                }

                if(resLst.Any())
                {
                    midPoint = new PointF();
                    resLst.ForEach(p => { midPoint.X += p.P.X; midPoint.Y += p.P.Y; });
                    midPoint.X /= resLst.Count;
                    midPoint.Y /= resLst.Count;
                    resLst.ForEach(p => { p.P.X -= midPoint.X; p.P.Y -= midPoint.Y; });
                }

                return resLst;
            }
            else return null;
        }
        */

        static public List<Producteur> Construire_Usine(List<Producteur> existant, params Pièce.EPièce[] pièces)
        {
            return Construire_Usine(existant, Créer_Producteurs(pièces));
        }

        public List<Producteur> Producteurs = new List<Producteur>();
        public double ConsommationRéel
        {
            get
            {
                if (Producteurs != null) return Producteurs.Sum(p => p.ConsommationRéel);
                else return 0.0;
            }
        }
        public double ConsommationMax
        {
            get
            {
                if (Producteurs != null) return Producteurs.Sum(p => p.ConsommationMax);
                else return 0.0;
            }
        }

        /*public Dictionary<Ingrédients, List<Producteur>> ProdUsine
        {
            get
            {
                Dictionary<Ingrédients, List<Producteur>> dicoProdUsine = new Dictionary<Ingrédients, List<Producteur>>();
                //Producteurs.ForEach(p => { if (p.ProgRecette != null && p.ProgRecette.Sortants.ingrédients != null && p.ProgRecette.Sortants.ingrédients.Any() && p.EstApprovisioné) dicoProdUsine.Add(p.ProgRecette.Sortants, p); });
                foreach(Producteur p in Producteurs)
                {
                    if (p.ProgRecette != null && p.ProgRecette.Sortants.ingrédients != null && p.ProgRecette.Sortants.ingrédients.Any() && p.EstApprovisioné)
                    {

                    }
                }
                return dicoProdUsine;
            }
        }*/
        public static void AccProductionUsine(Dictionary<Pièce.EPièce, List<Producteur>> dicoProdUsine, Producteur prod)
        {
            if (prod.ProgRecette != null && prod.ProgRecette.Sortants.ingrédients != null)
            {
                foreach (Ingrédient ing in prod.ProgRecette.Sortants.ingrédients)
                {
                    List<Producteur> lstPrd;
                    if (!dicoProdUsine.TryGetValue(ing.pièce.PType, out lstPrd))
                    {
                        lstPrd = new List<Producteur>();
                        dicoProdUsine.Add(ing.pièce.PType, lstPrd);
                    }
                    if (!lstPrd.Contains(prod)) lstPrd.Add(prod);
                }
            }
        }

        static public Dictionary<Pièce.EPièce, List<Producteur>> CréerDicoResExistantes(List<Producteur> Producteurs)
        {
            Dictionary<Pièce.EPièce, List<Producteur>> dicoProdUsine = new Dictionary<Pièce.EPièce, List<Producteur>>();
            if (Producteurs != null) foreach (Producteur prod in Producteurs) AccProductionUsine(dicoProdUsine, prod);
            return dicoProdUsine;
        }

        public Dictionary<Pièce.EPièce, List<Producteur>> ProdEPièceUsine
        {
            get
            {
                return CréerDicoResExistantes(Producteurs);
            }
        }

        public Usine() { }

        public Usine(List<Producteur> prod) { Producteurs = prod; Calculer(); }

        public Usine(XmlNode nd, PointF dropP)
        {
            Dictionary<uint, Producteur> mapProd = new Dictionary<uint, Producteur>();
            Producteurs = new List<Producteur>();
            foreach (XmlNode cnd in nd.ChildNodes)
            {
                Producteur prod = new Producteur(cnd, dropP);
                Producteurs.Add(prod);
                mapProd.Add(prod.id, prod);
            }
            foreach (XmlNode cnd in nd.ChildNodes)
            {
                uint id = uint.Parse(cnd.Attributes.GetNamedItem("id").Value);
                mapProd[id].ChargerLiensSortants(cnd.ChildNodes, mapProd);
            }
            if (Producteurs.Any() == false) Producteurs = null;
        }

        public void Dessiner(Graphics g)
        {
            if (Producteurs != null)
            {
                foreach (Producteur prd in Producteurs) prd.Dessiner(g);
                foreach (Producteur prd in Producteurs) prd.DessinerLiensSortant(g);
            }
        }

        public (Producteur, Producteur.SelectionMode, Pièce.EPièce) Selection(PointF p)
        {
            Producteur prod = Producteurs?.LastOrDefault(x => x.EstDans(p));
            if (prod != null)
            {
                var mod = prod.ObtenirMode(p);
                if((mod.Item1 == Producteur.SelectionMode.LienEntrant || mod.Item1 == Producteur.SelectionMode.LienSortant) && mod.Item2 == Pièce.EPièce.vide)
                    return (null, Producteur.SelectionMode.Déplacement, Pièce.EPièce.vide);
                return (prod, mod.Item1, mod.Item2);
            }
            else return (null, Producteur.SelectionMode.None, Pièce.EPièce.vide);
        }

        public Producteur SelectionSeule(PointF p)
        {
            return Producteurs?.LastOrDefault(x => x.EstDans(p));
        }

        public IEnumerable<Producteur> SelectionMulty(RectangleF rect)
        {
            return Producteurs?.Where(prd => rect.X <= prd.P.X && prd.P.X < (rect.X + rect.Width) && rect.Y <= prd.P.Y && prd.P.Y < (rect.Y + rect.Height));
        }

        public Lien SelectionLien(PointF p)
        {
            if (Producteurs != null)
            {
                for (int i = Producteurs.Count - 1; i >= 0; --i)
                {
                    Lien ln = Producteurs[i].SelectionLienSortant(p);
                    if(ln != null) return ln;
                }
                return null;
            }
            else return null;
        }

        public void SupprimerProducteur(Producteur prod)
        {
            if(Producteurs.Remove(prod)) prod.SupprimerLiens();
        }

        public void SupprimerProducteurAvecSource(Producteur producteur)
        {
            List<Producteur> prods = new List<Producteur>() { producteur };
            List<Producteur> nextProds;
            while(prods != null && prods.Any())
            {
                nextProds = new List<Producteur>();
                foreach(Producteur prod in prods)
                {
                    List<Lien> lns = prod.Sources;
                    if (lns != null && lns.Any())
                    {
                        nextProds.AddRange(lns.Select(l => l.ProducteurSource)
                                   .Where(lps => lps.Destinataires == null || lps.Destinataires.All(lpsd => lpsd.ProducteurDestinataire == prod)));
                    }
                    prod.SupprimerLiens();
                    Producteurs.Remove(prod);
                }
                prods = nextProds;
            }
        }

        public void Fusionner(Usine us)
        {
            if (Producteurs != null) Producteurs.AddRange(us.Producteurs);
            else Producteurs = us.Producteurs;
        }

        public void Ajouter(List<Producteur> prods)
        {
            if (Producteurs != null) Producteurs.AddRange(prods.Where(p => !Producteurs.Contains(p)));
            else Producteurs = prods;
        }

        public void Sauvegarder()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string flt = "files (*.XML)|*.XML";
            saveFileDialog.FileName = Guid.NewGuid().ToString()+".xml";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                XmlNode racine = doc.CreateNode(XmlNodeType.Element, "Usine", "");
                doc.AppendChild(racine);
                if (Producteurs != null && Producteurs.Any())
                {
                    PointF dropP = new PointF();
                    for(int i = 0; i < Producteurs.Count; ++i)
                    {
                        dropP.X += Producteurs[i].P.X;
                        dropP.Y += Producteurs[i].P.Y;
                        Producteurs[i].id = (uint)(1 + i);
                    }
                    dropP.X /= Producteurs.Count;
                    dropP.Y /= Producteurs.Count;
                    Producteurs.ForEach(x => racine.AppendChild(x.Sérialiser(doc, dropP)));
                }
                doc.Save(saveFileDialog.FileName);
            }
        }

        public void Photographier(float echelle = 1.0f)
        {
            if (Producteurs != null && Producteurs.Any())
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                string flt = "files (*.PNG)|*.PNG";
                saveFileDialog.FileName = Guid.NewGuid().ToString() + ".png";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    float xmin = float.MaxValue, ymin = float.MaxValue, xmax = float.MinValue, ymax = float.MinValue;
                    Producteurs.ForEach(p =>
                    {
                        SizeF taille = p.Taille;
                        xmin = Math.Min(xmin, (p.P.X - taille.Width / 2.0f));
                        ymin = Math.Min(ymin, (p.P.Y - taille.Height / 2.0f));
                        xmax = Math.Max(xmax, (p.P.X + taille.Width / 2.0f));
                        ymax = Math.Max(ymax, (p.P.Y + taille.Height / 2.0f));
                    });
                    xmin *= echelle;
                    ymin *= echelle;
                    xmax *= echelle;
                    ymax *= echelle;

                    Bitmap photo = new Bitmap(2 + (int)(xmax - xmin), 2 + (int)(ymax - ymin));
                    Graphics g = Graphics.FromImage(photo);
                    g.Clear(Color.White);
                    g.ScaleTransform(echelle, echelle);
                    g.TranslateTransform((2 + (int)(xmax - xmin)) / 2 - (xmax + xmin) / 2, (2 + (int)(ymax - ymin)) / 2 - (ymax + ymin) / 2);
                    Dessiner(g);
                    photo.Save(saveFileDialog.FileName);
                }
            }
        }

        public void Calculer()
        {
            if(Producteurs != null)
            {
                Producteurs.ForEach(p => { if (p.Destinataires != null) p.Destinataires.ForEach(l => l.RéinitialiserCalcul()); p.RéinitialiserCalcul(); } );
                IEnumerable<Producteur> prods = Producteurs.Where(p => p.ProgRecette != null && p.ProgRecette.Entrants.EstVide).ToList();

                while(prods != null && prods.Any())
                {
                    foreach (Producteur prod in prods) prod.Calculer();
                    prods = prods.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstApproCalculable).Distinct().ToList();
                }
            }
        }

        public void Optimiser()
        {
            if (Producteurs != null)
            {
                Producteurs.ForEach(p => { p.Etat &= ~Producteur.EEtat.Est_Optimisé; });
                IEnumerable<Producteur> prods = Producteurs.Where(p => p.ProgRecette != null && p.ProgRecette.Entrants.EstVide).ToList();

                while (prods != null && prods.Any())
                {
                    foreach (Producteur prod in prods) prod.Optimiser();
                    prods = prods.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Optimisé) && prd.EstOptimisable).Distinct().ToList();
                }
            }
        }

        /*
        public void OptimiserFlux(int nbStep)
        {
            if (Producteurs != null)
            {
                for (int i = 0; i < nbStep; ++i)
                {
                    Producteurs.ForEach(p => p.Etat &= ~Producteur.EEtat.Est_Flux_Optimisé);
                    IEnumerable<Producteur> prods = Producteurs.Where(p => p.ProgRecette != null && (p.Sources != null && p.Sources.Any()) && !(p.Destinataires != null && p.Destinataires.Any())).ToList();

                    while (prods != null && prods.Any())
                    {
                        foreach (Producteur prod in prods) prod.OptimiserFlux();
                        prods = prods.Where(prd => prd.Sources != null).SelectMany(prd => prd.Sources.Select(d => d.ProducteurSource)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Flux_Optimisé) && prd.EstFluxOptimisable).Distinct().ToList();
                    }

                    //Optimiser();
                }
            }
        }
        */

        public void OptimiserFlux(int nbStep, double stepFact = 1.0)
        {
            if (Producteurs != null)
            {
                for (int i = 0; i < nbStep; ++i)
                {
                    Producteurs.ForEach(p => p.Etat &= ~Producteur.EEtat.Est_Flux_Optimisé);
                    IEnumerable<Producteur> prods = Producteurs.Where(p => p.ProgRecette != null && (p.Sources != null && p.Sources.Any()) && !(p.Destinataires != null && p.Destinataires.Any())).ToList();

                    while (prods != null && prods.Any())
                    {
                        foreach (Producteur prod in prods) prod.OptimiserFlux(stepFact);
                        prods = prods.Where(prd => prd.Sources != null).SelectMany(prd => prd.Sources.Select(d => d.ProducteurSource)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Flux_Optimisé) && prd.EstFluxOptimisable).Distinct().ToList();
                    }
                    Optimiser();
                }
            }
        }

        static public void Calculer(List<Producteur> Producteurs)
        {
            if (Producteurs != null)
            {
                Producteurs.ForEach(p => { p.RéinitialiserCalcul(); });
                IEnumerable<Producteur> prods = Producteurs.Where(p => p.ProgRecette != null && p.ProgRecette.Entrants.EstVide).ToList();

                while (prods != null && prods.Any())
                {
                    foreach (Producteur prod in prods) prod.Calculer();
                    prods = prods.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstApproCalculable).ToList();
                }
            }
        }

        static public void Optimiser(List<Producteur> Producteurs)
        {
            if (Producteurs != null)
            {
                Producteurs.ForEach(p => { if (p.Destinataires != null) p.Destinataires.ForEach(l => l.RéinitialiserCalcul()); p.RéinitialiserCalcul(); });
                IEnumerable<Producteur> prods = Producteurs.Where(p => p.ProgRecette != null && p.ProgRecette.Entrants.EstVide).ToList();

                while (prods != null && prods.Any())
                {
                    foreach (Producteur prod in prods) prod.Optimiser();
                    prods = prods.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstOptimisable).Distinct().ToList();
                }
            }
        }

        static public void ReCalculer(List<Producteur> producteurs)
        {
            if (producteurs != null)
            {
                while (producteurs != null && producteurs.Any())
                {
                    foreach (Producteur p in producteurs)
                    {
                        if (p.Destinataires != null) p.Destinataires.ForEach(l => { l.RéinitialiserCalcul(); l.ProducteurDestinataire.RéinitialiserCalcul(); });
                        p.RéinitialiserCalcul();
                        /*p.FacteurAppro = 0.0;
                        p.FacteurThéo = 0.0;
                        p.Etat &= ~Producteur.EEtat.Est_Calculé;*/
                        p.Calculer();
                    }
                    producteurs = producteurs.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Calculé) && prd.EstApproCalculable).Distinct().ToList();
                }
            }
        }

        static public void ReOptimiser(List<Producteur> producteurs)
        {
            if (producteurs != null)
            {
                while (producteurs != null && producteurs.Any())
                {
                    foreach (Producteur p in producteurs)
                    {
                        if (p.Destinataires != null) p.Destinataires.ForEach(l => { l.ProducteurDestinataire.Etat &= ~Producteur.EEtat.Est_Optimisé; });
                        p.Etat &= ~Producteur.EEtat.Est_Optimisé;
                        p.Optimiser();
                    }
                    producteurs = producteurs.Where(prd => prd.Destinataires != null).SelectMany(prd => prd.Destinataires.Select(d => d.ProducteurDestinataire)).Where(prd => !prd.Etat.HasFlag(Producteur.EEtat.Est_Optimisé) && prd.EstOptimisable).Distinct().ToList();
                }
            }
        }
    }
}
