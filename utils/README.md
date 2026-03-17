# Utils

Standalone utility scripts for operational and data management tasks.

## Tools

| Directory | Language | Purpose |
|---|---|---|
| [`retrieve-user-data/`](retrieve-user-data/README.md) | Node.js | Looks up user details (email, name, organisations) from the DfE Sign-in Public API given a list of DSI user IDs |
| [`update-gias-data/`](update-gias-data/README.md) | Python (uv) | Fetches the latest GIAS establishment and group data and updates the `dbo.establishmentGroup` and `dbo.establishment` tables |

## When to use these

- **`retrieve-user-data`** — ad-hoc data requests where you have a list of DSI references and need to identify users. Requires DSI client credentials from the PTFYS DSI Manage console.

- **`update-gias-data`** — runs automatically via GitHub Actions on a daily schedule (`update-gias-data-scheduled.yml`). Run manually if the scheduled job fails or if you need an out-of-cycle refresh. GIAS data has no public API; the tool uses Playwright to download a ZIP of group and establishment data from the GIAS website.
