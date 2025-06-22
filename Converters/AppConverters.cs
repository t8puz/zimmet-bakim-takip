using System;
using System.Windows;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// Uygulama genelinde kullanılan dönüştürücülere (Converter) erişim noktası.
    /// Bu sınıf, XAML içinde StaticResource yerine static özellikleri kullanarak dönüştürücülere erişim sağlar.
    /// </summary>
    public static class AppConverters
    {
        // StringLengthToVisibilityConverter'a erişim için static özellik
        private static readonly StringLengthToVisibilityConverter _stringLengthToVisibilityConverter = new StringLengthToVisibilityConverter();
        public static StringLengthToVisibilityConverter StringLengthToVisibilityConverter => _stringLengthToVisibilityConverter;

        // StatusToColorConverter'a erişim için static özellik
        private static readonly StatusToColorConverter _statusToColorConverter = new StatusToColorConverter();
        public static StatusToColorConverter StatusToColorConverter => _statusToColorConverter;

        // BooleanToStatusConverter'a erişim için static özellik
        private static readonly BooleanToStatusConverter _booleanToStatusConverter = new BooleanToStatusConverter();
        public static BooleanToStatusConverter BooleanToStatusConverter => _booleanToStatusConverter;

        // StatusToBrushConverter'a erişim için static özellik
        private static readonly StatusToBrushConverter _statusToBrushConverter = new StatusToBrushConverter();
        public static StatusToBrushConverter StatusToBrushConverter => _statusToBrushConverter;
    }
} 