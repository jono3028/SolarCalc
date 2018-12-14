using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SolarCalc.Function
{
    public static class SolarCalc
    {
        private struct SolarCalcReturn
        {
            public DateTime sunrise;
            public DateTime sunset;
        }
        public struct SolarInput 
        {
            public float lat;
            public float lon;
            public string day;
        }
        [FunctionName("SolarCalc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SolarInput data = JsonConvert.DeserializeObject<SolarInput>(requestBody);

            float lat = data.lat;
            float lon = data.lon;
            DateTime day = new DateTime();
            day = DateTime.Parse(data.day);

            double julianDay = 2415018 + day.ToOADate();
            double julianCentury = (julianDay - 2451545)/36525;

            double meanSolarLong = (280.4664 + julianCentury * (36000.76983 + julianCentury * .0003032)) % 360;
            double meanAnom = 357.52911 + julianCentury * (35999.05029 - 0.0001537 * julianCentury);
            double eccentEarthOrbit = 0.016708634 - julianCentury * (0.000042037 + 0.0000001267 * julianCentury);

            double sunEqOfCtr = Math.Sin((Math.PI * meanAnom) / 180) * (1.914602 - julianCentury * (0.004817+0.000014 * julianCentury)) + Math.Sin((2 * meanAnom * Math.PI) / 180) * (0.019993 - 0.000101 * julianCentury) + Math.Sin((3 * meanAnom * Math.PI) / 180) * 0.000289;
            double sunTrueLon = meanSolarLong + sunEqOfCtr;
            double sunTruAnom = meanAnom + sunEqOfCtr;

            double sunAppLon = sunTrueLon - 0.00569-0.00478 * Math.Sin(Math.PI * (125.04 - 1934.136 * julianCentury) / 180);
            double meanObliqEcliptic = 23 + (26 + ((21.448 - julianCentury * (46.815 + julianCentury * (0.00059 - julianCentury * 0.001813)))) / 60) / 60;
            double obliqCorr = meanObliqEcliptic + 0.00256 * Math.Cos((Math.PI * (125.04-1934.136 * julianCentury)) / 180);

            double sunDeclin = (180 * (Math.Asin(Math.Sin((Math.PI * obliqCorr) / 180)*Math.Sin((Math.PI * sunAppLon) / 180)))) / Math.PI;
            double vary = Math.Tan((Math.PI * obliqCorr/2) / 180) * Math.Tan((Math.PI * obliqCorr/2) / 180);

            double eqOfTime = (180 * (vary * Math.Sin(2 * ((Math.PI * meanSolarLong) / 180)) - 2 * eccentEarthOrbit * Math.Sin((Math.PI * meanAnom) / 180) + 4 * eccentEarthOrbit * vary * Math.Sin(((Math.PI * meanAnom) / 180)) * Math.Cos(2 * ((Math.PI * meanSolarLong) / 180)) - 0.5 * vary * vary * Math.Sin(4 * ((Math.PI * meanSolarLong) / 180)) - 1.25 * eccentEarthOrbit * eccentEarthOrbit * Math.Sin(2 * ((Math.PI * meanAnom) / 180)))) / Math.PI;
            double haSunrise = ((180 * (Math.Acos(Math.Cos((Math.PI * 90.833) / 180) / (Math.Cos((Math.PI * lat) / 180) * Math.Cos((Math.PI * sunDeclin) / 180)) - Math.Tan((Math.PI * lat) / 180) * Math.Tan((Math.PI * sunDeclin) / 180)))) / Math.PI)/360;
            
            double daySolarNoon = (180 - lon - eqOfTime)/360; // Solar noon in fraction of a day
            DateTime solarNoon = day + TimeSpan.FromDays(daySolarNoon);

            SolarCalcReturn retObj = new SolarCalcReturn();
            retObj.sunrise = solarNoon - TimeSpan.FromDays(haSunrise);
            retObj.sunset = solarNoon + TimeSpan.FromDays(haSunrise);

            return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(retObj));
        }
    }
}
