# Tools

This directory contains tooling to inventory languages and file types
in the repository in order to support decisions about linters/formatters.

These outputs can be used to get an overview of what languages we're using
and therefore the amount of configuration needed in the formatting configs.

## Scripts

All scripts are designed to be run from anywhere inside the repo.

### Inventory (tracked files)

This script writes the tracked files list to:

- `linting/output/tracked-files.txt`

and then prints reports.

```bash
./linting/scripts/list-file-extensions.sh
```

#### Outputs

`output/tracked-files.txt` (ignored)

- raw list of tracked files used as input

## Repository configuration

### [.gitattributes](../.gitattributes)

Tells git what line endings to use.

```config
* text=auto eol=lf

# Keep Windows scripts sane if you have them (optional; delete if you don't)
*.bat text eol=crlf
*.cmd text eol=crlf
```

### [.pre-commit-config.yaml](../.pre-commit-config.yaml)

As pre-commit passes filenames after `--include` we can configure how to
treat specific files.

That hooks run only on staged files because pre-commit passes filenames after `--include`.

#### Universal hygiene

- Checks JSON
- Checks yaml
- Fixes ends of files
- Removes trailing whitespace
- Ensures line endings become LF

#### Markdown

- Formats markdown files

#### .NET formatting (avoids opinionated style rewrites)

- Uses CSharpier
- Formats C# code according to accepted guidelines

#### Web and config formats

- Uses prettier to format JavaScript, Typescript, styling, HTML, YAML, JSON

### [.prettier-ignore](../.prettier-ignore)

Used to disregard key files.

- Minified JavaScript
- Map files,
- Anything in library folders
  - wwwroot/lib
  - /node_modules/
  - /dist/
  - /build/
