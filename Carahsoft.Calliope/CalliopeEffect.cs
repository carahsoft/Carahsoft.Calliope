namespace Carahsoft.Calliope
{
    public enum CalliopeEffect
    {
        None,
        ScanlineGradient,
        Phoenix,
        Unicorn,
        Grayscale,
        GalacticCrush,
        [Obsolete("Use Grayscale")]
        Greyscale = Grayscale,
    }
}