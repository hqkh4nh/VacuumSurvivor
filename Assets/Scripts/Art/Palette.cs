using UnityEngine;

namespace VacuumSurvivor
{
    /// The whole game's art colours in one place. One warm family (floor, breadcrumb, cat), one cool
    /// family (moonlight, robot), a near-black ink for outlines, and a single vivid accent reserved
    /// for "power": the suction ring, the charging dock and the dust you collect. Keeping the set this
    /// small is what makes the generated art read as one deliberate style instead of noise.
    public static class Palette
    {
        public static readonly Color NightBg    = new Color(0.106f, 0.106f, 0.184f); // #1B1B2F
        public static readonly Color FloorBoard = new Color(0.227f, 0.180f, 0.157f); // #3A2E28 warm dark wood
        public static readonly Color FloorSeam  = new Color(0.157f, 0.122f, 0.106f); // #281F1B between planks
        public static readonly Color Moonlight  = new Color(0.682f, 0.745f, 0.839f); // #AEBED6 cool top-light
        public static readonly Color Ink        = new Color(0.078f, 0.063f, 0.094f); // #141018 outline

        public static readonly Color Accent     = new Color(0.957f, 0.447f, 0.714f); // #F472B6 power

        public static readonly Color Robot      = new Color(0.839f, 0.855f, 0.894f); // #D6DAE4 cool metal
        public static readonly Color DustBunny  = new Color(0.549f, 0.541f, 0.627f); // #8C8AA0 grey-lilac
        public static readonly Color Breadcrumb = new Color(0.851f, 0.702f, 0.329f); // #D9B354 warm tan
        public static readonly Color HairClump  = new Color(0.282f, 0.282f, 0.333f); // #484855 muddy charcoal
        public static readonly Color Cat        = new Color(0.700f, 0.550f, 0.420f); // #B38C6B warm grey-brown
        public static readonly Color DustMote   = new Color(0.976f, 0.659f, 0.831f); // #F9A8D4 pale accent
    }
}
