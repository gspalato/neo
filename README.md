<p align="center">
	<img src="https://i.ibb.co/G9rQ1cC/custom-1.png" width="800"/>
</p>
<p align="center">
	<img href="https://forthebadge.com" src="https://forthebadge.com/images/badges/made-with-c-sharp.svg">
	<img href="https://forthebadge.com" src="https://forthebadge.com/images/badges/built-with-love.svg">
	<img href="https://forthebadge.com" src="https://forthebadge.com/images/badges/gluten-free.svg">
	<br/>
	<img src="https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2Fgspalato%2Faxion%2Fbadge%3Fref%3Dv2%26token%3Df01e610fbbe81abcbd4fdc800e745acefd140a17&style=for-the-badge">
</p>

## 📦 Getting Started
### Prerequisites

-  [x] [Visual Studio 2019+](https://visualstudio.microsoft.com/)
-  [x] [.NET Core 3.1](https://dotnet.microsoft.com/)
-  [x] [Lavalink](https://github.com/Frederikam/Lavalink) 
-  [x] [MongoDB](https://www.mongodb.com/)

### Installing

* Clone the repository;
* Restore;
* Compile;
* Create an `appsettings.json` file on the root folder:
```json
{
	"TOKEN": "lmao.u.gay",
	"PREFIX": "",
	"LAVALINK": "",
	"MongoDB": {
    	"Database": "axion",
    	"ConnectionStrings": "mongodb://localhost:27017"
	}
}
```

#### With Docker
* Refer to the **Installing** step.
* Pull a MongoDB docker image and configure it accordingly;
* Make a custom lavalink image with the proper configuration file at `Axion.Docker`;
* Make the bot's image through `docker build .`;
* Running the bot should be close to this resumed command:
```bash
docker run -d -p 27017:27017 --network host --name g_db mongo             # Run the database
docker run -d -p 2333:2333   --network host --name g_ll gspalato/lavalink # Run Lavalink
docker run -d                --network host --name g_axion gspalato/axion # And then run the bot.
```

## 👨‍🏫 Authors

- **Gabriel Spalato Marques** (ohinoki, o_Hinoki) - Only developer

## 📝 License

Unlicensed, you must not copy, modify, redistribute, sell, "remix" or share in any medium, shape or form. 

## 👥 Acknowledgements

- [**foxbot**](https://github.com/foxbot) and others \- Developer(s) of Discord.NET;
- [**Quahu**](https://github.com/Quahu) \- Developing Qmmands;
- [**Izumemori**](https://github.com/Izumemori) \- Helping me understand C# concepts and fixing my crap;
- [**trinitrotoluene**](https://github.com/trinitrotoluene) \- Emotional support (or lack thereof);
