using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimSatisfactory
{
    public struct Pièce
    {
        public enum EPièce
        {
            vide = 0,
            bois,
            feuille,
            pétales,
            organe,
            carapasse,
            mycélium,
            minerai_fer,
            minerai_cuivre,
            minerai_calcaire,
            minerai_charbon,
            minerai_catérium,
            minerai_soufre,
            minerai_quartz,
            minerai_bauxite,
            minerai_uranium,
            fluide_eau,
            fluide_pétrole_brut,
            fluide_résidu_pétrole_lourd,
            fluide_carburant,
            fluide_biocarburant,
            fluide_alumine,
            fluide_acide_sulfurique,
            fluide_acide_nitrique,
            fluide_azote,
            biomasse,
            bio_carburant_solide,
            lingo_fer,
            lingo_cuivre,
            béton,
            lingo_acier,
            lingo_catérium,
            lingo_aluminium,
            cristal_quartz,
            silice,
            plaque_fer,
            plaque_fer_renforcée,
            tige_fer,
            vis,
            tôle_cuivre,
            fil_électrique,
            câble,
            filactif,
            rebus_aluminium,
            poutre_acier,
            tuyau_acier,
            boitier_aluminium,
            barre_pointe,
            cartouche,
            plastique,
            caoutchouc,
            résine_polymère,
            coke_pétrole,
            bidon,
            réservoir,
            bidon_eau,
            bidon_pétrole,
            bidon_résidu_pétrole_lourd,
            bidon_biocarburant,
            bidon_alumine,
            bidon_carburant,
            bidon_acide_sulfurique,
            réservoir_acide_nitrique,
            réservoir_azote,
            cadre_modulaire,
            poutre_béton_armé,
            tôle_aluminium,
            rotor,
            stator,
            moteur,
            dissipateur,
            placage_intélligent,
            structure_polyvalente,
            câblage_automatisé,
            unité_contrôle_adaptative,
            supercalculateur,
            système_directeur_assemblage,
            contrôleur_ia,
            circuit_imprimé,
            oscillateur_cristal,
            ordinateur,
            poudre_noire,
            nobelisk,
            tissu,
            tige_contrôle_électromagnétique,
            balise,
            cadre_modulaire_lourd,
            cadre_modulaire_fusionné,
            cube_conversion_pression,
            pâte_nucléaire,
            unité_contrôle_radio,
            moteur_modulaire,
            générateur_champ_magnétique,
            fusée_propulsion_thermique,
            filtre_gaz,
            filtre_infusion_iode,
            cartouche_fusil,
            connecteur_haute_vitesse,
            système_refroidissement,
            moteur_turbo,
            cellule_uranium,
            barre_combustible_uranium,
            batterie,
            poudre_de_cuivre,
            uranium_non_fissile,
            granulé_de_plutonium,
            cellule_de_plutonium,
            barre_de_plutonium,
            déchets_uranium
        }

        public enum ENiveauMK
        {
            vide = 0,
            mk1 = 1,
            mk2 = 2,
            mk3 = 3,
            mk4 = 4,
            mk5 = 5,
        }

        static public readonly double[] débit_convoyeur = { 0.0, 60.0, 120.0, 270.0, 480.0, 780.0 };
        static public readonly double débit_convoyeur_max = débit_convoyeur.Max();
        static public readonly int Img_NbElm_Colonne = 11;

        static public readonly HashSet<EPièce> fluide = new HashSet<EPièce>()
        {
            EPièce.fluide_eau,
            EPièce.fluide_pétrole_brut,
            EPièce.fluide_résidu_pétrole_lourd,
            EPièce.fluide_carburant,
            EPièce.fluide_biocarburant,
            EPièce.fluide_alumine,
            EPièce.fluide_acide_sulfurique,
            EPièce.fluide_azote
        };
        static public double ObtenirLimiteLogistique(EPièce PType)
        {
            if (PType == EPièce.vide) return 0.0;
            if (fluide.Contains(PType)) return 600.0;
            else return 780.00;
        }

        public double LimiteLogistique{ get => ObtenirLimiteLogistique(PType); }

        public EPièce PType;

        public Pièce(EPièce PType){ this.PType = PType; }

        public override int GetHashCode()
        {
            return PType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return PType.Equals(obj);
        }

        public override string ToString()
        {
            return PType.ToString();
        }
    }
}
