namespace BuyuFisiltisi.Core.Events
{
    /// <summary>
    /// Olay yoluna ihtiyaç duyan bileşenler bu arayüzü uygular.
    /// Bootstrap (kompozisyon kökü) oyun başında tek bir EventBus oluşturup bunlara enjekte eder;
    /// böylece static singleton kullanmayız.
    /// </summary>
    public interface IRequiresEventBus
    {
        void InjectEventBus(IEventBus eventBus);
    }
}
