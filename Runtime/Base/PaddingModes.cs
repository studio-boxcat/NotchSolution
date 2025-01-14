namespace E7.NotchSolution
{
    /// <summary>
    ///     How a component looks at a particular edge to take the edge's property.
    ///     Meaning depends on context of that component.
    /// </summary>
    public enum PaddingMode : byte
    {
        /// <summary>
        ///     Use a value reported from that edge.
        /// </summary>
        On = 0,

        /// <summary>
        ///     Like <see cref="On"/> but also look at the opposite edge,
        ///     if the value reported is higher on the other side, assume that value instead.
        /// </summary>
        Balanced = 1,

        /// <summary>
        ///     Do not use a value reported from that edge.
        /// </summary>
        Off = 2,
    }

    public struct PaddingModes
    {
        public PaddingMode left;
        public PaddingMode bottom;
        public PaddingMode top;
        public PaddingMode right;

        public PaddingModes(PaddingMode left, PaddingMode bottom, PaddingMode top, PaddingMode right)
        {
            this.left = left;
            this.bottom = bottom;
            this.top = top;
            this.right = right;
        }
    }
}