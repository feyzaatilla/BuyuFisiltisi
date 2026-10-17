namespace BuyuFisiltisi.Core.Navigation
{
    /// <summary>
    /// Bir NavMesh Agent Type'ın beklenen ölçüleri. Navigation penceresindeki değerler bunlarla aynı girilir.
    /// Değiştirilemez (immutable) veri sınıfıdır.
    /// </summary>
    public sealed class AgentTypeSpec
    {
        public string Name { get; }
        public float Radius { get; }
        public float Height { get; }
        public float StepHeight { get; }
        public float MaxSlope { get; }

        public AgentTypeSpec(string name, float radius, float height, float stepHeight, float maxSlope)
        {
            Name = name;
            Radius = radius;
            Height = height;
            StepHeight = stepHeight;
            MaxSlope = maxSlope;
        }

        /// <summary>
        /// Bu boyuttaki ajan verilen genişlikteki bir yarıktan geçebilir mi?
        /// NavMesh, duvarları ajan yarıçapı kadar her iki yandan aşındırır; geçiş için genişlik çaptan büyük olmalı.
        /// </summary>
        public bool FitsThrough(float gapWidth) => gapWidth > Radius * 2f;
    }

    /// <summary>
    /// Projedeki iki Agent Type ve kök duvarındaki yarığın ölçüsü.
    /// Yarık yalnızca Fısıl'ın NavMesh'inde yürünebilir olmalıdır (şartname: 0,3 &lt; genişlik &lt; 0,5).
    /// </summary>
    public static class AgentTypes
    {
        /// <summary>Cadı boyutu. Cadı CharacterController ile yürür; bu NavMesh yalnızca yarığın Cadı'ya kapalı olduğunu göstermek için.</summary>
        public static readonly AgentTypeSpec Cadi = new AgentTypeSpec("Cadi", 0.5f, 2.0f, 0.4f, 45f);

        /// <summary>Fısıl boyutu. NavMeshAgent bu tipi kullanır; görsel model baseOffset ~1,2 ile havada süzülür.</summary>
        public static readonly AgentTypeSpec Fisil = new AgentTypeSpec("Fisil", 0.15f, 1.5f, 0.3f, 45f);

        /// <summary>Kök duvarındaki yarığın genişliği (metre). Sahneyi kuran bu değeri kullanır.</summary>
        public const float RootWallGapWidth = 0.45f;

        public static readonly AgentTypeSpec[] All = { Cadi, Fisil };
    }
}
