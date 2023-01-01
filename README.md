# Falcon 🐦

Terminal/PowerShell chat application.

## Description 🧾

Falcon is terminal chat app built on .NET platform.

## Get started 🚀

### Develop localy 💻

To develop falcon app first clone this repository using git:

```bash
git clone https://github.com/MaciekWin3/Falcon
cd Falcon
dotnet restore
```

After downloading project you will need to setup postgresql database and update `appsettings.json` in both projects. You can setup your postgres locally with whole server app, but if you want to host it on Azure we recomend using [Supabase](https://supabase.com/).

descripbe setup

After database is created you need to update appsettings.json files. In server project under ConnectionStrings:Supabase put your connection string that you acquired from Supabase. After that you need to setup Entity Framework with these command:

```bash
Commands
```

Then run both project with and check if you can login. If you are being redirected to Lobby windows, then congratulations you successfuly seted up Falcon Application 🎉

### Setup Server with Azure ☁

The easiest way to host your falcon server is to use Azure services. To do this you need to ...

### Use client app 💬

To connect to server download falcon client app from nuget ussing this command from your terminal:

```bash
dotnet tool install ...
```

For this command to work you must have the latest .NET version installed on your machine.

## Contribute 🤝

Contributions to project are welcome!

## License 📕

This project is licensed MIT

## Author 📝

Maciej Winnik
