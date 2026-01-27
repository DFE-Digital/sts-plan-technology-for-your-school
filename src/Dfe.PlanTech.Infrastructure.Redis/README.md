# Dfe.PlanTech.Infrastructure.Redis

Project containing code for caching with Redis

## How to use

You will need to set the following .NET secret (for Dfe.PlanTech.Web) to setup your connection to redis

| Env variable            | Description                           | Example                                          |
| ----------------------- | ------------------------------------- | ------------------------------------------------ |
| ConnectionStrings:Redis | Connection string for the redis cache | plantech.redis.cache,password=password,flag=flag |

These should be set in the `Azure KeyVault` for deployed instances, or `dotnet user-secrets` for local testing.

## Running Locally

You can run an instance of Redis locally for testing, instead of using the development one in Azure.
There is more than one way to do this, but heres how you can do so with docker:

1. Run a redis container locally
   ```bash
   docker run -p 6379:6379 --name plantech-redis -d redis
   ```
2. Set your connection string to `localhost:6379,abortConnect=false`
3. Start plan tech as normal

## Additional Information

If you're using a JetBrains IDE you can connect to the redis instance with the same connection string for easy viewing and editing of items
