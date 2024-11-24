# QA visualiser

Python project for creating visualisations of the different question pathways for each topic, to support QA testing.

## Local setup

To run the qa-visualiser locally:
1. Install [uv](https://github.com/astral-sh/uv)
2. Setup your .env file by copying .env.example and adding the relevant keys
3. create and activate a virtual environment by running
    ```bash
    cd qa-visualiser
    uv venv
    source .venv/bin/activate
    ```
4. Run the tool to generate visualisations with
    ```bash
    uv run main.py
    ```

## Linting and formatting

Linting and formatting can be done with the commands:
```bash
uv run ruff check
uv run ruff check --fix # fix linting errors automatically where possible
uv run ruff format
```
