namespace P4.Core.Graphics
{
    public enum EActivationZone
    {
        /// <summary>
        /// Everytime the component will get updated
        /// </summary>
        Everytime = 0,
        /// <summary>
        /// When seen by a renderer, the component will get updated
        /// </summary>
        Renderer = 1,
        /// <summary>
        /// When the bounds are in camera range, the component will get updated
        /// </summary>
        Bounds = 2
    }
}