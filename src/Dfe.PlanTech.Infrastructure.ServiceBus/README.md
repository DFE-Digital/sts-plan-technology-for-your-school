# Dfe.PlanTech.Infrastructure.ServiceBus

Project that handles concrete implementation of connection/usage of an Azure Service Bus.

Currently used for recieving CMS updates from contentful and invalidating the relevant cache items.
For more information check out our [Contentful documentation](/docs/cms/README.md)

## Setup

### Environment Variables

When adding the required services from this project to the web app using the [AddDbWriterServices method in the DependencyInjection class](./DependencyInjection.cs), it configures a connection to an Azure Service Bus. To do this, it uses connection string `ServiceBus` for the Service Bus connection string.

For more information on how to configure your environment variables, please refer to the [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers).
