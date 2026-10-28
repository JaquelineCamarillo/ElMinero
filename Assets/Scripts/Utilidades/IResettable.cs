namespace ElMinero.Utilidades
{
    /// <summary>
    /// Interfaz para objetos del nivel que deben restaurar su estado original
    /// cuando el nivel se reinicia (o tras la reaparición del jugador en checkpoint).
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// Restablece el objeto a su estado inicial.
        /// </summary>
        void ResetObject();
    }
}
