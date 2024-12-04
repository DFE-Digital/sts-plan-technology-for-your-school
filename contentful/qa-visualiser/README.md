# QA visualiser

Python project for creating visualisations of the different question pathways for each topic, to support QA testing.

## Overview

The tool makes a call to a cms endpoint within the plan-tech web app that returns all sections from Contentful.
Then for each section, it generates a graph of all possible pathways through the section by making every question a
node, and every link between an answer and its next question an edge.

The `models.py` file uses Pydantic to ensure the data coming back from the cms endpoint has the fields required for the
visualisation generator to work.
The models do not contain all the fields that will be present within the json content but just those that are essential
for the generator.
Extra fields are allowed but missing fields raise a validation error.

## Local setup

To run the qa-visualiser locally:

1. Install [uv](https://github.com/astral-sh/uv)
2. Install [graphviz](https://graphviz.org/download/)
3. Setup your .env file by copying .env.example and filling in the required environment variables:

   | Variable         | Description                                            | Example / Location                                 |
   |------------------|--------------------------------------------------------|----------------------------------------------------|
   | PLANTECH_API_KEY | The API key the cms controller uses for authentication | Keyvault value for `api--authentication--keyvalue` |
   | PLANTECH_API_URL | Base url of the cms controller                         | https://localhost:8080/api/cms                     |

4. create and activate a virtual environment with:
    ```bash
    cd qa-visualiser
    uv venv
    source .venv/bin/activate
    ```
5. Install project dependencies with:
    ```bash
    uv sync
    ```
6. Run the tool to generate visualisations with:
    ```bash
    uv run main.py
    ```

## Linting and formatting

Linting and formatting can be done with:

```bash
uv run ruff check
uv run ruff check --fix # fix linting errors automatically where possible
uv run ruff format
```

## Tests

The tests use pytest and can be run with:

```bash
uv run pytest
```

## Certificates

By default, the python project may not trust your locally running instance of plan-tech.
You can provide a path to its certificate in your environment variables to fix this.

First from the plan-tech repo run
```bash
dotnet dev-certs https --export-path contentful/qa-visualiser/localhost.crt --format PEM
```

Then either run
```bash
export REQUESTS_CA_BUNDLE=path/to/localhost.crt
```
or put `REQUESTS_CA_BUNDLE` in your `.env` file.
