namespace Geoservice
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
            Geoservice geoservice = new Geoservice();
            await geoservice.ProcessAsync();
            Console.WriteLine("Success");
            Console.ReadKey();
            return;
        }
	}
}
