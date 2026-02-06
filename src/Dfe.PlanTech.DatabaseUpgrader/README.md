# Dfe.PlanTech.DatabaseUpgrader

This is a project that updates SQL database changes by using [DbUp](https://dbup.readthedocs.io/en/latest/)

## Execution

When running the project, it takes several command line arguments:

| Long name        | Short Name | Purpose                                                            | Required | Example                                                                          |
| ---------------- | ---------- | ------------------------------------------------------------------ | -------- | -------------------------------------------------------------------------------- |
| connectionstring | c          | Connection string to execute scripts on                            | Yes      | -c Server=[server]; Database=[database]; User ID=[username]; Password=[password] |
| env              | N/A        | Set environment(s) to run specific environment specific scripts on | No       | --env dev test                                                                   |
| sql-params       | p          | Any SQL params that might be needed for SQL scripts                | No       | -p firstName=Bob surname=Dole                                                    |

## SQL Transactions

DbUp has been configured to run in single transaction mode which means if one script fails then all changes will be rolled back. There are limitations to this, for more details please refer to the following [Microsoft Article](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/transactions-sql-data-warehouse?view=aps-pdw-2016-au7#limitations-and-restrictions).

## MSSQL scripts

> **⚠ Do not add the sql scripts directly within Visual Studio, they are picked up automatically when added to the scripts folder ⚠**

The Scripts folder contains a folder for each year, which contains the MSSQL scripts for that year. The naming convention for a MSSQL scripts is `[year]_[time]_ScriptName.sql`. The scripts are executed in ascending order, so it's important to make sure that the year and time of the script name reflects the intended order of execution.

Once a script has run, the DbUp services writes an entiry to the SchemaVersions table on the database. This is used to track whether a script has been run. Once a script name is added to SchemaVersions, the DbUp service will not run that script again unless you remove the scripts entry from SchemaVersions.

You can add a new script by placing it in the relevant year folder in Scripts. The DbUp service has the following flag set in the .cspoj file in order to import it automatically:

```
  <ItemGroup>
    <EmbeddedResource Include="**\*.sql" />
  </ItemGroup>
```

### Environment Specific Scripts

There may be certain SQL scripts that should be executed only against certain environments. These are stored in the [EnvironmentSpecificScripts](./EnvironmentSpecificScripts) folder, then within a subfolder with their respective environment, finally they follow the convention mentioned above (i.e. a subfolder per year, then `[year]_[time]_[script_name].sql`)

E.g. scripts created in 2023 for dev environment should be in `/EnvironmentSpecificScripts/dev/2023/`
