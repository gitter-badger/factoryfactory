namespace Nooshka.Lifecycle
{
    public class ScopedLifecycle : ILifecycle
    {
        public ILifecycleManager GetLifecycleManager(ServiceRequest request)
        {
            return request.Container.LifecycleManager;
        }

        public Container GetServicingContainer(ServiceRequest request)
        {
            return request.Container;
        }
    }
}