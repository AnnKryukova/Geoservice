using Dadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Geoservice
{
    class Geoservice
    {
        private const string GEO_CODING_REQUEST_TEXT = "http://nominatim.openstreetmap.org/";
        private const string SEARCH_TEXT = "search";
        private const string COUNTRY_PARAMETER = "country";
        private const string CITY_PARAMETER = "city";
        private const string STREET_PARAMETER = "street";
        private const string FORMAT_PARAMETER = "format=json";
        private const string LIMIT_PARAMETER = "limit=2";
        private const string API_KEY = "780bbb7669ba137c1908c8a8df1d7d104f65225b";

        List<Address> addressList = new List<Address>();

        string logFilePath;

        public Geoservice() 
        {
            logFilePath = Path.GetTempFileName();
        }

        public async Task ProcessAsync()
        {
            CreateAddress();
            foreach (var address in addressList)
            {
                await Code(address);
                await Decode(address.Lat, address.Lon);
            }
        }

        private async Task Code(Address address)
        {
            try
            {
                string url = string.Format("{0}{1}?{2}", new string[] { GEO_CODING_REQUEST_TEXT, SEARCH_TEXT, CreateUri(address.Country, address.City, address.Street) });
                
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Geoservice App");
                    httpClient.BaseAddress = new Uri(GEO_CODING_REQUEST_TEXT);
                    using (HttpResponseMessage response = await httpClient.GetAsync(url))
                    {
                        File.AppendAllText(logFilePath, response.RequestMessage.RequestUri + "\r\n\r\n");
                        var content = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var result = (JArray)JsonConvert.DeserializeObject(content);
                            address.Lat = (double)result[0]["lat"];
                            address.Lon = (double)result[0]["lon"];
                        }

                        File.AppendAllText(logFilePath, "StatusCode = " + response.StatusCode.ToString() + ", body = " + content + "\r\n\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFilePath, "При получении геоданных возникла ошибка. \r\n" + ex.Message.ToString() + "\r\n\r\n");
                throw new Exception("При получении геоданных возникла ошибка. \r\n" + ex.Message.ToString());
            }

        }

        private async Task Decode(double lat, double lon)
        {
            try
            {
                var token = API_KEY;
                var api = new SuggestClientAsync(token);
                var result = await api.Geolocate(lat: lat, lon: lon, count: 10, radius_meters: 1000);
                File.AppendAllText(logFilePath, string.Join("\r\n", result.suggestions.Select(x => x.value).ToList()) + "\r\n\r\n");
            }
            catch (Exception ex) 
            {
                throw new Exception("При получении адресов возникла ошибка. \r\n" + ex.Message.ToString());
            }
        }

        private string CreateUri(string country, string city, string street)
        {
            return COUNTRY_PARAMETER + "=" + country + "&" + CITY_PARAMETER + "=" + city + "&" + STREET_PARAMETER + "=" + street + "&" + FORMAT_PARAMETER + "&" + LIMIT_PARAMETER;
        }

        private void CreateAddress()
        {
            //Добавляем адреса для теста
            addressList.Add(new Address("Россия", "Москва", "Башиловская"));
            addressList.Add(new Address("Россия", "Старый Оскол", "Дубрава"));
            addressList.Add(new Address("Россия", "Екатеринбург"));
            addressList.Add(new Address("Россия", street: "Ленина"));
            addressList.Add(new Address("Германия", "Мюнхен", "Мариенплац"));
        }
    }

    public class Address
    {
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public Address(string? country = null, string? city = null, string? street = null)
        {
            Country = country;
            City = city;
            Street = street;
        }
    }
}
