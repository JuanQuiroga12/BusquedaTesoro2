using UnityEngine;

namespace BusquedaTesoro.Core
{
    /// <summary>
    /// Datos estáticos del equipo seleccionado, persistentes entre escenas.
    /// Incluye color y nombre para aplicar el tema visual.
    /// </summary>
    public static class TeamData
    {
        public enum TeamID
        {
            Rojo = 0,
            Azul = 1,
            Verde = 2,
            Rosa = 3,
            Naranja = 4,
            Gris = 5,
            Amarillo = 6,
            Morado = 7
        }

        private static TeamID selectedTeam = TeamID.Rojo;

        public static TeamID SelectedTeam
        {
            get => selectedTeam;
            set => selectedTeam = value;
        }

        public static int TeamNumber => (int)selectedTeam + 1;

        /// <summary>
        /// Retorna el nombre para mostrar en la UI ("LOS ROJOS", etc.)
        /// </summary>
        public static string TeamDisplayName => selectedTeam switch
        {
            TeamID.Rojo => "LOS ROJOS",
            TeamID.Azul => "LOS AZULES",
            TeamID.Verde => "LOS VERDES",
            TeamID.Rosa => "LOS ROSA",
            TeamID.Naranja => "LOS NARANJA",
            TeamID.Gris => "LOS GRISES",
            TeamID.Amarillo => "LOS AMARILLOS",
            TeamID.Morado => "LOS MORADO",
            _ => "EQUIPO"
        };

        /// <summary>
        /// Color principal del equipo (barras, acentos, stickers activos).
        /// </summary>
        public static Color TeamColor => selectedTeam switch
        {
            TeamID.Rojo => HexColor("#CC2222"),
            TeamID.Azul => HexColor("#2266CC"),
            TeamID.Verde => HexColor("#2D8E3C"),
            TeamID.Rosa => HexColor("#D45BA0"),
            TeamID.Naranja => HexColor("#D4832A"),
            TeamID.Gris => HexColor("#7A7A7A"),
            TeamID.Amarillo => HexColor("#C4A820"),
            TeamID.Morado => HexColor("#7B3FA0"),
            _ => Color.white
        };

        /// <summary>
        /// Color oscuro del equipo (fondo del header, tinte general).
        /// </summary>
        public static Color TeamDarkColor => selectedTeam switch
        {
            TeamID.Rojo => HexColor("#3D0A0A"),
            TeamID.Azul => HexColor("#0A1A3D"),
            TeamID.Verde => HexColor("#0A2E12"),
            TeamID.Rosa => HexColor("#3D0A2A"),
            TeamID.Naranja => HexColor("#3D1F0A"),
            TeamID.Gris => HexColor("#1A1A1A"),
            TeamID.Amarillo => HexColor("#2E2A0A"),
            TeamID.Morado => HexColor("#1F0A3D"),
            _ => Color.black
        };

        private static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        /// <summary>Tiempo final del equipo, guardado al completar el juego.</summary>
        public static float FinalTime { get; set; } = 0f;

        /// <summary>Retorna el tiempo formateado como MM:SS.ms</summary>
        public static string FinalTimeFormatted
        {
            get
            {
                int minutes = Mathf.FloorToInt(FinalTime / 60f);
                int seconds = Mathf.FloorToInt(FinalTime % 60f);
                int milliseconds = Mathf.FloorToInt((FinalTime % 1f) * 100f);
                return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
            }
        }
    }
}