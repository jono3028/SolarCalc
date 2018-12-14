# SolarCalc
Azure Function to determine sunrise and sunset times.

## Description
A simple and straightforward function that when given latitude, longitude, and date returns the sunrise and sunset times of the given location. The times returned are currently in UTC with plans to return local times as well.

## Usage
A `POST` request made to the Azure Function with a JSON payload. The JSON has three parameters, "lat", "lon", and "day" (lat and lon are floats, day is a DateTime in ISO 8601 format).

Payload example:
```javascript
{
    "lat": "47.6",
    "lon": "-122",
    "day": "2018-12-14"
}
```
**Note:** The Day parameter must exclude the time info or be set to midnight UTC (2018-12-14T00:00:00.000Z is also acceptable).

## ToDo
* Input validation and error handling
* Time zone adjustment
* Refactor, refactor, refactor

## Authors

* **Jonathan Owen** - *Initial work* - [jono3028](https://github.com/jono3028)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments
* [NOAA Solar Calulator](https://www.esrl.noaa.gov/gmd/grad/solcalc/)