<p align="center">
	<img src="https://i.ibb.co/GchDVq0/Logo-v2-Text.png" width="800"/>
</p>
<p align="center">
	<img href="https://forthebadge.com" src="https://forthebadge.com/images/badges/made-with-c-sharp.svg">
	<img href="https://forthebadge.com" src="https://forthebadge.com/images/badges/built-with-love.svg">
	<img href="https://forthebadge.com" src="https://forthebadge.com/images/badges/gluten-free.svg">
	<br/>
	A work-in-progress bot with music and it's own interactivity library.
	<br/>
</p>

## 📦 Getting Started

### Prerequisites

- [x] [.NET 7](https://dotnet.microsoft.com/)
- [x] [Lavalink](https://github.com/Frederikam/Lavalink/)
- [x] [MongoDB](https://www.mongodb.com/)
- [x] [Docker](https://docker.com/)

### Installing

- Clone the repository;
- Restore;
- Compile;
- Create a `.env` file on the root folder:

```env
# Discord
NEO__Discord__Token=
NEO__Discord__MainGuildId=

# Lavalink
NEO__Lavalink__RestUri=http://lavalink:2333
NEO__Lavalink__WebSocketUri=ws://lavalink:2333
NEO__Lavalink__Password=youshallnotpass

# Database
NEO__Database__Url=mongodb://root:example@db
NEO__Database__Name=neo

MONGO_INITDB_ROOT_USERNAME=root
MONGO_INITDB_ROOT_PASSWORD=example
```

#### With Docker

- Run `docker compose build && docker compose up -d`.

## 👨‍🏫 Authors

- **Gabriel Spalato Marques** (@gspalato) ➔ CEO & Founder of C# shitcode.

## 📝 License

```
Copyright 2023 Gabriel Spalato Marques

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

## 👥 Acknowledgements

- [**foxbot**](https://github.com/foxbot) and others \- Developer(s) of Discord.NET;
