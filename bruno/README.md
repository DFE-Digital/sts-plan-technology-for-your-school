# Bruno requests

This folder contains a collection of useful [bruno](https://usebruno.com/) requests, for our web app, Contentful, and other APIs we might need to interface with in the future.

## Usage

### Setup

1. [Download and install bruno](https://www.usebruno.com/downloads)
2. Open bruno
3. Click "Open Collection"
4. Browse to this directory, and open it

### Environment variables

There are various variables used for some requests in the collection.

A lot of them are set at the _collection_ level. These have been configured to use environment variables.

To set these up:

1. Copy the `.env.example` file
2. Rename it to `.env`
3. Set the various required secrets

More information on bruno's environment variables usage is available

### Other variables

Some other requests use variables that are defined per request. E.g. `CMS Webhook API` has various variables configured for the specific headers. These can be configured by going to the `vars` tab in the specific request

## Future work

We should additional requests in the collection. These could/should be additional Contentful requests, or requests to other services such as DFE Sign In.

We could add API tests to the collection. At the moment we only have the one route to test (the CMS webhook route), but it would be useful to add some basic testing here.

## Additional information

- ["what is bruno"](https://docs.usebruno.com/introduction/what-is-bruno) (bruno documentation)
- [Variables overview](https://docs.usebruno.com/get-started/variables/overview) (bruno documentation)
- [dotenv file information](https://docs.usebruno.com/secrets-management/dotenv-file) (bruno documentation)
