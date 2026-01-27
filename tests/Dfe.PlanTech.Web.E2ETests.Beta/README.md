# README

## Extensions

- Cucumber for visual studio

## Structure of tests

The structure of the tests are separated into two workspaces. There is "regressions" and "smoke".

regression - full e2e regression suite for more thorough e2e testing on test environments.
smoke - small set of tests when merging code in/deployments.

Since there is overlap between step definition names, the project has two different workspaces so the intellisense works correctly.

In VS Code go to file and open workspace and select either smoke or regression. This will align intellisense to the step definitions. You will not be able to run both sets of tests under the same workspace.

### Regression tests

The regression tests have tags with either @parallel or @serial. This is to help speed the tests up. Parallel tests are ones which require no assessments to be done. (e.g. cookies, component page tests etc). Everything else is tagged with @serial

## Setting up Login

Run `npm run test:login` - This will create the different session.jsons that the tests uses to login. It will store these in the storage folder.

## Running the tests

### Regression

| Script Name          | Command                                                                                                                                                     | Description                                                                  |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| test:record:parallel | cucumber-js -p regression --parallel 4 --retry 4 --format json:reports/cucumber-parallel.json --world-parameters="{\"record\": true}" --tags "not @serial"  | Runs regression tests in parallel (4 workers), records run, excludes @serial |
| test:record:serial   | cucumber-js -p regression --retry 4 --format json:reports/cucumber-serial.json --world-parameters="{\"record\": true}" --tags @serial                       | Runs @serial regression tests sequentially with recording enabled            |
| test:parallel        | cucumber-js -p regression --retry 4 --parallel 4 --format json:reports/cucumber-parallel.json --world-parameters="{\"record\": false}" --tags "not @serial" | Runs regression tests in parallel (4 workers) without recording              |
| test:serial          | cucumber-js -p regression --retry 4 --format json:reports/cucumber-serial.json --world-parameters="{\"record\": false}" --tags @serial                      | Runs @serial regression tests sequentially without recording                 |
| test:record          | npm run test:record:parallel && npm run test:record:serial                                                                                                  | Runs all regression tests with recording (parallel first, then serial)       |
| test:all             | npm run test:parallel && npm run test:serial                                                                                                                | Runs all regression tests without recording                                  |
| test:record:smoke    | cucumber-js -p smoke --retry 4 --format json:reports/cucumber-smoke.json --world-parameters="{\"record\": true}"                                            | Runs smoke tests with recording enabled                                      |
| test:smoke           | cucumber-js -p smoke                                                                                                                                        | Runs smoke tests with default settings                                       |

Key commands are:

regression:

npm run test:all - runs all regression tests
npm run test:record - runs all regression tests and records the videos/traces/screenshots

smoke:

npm run test:smoke - runs all smoke tests
npm run test:record:smoke - runs all smoke test and records the videos/traces/screenshots

## Environment Variables

| Variable                     | Value                                                                                                                                                |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| URL                          | URL of the app (end with a /)                                                                                                                        |
| DSI_SCHOOL_EMAIL             | DSI School login email                                                                                                                               |
| DSI_SCHOOL_PASSWORD          | DSI School login password                                                                                                                            |
| DSI_SCHOOL_ESTABLISHMENT_REF | DSI School establishment ref (used for clearing db)                                                                                                  |
| DSI_MAT_EMAIL                | DSI MAT login email                                                                                                                                  |
| DSI_MAT_PASSWORD             | DSI MAT login password                                                                                                                               |
| DSI_MAT_ESTABLISHMENT_REF    | DSI MAT establishment ref (used for clearing db)                                                                                                     |
| DSI_NOORG_EMAIL              | DSI No organisation login email                                                                                                                      |
| DSI_NOORG_PASSWORD           | DSI No organisation login password                                                                                                                   |
| HEADLESS                     | false or true - whether you want to run headless or not. This is used in CI.                                                                         |
| DB_USER                      | Database User                                                                                                                                        |
| DB_PASSWORD                  | Database Password                                                                                                                                    |
| DB_SERVER                    | Database Server (localhost)                                                                                                                          |
| DB_PORT                      | Database Port (1433)                                                                                                                                 |
| DB_DATABASE                  | Database name (plantech-db)                                                                                                                          |
| DB_MODE                      | sql or azure (Set to SQL if running db locally, set to azure if using azure db. There are different methods we use to authenticate + clear the db. ) |

DSI_EMAIL\* variables can be found in the azure keyvault under "e2e-tests" for the respective environments.
URL - this is the URL the tests will be running against - e.g. https://localhost:8080/ or https://environment.plan-technology-for-your-school.gov.uk/

## Test tags

@parallel - runs the tests in parallel with the other tests tagged with @parallel
@serial - runs those sets of tests one after another.

@user-school - runs the test under the DSI School Login account
@user-mat - runs the test under the DSI School MAT account
@user-noorg - runs the test under the DSI School no-org account.

@clear-data-school - runs the clear establishment data stored proc against the db against the school ref listed in environment variable DSI_School_Establishment_Ref

@selected-school-miscellaneous - for MAT tests, it will select the miscellaneous school in the select-a-school page. This is generated from the matConstants.ts file - e.g. you can use @selected-school-community, @selected-school-foundation

@smoke - Tests which are for the smoke tests.
