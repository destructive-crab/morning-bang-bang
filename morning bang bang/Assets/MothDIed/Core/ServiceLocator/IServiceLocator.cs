using System;

namespace MothDIed.ServiceLocators
{
    public interface IServiceLocator
    {
        bool Contains(Type serviceType);

        object GetBlind(Type serviceType);
    }
}