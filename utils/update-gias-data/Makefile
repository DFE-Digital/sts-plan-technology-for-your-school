lint:
	uv run ruff check --fix
	uv run ruff format

run:
	uv run main.py

setup:
	uv venv
	(source .venv/bin/activate)
	uv sync
	uv run playwright install --with-deps webkit
