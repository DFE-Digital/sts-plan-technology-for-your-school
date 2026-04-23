# QA Visualiser

A Python tool that generates visual flowcharts of every possible question pathway through each questionnaire section. Used to support QA testing by making the branching logic of the questionnaire easy to inspect visually.

## What it produces

PNG flowcharts saved to `visualisations/`, one per section. Each chart shows:

- Question nodes (grey boxes)
- Answer nodes (grey ellipses) with edges to the next question
- A "Check Answers" endpoint
- Optionally: recommendation nodes (yellow) with colour-coded edges — green for completing recommendations, orange for in-progress

## Language and tooling

Python 3.12+, managed with [uv](https://github.com/astral-sh/uv). Uses [Pydantic](https://docs.pydantic.dev/) for data validation and [Graphviz](https://graphviz.org/) for rendering.

## Local setup

1. Install [uv](https://github.com/astral-sh/uv)
2. Install [Graphviz](https://graphviz.org/download/) (must be on your PATH)
3. Copy `.env.example` to `.env` and fill in the values:

| Variable | Description | Example |
|---|---|---|
| `PLANTECH_API_KEY` | API key for the CMS controller | From Key Vault: `api--authentication--keyvalue` |
| `PLANTECH_API_URL` | Base URL of the CMS controller | `https://localhost:8080/api/cms` |
| `DISPLAY_RECOMMENDATIONS` | Show recommendation headers on the chart | `true` / `false` |
| `REQUESTS_CA_BUNDLE` | Path to SSL certificate (local dev only) | `./localhost.crt` |

4. Create and activate a virtual environment:

```bash
cd contentful/qa-visualiser
uv venv
source .venv/bin/activate   # Windows: .venv\Scripts\activate
```

5. Install dependencies:

```bash
uv sync
```

6. Run the tool:

```bash
uv run main.py
```

Flowcharts are saved to `visualisations/`.

## Trusting the local dev certificate

By default Python does not trust the ASP.NET Core development certificate. Export it first:

```bash
dotnet dev-certs https --export-path contentful/qa-visualiser/localhost.crt --format PEM
```

Then set `REQUESTS_CA_BUNDLE=./localhost.crt` in `.env`.

## Tests

```bash
uv run pytest
```

## Linting and formatting

```bash
uv run ruff check          # lint
uv run ruff check --fix    # auto-fix where possible
uv run ruff format         # format
```

## See also

- [Export processor](../export-processor/README.md) — provides the question and answer data this tool visualises
- [Contentful tooling overview](../README.md)
- [GitHub Actions workflows](../../.github/README.md) — `qa-viz.yml` runs this tool
