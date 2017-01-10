using SoftwareUpdater.Configuration;

namespace SoftwareUpdater.Interface
{
    public interface IGetConfig
    {
        Config ImportConfiguration(string fileName);
    }
}