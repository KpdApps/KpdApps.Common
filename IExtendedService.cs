namespace KpdApps.MsCrm.Common
{
	public interface IExtendedService
	{
		ServiceProvider Provider
		{
			get;
		}

		void Init(ServiceProvider provider);

		void InitDependencies();
	}
}
