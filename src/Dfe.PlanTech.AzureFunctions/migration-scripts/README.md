# Contentful Migration Helper Scripts

## Export entries from contentful

### Prerequisite to running contenful export
1. Install Contentful CLI - ``npm install -g contentful-cli``

#### Configure export

To export all entry data from contentful we make use of the contentful CLI which exports the entry data for us in JSON format, which we will use to invoke the contentful webhook.

We configure which enviroment we want to export data from in a ``config.json`` file

```
{
  "spaceId": "",           #The spaceID for contentful - can be found on the web app or secrets.
  "managementToken": "",   #This token can be generated from the contentful web app.
  "environmentId" : "dev", #The Environment we are exporting in contentful.
  "contentOnly" : true     #This will only export content data for us
}
```
#### Run export

Use the following command to begin the export when you are happy with the config.

```contentful space export --config config.json```

This will export all the contentful entries we need to migrate into a file with format ```contentful-export-<spaceId>-<env>-<date-time>.json```

The output will be similar to this:

```
  ✔ Initialize client (1s)
  ✔ Fetching data from space
    ✔ Connecting to space (1s)
    ↓ Fetching content types data [skipped]
    ✔ Fetching tags data (1s)
    ↓ Fetching editor interfaces data [skipped]
    ✔ Fetching content entries data (1s)
    ✔ Fetching assets data (1s)
    ↓ Fetching locales data [skipped]
    ↓ Fetching webhooks data [skipped]
    ↓ Fetching roles data [skipped]
  ↓ Download assets [skipped]
  ✔ Write export log file
    ✔ Lookup directory to store the logs
    ✔ Create log directory
    ✔ Writing data to file
┌───────────────────┐
│ Exported entities │
├───────────┬───────┤
│ Tags      │ 0     │
├───────────┼───────┤
│ Entries   │ 695   │
├───────────┼───────┤
│ Assets    │ 0     │
└───────────┴───────┘
The export took less than a minute (2s)

Stored space data to json file at: <Filename>.json
No errors or warnings occurred
The export was successful.

```

## Invoking the contentful webhook
To upload the data into db we will use the ```contentful-migration.py``` script to call the contentful webhook.

#### Prerequisite to running scripts
1. Python installed
2. Install `requests` package using `pip3 install requests`

We can call the script by passing in the filename of the export and the contentful webhook URL e.g 

```python3 contentful-migtation.py <contentful-entry-export>.json "<contnetful-webhook-url>"``` 

This will do a post for every entry in the export file.

**N.B** *This will cause failures in some component migrations due to dependencies with other content types - to get around this issue use the optional args described below.*

### Optional arguments

To give us some more control over the migration and aid reconciliation of exported data we can pass in some optional arguments to the script

#### Content Type

```--content-type``` - We can pass in specific content types we want to migrate for example ```header``` ```page``` etc 

*There is a helper script called `unique-content-types.json` which will give us a list of all the different content types in the export.*

#### Make id's Empty

```--make-ids-empty``` In some cases we have content types which are not self referential, this can cause issues in the migration process if the dependencies we require have not been migrated. To get around this issue we have enhanced the script to strip out id which reference other content types
for example:

When migrating questions from the export file we would need to strip out id's for answer content types as they would not exist in the database so we could run the script as follows:

```python3 contentful-migtation.py <contentful-entry-export>.json "<contnetful-webhook-url>" --content-type question --make-ids-empty```

Once the answer components have been migrated we could re run the question component migration by removing the ```make-ids-empty``` flag and this would update the question data and link to the correct answer data.

#### Delays between entry migration

```--delay``` This optional argument is available to set a delay in between each call the default delay is `1.0` in `seconds`


## Appendix

### Prerequisite to running scripts
1. Python installed
2. Install `requests` package using `pip3 install requests`

### unique-content-types.py
This script will give us a list of all the different content types in the export from contentful. 

**N.B** *You'll need to change the filename inside the script* 

**TODO:** *Make file name external arg*

Sample output:

```
Unique Content Types:
section
componentDropDown
button
navigationLink
question
dynamicContent
category
page
warningComponent
insetText
buttonWithEntryReference
header
textBody
title
buttonWithLink
recommendationPage
answer

```

This script is useful when migrating entries one component type at a time.



